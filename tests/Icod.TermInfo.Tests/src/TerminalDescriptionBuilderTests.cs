using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class TerminalDescriptionBuilderTests
{
    [Fact]
    public void BuildCreatesImmutableSnapshot()
    {
        TerminalDescriptionBuilder builder =
            new TerminalDescriptionBuilder("sample")
                .AddAlias("sample-a")
                .SetBoolean(BooleanCapability.AutoRightMargin)
                .SetNumber(NumericCapability.Columns, 80)
                .SetString(StringCapability.Bell, "\a");

        TerminalDescription first = builder.Build();

        builder
            .AddAlias("sample-b")
            .SetBoolean(BooleanCapability.AutoRightMargin, false)
            .SetNumber(NumericCapability.Columns, 132)
            .SetString(StringCapability.Bell, "bell");

        TerminalDescription second = builder.Build();

        Assert.Equal("sample", first.Name);
        Assert.Equal(new[] { "sample-a" }, first.Aliases.ToArray());
        Assert.True(first.GetBoolean(BooleanCapability.AutoRightMargin));
        Assert.Equal<int?>(80, first.GetNumber(NumericCapability.Columns));
        Assert.Equal("\a", first.GetString(StringCapability.Bell));

        Assert.Equal(new[] { "sample-a", "sample-b" }, second.Aliases.ToArray());
        Assert.False(second.GetBoolean(BooleanCapability.AutoRightMargin));
        Assert.Equal<int?>(132, second.GetNumber(NumericCapability.Columns));
        Assert.Equal("bell", second.GetString(StringCapability.Bell));
    }

    [Fact]
    public void BuilderCanRemoveOptionalCapabilities()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("sample")
                .SetNumber(NumericCapability.Columns, 80)
                .RemoveNumber(NumericCapability.Columns)
                .SetString(StringCapability.ClearScreen, "clear")
                .RemoveString(StringCapability.ClearScreen)
                .Build();

        Assert.Null(terminal.GetNumber(NumericCapability.Columns));
        Assert.Null(terminal.GetString(StringCapability.ClearScreen));
    }

    [Fact]
    public void BuilderRejectsInvalidNamesAndAliases()
    {
        Assert.Throws<ArgumentNullException>(
            () => new TerminalDescriptionBuilder(null!));
        Assert.Throws<ArgumentException>(
            () => new TerminalDescriptionBuilder("   "));

        TerminalDescriptionBuilder builder =
            new TerminalDescriptionBuilder("sample");

        Assert.Throws<ArgumentException>(
            () => builder.AddAlias("sample"));

        builder.AddAlias("sample-a");

        Assert.Throws<ArgumentException>(
            () => builder.AddAlias("sample-a"));
        Assert.Throws<ArgumentException>(
            () => builder.AddAlias(" "));
    }

    [Fact]
    public void BuilderRejectsInvalidCapabilitiesAndNullStrings()
    {
        TerminalDescriptionBuilder builder =
            new TerminalDescriptionBuilder("sample");

        Assert.Throws<ArgumentOutOfRangeException>(
            () => builder.SetBoolean((BooleanCapability)(-1)));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => builder.SetNumber((NumericCapability)(-1), 1));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => builder.SetString((StringCapability)(-1), "value"));
        Assert.Throws<ArgumentNullException>(
            () => builder.SetString(StringCapability.Bell, null!));
    }

    [Fact]
    public void AliasComparisonIsOrdinalAndCaseSensitive()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("sample")
                .AddAlias("SAMPLE")
                .Build();

        Assert.Equal(new[] { "SAMPLE" }, terminal.Aliases);
    }
}
