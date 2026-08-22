using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T24ExpansionReuseTests
{
    [Fact]
    public void StandardExpansionReusesOneParsedProgramPerCapability()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("cache-standard")
                .SetString(
                    StringCapability.CursorAddress,
                    "\u001b[%i%p1%d;%p2%dH")
                .Build();

        Assert.Equal(0, terminal.CachedStandardParameterProgramCount);

        Assert.Equal(
            "\u001b[4;8H",
            terminal.Expand(
                StringCapability.CursorAddress,
                3,
                7));
        Assert.Equal(1, terminal.CachedStandardParameterProgramCount);

        TermInfoParameterProgram first =
            terminal.GetParameterProgram(
                StringCapability.CursorAddress);

        Assert.Equal(
            "\u001b[10;12H",
            terminal.Expand(
                StringCapability.CursorAddress,
                9,
                11));

        TermInfoParameterProgram second =
            terminal.GetParameterProgram(
                StringCapability.CursorAddress);

        Assert.Same(first, second);
        Assert.Equal(1, terminal.CachedStandardParameterProgramCount);
    }

    [Fact]
    public void ExtendedStringExpansionUsesTheSameParameterRuntime()
    {
        const string Source =
            "%?%p1%{8}%<%tindexed:%p1%d%e"
            + "rgb:%p1%{65536}%/%d:%p1%{256}%/%{255}%&%d:"
            + "%p1%{255}%&%d%;";

        TerminalDescription terminal =
            new TerminalDescriptionBuilder("cache-extended")
                .SetExtendedString("X_COLOR", Source)
                .Build();

        Assert.Equal(0, terminal.CachedExtendedParameterProgramCount);

        Assert.Equal(
            "indexed:5",
            terminal.ExpandExtendedString("X_COLOR", 5));
        Assert.Equal(
            "rgb:1:2:3",
            terminal.ExpandExtendedString("X_COLOR", 0x010203));

        Assert.Equal(1, terminal.CachedExtendedParameterProgramCount);
        Assert.Same(
            terminal.GetExtendedParameterProgram("X_COLOR"),
            terminal.GetExtendedParameterProgram("X_COLOR"));
    }

    [Fact]
    public void ExtendedStringExpansionSupportsCallerOwnedPersistentContext()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("cache-context")
                .SetExtendedString("X_SAVE", "%p1%PA")
                .SetExtendedString("X_READ", "%gA%s")
                .Build();
        TermInfoExpansionContext context = new();

        Assert.Equal(
            string.Empty,
            terminal.ExpandExtendedString(
                "X_SAVE",
                context,
                "stored"));
        Assert.Equal(
            "stored",
            terminal.ExpandExtendedString(
                "X_READ",
                context));

        Assert.Throws<TermInfoEvaluationException>(
            () => terminal.ExpandExtendedString(
                "X_READ"));
    }

    [Fact]
    public void ExtendedExpansionRejectsMissingAndNonStringCapabilities()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("extended-errors")
                .SetExtendedBoolean("X_BOOL")
                .SetExtendedNumber("X_NUM", 17)
                .Build();

        Assert.Throws<InvalidOperationException>(
            () => terminal.ExpandExtendedString("X_MISSING"));
        Assert.Throws<InvalidOperationException>(
            () => terminal.ExpandExtendedString("X_BOOL"));
        Assert.Throws<InvalidOperationException>(
            () => terminal.ExpandExtendedString("X_NUM"));

        Assert.Equal(0, terminal.CachedExtendedParameterProgramCount);
    }

    [Fact]
    public void StandardAndExtendedCachesRemainSeparateNamespaces()
    {
        const string Source = "%p1%{1}%+%d";

        TerminalDescription terminal =
            new TerminalDescriptionBuilder("cache-namespaces")
                .SetString(
                    StringCapability.RepeatCharacter,
                    Source)
                .SetExtendedString("X_REPEAT", Source)
                .Build();

        Assert.Equal(
            "8",
            terminal.Expand(
                StringCapability.RepeatCharacter,
                7));
        Assert.Equal(
            "8",
            terminal.ExpandExtendedString(
                "X_REPEAT",
                7));

        Assert.NotSame(
            terminal.GetParameterProgram(
                StringCapability.RepeatCharacter),
            terminal.GetExtendedParameterProgram(
                "X_REPEAT"));
        string standardName =
            StandardCapabilityCatalog
                .GetMetadata(
                    StringCapability.RepeatCharacter)
                .ShortName;

        Assert.Throws<InvalidOperationException>(
            () => terminal.ExpandExtendedString(
                standardName,
                7));
        Assert.Throws<InvalidOperationException>(
            () => terminal.ExpandExtendedString(
                "x_repeat",
                7));

        Assert.Equal(1, terminal.CachedStandardParameterProgramCount);
        Assert.Equal(1, terminal.CachedExtendedParameterProgramCount);
    }

    [Fact]
    public void CacheCannotGrowFromMissingCapabilities()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("cache-bounds")
                .SetString(
                    StringCapability.CursorAddress,
                    "%p1%d")
                .SetString(
                    StringCapability.SetForegroundColor,
                    "%p1%d")
                .SetExtendedString("X_ONE", "%p1%d")
                .SetExtendedString("X_TWO", "%p1%d")
                .SetExtendedBoolean("X_FLAG")
                .Build();

        terminal.Expand(
            StringCapability.CursorAddress,
            1);
        terminal.Expand(
            StringCapability.SetForegroundColor,
            2);
        terminal.ExpandExtendedString(
            "X_ONE",
            3);
        terminal.ExpandExtendedString(
            "X_TWO",
            4);

        Assert.Equal(2, terminal.CachedStandardParameterProgramCount);
        Assert.Equal(2, terminal.CachedExtendedParameterProgramCount);

        Assert.Throws<InvalidOperationException>(
            () => terminal.Expand(
                StringCapability.ClearScreen));

        for (int i = 0; i < 32; i++)
        {
            string name = $"X_MISSING_{i}";

            Assert.Throws<InvalidOperationException>(
                () => terminal.ExpandExtendedString(name));
        }

        Assert.Throws<InvalidOperationException>(
            () => terminal.ExpandExtendedString("X_FLAG"));

        Assert.Equal(2, terminal.CachedStandardParameterProgramCount);
        Assert.Equal(2, terminal.CachedExtendedParameterProgramCount);
    }

    [Fact]
    public void SeparateDescriptionsNeverShareParsedPrograms()
    {
        const string Source = "%p1%d";

        TerminalDescription first =
            new TerminalDescriptionBuilder("cache-first")
                .SetString(StringCapability.CursorAddress, Source)
                .SetExtendedString("X_VALUE", Source)
                .Build();
        TerminalDescription second =
            new TerminalDescriptionBuilder("cache-second")
                .SetString(StringCapability.CursorAddress, Source)
                .SetExtendedString("X_VALUE", Source)
                .Build();

        Assert.NotSame(
            first.GetParameterProgram(
                StringCapability.CursorAddress),
            second.GetParameterProgram(
                StringCapability.CursorAddress));
        Assert.NotSame(
            first.GetExtendedParameterProgram("X_VALUE"),
            second.GetExtendedParameterProgram("X_VALUE"));

        Assert.Equal(1, first.CachedStandardParameterProgramCount);
        Assert.Equal(1, first.CachedExtendedParameterProgramCount);
        Assert.Equal(1, second.CachedStandardParameterProgramCount);
        Assert.Equal(1, second.CachedExtendedParameterProgramCount);
    }

    [Fact]
    public async Task ConcurrentFirstUseInitializesOneProgramPerCapability()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("cache-concurrent")
                .SetString(
                    StringCapability.CursorAddress,
                    "%p1%d")
                .SetExtendedString(
                    "X_VALUE",
                    "%p1%d")
                .Build();

        Task<TermInfoParameterProgram>[] standardTasks =
            Enumerable.Range(0, 64)
                .Select(
                    _ => Task.Run(
                        () => terminal.GetParameterProgram(
                            StringCapability.CursorAddress)))
                .ToArray();
        Task<TermInfoParameterProgram>[] extendedTasks =
            Enumerable.Range(0, 64)
                .Select(
                    _ => Task.Run(
                        () => terminal.GetExtendedParameterProgram(
                            "X_VALUE")))
                .ToArray();

        TermInfoParameterProgram[] standardPrograms =
            await Task.WhenAll(standardTasks);
        TermInfoParameterProgram[] extendedPrograms =
            await Task.WhenAll(extendedTasks);

        Assert.All(
            standardPrograms,
            program => Assert.Same(
                standardPrograms[0],
                program));
        Assert.All(
            extendedPrograms,
            program => Assert.Same(
                extendedPrograms[0],
                program));

        Assert.Equal(1, terminal.CachedStandardParameterProgramCount);
        Assert.Equal(1, terminal.CachedExtendedParameterProgramCount);
        Assert.Equal(
            "23",
            terminal.Expand(
                StringCapability.CursorAddress,
                23));
        Assert.Equal(
            "31",
            terminal.ExpandExtendedString(
                "X_VALUE",
                31));
    }

    [Fact]
    public void StandardAndExtendedExpansionShareTypeCheckingSemantics()
    {
        const string Source = "%p1%l%d";

        TerminalDescription terminal =
            new TerminalDescriptionBuilder("cache-types")
                .SetString(
                    StringCapability.CursorAddress,
                    Source)
                .SetExtendedString(
                    "X_LENGTH",
                    Source)
                .Build();

        Assert.Equal(
            "5",
            terminal.Expand(
                StringCapability.CursorAddress,
                "hello"));
        Assert.Equal(
            "5",
            terminal.ExpandExtendedString(
                "X_LENGTH",
                "hello"));

        Assert.Throws<TermInfoEvaluationException>(
            () => terminal.Expand(
                StringCapability.CursorAddress,
                5));
        Assert.Throws<TermInfoEvaluationException>(
            () => terminal.ExpandExtendedString(
                "X_LENGTH",
                5));
    }

    [Fact]
    public void DirectProgramParsingRemainsIndependentOfDescriptionCaches()
    {
        const string Source = "%p1%{2}%*%d";

        TermInfoParameterProgram first =
            TermInfoParameterProgram.Parse(Source);
        TermInfoParameterProgram second =
            TermInfoParameterProgram.Parse(Source);

        Assert.NotSame(first, second);
        Assert.Equal("12", first.Expand(6));
        Assert.Equal("14", second.Expand(7));
    }
}
