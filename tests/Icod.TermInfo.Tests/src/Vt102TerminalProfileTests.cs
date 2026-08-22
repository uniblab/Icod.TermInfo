using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class Vt102TerminalProfileTests
{
    [Fact]
    public void BuiltInDatabaseLoadsVt102Exactly()
    {
        TerminalDescription terminal = TerminalDatabase.BuiltIn.Load("vt102");

        Assert.Same(TerminalProfiles.Vt102, terminal);
        Assert.Equal("vt102", terminal.Name);
        Assert.Empty(terminal.Aliases);
    }

    [Fact]
    public void Vt102PreservesVt100BaseAndAddsOnlyEditingDelta()
    {
        TerminalDescription vt100 = TerminalProfiles.Vt100;
        TerminalDescription vt102 = TerminalProfiles.Vt102;
        Dictionary<StringCapability, string> editingDelta = new()
        {
            [StringCapability.DeleteCharacter] = "\u001b[P",
            [StringCapability.DeleteLine] = "\u001b[M",
            [StringCapability.InsertLine] = "\u001b[L",
            [StringCapability.ExitInsertMode] = "\u001b[4l",
            [StringCapability.EnterInsertMode] = "\u001b[4h",
        };

        foreach (BooleanCapability capability in Enum.GetValues<BooleanCapability>())
        {
            Assert.Equal(
                vt100.GetBoolean(capability),
                vt102.GetBoolean(capability));
        }

        foreach (NumericCapability capability in Enum.GetValues<NumericCapability>())
        {
            Assert.Equal(
                vt100.GetNumber(capability),
                vt102.GetNumber(capability));
        }

        foreach (StringCapability capability in Enum.GetValues<StringCapability>())
        {
            if (editingDelta.TryGetValue(capability, out string? expected))
            {
                Assert.Equal(expected, vt102.GetString(capability));
            }
            else
            {
                Assert.Equal(
                    vt100.GetString(capability),
                    vt102.GetString(capability));
            }
        }

        Assert.Empty(vt102.ExtendedCapabilities);
    }

    [Fact]
    public void Vt102EditingDeltaMatchesCanonicalTerminfoEntry()
    {
        TerminalDescription terminal = TerminalProfiles.Vt102;

        Assert.Equal(
            "\u001b[P",
            terminal.GetRequiredString(StringCapability.DeleteCharacter));
        Assert.Equal(
            "\u001b[M",
            terminal.GetRequiredString(StringCapability.DeleteLine));
        Assert.Equal(
            "\u001b[L",
            terminal.GetRequiredString(StringCapability.InsertLine));
        Assert.Equal(
            "\u001b[4h",
            terminal.GetRequiredString(StringCapability.EnterInsertMode));
        Assert.Equal(
            "\u001b[4l",
            terminal.GetRequiredString(StringCapability.ExitInsertMode));

        Assert.Null(terminal.GetString(StringCapability.DeleteCharacters));
        Assert.Null(terminal.GetString(StringCapability.InsertCharacter));
        Assert.Null(terminal.GetString(StringCapability.InsertCharacters));
        Assert.Null(terminal.GetString(StringCapability.DeleteLines));
        Assert.Null(terminal.GetString(StringCapability.InsertLines));
        Assert.Null(terminal.GetString(StringCapability.EraseCharacters));
    }

    [Fact]
    public void Vt102RemainsMonochrome()
    {
        TerminalColorSupport support =
            TerminalColors.GetColorSupport(TerminalProfiles.Vt102);

        Assert.Equal(TerminalColorModel.None, support.Model);
        Assert.Equal(TerminalColorTier.Monochrome, support.Tier);
        Assert.Null(support.ColorCount);
        Assert.Null(support.ColorPairCount);
    }

    [Theory]
    [InlineData("vt102-w")]
    [InlineData("vt102-nsgr")]
    public void UnimplementedVt102VariantsDoNotResolve(string name)
    {
        Assert.False(
            TerminalDatabase.BuiltIn.TryLoad(
                name,
                out TerminalDescription? terminal));
        Assert.Null(terminal);
    }
}
