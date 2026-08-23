using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Icod.TermInfo.PackageVerifier;

internal static class Program
{
    private const string PackageId = "Icod.TermInfo";
    private const string RepositoryUrl =
        "https://github.com/uniblab/Icod.TermInfo";
    private const string AssemblyPath =
        "lib/net10.0/Icod.TermInfo.dll";
    private const string DocumentationPath =
        "lib/net10.0/Icod.TermInfo.xml";
    private const string PdbPath =
        "lib/net10.0/Icod.TermInfo.pdb";

    public static int Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length > 1)
        {
            Console.Error.WriteLine(
                "Usage: dotnet run --project tools/package-verifier/"
                + "Icod.TermInfo.PackageVerifier.csproj -- [artifact-directory]");
            return 2;
        }

        try
        {
            string root = FindRepositoryRoot();
            string artifactDirectory =
                args.Length == 0
                    ? Path.Combine(root, "artifacts")
                    : Path.GetFullPath(args[0], root);

            Directory.CreateDirectory(artifactDirectory);

            string packageVersion = ReadAndValidatePackageVersion(root);
            string nupkg =
                Path.Combine(
                    artifactDirectory,
                    $"{PackageId}.{packageVersion}.nupkg");
            string snupkg =
                Path.Combine(
                    artifactDirectory,
                    $"{PackageId}.{packageVersion}.snupkg");

            Require(File.Exists(nupkg), $"Package not found: {nupkg}");
            Require(File.Exists(snupkg), $"Symbol package not found: {snupkg}");

            VerifyParameterizationArchitecture(root);
            string commit = VerifyPrimaryPackage(nupkg, packageVersion);
            VerifySymbolPackage(snupkg, commit);

            Console.WriteLine(
                "Verified package structure, dependency closure, symbols, "
                + $"and Source Link for {packageVersion}.");
            return 0;
        }
        catch (Exception exception) when (
            exception is IOException
            or UnauthorizedAccessException
            or InvalidDataException
            or InvalidOperationException
            or XmlException)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
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
                            "src")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }
        }

        throw new InvalidOperationException(
            "Unable to locate the Icod.TermInfo repository root.");
    }

    private static string ReadAndValidatePackageVersion(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        XDocument project =
            XDocument.Load(
                Path.Combine(root, "Icod.TermInfo.csproj"),
                LoadOptions.None);

        string? version =
            project
                .Descendants()
                .FirstOrDefault(
                    element =>
                        element.Name.LocalName == "Version")
                ?.Value
                .Trim();
        string? packageVersion =
            project
                .Descendants()
                .FirstOrDefault(
                    element =>
                        element.Name.LocalName == "PackageVersion")
                ?.Value
                .Trim();

        Require(
            !string.IsNullOrWhiteSpace(version)
                && !string.IsNullOrWhiteSpace(packageVersion)
                && string.Equals(
                    version,
                    packageVersion,
                    StringComparison.Ordinal),
            "Version and PackageVersion must both be present and identical.");

        return packageVersion!;
    }

    private static void VerifyParameterizationArchitecture(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        string parameterizationRoot =
            Path.Combine(root, "src", "Parameterization");
        Require(
            Directory.Exists(parameterizationRoot),
            "The parameterization source directory is missing.");

        foreach (
            string file
            in Directory.EnumerateFiles(
                parameterizationRoot,
                "*",
                SearchOption.AllDirectories))
        {
            string text = File.ReadAllText(file);
            if (text.Contains("TerminalProfiles.", StringComparison.Ordinal)
                || text.Contains("TerminalProfile", StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "The generic parameterization layer contains a "
                    + $"terminal-profile-specific reference in {file}.");
            }
        }
    }

    private static string VerifyPrimaryPackage(
        string packagePath,
        string expectedVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedVersion);

        using ZipArchive package =
            ZipFile.OpenRead(packagePath);

        HashSet<string> names =
            package.Entries
                .Select(entry => entry.FullName)
                .ToHashSet(StringComparer.Ordinal);

        string[] required =
        [
            "README.md",
            AssemblyPath,
            DocumentationPath,
        ];

        string[] missing =
            required
                .Where(name => !names.Contains(name))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
        Require(
            missing.Length == 0,
            "Primary package is missing required entries: "
                + string.Join(", ", missing));

        Require(
            !names.Any(
                name =>
                    name.StartsWith(
                        "runtimes/",
                        StringComparison.Ordinal)),
            "Primary package unexpectedly contains a runtimes/ payload.");
        Require(
            !names.Any(
                name =>
                    ($"/{name.ToLowerInvariant()}/")
                        .Contains("/native/", StringComparison.Ordinal)),
            "Primary package unexpectedly contains a native payload directory.");

        string[] dlls =
            names
                .Where(
                    name =>
                        name.EndsWith(
                            ".dll",
                            StringComparison.OrdinalIgnoreCase))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
        Require(
            dlls.SequenceEqual(
                new[] { AssemblyPath },
                StringComparer.Ordinal),
            "Primary package contains unexpected DLL payloads: "
                + string.Join(", ", dlls));

        Require(
            !names.Any(HasNativeLibraryExtension),
            "Primary package unexpectedly contains a native library payload.");
        Require(
            !names.Any(IsRepositoryOnlyEntry),
            "Primary package unexpectedly contains repository-only "
                + "fixture/tooling data.");

        ZipArchiveEntry[] nuspecs =
            package.Entries
                .Where(
                    entry =>
                        entry.FullName.EndsWith(
                            ".nuspec",
                            StringComparison.OrdinalIgnoreCase))
                .ToArray();
        Require(
            nuspecs.Length == 1,
            $"Expected one nuspec, found {nuspecs.Length}.");

        using Stream nuspecStream = nuspecs[0].Open();
        XDocument nuspec =
            XDocument.Load(nuspecStream, LoadOptions.None);
        XElement? metadata =
            nuspec
                .Descendants()
                .FirstOrDefault(
                    element =>
                        element.Name.LocalName == "metadata");
        Require(metadata is not null, "Package nuspec has no metadata element.");

        Require(
            GetMetadataText(metadata!, "id") == PackageId,
            "Unexpected package id.");
        Require(
            GetMetadataText(metadata!, "version") == expectedVersion,
            "Unexpected package version.");
        Require(
            !metadata!
                .Descendants()
                .Any(
                    element =>
                        element.Name.LocalName == "dependency"),
            "Icod.TermInfo must not have runtime NuGet dependencies.");

        XElement? repository =
            metadata!
                .Descendants()
                .FirstOrDefault(
                    element =>
                        element.Name.LocalName == "repository");
        Require(repository is not null, "Package metadata has no repository element.");
        Require(
            repository!.Attribute("type")?.Value == "git",
            "Repository metadata is not git.");
        Require(
            repository.Attribute("url")?.Value == RepositoryUrl,
            "Unexpected repository URL in package metadata.");

        string commit =
            repository.Attribute("commit")?.Value ?? string.Empty;
        Require(
            Regex.IsMatch(
                commit,
                "^[0-9a-fA-F]{40}$",
                RegexOptions.CultureInvariant),
            $"Repository metadata has an invalid commit id: '{commit}'.");

        return commit;
    }

    private static void VerifySymbolPackage(
        string packagePath,
        string commit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(commit);

        using ZipArchive symbols =
            ZipFile.OpenRead(packagePath);
        ZipArchiveEntry? pdbEntry =
            symbols.GetEntry(PdbPath);
        Require(
            pdbEntry is not null,
            "Symbol package is missing the portable PDB.");

        using Stream stream = pdbEntry!.Open();
        using MemoryStream buffer = new();
        stream.CopyTo(buffer);
        byte[] pdb = buffer.ToArray();

        Require(
            pdb.AsSpan().StartsWith("BSJB"u8),
            "Icod.TermInfo.pdb is not a portable PDB.");
        Require(
            ContainsAscii(
                pdb,
                "raw.githubusercontent.com/uniblab/Icod.TermInfo/"),
            "Portable PDB does not contain the expected GitHub Source Link mapping.");
        Require(
            ContainsAscii(pdb, commit),
            "Portable PDB Source Link data does not contain the package "
                + "repository commit.");
    }

    private static string? GetMetadataText(
        XElement metadata,
        string name)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return metadata
            .Elements()
            .FirstOrDefault(
                element =>
                    element.Name.LocalName == name)
            ?.Value;
    }

    private static bool HasNativeLibraryExtension(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return name.EndsWith(".so", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith(".dylib", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith(".a", StringComparison.OrdinalIgnoreCase)
            || name.EndsWith(".lib", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRepositoryOnlyEntry(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        string lower = name.ToLowerInvariant();
        return name.StartsWith("tests/", StringComparison.Ordinal)
            || name.StartsWith("tools/", StringComparison.Ordinal)
            || name.StartsWith("fixtures/", StringComparison.Ordinal)
            || lower.Contains("compiled-terminfo", StringComparison.Ordinal)
            || lower.EndsWith(".ti", StringComparison.Ordinal)
            || lower.EndsWith(".bin", StringComparison.Ordinal);
    }

    private static bool ContainsAscii(
        byte[] data,
        string text)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(text);

        byte[] expected = Encoding.ASCII.GetBytes(text);
        return data.AsSpan().IndexOf(expected) >= 0;
    }

    private static void Require(
        bool condition,
        string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!condition)
        {
            throw new InvalidDataException(message);
        }
    }
}
