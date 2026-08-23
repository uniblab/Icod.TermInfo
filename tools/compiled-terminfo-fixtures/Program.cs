using System.Buffers.Binary;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Icod.TermInfo.FixtureGenerator;

internal static class Program
{
    private const string ExpectedTicVersion = "ncurses 6.5.20250216";
    private const ushort LegacyMagic = 0x011A;
    private const ushort ExtendedNumberMagic = 0x021E;

    public static int Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        bool checkOnly =
            args.Length == 1
            && string.Equals(
                args[0],
                "--check",
                StringComparison.Ordinal);

        if (args.Length != 0
            && !checkOnly)
        {
            Console.Error.WriteLine(
                "Usage: dotnet run --project tools/compiled-terminfo-fixtures/"
                + "Icod.TermInfo.FixtureGenerator.csproj [--check]");
            return 2;
        }

        Dictionary<string, byte[]>? snapshot = null;
        string? fixtureRoot = null;

        try
        {
            string root = FindRepositoryRoot();
            fixtureRoot =
                Path.Combine(
                    root,
                    "tests",
                    "Icod.TermInfo.Tests",
                    "fixtures",
                    "compiled-terminfo");

            if (checkOnly)
            {
                snapshot =
                    CaptureCorpus(
                        fixtureRoot);
            }

            string version =
                RunProcess("tic", ["-V"]).PreferredOutput.Trim();
            if (!string.Equals(
                    version,
                    ExpectedTicVersion,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "fixture provenance mismatch: expected "
                    + $"'{ExpectedTicVersion}', found '{version}'");
            }

            CompileSources(fixtureRoot);
            CreateAdversarialSeeds(fixtureRoot);

            if (checkOnly)
            {
                VerifyCorpusMatches(
                    fixtureRoot,
                    snapshot!);

                Console.WriteLine(
                    "Compiled fixture corpus matches pinned ncurses provenance.");
            }

            PrintHashes(fixtureRoot);
            return 0;
        }
        catch (Exception exception) when (
            exception is IOException
            or UnauthorizedAccessException
            or InvalidDataException
            or InvalidOperationException
            or Win32Exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
        finally
        {
            if (checkOnly
                && fixtureRoot is not null
                && snapshot is not null)
            {
                RestoreCorpus(
                    fixtureRoot,
                    snapshot);
            }
        }
    }

