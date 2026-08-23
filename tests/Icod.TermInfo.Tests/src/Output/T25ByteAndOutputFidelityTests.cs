using System.Text;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T25ByteAndOutputFidelityTests
{
    [Fact]
    public void CapabilityStringsPreserveOneToOneLatin1CodePoints()
    {
        const string Raw =
            "\u0001\u007f\u0080\u00a5\u00ff";

        TerminalDescription terminal =
            new TerminalDescriptionBuilder("latin1-storage")
                .SetString(
                    StringCapability.ClearScreen,
                    Raw)
                .SetExtendedString(
                    "X_BYTES",
                    Raw)
                .Build();

        Assert.Equal(
            Raw,
            terminal.GetString(
                StringCapability.ClearScreen));
        Assert.True(
            terminal.TryGetExtendedString(
                "X_BYTES",
                out string? extended));
        Assert.Equal(Raw, extended);

        Assert.Equal(
            new byte[]
            {
                0x01,
                0x7f,
                0x80,
                0xa5,
                0xff,
            },
            Encoding.Latin1.GetBytes(Raw));
    }

    [Fact]
    public void ParameterAndPaddingTransformsPreserveHighBytes()
    {
        Assert.Equal(
            "\u0080",
            TermInfoParameterExpander.Expand(
                "%p1%c",
                0x80));

        string expanded =
            TermInfoParameterExpander.Expand(
                "\u0080%p1%s$<1>\u00ff",
                "\u00a5");

        using MemoryStream stream = new();

        TermInfoOutput.TPuts(
            expanded,
            1,
            stream,
            Encoding.Latin1);

        Assert.Equal(
            new byte[]
            {
                0x80,
                0xa5,
                0xff,
            },
            stream.ToArray());
    }

    [Fact]
    public void TerminalAwareDelaySuppressesAdvisoryPaddingForXon()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("xon")
                .SetBoolean(BooleanCapability.XonXoff)
                .Build();
        RecordingDelayProvider delayProvider = new();
        TermInfoOutputOptions options =
            new(
                terminal,
                paddingMode: PaddingMode.Delay,
                delayProvider: delayProvider);
        using StringWriter writer = new();

        TermInfoOutput.TPuts(
            "A$<5>B$<7/>C",
            1,
            writer,
            options);

        Assert.Equal("ABC", writer.ToString());
        TermInfoDelay delay =
            Assert.Single(delayProvider.Delays);
        Assert.Equal(
            TimeSpan.FromMilliseconds(7),
            delay.Duration);
        Assert.True(delay.IsMandatory);
    }

    [Fact]
    public void PaddingBaudRateSuppressesAdvisoryPaddingBelowThreshold()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("pb")
                .SetNumber(
                    NumericCapability.PaddingBaudRate,
                    9600)
                .Build();
        RecordingDelayProvider belowProvider = new();
        RecordingDelayProvider atProvider = new();
        RecordingDelayProvider unknownProvider = new();

        TermInfoOutput.TPuts(
            "$<5>",
            1,
            TextWriter.Null,
            new TermInfoOutputOptions(
                terminal,
                baudRate: 4800,
                delayProvider: belowProvider));
        TermInfoOutput.TPuts(
            "$<5>",
            1,
            TextWriter.Null,
            new TermInfoOutputOptions(
                terminal,
                baudRate: 9600,
                delayProvider: atProvider));
        TermInfoOutput.TPuts(
            "$<5>",
            1,
            TextWriter.Null,
            new TermInfoOutputOptions(
                terminal,
                delayProvider: unknownProvider));

        Assert.Empty(belowProvider.Delays);
        Assert.Single(atProvider.Delays);
        Assert.Single(unknownProvider.Delays);
    }

    [Fact]
    public void MandatoryPaddingBypassesXonAndPaddingBaudThreshold()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("mandatory")
                .SetBoolean(BooleanCapability.XonXoff)
                .SetNumber(
                    NumericCapability.PaddingBaudRate,
                    115200)
                .Build();
        RecordingDelayProvider delayProvider = new();

        TermInfoOutput.TPuts(
            "$<2*/>",
            3,
            TextWriter.Null,
            new TermInfoOutputOptions(
                terminal,
                baudRate: 1200,
                delayProvider: delayProvider));

        TermInfoDelay delay =
            Assert.Single(delayProvider.Delays);
        Assert.Equal(
            TimeSpan.FromMilliseconds(6),
            delay.Duration);
        Assert.True(delay.IsMandatory);
    }

    [Fact]
    public void PadCharacterModeUsesFirstPadCharacterAndNineBitTiming()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("pad-character")
                .SetString(
                    StringCapability.PadChar,
                    "PX")
                .Build();
        TermInfoOutputOptions options =
            new(
                terminal,
                baudRate: 9000,
                paddingMode: PaddingMode.PadCharacters);
        using StringWriter writer = new();

        TermInfoOutput.TPuts(
            "A$<10>B",
            1,
            writer,
            options);

        Assert.Equal(
            "A" + new string('P', 10) + "B",
            writer.ToString());
    }

    [Fact]
    public void PadCharacterModeDefaultsToNulWhenPadIsAbsent()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("default-pad")
                .Build();
        List<char> output = [];

        TermInfoOutput.TPuts(
            "$<4>",
            1,
            output.Add,
            new TermInfoOutputOptions(
                terminal,
                baudRate: 9000,
                paddingMode: PaddingMode.PadCharacters));

        Assert.Equal(4, output.Count);
        Assert.All(
            output,
            character => Assert.Equal('\0', character));
    }

    [Fact]
    public void NoPadCharacterFallsBackToTimedDelay()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("npc")
                .SetBoolean(BooleanCapability.NoPadCharacter)
                .SetString(
                    StringCapability.PadChar,
                    "P")
                .Build();
        RecordingDelayProvider delayProvider = new();
        using StringWriter writer = new();

        TermInfoOutput.TPuts(
            "A$<4>B",
            1,
            writer,
            new TermInfoOutputOptions(
                terminal,
                paddingMode: PaddingMode.PadCharacters,
                delayProvider: delayProvider));

        Assert.Equal("AB", writer.ToString());
        TermInfoDelay delay =
            Assert.Single(delayProvider.Delays);
        Assert.Equal(
            TimeSpan.FromMilliseconds(4),
            delay.Duration);
    }

    [Fact]
    public void PadCharacterModeRequiresBaudWhenCharactersAreNeeded()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("missing-baud")
                .SetString(
                    StringCapability.PadChar,
                    "P")
                .Build();
        using StringWriter writer = new();

        Assert.Throws<InvalidOperationException>(
            () => TermInfoOutput.TPuts(
                "prefix$<5/>suffix",
                1,
                writer,
                new TermInfoOutputOptions(
                    terminal,
                    paddingMode: PaddingMode.PadCharacters)));

        Assert.Equal(
            string.Empty,
            writer.ToString());
    }

    [Fact]
    public void PadCharacterMustFitOneByteTerminfoRange()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("wide-pad")
                .SetString(
                    StringCapability.PadChar,
                    "\u0100")
                .Build();
        using StringWriter writer = new();

        Assert.Throws<InvalidOperationException>(
            () => TermInfoOutput.TPuts(
                "prefix$<5/>suffix",
                1,
                writer,
                new TermInfoOutputOptions(
                    terminal,
                    baudRate: 9600,
                    paddingMode: PaddingMode.PadCharacters)));

        Assert.Equal(
            string.Empty,
            writer.ToString());
    }

    [Fact]
    public void TerminalAwareAffectedLineMultiplicationIsPreserved()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("affected-lines")
                .Build();
        RecordingDelayProvider delayProvider = new();

        TermInfoOutput.TPuts(
            "$<2.5*>",
            4,
            TextWriter.Null,
            new TermInfoOutputOptions(
                terminal,
                delayProvider: delayProvider));

        TermInfoDelay delay =
            Assert.Single(delayProvider.Delays);
        Assert.Equal(
            TimeSpan.FromMilliseconds(10),
            delay.Duration);
    }

    [Fact]
    public void ExtremeAffectedLineMultiplicationCapsWithoutOverflow()
    {
        RecordingDelayProvider delayProvider = new();

        TermInfoOutput.TPuts(
            "$<79228162514264337593543950335*>",
            int.MaxValue,
            TextWriter.Null,
            PaddingMode.Delay,
            delayProvider);

        TermInfoDelay delay =
            Assert.Single(delayProvider.Delays);
        Assert.Equal(
            TimeSpan.FromSeconds(30),
            delay.Duration);
    }

    [Fact]
    public void ExcessivePadCharacterCountFailsBeforeAnyOutput()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("pad-bound")
                .SetString(
                    StringCapability.PadChar,
                    "P")
                .Build();
        using StringWriter writer = new();

        Assert.Throws<InvalidOperationException>(
            () => TermInfoOutput.TPuts(
                "prefix$<30000/>suffix",
                1,
                writer,
                new TermInfoOutputOptions(
                    terminal,
                    baudRate: int.MaxValue,
                    paddingMode: PaddingMode.PadCharacters)));

        Assert.Equal(
            string.Empty,
            writer.ToString());
    }

    [Fact]
    public void PaddingSourceLengthIsBounded()
    {
        string accepted =
            new(
                'x',
                TermInfoPaddingParser.MaximumSourceLength);
        string rejected =
            accepted + "x";

        IReadOnlyList<TermInfoOutputSegment> segments =
            TermInfoPaddingParser.Parse(accepted);

        Assert.Single(segments);
        Assert.Throws<TermInfoPaddingFormatException>(
            () => TermInfoPaddingParser.Parse(rejected));
    }

    [Fact]
    public void SimpleOutputRejectsPadCharacterModeWithoutTerminalFacts()
    {
        Assert.Throws<ArgumentException>(
            () => TermInfoOutput.TPuts(
                "$<1>",
                1,
                TextWriter.Null,
                PaddingMode.PadCharacters));
    }

    [Fact]
    public void OutputOptionsValidateCallerOwnedBaudRate()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("baud-validation")
                .Build();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new TermInfoOutputOptions(
                terminal,
                baudRate: 0));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new TermInfoOutputOptions(
                terminal,
                baudRate: -1));
    }

    [Fact]
    public async Task AsyncLatin1OutputPreservesBytesAndUsesAsyncDelay()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("async-latin1")
                .Build();
        RecordingDelayProvider delayProvider = new();
        using MemoryStream stream = new();

        await TermInfoOutput.TPutsAsync(
            "\u0080$<2/>\u00ff",
            1,
            stream,
            Encoding.Latin1,
            new TermInfoOutputOptions(
                terminal,
                delayProvider: delayProvider));

        Assert.Equal(
            new byte[]
            {
                0x80,
                0xff,
            },
            stream.ToArray());
        Assert.Equal(
            0,
            delayProvider.SynchronousDelayCount);
        Assert.Equal(
            1,
            delayProvider.AsynchronousDelayCount);
        Assert.Single(delayProvider.Delays);
    }

    [Fact]
    public async Task AsyncPadCharacterModeWritesExactLatin1Bytes()
    {
        TerminalDescription terminal =
            new TerminalDescriptionBuilder("async-pad")
                .SetString(
                    StringCapability.PadChar,
                    "\u00a5")
                .Build();
        using MemoryStream stream = new();

        await TermInfoOutput.TPutsAsync(
            "\u0080$<4/>\u00ff",
            1,
            stream,
            Encoding.Latin1,
            new TermInfoOutputOptions(
                terminal,
                baudRate: 9000,
                paddingMode: PaddingMode.PadCharacters));

        Assert.Equal(
            new byte[]
            {
                0x80,
                0xa5,
                0xa5,
                0xa5,
                0xa5,
                0xff,
            },
            stream.ToArray());
    }

    private sealed class RecordingDelayProvider : ITermInfoDelayProvider
    {
        internal List<TermInfoDelay> Delays { get; } = [];

        internal int SynchronousDelayCount { get; private set; }

        internal int AsynchronousDelayCount { get; private set; }

        public void Delay(TermInfoDelay delay)
        {
            SynchronousDelayCount++;
            Delays.Add(delay);
        }

        public ValueTask DelayAsync(
            TermInfoDelay delay,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            AsynchronousDelayCount++;
            Delays.Add(delay);
            return ValueTask.CompletedTask;
        }
    }
}
