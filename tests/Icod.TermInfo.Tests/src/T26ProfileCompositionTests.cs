using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T26ProfileCompositionTests
{
    [Fact]
    public void InheritCopiesCapabilitiesWithoutCopyingIdentity()
    {
        TerminalDescription source =
            new TerminalDescriptionBuilder("base-terminal")
                .SetDescription("Base terminal")
                .AddAlias("base-alias")
                .SetBoolean(BooleanCapability.AutoRightMargin)
                .SetNumber(NumericCapability.Columns, 80)
                .SetString(StringCapability.ClearScreen, "base-clear")
                .SetExtendedString("X_BASE", "base-value")
                .Build();

        TerminalDescription derived =
            new TerminalDescriptionBuilder("derived-terminal")
                .SetDescription("Derived terminal")
                .AddAlias("derived-alias")
                .Inherit(source)
                .Build();

        Assert.Equal("derived-terminal", derived.Name);
        Assert.Equal("Derived terminal", derived.Description);
        Assert.Equal(new[] { "derived-alias" }, derived.Aliases);
        Assert.True(derived.GetBoolean(BooleanCapability.AutoRightMargin));
        Assert.Equal<int?>(80, derived.GetNumber(NumericCapability.Columns));
        Assert.Equal("base-clear", derived.GetString(StringCapability.ClearScreen));
        Assert.True(derived.TryGetExtendedString("X_BASE", out string? extended));
        Assert.Equal("base-value", extended);
    }

    [Fact]
    public void ExplicitValuesOverrideInheritedValuesRegardlessOfOrder()
    {
        TerminalDescription source =
            new TerminalDescriptionBuilder("base-terminal")
                .SetBoolean(BooleanCapability.AutoRightMargin)
                .SetNumber(NumericCapability.Columns, 80)
                .SetString(StringCapability.ClearScreen, "base-clear")
                .SetExtendedString("X_VALUE", "base")
                .Build();

        TerminalDescription localFirst =
            new TerminalDescriptionBuilder("local-first")
                .SetBoolean(BooleanCapability.AutoRightMargin)
                .SetNumber(NumericCapability.Columns, 132)
                .SetString(StringCapability.ClearScreen, "local-clear")
                .SetExtendedString("X_VALUE", "local")
                .Inherit(source)
                .Build();

        TerminalDescription inheritedFirst =
            new TerminalDescriptionBuilder("inherited-first")
                .Inherit(source)
                .SetNumber(NumericCapability.Columns, 132)
                .SetString(StringCapability.ClearScreen, "local-clear")
                .SetExtendedString("X_VALUE", "local")
                .Build();

        Assert.True(localFirst.GetBoolean(BooleanCapability.AutoRightMargin));
        Assert.Equal<int?>(132, localFirst.GetNumber(NumericCapability.Columns));
        Assert.Equal("local-clear", localFirst.GetString(StringCapability.ClearScreen));
        Assert.True(localFirst.TryGetExtendedString("X_VALUE", out string? localExtended));
        Assert.Equal("local", localExtended);

        Assert.Equal<int?>(132, inheritedFirst.GetNumber(NumericCapability.Columns));
        Assert.Equal("local-clear", inheritedFirst.GetString(StringCapability.ClearScreen));
        Assert.True(
            inheritedFirst.TryGetExtendedString(
                "X_VALUE",
                out string? inheritedExtended));
        Assert.Equal("local", inheritedExtended);
    }

    [Fact]
    public void CancellationRemovesInheritedValuesAndBlocksLaterInheritance()
    {
        TerminalDescription first =
            CreateCompositionSource(
                "first",
                80,
                "first-clear",
                "first-extended");
        TerminalDescription second =
            CreateCompositionSource(
                "second",
                132,
                "second-clear",
                "second-extended");

        TerminalDescriptionBuilder builder =
            new TerminalDescriptionBuilder("derived")
                .Inherit(first)
                .CancelBoolean(BooleanCapability.AutoRightMargin)
                .CancelNumber(NumericCapability.Columns)
                .CancelString(StringCapability.ClearScreen)
                .CancelExtended("X_VALUE")
                .Inherit(second);

        TerminalDescription derived = builder.Build();

        Assert.False(derived.GetBoolean(BooleanCapability.AutoRightMargin));
        Assert.Null(derived.GetNumber(NumericCapability.Columns));
        Assert.Null(derived.GetString(StringCapability.ClearScreen));
        Assert.False(derived.TryGetExtendedCapability("X_VALUE", out _));

        Assert.True(builder.IsBooleanCanceled(BooleanCapability.AutoRightMargin));
        Assert.True(builder.IsNumberCanceled(NumericCapability.Columns));
        Assert.True(builder.IsStringCanceled(StringCapability.ClearScreen));
        Assert.True(builder.IsExtendedCanceled("X_VALUE"));
    }

    [Fact]
    public void ExplicitSetAfterCancellationReintroducesCapability()
    {
        TerminalDescriptionBuilder builder =
            new TerminalDescriptionBuilder("reintroduced")
                .CancelBoolean(BooleanCapability.AutoRightMargin)
                .CancelNumber(NumericCapability.Columns)
                .CancelString(StringCapability.ClearScreen)
                .CancelExtended("X_VALUE")
                .SetBoolean(BooleanCapability.AutoRightMargin)
                .SetNumber(NumericCapability.Columns, 100)
                .SetString(StringCapability.ClearScreen, "local-clear")
                .SetExtendedString("X_VALUE", "local");

        TerminalDescription terminal = builder.Build();

        Assert.True(terminal.GetBoolean(BooleanCapability.AutoRightMargin));
        Assert.Equal<int?>(100, terminal.GetNumber(NumericCapability.Columns));
        Assert.Equal("local-clear", terminal.GetString(StringCapability.ClearScreen));
        Assert.True(terminal.TryGetExtendedString("X_VALUE", out string? extended));
        Assert.Equal("local", extended);

        Assert.False(builder.IsBooleanCanceled(BooleanCapability.AutoRightMargin));
        Assert.False(builder.IsNumberCanceled(NumericCapability.Columns));
        Assert.False(builder.IsStringCanceled(StringCapability.ClearScreen));
        Assert.False(builder.IsExtendedCanceled("X_VALUE"));
    }

    [Fact]
    public void OrdinaryRemovalReturnsCanceledSlotsToAbsence()
    {
        TerminalDescription source =
            CreateCompositionSource(
                "source",
                80,
                "base-clear",
                "base-extended");

        TerminalDescriptionBuilder builder =
            new TerminalDescriptionBuilder("derived")
                .CancelBoolean(BooleanCapability.AutoRightMargin)
                .CancelNumber(NumericCapability.Columns)
                .CancelString(StringCapability.ClearScreen)
                .CancelExtended("X_VALUE")
                .SetBoolean(BooleanCapability.AutoRightMargin, false)
                .RemoveNumber(NumericCapability.Columns)
                .RemoveString(StringCapability.ClearScreen)
                .RemoveExtended("X_VALUE")
                .Inherit(source);

        TerminalDescription terminal = builder.Build();

        Assert.True(terminal.GetBoolean(BooleanCapability.AutoRightMargin));
        Assert.Equal<int?>(80, terminal.GetNumber(NumericCapability.Columns));
        Assert.Equal("base-clear", terminal.GetString(StringCapability.ClearScreen));
        Assert.True(terminal.TryGetExtendedString("X_VALUE", out string? extended));
        Assert.Equal("base-extended", extended);

        Assert.False(builder.IsBooleanCanceled(BooleanCapability.AutoRightMargin));
        Assert.False(builder.IsNumberCanceled(NumericCapability.Columns));
        Assert.False(builder.IsStringCanceled(StringCapability.ClearScreen));
        Assert.False(builder.IsExtendedCanceled("X_VALUE"));
    }

    [Fact]
    public void ExtendedCancellationIsCaseSensitiveAndCannotTargetStandardName()
    {
        TerminalDescription source =
            new TerminalDescriptionBuilder("source")
                .SetExtendedString("X_VALUE", "upper")
                .Build();
        TerminalDescriptionBuilder builder =
            new TerminalDescriptionBuilder("derived")
                .CancelExtended("x_value")
                .Inherit(source);

        TerminalDescription terminal = builder.Build();

        Assert.True(terminal.TryGetExtendedString("X_VALUE", out string? value));
        Assert.Equal("upper", value);
        Assert.True(builder.IsExtendedCanceled("x_value"));
        Assert.Throws<ArgumentException>(
            () => builder.CancelExtended("cup"));
    }

    [Fact]
    public void ExistingBuiltInsCanBeInheritedWithoutChangingEffectiveCapabilities()
    {
        TerminalDescription[] builtIns =
        [
            TerminalProfiles.Dumb,
            TerminalProfiles.Ansi,
            TerminalProfiles.Vt100,
            TerminalProfiles.Vt102,
            TerminalProfiles.Vt220,
            TerminalProfiles.Xterm,
            TerminalProfiles.Xterm16Color,
            TerminalProfiles.Xterm88Color,
            TerminalProfiles.Xterm256Color,
            TerminalProfiles.XtermDirect,
            TerminalProfiles.XtermDirect16,
            TerminalProfiles.XtermDirect256,
        ];

        foreach (TerminalDescription source in builtIns)
        {
            TerminalDescription clone =
                new TerminalDescriptionBuilder($"clone-{source.Name}")
                    .Inherit(source)
                    .Build();

            Assert.Equal(
                source.BooleanCapabilities.ToArray(),
                clone.BooleanCapabilities.ToArray());
            Assert.Equal(
                source.NumericCapabilities.ToArray(),
                clone.NumericCapabilities.ToArray());
            Assert.Equal(
                source.StringCapabilities.ToArray(),
                clone.StringCapabilities.ToArray());
            Assert.Equal(
                source.ExtendedCapabilities
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .ToArray(),
                clone.ExtendedCapabilities
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .ToArray());
        }
    }

    private static TerminalDescription CreateCompositionSource(
        string name,
        int columns,
        string clearScreen,
        string extendedValue)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(clearScreen);
        ArgumentNullException.ThrowIfNull(extendedValue);

        return new TerminalDescriptionBuilder(name)
            .SetBoolean(BooleanCapability.AutoRightMargin)
            .SetNumber(NumericCapability.Columns, columns)
            .SetString(StringCapability.ClearScreen, clearScreen)
            .SetExtendedString("X_VALUE", extendedValue)
            .Build();
    }
}