    private static string FindRepositoryRoot()
    {
        string[] starts =
        [
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory,
        ];

        foreach (string start in starts)
        {
            DirectoryInfo? current = new(start);
            while (current is not null)
            {
                if (File.Exists(
                        Path.Combine(
                            current.FullName,
                            "Icod.TermInfo.csproj"))
                    && Directory.Exists(
                        Path.Combine(
                            current.FullName,
                            "tests",
                            "Icod.TermInfo.Tests",
                            "fixtures",
                            "compiled-terminfo")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }
        }

        throw new InvalidOperationException(
            "Unable to locate the Icod.TermInfo repository root.");
    }

    private static void CompileSources(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        string compiled = Path.Combine(root, "compiled");
        Directory.CreateDirectory(compiled);

        string temp =
            Path.Combine(
                Path.GetTempPath(),
                "icod-terminfo-t29-" + Guid.NewGuid().ToString("N"));

        try
        {
            string sourceDirectory = Path.Combine(root, "source");
            string[] sources =
                Directory
                    .EnumerateFiles(sourceDirectory, "*.ti")
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToArray();

            foreach (string source in sources)
            {
                ResetDirectory(temp);

                RunProcess(
                    "tic",
                    [
                        "-x",
                        "-o",
                        temp,
                        source,
                    ]);

                string terminalName =
                    Path.GetFileNameWithoutExtension(source);
                string[] matches =
                    Directory
                        .EnumerateFiles(
                            temp,
                            terminalName,
                            SearchOption.AllDirectories)
                        .ToArray();

                if (matches.Length != 1)
                {
                    throw new InvalidOperationException(
                        $"expected one compiled entry for {terminalName}, "
                        + $"got {matches.Length}");
                }

                File.Copy(
                    matches[0],
                    Path.Combine(compiled, terminalName + ".bin"),
                    overwrite: true);
            }
        }
        finally
        {
            if (Directory.Exists(temp))
            {
                Directory.Delete(temp, recursive: true);
            }
        }

        string edgePath =
            Path.Combine(compiled, "t29-legacy-edge.bin");
        byte[] edge = File.ReadAllBytes(edgePath);
        ushort namesSize = ReadUInt16(edge, 2);
        ushort booleanCount = ReadUInt16(edge, 4);
        if (booleanCount < 1)
        {
            throw new InvalidDataException(
                "edge fixture has no Boolean table");
        }

        edge[12 + namesSize] = 0xFE;
        File.WriteAllBytes(edgePath, edge);
    }

    private static void CreateAdversarialSeeds(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        string compiled = Path.Combine(root, "compiled");
        string malformed = Path.Combine(root, "malformed");
        Directory.CreateDirectory(malformed);

        byte[] minimal =
            File.ReadAllBytes(
                Path.Combine(compiled, "t29-legacy-minimal.bin"));
        byte[] extended =
            File.ReadAllBytes(
                Path.Combine(compiled, "t29-extended.bin"));

        File.WriteAllBytes(
            Path.Combine(malformed, "truncated-header.bin"),
            minimal[..8]);

        byte[] data = (byte[])minimal.Clone();
        WriteUInt16(data, 4, 0xFFFF);
        File.WriteAllBytes(
            Path.Combine(malformed, "impossible-count.bin"),
            data);

        data = (byte[])minimal.Clone();
        ushort namesSize = ReadUInt16(data, 2);
        data[12 + namesSize - 1] = (byte)'X';
        File.WriteAllBytes(
            Path.Combine(malformed, "bad-names-terminator.bin"),
            data);

        data = (byte[])minimal.Clone();
        ushort names = ReadUInt16(data, 2);
        ushort booleans = ReadUInt16(data, 4);
        ushort numbers = ReadUInt16(data, 6);
        ushort strings = ReadUInt16(data, 8);
        ushort table = ReadUInt16(data, 10);
        int offset = 12 + names + booleans;
        if ((offset & 1) != 0)
        {
            offset++;
        }

        offset += numbers * 2;
        if (strings < 1)
        {
            throw new InvalidDataException(
                "minimal fixture has no string offsets");
        }

        WriteInt16(
            data,
            offset,
            checked((short)(table + 10)));
        File.WriteAllBytes(
            Path.Combine(malformed, "illegal-string-offset.bin"),
            data);

        data = (byte[])minimal.Clone();
        WriteUInt16(data, 0, 0x1234);
        File.WriteAllBytes(
            Path.Combine(malformed, "unsupported-magic.bin"),
            data);

        int end = ConventionalEnd(extended);
        if ((end & 1) != 0)
        {
            end++;
        }

        if (end >= extended.Length)
        {
            throw new InvalidDataException(
                "extended fixture has no ncurses extension");
        }

        File.WriteAllBytes(
            Path.Combine(malformed, "malformed-extended-header.bin"),
            extended[..(end + 6)]);

        data = (byte[])extended.Clone();
        WriteUInt16(data, end, 0xFFFF);
        File.WriteAllBytes(
            Path.Combine(malformed, "impossible-extended-count.bin"),
            data);

        data = (byte[])extended.Clone();
        ushort extBooleans = ReadUInt16(data, end);
        ushort extNumbers = ReadUInt16(data, end + 2);
        ushort extStrings = ReadUInt16(data, end + 4);
        ushort extTableSize = ReadUInt16(data, end + 8);
        int extOffset = end + 10 + extBooleans;
        if ((extOffset & 1) != 0)
        {
            extOffset++;
        }

        int numericWidth =
            ReadUInt16(data, 0) == ExtendedNumberMagic
                ? 4
                : 2;
        extOffset += extNumbers * numericWidth;
        if (extStrings < 1)
        {
            throw new InvalidDataException(
                "extended fixture has no extended string offset");
        }

        WriteInt16(
            data,
            extOffset,
            checked((short)(extTableSize + 5)));
        File.WriteAllBytes(
            Path.Combine(
                malformed,
                "illegal-extended-string-offset.bin"),
            data);

        byte[] collision = (byte[])extended.Clone();
        int collisionOffset =
            LastIndexOf(
                collision,
                [(byte)'x', (byte)'y', (byte)'z', 0]);
        if (collisionOffset < 0)
        {
            throw new InvalidDataException(
                "extended fixture has no xyz capability name");
        }

        byte[] replacement = [(byte)'c', (byte)'u', (byte)'p', 0];
        replacement.CopyTo(collision, collisionOffset);
        File.WriteAllBytes(
            Path.Combine(
                malformed,
                "extended-standard-name-collision.bin"),
            collision);
    }

    private static int ConventionalEnd(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        ushort magic = ReadUInt16(data, 0);
        ushort names = ReadUInt16(data, 2);
        ushort booleans = ReadUInt16(data, 4);
        ushort numbers = ReadUInt16(data, 6);
        ushort strings = ReadUInt16(data, 8);
        ushort table = ReadUInt16(data, 10);

        if (magic != LegacyMagic
            && magic != ExtendedNumberMagic)
        {
            throw new InvalidDataException(
                $"unsupported generated magic: 0x{magic:X4}");
        }

        int offset = 12 + names + booleans;
        if ((offset & 1) != 0)
        {
            offset++;
        }

        int numericWidth =
            magic == ExtendedNumberMagic
                ? 4
                : 2;
        return offset
            + (numbers * numericWidth)
            + (strings * 2)
            + table;
    }

    private static Dictionary<string, byte[]> CaptureCorpus(
        string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        Dictionary<string, byte[]> files =
            new(
                StringComparer.Ordinal);

        foreach (string folder in new[] { "compiled", "malformed" })
        {
            string directory =
                Path.Combine(
                    root,
                    folder);

            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (
                string path
                in Directory
                    .EnumerateFiles(
                        directory,
                        "*.bin")
                    .OrderBy(
                        path => path,
                        StringComparer.Ordinal))
            {
                string relative =
                    Path.GetRelativePath(
                            root,
                            path)
                        .Replace(
                            Path.DirectorySeparatorChar,
                            '/');

                files.Add(
                    relative,
                    File.ReadAllBytes(path));
            }
        }

        return files;
    }

    private static void VerifyCorpusMatches(
        string root,
        IReadOnlyDictionary<string, byte[]> expected)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentNullException.ThrowIfNull(expected);

        Dictionary<string, byte[]> actual =
            CaptureCorpus(
                root);

        string[] expectedNames =
            expected.Keys
                .OrderBy(
                    name => name,
                    StringComparer.Ordinal)
                .ToArray();
        string[] actualNames =
            actual.Keys
                .OrderBy(
                    name => name,
                    StringComparer.Ordinal)
                .ToArray();

        if (!expectedNames.SequenceEqual(
                actualNames,
                StringComparer.Ordinal))
        {
            throw new InvalidDataException(
                "Pinned fixture corpus file set changed.");
        }

        foreach (string name in expectedNames)
        {
            if (!expected[name].AsSpan().SequenceEqual(
                    actual[name]))
            {
                throw new InvalidDataException(
                    $"Pinned fixture differs after regeneration: {name}");
            }
        }
    }

    private static void RestoreCorpus(
        string root,
        IReadOnlyDictionary<string, byte[]> snapshot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentNullException.ThrowIfNull(snapshot);

        foreach (string folder in new[] { "compiled", "malformed" })
        {
            string directory =
                Path.Combine(
                    root,
                    folder);

            if (Directory.Exists(directory))
            {
                foreach (
                    string path
                    in Directory.EnumerateFiles(
                        directory,
                        "*.bin"))
                {
                    File.Delete(
                        path);
                }
            }
        }

        foreach (
            KeyValuePair<string, byte[]> file
            in snapshot.OrderBy(
                pair => pair.Key,
                StringComparer.Ordinal))
        {
            string path =
                Path.Combine(
                    root,
                    file.Key.Replace(
                        '/',
                        Path.DirectorySeparatorChar));
            string? directory =
                Path.GetDirectoryName(
                    path);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(
                    directory);
            }

            File.WriteAllBytes(
                path,
                file.Value);
        }
    }

