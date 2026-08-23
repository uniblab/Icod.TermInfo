using System.Reflection;
using System.Text;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T31CompletionGateTests
{
    [Fact]
    public void AssemblyIdentifiesFinal08Release()
    {
        Assembly assembly = typeof(TerminalDescription).Assembly;
        Version? assemblyVersion = assembly.GetName().Version;
        string? informationalVersion =
            assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

        Assert.NotNull(assemblyVersion);
        Assert.Equal(new Version(0, 8, 0, 0), assemblyVersion);
        Assert.NotNull(informationalVersion);
        Assert.True(
            string.Equals(
                informationalVersion,
                "0.8.0",
                StringComparison.Ordinal)
            || informationalVersion!.StartsWith(
                "0.8.0+",
                StringComparison.Ordinal),
            $"Unexpected informational version '{informationalVersion}'.");
    }

    [Fact]
    public void EveryFrozenBuiltInResolvesToItsExactDescription()
    {
        TerminalDescription[] terminals =
        [
            TerminalProfiles.Ansi,
            TerminalProfiles.Dumb,
            TerminalProfiles.MsTerminal,
            TerminalProfiles.MsTerminalDirect,
            TerminalProfiles.Vt100,
            TerminalProfiles.Vt102,
            TerminalProfiles.Vt220,
            TerminalProfiles.WinConsole,
            TerminalProfiles.Xterm,
            TerminalProfiles.Xterm16Color,
            TerminalProfiles.Xterm88Color,
            TerminalProfiles.Xterm256Color,
            TerminalProfiles.XtermDirect,
            TerminalProfiles.XtermDirect16,
            TerminalProfiles.XtermDirect256,
        ];

        Assert.Equal(
            terminals.Length,
            terminals
                .Select(terminal => terminal.Name)
                .Distinct(StringComparer.Ordinal)
                .Count());

        foreach (TerminalDescription terminal in terminals)
        {
            Assert.Same(
                terminal,
                TerminalDatabase.BuiltIn.Load(terminal.Name));
        }
    }

    [Fact]
    public void FutureCompiledSemanticShapeFitsFrozenPublicModel()
    {
        const string standardProgram = "\u0080%p1%{1}%+%d$<1>";
        const string extendedProgram = "\u00ff%p1%s";

        TerminalDescription terminal =
            new TerminalDescriptionBuilder("t31-future-shape")
                .SetDescription("T31 future compiled semantic shape")
                .AddAlias("t31fs")
                .SetBoolean(BooleanCapability.AutoRightMargin)
                .SetNumber(NumericCapability.Colors, 16_777_216)
                .SetNumber(NumericCapability.ColorPairs, 65_536)
                .SetString(StringCapability.CursorAddress, standardProgram)
                .SetExtendedBoolean("XBool")
                .SetExtendedNumber("XNum", 2_147_483_640)
                .SetExtendedString("XStr", extendedProgram)
                .Build();

        ITerminalDescriptionProvider provider =
            new InMemoryTerminalDescriptionProvider(
                new[] { terminal });
        TerminalDatabase database =
            new(
                new[]
                {
                    provider,
                });

        Assert.Same(terminal, database.Load("t31-future-shape"));
        Assert.Same(terminal, database.Load("t31fs"));
        Assert.Equal(
            "T31 future compiled semantic shape",
            terminal.Description);

        Assert.True(
            terminal.GetBoolean(BooleanCapability.AutoRightMargin));
        Assert.False(
            terminal.GetBoolean(BooleanCapability.GenericType));
        Assert.Equal<int?>(
            16_777_216,
            terminal.GetNumber(NumericCapability.Colors));
        Assert.Equal<int?>(
            65_536,
            terminal.GetNumber(NumericCapability.ColorPairs));
        Assert.Null(terminal.GetNumber(NumericCapability.Lines));
        Assert.Null(terminal.GetString(StringCapability.Bell));

        Assert.Contains(
            BooleanCapability.AutoRightMargin,
            terminal.BooleanCapabilities);
        Assert.Equal(
            16_777_216,
            terminal.NumericCapabilities
                .Single(pair => pair.Key == NumericCapability.Colors)
                .Value);
        Assert.Equal(
            standardProgram,
            terminal.StringCapabilities
                .Single(pair => pair.Key == StringCapability.CursorAddress)
                .Value);
        Assert.Equal(3, terminal.ExtendedCapabilities.Count);

        Assert.True(
            terminal.TryGetNumber("colors", out int colors));
        Assert.Equal(16_777_216, colors);
        Assert.True(
            terminal.TryGetExtendedBoolean("XBool", out bool extendedBoolean));
        Assert.True(extendedBoolean);
        Assert.True(
            terminal.TryGetExtendedNumber("XNum", out int extendedNumber));
        Assert.Equal(2_147_483_640, extendedNumber);
        Assert.True(
            terminal.TryGetExtendedString("XStr", out string? extendedString));
        Assert.Equal(extendedProgram, extendedString);

        TermInfoParameterProgram parsed =
            TermInfoParameterProgram.Parse(standardProgram);
        string parsedExpansion =
            parsed.Expand(new TermInfoParameter(41));
        string standardExpansion =
            terminal.Expand(
                StringCapability.CursorAddress,
                new TermInfoParameter(41));
        string extendedExpansion =
            terminal.ExpandExtendedString(
                "XStr",
                new TermInfoParameter("ok"));

        Assert.Equal("\u008042$<1>", parsedExpansion);
        Assert.Equal(parsedExpansion, standardExpansion);
        Assert.Equal("\u00ffok", extendedExpansion);

        using MemoryStream standardBytes = new();
        TermInfoOutputOptions options =
            new(
                terminal,
                paddingMode: PaddingMode.Ignore);
        TermInfoOutput.TPuts(
            standardExpansion,
            affectedLines: 1,
            standardBytes,
            Encoding.Latin1,
            options);

        Assert.Equal(
            new byte[]
            {
                0x80,
                (byte)'4',
                (byte)'2',
            },
            standardBytes.ToArray());

        using MemoryStream extendedBytes = new();
        TermInfoOutput.TPuts(
            extendedExpansion,
            affectedLines: 1,
            extendedBytes,
            Encoding.Latin1,
            options);

        Assert.Equal(
            new byte[]
            {
                0xff,
                (byte)'o',
                (byte)'k',
            },
            extendedBytes.ToArray());
    }
}
