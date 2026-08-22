using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class ExtendedCapabilityTests
{
    [Fact]
    public void AllValueKindsRoundTripExactly()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("sample")
                .SetExtendedBoolean("AX", true)
                .SetExtendedNumber("RGB", 8)
                .SetExtendedString("XM", "\x1b[?1000%?%p1%{1}%=%th%el%;")
                .Build();

        Assert.True(terminal.TryGetExtendedBoolean("AX", out bool booleanValue));
        Assert.True(booleanValue);

        Assert.True(terminal.TryGetExtendedNumber("RGB", out int numberValue));
        Assert.Equal(8, numberValue);

        Assert.True(terminal.TryGetExtendedString("XM", out string? stringValue));
        Assert.Equal("\x1b[?1000%?%p1%{1}%=%th%el%;", stringValue);

        Assert.True(
            terminal.TryGetExtendedCapability(
                "RGB",
                out TermInfoCapabilityValue genericValue));
        Assert.Equal(TermInfoCapabilityValueKind.Number, genericValue.Kind);
        Assert.Equal(8, genericValue.NumberValue);
    }

    [Fact]
    public void CapabilityValuePreservesTypeIdentity()
    {
        TermInfoCapabilityValue booleanValue = new(true);
        TermInfoCapabilityValue numberValue = new(256);
        TermInfoCapabilityValue stringValue = new("8/8/8");

        Assert.True(booleanValue.IsBoolean);
        Assert.False(booleanValue.IsNumber);
        Assert.False(booleanValue.IsString);
        Assert.True(booleanValue.BooleanValue);

        Assert.True(numberValue.IsNumber);
        Assert.Equal(256, numberValue.NumberValue);

        Assert.True(stringValue.IsString);
        Assert.Equal("8/8/8", stringValue.StringValue);

        Assert.Throws<InvalidOperationException>(
            () => _ = numberValue.BooleanValue);
        Assert.Throws<InvalidOperationException>(
            () => _ = stringValue.NumberValue);
        Assert.Throws<InvalidOperationException>(
            () => _ = booleanValue.StringValue);
    }

    [Fact]
    public void ExtendedNamesAreOrdinalAndCaseSensitive()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("sample")
                .SetExtendedNumber("RGB", 8)
                .SetExtendedString("rgb", "8/8/8")
                .Build();

        Assert.True(terminal.TryGetExtendedNumber("RGB", out int numeric));
        Assert.Equal(8, numeric);
        Assert.True(terminal.TryGetExtendedString("rgb", out string? text));
        Assert.Equal("8/8/8", text);
        Assert.False(terminal.TryGetExtendedCapability("Rgb", out _));
    }

    [Fact]
    public void WrongTypedLookupIsNotReportedAsAbsent()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("sample")
                .SetExtendedNumber("RGB", 8)
                .Build();

        Assert.False(terminal.TryGetExtendedBoolean("missing", out _));
        Assert.False(terminal.TryGetExtendedNumber("missing", out _));
        Assert.False(terminal.TryGetExtendedString("missing", out _));

        Assert.Throws<InvalidOperationException>(
            () => terminal.TryGetExtendedBoolean("RGB", out _));
        Assert.Throws<InvalidOperationException>(
            () => terminal.TryGetExtendedString("RGB", out _));
    }

    [Fact]
    public void StandardCapabilityNamesCannotBeShadowed()
    {
        TerminalDescriptionBuilder builder =
            new TerminalDescriptionBuilder("sample");

        Assert.Throws<ArgumentException>(
            () => builder.SetExtendedBoolean("am"));
        Assert.Throws<ArgumentException>(
            () => builder.SetExtendedNumber("colors", 256));
        Assert.Throws<ArgumentException>(
            () => builder.SetExtendedString("cup", "value"));
        Assert.Throws<ArgumentException>(
            () => builder.SetExtended(
                "bel",
                new TermInfoCapabilityValue("value")));

        builder.SetExtendedBoolean("AM");
        TerminalDescription terminal = builder.Build();

        Assert.True(terminal.TryGetExtendedBoolean("AM", out bool value));
        Assert.True(value);
    }

    [Fact]
    public void BuilderRejectsInvalidExtendedNamesAndNullStrings()
    {
        TerminalDescriptionBuilder builder =
            new TerminalDescriptionBuilder("sample");

        Assert.Throws<ArgumentNullException>(
            () => builder.SetExtendedBoolean(null!));
        Assert.Throws<ArgumentException>(
            () => builder.SetExtendedNumber("   ", 1));
        Assert.Throws<ArgumentNullException>(
            () => builder.SetExtendedString("XS", null!));
        Assert.Throws<ArgumentNullException>(
            () => builder.RemoveExtended(null!));
    }

    [Fact]
    public void BuildCreatesImmutableExtendedCapabilitySnapshot()
    {
        TerminalDescriptionBuilder builder =
            new TerminalDescriptionBuilder("sample")
                .SetExtendedNumber("RGB", 8)
                .SetExtendedBoolean("AX");

        TerminalDescription first = builder.Build();

        builder
            .SetExtendedString("RGB", "8/8/8")
            .SetExtendedBoolean("AX", false)
            .SetExtendedString("XM", "mouse")
            .RemoveExtended("XM")
            .SetExtendedNumber("CO", 8);

        TerminalDescription second = builder.Build();

        Assert.Equal(2, first.ExtendedCapabilities.Count);
        Assert.True(first.TryGetExtendedNumber("RGB", out int firstRgb));
        Assert.Equal(8, firstRgb);
        Assert.True(first.TryGetExtendedBoolean("AX", out bool firstAx));
        Assert.True(firstAx);
        Assert.False(first.TryGetExtendedCapability("CO", out _));

        Assert.Equal(2, second.ExtendedCapabilities.Count);
        Assert.True(second.TryGetExtendedString("RGB", out string? secondRgb));
        Assert.Equal("8/8/8", secondRgb);
        Assert.False(second.TryGetExtendedCapability("AX", out _));
        Assert.True(second.TryGetExtendedNumber("CO", out int secondCo));
        Assert.Equal(8, secondCo);
    }

    [Fact]
    public void ProviderCompositionPreservesExtendedCapabilities()
    {
        TerminalDescription first =
            new TerminalDescriptionBuilder("shared")
                .SetExtendedNumber("RGB", 8)
                .Build();
        TerminalDescription second =
            new TerminalDescriptionBuilder("shared")
                .SetExtendedNumber("RGB", 24)
                .Build();
        TerminalDatabase database =
            new(
                new ITerminalDescriptionProvider[]
                {
                    new InMemoryTerminalDescriptionProvider(new[] { first }),
                    new InMemoryTerminalDescriptionProvider(new[] { second }),
                });

        TerminalDescription loaded = database.Load("shared");

        Assert.Same(first, loaded);
        Assert.True(loaded.TryGetExtendedNumber("RGB", out int value));
        Assert.Equal(8, value);
    }

    [Fact]
    public void ExtendedCapabilitiesSupportConcurrentReads()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("sample")
                .SetExtendedBoolean("AX")
                .SetExtendedNumber("RGB", 8)
                .SetExtendedString("XM", "mouse")
                .Build();

        Parallel.For(
            0,
            256,
            _ =>
            {
                Assert.True(
                    terminal.TryGetExtendedBoolean(
                        "AX",
                        out bool booleanValue));
                Assert.True(booleanValue);

                Assert.True(
                    terminal.TryGetExtendedNumber(
                        "RGB",
                        out int numberValue));
                Assert.Equal(8, numberValue);

                Assert.True(
                    terminal.TryGetExtendedString(
                        "XM",
                        out string? stringValue));
                Assert.Equal("mouse", stringValue);

                Assert.Equal(3, terminal.ExtendedCapabilities.Count);
            });
    }

    [Fact]
    public void ExistingBuiltInProfilesRemainExtensionFreeAndBehaviorallyStable()
    {
        Assert.Empty(TerminalProfiles.Ansi.ExtendedCapabilities);
        Assert.Empty(TerminalProfiles.Vt100.ExtendedCapabilities);
        Assert.Empty(TerminalProfiles.Dumb.ExtendedCapabilities);

        Assert.Equal<int?>(
            8,
            TerminalProfiles.Ansi.GetNumber(NumericCapability.Colors));
        Assert.Equal<int?>(
            64,
            TerminalProfiles.Ansi.GetNumber(NumericCapability.ColorPairs));
        Assert.Null(
            TerminalProfiles.Vt100.GetNumber(NumericCapability.Colors));
        Assert.Null(
            TerminalProfiles.Dumb.GetNumber(NumericCapability.Colors));
    }
}
