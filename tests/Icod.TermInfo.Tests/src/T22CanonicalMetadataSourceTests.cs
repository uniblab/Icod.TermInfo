using System.Globalization;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T22CanonicalMetadataSourceTests
{
    private const string CanonicalHeader =
        "Kind\tBinaryIndex\tShortName\tLongName\tTermcapCode\tManagedName";

    [Fact]
    public void CanonicalTsvMatchesRuntimeMetadata()
    {
        string sourcePath = Path.Combine(
            AppContext.BaseDirectory,
            "metadata",
            "standard-capabilities.tsv");

        Assert.True(
            File.Exists(sourcePath),
            $"Canonical metadata test asset not found at '{sourcePath}'.");

        string[] dataLines =
            File.ReadAllLines(sourcePath)
                .Where(line =>
                    line.Length > 0
                    && line[0] != '#')
                .ToArray();

        Assert.NotEmpty(dataLines);
        Assert.Equal(
            CanonicalHeader,
            dataLines[0]);

        CanonicalRow[] expected =
            dataLines
                .Skip(1)
                .Select(ParseRow)
                .ToArray();

        CanonicalRow[] actual =
        [
            .. StandardCapabilityCatalog.BooleanCapabilities
                .Select(item =>
                    ToRow("B", item)),
            .. StandardCapabilityCatalog.NumericCapabilities
                .Select(item =>
                    ToRow("N", item)),
            .. StandardCapabilityCatalog.StringCapabilities
                .Select(item =>
                    ToRow("S", item)),
        ];

        Assert.Equal(expected, actual);
    }

    private static CanonicalRow ParseRow(
        string line)
    {
        ArgumentNullException.ThrowIfNull(line);

        string[] fields =
            line.Split(
                '\t',
                StringSplitOptions.None);

        if (fields.Length != 6)
        {
            throw new InvalidDataException(
                $"Canonical metadata row has {fields.Length} "
                + "fields; expected 6.");
        }

        if (!int.TryParse(
                fields[1],
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int binaryIndex))
        {
            throw new InvalidDataException(
                $"Canonical metadata binary index '{fields[1]}' "
                + "is invalid.");
        }

        return new CanonicalRow(
            fields[0],
            binaryIndex,
            fields[2],
            fields[3],
            fields[4],
            fields[5]);
    }

    private static CanonicalRow ToRow<TCapability>(
        string kind,
        StandardCapabilityMetadata<TCapability> metadata)
        where TCapability : struct, Enum
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        ArgumentNullException.ThrowIfNull(metadata);

        string managedName =
            metadata.Capability.ToString()
            ?? throw new InvalidOperationException(
                "The capability enum member has no managed name.");

        return new CanonicalRow(
            kind,
            metadata.BinaryIndex,
            metadata.ShortName,
            metadata.LongName,
            metadata.TermcapCode,
            managedName);
    }

    private sealed record CanonicalRow(
        string Kind,
        int BinaryIndex,
        string ShortName,
        string LongName,
        string TermcapCode,
        string ManagedName);
}
