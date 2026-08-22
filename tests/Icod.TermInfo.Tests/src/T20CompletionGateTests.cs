using System.Reflection;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T20CompletionGateTests
{
    [Fact]
    public void AssemblyIdentifiesFinal07Release()
    {
        Assembly assembly = typeof(TerminalDescription).Assembly;
        AssemblyInformationalVersionAttribute? attribute =
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        Assert.NotNull(attribute);

        string releaseVersion = attribute!.InformationalVersion;
        int metadataIndex = releaseVersion.IndexOf('+');
        if (metadataIndex >= 0)
        {
            releaseVersion = releaseVersion[..metadataIndex];
        }

        Assert.Equal("0.7.0", releaseVersion);
    }

    [Theory]
    [InlineData("ms-terminal")]
    [InlineData("ms-terminal-direct")]
    [InlineData("windows-terminal")]
    [InlineData("conhost")]
    public void Reserved08TerminalIdentitiesRemainUnsupported(string name)
    {
        Assert.False(
            TerminalDatabase.BuiltIn.TryLoad(
                name,
                out TerminalDescription? terminal));
        Assert.Null(terminal);
    }

    [Fact]
    public void XtermFamilyRetainsCursorAddressingAndVisibilityPrimitives()
    {
        TerminalDescription baseline = TerminalProfiles.Xterm;
        TerminalDescription[] variants =
        [
            TerminalProfiles.Xterm16Color,
            TerminalProfiles.Xterm88Color,
            TerminalProfiles.Xterm256Color,
            TerminalProfiles.XtermDirect,
            TerminalProfiles.XtermDirect16,
            TerminalProfiles.XtermDirect256,
        ];
        StringCapability[] capabilities =
        [
            StringCapability.EnterCursorAddressingMode,
            StringCapability.ExitCursorAddressingMode,
            StringCapability.CursorInvisible,
            StringCapability.CursorNormal,
            StringCapability.CursorVeryVisible,
        ];

        foreach (StringCapability capability in capabilities)
        {
            string expected = baseline.GetRequiredString(capability);
            Assert.NotEmpty(expected);

            foreach (TerminalDescription variant in variants)
            {
                Assert.Equal(
                    expected,
                    variant.GetRequiredString(capability));
            }
        }
    }
}
