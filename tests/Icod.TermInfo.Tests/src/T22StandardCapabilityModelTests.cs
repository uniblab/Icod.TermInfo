using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T22StandardCapabilityModelTests
{
    [Fact]
    public void StandardCatalogHasFrozenCompiledTableCounts()
    {
        Assert.Equal(44, StandardCapabilityCatalog.BooleanCapabilities.Count);
        Assert.Equal(39, StandardCapabilityCatalog.NumericCapabilities.Count);
        Assert.Equal(414, StandardCapabilityCatalog.StringCapabilities.Count);

        Assert.Equal(
            44,
            Enum.GetValues<BooleanCapability>().Length);
        Assert.Equal(
            39,
            Enum.GetValues<NumericCapability>().Length);
        Assert.Equal(
            414,
            Enum.GetValues<StringCapability>().Length);
    }

    [Fact]
    public void BooleanMetadataIsCompleteAndOrdered()
    {
        AssertMetadata(
            StandardCapabilityCatalog.BooleanCapabilities,
            TermInfoCapabilityValueKind.Boolean);
    }

    [Fact]
    public void NumericMetadataIsCompleteAndOrdered()
    {
        AssertMetadata(
            StandardCapabilityCatalog.NumericCapabilities,
            TermInfoCapabilityValueKind.Number);
    }

    [Fact]
    public void StringMetadataIsCompleteAndOrdered()
    {
        AssertMetadata(
            StandardCapabilityCatalog.StringCapabilities,
            TermInfoCapabilityValueKind.String);
    }

    [Fact]
    public void ManagedEnumValuesAreIndependentFromCompiledIndices()
    {
        StandardCapabilityMetadata<BooleanCapability> autoMargin =
            StandardCapabilityCatalog.GetMetadata(
                BooleanCapability.AutoRightMargin);
        StandardCapabilityMetadata<NumericCapability> lines =
            StandardCapabilityCatalog.GetMetadata(
                NumericCapability.Lines);
        StandardCapabilityMetadata<StringCapability> bell =
            StandardCapabilityCatalog.GetMetadata(
                StringCapability.Bell);

        Assert.Equal(0, (int)BooleanCapability.AutoRightMargin);
        Assert.Equal(1, autoMargin.BinaryIndex);
        Assert.Equal(1, (int)NumericCapability.Lines);
        Assert.Equal(2, lines.BinaryIndex);
        Assert.Equal(0, (int)StringCapability.Bell);
        Assert.Equal(1, bell.BinaryIndex);
    }

    [Fact]
    public void MetadataCanBeLookedUpByShortName()
    {
        Assert.True(
            StandardCapabilityCatalog.TryGetBoolean(
                "bw",
                out StandardCapabilityMetadata<BooleanCapability>? booleanMetadata));
        Assert.Equal(BooleanCapability.AutoLeftMargin, booleanMetadata!.Capability);
        Assert.Equal("auto_left_margin", booleanMetadata!.LongName);
        Assert.Equal("bw", booleanMetadata!.TermcapCode);
        Assert.Equal(0, booleanMetadata!.BinaryIndex);

        Assert.True(
            StandardCapabilityCatalog.TryGetNumeric(
                "pb",
                out StandardCapabilityMetadata<NumericCapability>? numericMetadata));
        Assert.Equal(NumericCapability.PaddingBaudRate, numericMetadata!.Capability);
        Assert.Equal("padding_baud_rate", numericMetadata!.LongName);

        Assert.True(
            StandardCapabilityCatalog.TryGetString(
                "pad",
                out StandardCapabilityMetadata<StringCapability>? stringMetadata));
        Assert.Equal(StringCapability.PadChar, stringMetadata!.Capability);
        Assert.Equal("pad_char", stringMetadata!.LongName);
    }

    [Fact]
    public void NewlyCompletedStandardNamesUseNormalTypedLookup()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("completed-standard-model")
                .SetBoolean(BooleanCapability.AutoLeftMargin)
                .SetNumber(NumericCapability.PaddingBaudRate, 9600)
                .SetString(StringCapability.PadChar, "\0")
                .Build();

        Assert.True(terminal.TryGetBoolean("bw", out bool booleanValue));
        Assert.True(booleanValue);
        Assert.True(terminal.TryGetNumber("pb", out int numericValue));
        Assert.Equal(9600, numericValue);
        Assert.True(terminal.TryGetString("pad", out string? stringValue));
        Assert.Equal("\0", stringValue);

        Assert.Throws<ArgumentException>(
            () => new TerminalDescriptionBuilder("collision")
                .SetExtendedString("pad", "not-an-extension"));
    }

    [Fact]
    public void StandardDescriptionEnumerationUsesBinaryOrder()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("enumeration-test")
                .SetBoolean(BooleanCapability.AutoRightMargin)
                .SetBoolean(BooleanCapability.AutoLeftMargin)
                .SetNumber(NumericCapability.Lines, 24)
                .SetNumber(NumericCapability.Columns, 80)
                .SetString(StringCapability.Bell, "\a")
                .SetString(StringCapability.BackTab, "\x1b[Z")
                .Build();

        Assert.Equal(
            new[]
            {
                BooleanCapability.AutoLeftMargin,
                BooleanCapability.AutoRightMargin,
            },
            terminal.BooleanCapabilities);

        Assert.Equal(
            new[]
            {
                new KeyValuePair<NumericCapability, int>(
                    NumericCapability.Columns,
                    80),
                new KeyValuePair<NumericCapability, int>(
                    NumericCapability.Lines,
                    24),
            },
            terminal.NumericCapabilities);

        Assert.Equal(
            new[]
            {
                new KeyValuePair<StringCapability, string>(
                    StringCapability.BackTab,
                    "\x1b[Z"),
                new KeyValuePair<StringCapability, string>(
                    StringCapability.Bell,
                    "\a"),
            },
            terminal.StringCapabilities);
    }

    [Fact]
    public void DescriptionIsDistinctFromCanonicalNameAndAliases()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("sample")
                .AddAlias("sample-alias")
                .SetDescription("Sample terminal description")
                .Build();

        Assert.Equal("sample", terminal.Name);
        Assert.Equal(new[] { "sample-alias" }, terminal.Aliases);
        Assert.Equal("Sample terminal description", terminal.Description);

        TerminalDescription withoutDescription =
            new TerminalDescriptionBuilder("sample-without-description")
                .Build();
        Assert.Null(withoutDescription.Description);

        Assert.Throws<ArgumentException>(
            () => new TerminalDescriptionBuilder("invalid-description")
                .SetDescription("   "));
    }

    [Fact]
    public void BuiltInProfilesExposeAuthoritativeVerboseDescriptions()
    {
        Assert.Equal("80-column dumb tty", TerminalProfiles.Dumb.Description);
        Assert.Equal(
            "ansi/pc-term compatible with color",
            TerminalProfiles.Ansi.Description);
        Assert.Equal(
            "DEC VT100 (w/advanced video)",
            TerminalProfiles.Vt100.Description);
        Assert.Equal("DEC VT102", TerminalProfiles.Vt102.Description);
        Assert.Equal("DEC VT220", TerminalProfiles.Vt220.Description);
        Assert.Equal(
            "xterm terminal emulator (X Window System)",
            TerminalProfiles.Xterm.Description);
        Assert.Equal(
            "xterm with 16 colors like aixterm",
            TerminalProfiles.Xterm16Color.Description);
        Assert.Equal(
            "xterm with 88 colors",
            TerminalProfiles.Xterm88Color.Description);
        Assert.Equal(
            "xterm with 256 colors",
            TerminalProfiles.Xterm256Color.Description);
        Assert.Equal(
            "xterm with direct-color indexing",
            TerminalProfiles.XtermDirect.Description);
        Assert.Equal(
            "xterm with direct-colors and 16 indexed colors",
            TerminalProfiles.XtermDirect16.Description);
        Assert.Equal(
            "xterm with direct-colors and 256 indexed colors",
            TerminalProfiles.XtermDirect256.Description);
    }

    [Theory]
    [InlineData(32768)]
    [InlineData(65535)]
    [InlineData(0x01000000)]
    [InlineData(int.MaxValue)]
    public void NumericCapabilitiesPreserveSigned32BitValues(int expected)
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("numeric-width-test")
                .SetNumber(NumericCapability.Colors, expected)
                .SetExtendedNumber("X_NUMBER", expected)
                .Build();

        Assert.Equal<int?>(
            expected,
            terminal.GetNumber(NumericCapability.Colors));
        Assert.True(
            terminal.TryGetExtendedNumber(
                "X_NUMBER",
                out int extendedValue));
        Assert.Equal(expected, extendedValue);
    }

    private static void AssertMetadata<TCapability>(
        IReadOnlyList<StandardCapabilityMetadata<TCapability>> metadata,
        TermInfoCapabilityValueKind expectedKind)
        where TCapability : struct, Enum
    {
        Assert.Equal(
            Enumerable.Range(0, metadata.Count),
            metadata.Select(item => item.BinaryIndex));
        Assert.Equal(
            metadata.Count,
            metadata.Select(item => item.ShortName).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(
            metadata.Count,
            metadata.Select(item => item.LongName).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(
            metadata.Count,
            metadata.Select(item => item.Capability).Distinct().Count());
        Assert.All(
            metadata,
            item =>
            {
                Assert.Equal(expectedKind, item.Kind);
                Assert.False(string.IsNullOrWhiteSpace(item.ShortName));
                Assert.False(string.IsNullOrWhiteSpace(item.LongName));
                Assert.False(string.IsNullOrWhiteSpace(item.TermcapCode));
            });
    }
}