    private static void PrintHashes(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        foreach (string folder in new[] { "compiled", "malformed" })
        {
            string directory = Path.Combine(root, folder);
            foreach (
                string path
                in Directory
                    .EnumerateFiles(directory, "*.bin")
                    .OrderBy(path => path, StringComparer.Ordinal))
            {
                byte[] digest =
                    SHA256.HashData(File.ReadAllBytes(path));
                string relative =
                    Path.GetRelativePath(root, path)
                        .Replace(
                            Path.DirectorySeparatorChar,
                            '/');
                Console.WriteLine(
                    $"{Convert.ToHexString(digest).ToLowerInvariant()}  {relative}");
            }
        }
    }

    private static ushort ReadUInt16(
        byte[] data,
        int offset)
    {
        ArgumentNullException.ThrowIfNull(data);

        return BinaryPrimitives.ReadUInt16LittleEndian(
            data.AsSpan(offset, sizeof(ushort)));
    }

    private static void WriteUInt16(
        byte[] data,
        int offset,
        ushort value)
    {
        ArgumentNullException.ThrowIfNull(data);

        BinaryPrimitives.WriteUInt16LittleEndian(
            data.AsSpan(offset, sizeof(ushort)),
            value);
    }

    private static void WriteInt16(
        byte[] data,
        int offset,
        short value)
    {
        ArgumentNullException.ThrowIfNull(data);

        BinaryPrimitives.WriteInt16LittleEndian(
            data.AsSpan(offset, sizeof(short)),
            value);
    }

