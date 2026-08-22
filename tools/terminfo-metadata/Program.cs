using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Icod.TermInfo.MetadataGenerator;

internal static class Program
{
    private const string CanonicalHeader =
        "Kind\tBinaryIndex\tShortName\tLongName\tTermcapCode\tManagedName";

    private static readonly IReadOnlyDictionary<string, int> ExpectedCounts =
        new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["B"] = 44,
            ["N"] = 39,
            ["S"] = 414,
        };

    private static readonly IReadOnlyDictionary<string, KindInfo> KindInformation =
        new Dictionary<string, KindInfo>(StringComparer.Ordinal)
        {
            ["B"] = new("Boolean", "BooleanCapability", "Boolean"),
            ["N"] = new("Numeric", "NumericCapability", "Number"),
            ["S"] = new("String", "StringCapability", "String"),
        };

    private static readonly IReadOnlyDictionary<string, string> EnumFiles =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["B"] = "BooleanCapability.cs",
            ["N"] = "NumericCapability.cs",
            ["S"] = "StringCapability.cs",
        };

    private static readonly Regex EnumMemberPattern = new(
        @"^\s{4}([A-Za-z_][A-Za-z0-9_]*)\s*(?:=\s*[^,]+)?,$",
        RegexOptions.Multiline | RegexOptions.CultureInvariant);

    public static int Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (!TryParseArguments(args, out bool check))
        {
            Console.Error.WriteLine(
                "Usage: dotnet run --project tools/terminfo-metadata/"
                + "Icod.TermInfo.MetadataGenerator.csproj -- [--check]");
            return 2;
        }

        try
        {
            string root = FindRepositoryRoot();
            string sourcePath = Path.Combine(
                root,
                "tools",
                "terminfo-metadata",
                "standard-capabilities.tsv");
            string outputPath = Path.Combine(
                root,
                "src",
                "Capabilities",
                "StandardCapabilityDefinitions.Generated.cs");

            IReadOnlyList<Capability> capabilities =
                ParseSource(sourcePath);
            Validate(capabilities, root);

            string generated = Generate(capabilities);
            if (check)
            {
                return CheckGeneratedFile(
                    root,
                    outputPath,
                    generated);
            }

            File.WriteAllText(
                outputPath,
                generated,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            return 0;
        }
        catch (Exception exception) when (
            exception is IOException
            or UnauthorizedAccessException
            or InvalidDataException
            or InvalidOperationException)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    private static bool TryParseArguments(
        IReadOnlyList<string> args,
        out bool check)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Count == 0)
        {
            check = false;
            return true;
        }

        if (args.Count == 1
            && string.Equals(
                args[0],
                "--check",
                StringComparison.Ordinal))
        {
            check = true;
            return true;
        }

        check = false;
        return false;
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
            DirectoryInfo? current =
                new DirectoryInfo(start);

            while (current is not null)
            {
                if (File.Exists(
                        Path.Combine(
                            current.FullName,
                            "Icod.TermInfo.csproj"))
                    && Directory.Exists(
                        Path.Combine(
                            current.FullName,
                            "tools",
                            "terminfo-metadata")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }
        }

        throw new InvalidOperationException(
            "Unable to locate the Icod.TermInfo repository root.");
    }

    private static IReadOnlyList<Capability> ParseSource(
        string sourcePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        string[] dataLines =
            File.ReadAllLines(sourcePath)
                .Where(line =>
                    line.Length > 0
                    && line[0] != '#')
                .ToArray();

        if (dataLines.Length == 0)
        {
            throw new InvalidDataException(
                "The canonical capability table is empty.");
        }

        if (!string.Equals(
                dataLines[0],
                CanonicalHeader,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                "Unexpected canonical capability table header.");
        }

        List<Capability> capabilities = [];

        for (int i = 1; i < dataLines.Length; i++)
        {
            string[] fields =
                dataLines[i].Split(
                    '\t',
                    StringSplitOptions.None);

            if (fields.Length != 6)
            {
                throw new InvalidDataException(
                    $"Canonical data row {i + 1} has "
                    + $"{fields.Length} fields; expected 6.");
            }

            string kind = fields[0];
            if (!KindInformation.ContainsKey(kind))
            {
                throw new InvalidDataException(
                    $"Unknown capability kind '{kind}' "
                    + $"on canonical data row {i + 1}.");
            }

            if (!int.TryParse(
                    fields[1],
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out int binaryIndex))
            {
                throw new InvalidDataException(
                    $"Invalid binary index '{fields[1]}' "
                    + $"on canonical data row {i + 1}.");
            }

            capabilities.Add(
                new Capability(
                    kind,
                    binaryIndex,
                    fields[2],
                    fields[3],
                    fields[4],
                    fields[5]));
        }

        return capabilities;
    }

    private static void Validate(
        IReadOnlyList<Capability> capabilities,
        string root)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        foreach (
            KeyValuePair<string, int> expected
            in ExpectedCounts)
        {
            Capability[] items =
                capabilities
                    .Where(item =>
                        string.Equals(
                            item.Kind,
                            expected.Key,
                            StringComparison.Ordinal))
                    .ToArray();

            if (items.Length != expected.Value)
            {
                throw new InvalidDataException(
                    $"Capability kind {expected.Key} has "
                    + $"{items.Length} rows; expected "
                    + $"{expected.Value}.");
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].BinaryIndex != i)
                {
                    throw new InvalidDataException(
                        $"Capability kind {expected.Key} "
                        + "does not have contiguous binary indices.");
                }
            }

            AssertUnique(
                items.Select(item => item.ShortName),
                expected.Key,
                "short names");
            AssertUnique(
                items.Select(item => item.LongName),
                expected.Key,
                "long names");
            AssertUnique(
                items.Select(item => item.ManagedName),
                expected.Key,
                "managed names");

            ValidateEnumNames(
                root,
                expected.Key,
                items);
        }
    }

    private static void AssertUnique(
        IEnumerable<string> values,
        string kind,
        string label)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);

        string[] materialized = values.ToArray();
        int uniqueCount =
            materialized
                .Distinct(StringComparer.Ordinal)
                .Count();

        if (uniqueCount != materialized.Length)
        {
            throw new InvalidDataException(
                $"Capability kind {kind} contains "
                + $"duplicate {label}.");
        }
    }

    private static void ValidateEnumNames(
        string root,
        string kind,
        IReadOnlyList<Capability> items)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentNullException.ThrowIfNull(items);

        string enumPath = Path.Combine(
            root,
            "src",
            "Capabilities",
            EnumFiles[kind]);
        string source = File.ReadAllText(enumPath);

        HashSet<string> actualNames =
            EnumMemberPattern
                .Matches(source)
                .Cast<Match>()
                .Select(match =>
                    match.Groups[1].Value)
                .ToHashSet(StringComparer.Ordinal);
        HashSet<string> expectedNames =
            items
                .Select(item =>
                    item.ManagedName)
                .ToHashSet(StringComparer.Ordinal);

        string[] missing =
            expectedNames
                .Except(
                    actualNames,
                    StringComparer.Ordinal)
                .OrderBy(
                    value => value,
                    StringComparer.Ordinal)
                .ToArray();
        string[] extra =
            actualNames
                .Except(
                    expectedNames,
                    StringComparer.Ordinal)
                .OrderBy(
                    value => value,
                    StringComparer.Ordinal)
                .ToArray();

        if (missing.Length == 0
            && extra.Length == 0)
        {
            return;
        }

        List<string> details = [];

        if (missing.Length > 0)
        {
            details.Add(
                "missing enum members: "
                + string.Join(", ", missing));
        }

        if (extra.Length > 0)
        {
            details.Add(
                "unmapped enum members: "
                + string.Join(", ", extra));
        }

        throw new InvalidDataException(
            string.Join("; ", details));
    }

    private static string Generate(
        IReadOnlyList<Capability> capabilities)
    {
        ArgumentNullException.ThrowIfNull(capabilities);

        StringBuilder builder = new();

        AppendLine(builder, "// <auto-generated>");
        AppendLine(
            builder,
            "// Generated from tools/terminfo-metadata/"
            + "standard-capabilities.tsv.");
        AppendLine(
            builder,
            "// Do not edit by hand; regenerate with the .NET tool "
            + "in tools/terminfo-metadata/.");
        AppendLine(builder, "// </auto-generated>");
        AppendLine(builder);
        AppendLine(builder, "namespace Icod.TermInfo;");
        AppendLine(builder);
        AppendLine(
            builder,
            "internal static class StandardCapabilityDefinitions");
        AppendLine(builder, "{");

        string[] kinds = ["B", "N", "S"];

        foreach (string kind in kinds)
        {
            KindInfo info = KindInformation[kind];
            Capability[] items =
                capabilities
                    .Where(item =>
                        string.Equals(
                            item.Kind,
                            kind,
                            StringComparison.Ordinal))
                    .ToArray();

            AppendLine(
                builder,
                "    internal static readonly "
                + $"StandardCapabilityMetadata<{info.EnumType}>[] "
                + $"{info.Label} =");
            AppendLine(builder, "    [");

            foreach (Capability item in items)
            {
                AppendLine(
                    builder,
                    "        new("
                    + $"{info.EnumType}.{item.ManagedName}, "
                    + $"{item.BinaryIndex}, "
                    + $"\"{Escape(item.ShortName)}\", "
                    + $"\"{Escape(item.LongName)}\", "
                    + $"\"{Escape(item.TermcapCode)}\", "
                    + "TermInfoCapabilityValueKind."
                    + $"{info.ValueKind}),");
            }

            AppendLine(builder, "    ];");
            AppendLine(builder);
        }

        AppendLine(builder, "}");
        return builder.ToString();
    }

    private static int CheckGeneratedFile(
        string root,
        string outputPath,
        string generated)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(generated);

        if (!File.Exists(outputPath))
        {
            Console.Error.WriteLine(
                Path.GetRelativePath(
                    root,
                    outputPath)
                + " does not exist; regenerate it.");
            return 1;
        }

        string current =
            NormalizeLineEndings(
                File.ReadAllText(outputPath));

        if (!string.Equals(
                current,
                generated,
                StringComparison.Ordinal))
        {
            Console.Error.WriteLine(
                Path.GetRelativePath(
                    root,
                    outputPath)
                + " is not current; regenerate it.");
            return 1;
        }

        return 0;
    }

    private static string NormalizeLineEndings(
        string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value
            .Replace(
                "\r\n",
                "\n",
                StringComparison.Ordinal)
            .Replace(
                '\r',
                '\n');
    }

    private static string Escape(
        string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value
            .Replace(
                "\\",
                "\\\\",
                StringComparison.Ordinal)
            .Replace(
                "\"",
                "\\\"",
                StringComparison.Ordinal);
    }

    private static void AppendLine(
        StringBuilder builder,
        string value = "")
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(value);

        builder.Append(value);
        builder.Append('\n');
    }

    private sealed record Capability(
        string Kind,
        int BinaryIndex,
        string ShortName,
        string LongName,
        string TermcapCode,
        string ManagedName);

    private sealed record KindInfo(
        string Label,
        string EnumType,
        string ValueKind);
}