    private static int LastIndexOf(
        byte[] data,
        byte[] value)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length == 0
            || value.Length > data.Length)
        {
            return -1;
        }

        for (int i = data.Length - value.Length; i >= 0; i--)
        {
            if (data.AsSpan(i, value.Length).SequenceEqual(value))
            {
                return i;
            }
        }

        return -1;
    }

    private static void ResetDirectory(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private static ProcessResult RunProcess(
        string fileName,
        IReadOnlyList<string> arguments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(arguments);

        ProcessStartInfo startInfo =
            new()
            {
                FileName = fileName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using Process process = new() { StartInfo = startInfo };
        if (!process.Start())
        {
            throw new InvalidOperationException(
                $"Unable to start {fileName}.");
        }

        Task<string> standardOutput =
            process.StandardOutput.ReadToEndAsync();
        Task<string> standardError =
            process.StandardError.ReadToEndAsync();

        process.WaitForExit();
        Task.WaitAll(standardOutput, standardError);

        ProcessResult result =
            new(
                process.ExitCode,
                standardOutput.Result,
                standardError.Result);

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"{fileName} exited with code {result.ExitCode}: "
                + result.PreferredOutput.Trim());
        }

        return result;
    }

    private readonly record struct ProcessResult(
        int ExitCode,
        string StandardOutput,
        string StandardError)
    {
        internal string PreferredOutput =>
            !string.IsNullOrWhiteSpace(StandardOutput)
                ? StandardOutput
                : StandardError;
    }
}
