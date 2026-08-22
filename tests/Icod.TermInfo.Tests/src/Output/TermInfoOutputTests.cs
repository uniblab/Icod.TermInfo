using System.Text;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class TermInfoOutputTests
{
    [Fact]
    public void IgnoreModeStripsPaddingWithoutDelaying()
    {
        using StringWriter writer = new();
        RecordingDelayProvider delayProvider = new();

        TermInfoOutput.TPuts(
            "A$<2>B$<3*/>C",
            4,
            writer,
            PaddingMode.Ignore,
            delayProvider);

        Assert.Equal("ABC", writer.ToString());
        Assert.Empty(delayProvider.Delays);
    }

    [Fact]
    public void DelayModeInvokesDelaysInOutputOrder()
    {
        using StringWriter writer = new();
        RecordingDelayProvider delayProvider = new();

        TermInfoOutput.TPuts(
            "A$<2>B$<3*/>C$<1.5/>D",
            4,
            writer,
            PaddingMode.Delay,
            delayProvider);

        Assert.Equal("ABCD", writer.ToString());
        Assert.Collection(
            delayProvider.Delays,
            delay =>
            {
                Assert.Equal(TimeSpan.FromMilliseconds(2), delay.Duration);
                Assert.False(delay.IsMandatory);
            },
            delay =>
            {
                Assert.Equal(TimeSpan.FromMilliseconds(12), delay.Duration);
                Assert.True(delay.IsMandatory);
            },
            delay =>
            {
                Assert.Equal(TimeSpan.FromMilliseconds(1.5), delay.Duration);
                Assert.True(delay.IsMandatory);
            });
    }

    [Fact]
    public void MultiplicativePaddingUsesAffectedLineCount()
    {
        using StringWriter writer = new();
        RecordingDelayProvider delayProvider = new();

        TermInfoOutput.TPuts(
            "A$<2.5*>B",
            3,
            writer,
            PaddingMode.Delay,
            delayProvider);

        TermInfoDelay delay = Assert.Single(delayProvider.Delays);
        Assert.Equal(TimeSpan.FromMilliseconds(7.5), delay.Duration);
        Assert.False(delay.IsMandatory);
    }

    [Fact]
    public void ZeroAffectedLinesProducesZeroMultiplicativeDelay()
    {
        using StringWriter writer = new();
        RecordingDelayProvider delayProvider = new();

        TermInfoOutput.TPuts(
            "$<5*>",
            0,
            writer,
            PaddingMode.Delay,
            delayProvider);

        TermInfoDelay delay = Assert.Single(delayProvider.Delays);
        Assert.Equal(TimeSpan.Zero, delay.Duration);
    }

    [Fact]
    public void DelayIsCappedAtThirtySeconds()
    {
        using StringWriter writer = new();
        RecordingDelayProvider delayProvider = new();

        TermInfoOutput.TPuts(
            "$<20000*>",
            2,
            writer,
            PaddingMode.Delay,
            delayProvider);

        TermInfoDelay delay = Assert.Single(delayProvider.Delays);
        Assert.Equal(TimeSpan.FromSeconds(30), delay.Duration);
    }

    [Fact]
    public void PutPUsesOneAffectedLine()
    {
        using StringWriter writer = new();
        RecordingDelayProvider delayProvider = new();

        TermInfoOutput.PutP(
            "A$<4*>B",
            writer,
            PaddingMode.Delay,
            delayProvider);

        Assert.Equal("AB", writer.ToString());
        TermInfoDelay delay = Assert.Single(delayProvider.Delays);
        Assert.Equal(TimeSpan.FromMilliseconds(4), delay.Duration);
    }

    [Fact]
    public void CharacterCallbackReceivesOnlyTerminalCharacters()
    {
        List<char> characters = [];

        TermInfoOutput.TPuts(
            "A$<5>B",
            1,
            characters.Add);

        Assert.Equal("AB", new string(characters.ToArray()));
    }

    [Fact]
    public void StreamOutputUsesCallerSuppliedEncoding()
    {
        using MemoryStream stream = new();

        TermInfoOutput.TPuts(
            "\x1b[A$<2>é",
            1,
            stream,
            Encoding.UTF8);

        Assert.Equal(
            Encoding.UTF8.GetBytes("\x1b[Aé"),
            stream.ToArray());
    }

    [Fact]
    public async Task AsyncTextWriterUsesAsyncDelayProvider()
    {
        using StringWriter writer = new();
        RecordingDelayProvider delayProvider = new();

        await TermInfoOutput.TPutsAsync(
            "A$<2.5/>B",
            1,
            writer,
            PaddingMode.Delay,
            delayProvider);

        Assert.Equal("AB", writer.ToString());
        Assert.Equal(0, delayProvider.SynchronousDelayCount);
        Assert.Equal(1, delayProvider.AsynchronousDelayCount);

        TermInfoDelay delay = Assert.Single(delayProvider.Delays);
        Assert.Equal(TimeSpan.FromMilliseconds(2.5), delay.Duration);
        Assert.True(delay.IsMandatory);
    }

    [Fact]
    public async Task AsyncStreamWritesBytesAndDelaysWithoutSleeping()
    {
        using MemoryStream stream = new();
        RecordingDelayProvider delayProvider = new();

        await TermInfoOutput.TPutsAsync(
            "A$<3>B",
            1,
            stream,
            Encoding.ASCII,
            PaddingMode.Delay,
            delayProvider);

        Assert.Equal(Encoding.ASCII.GetBytes("AB"), stream.ToArray());
        Assert.Equal(1, delayProvider.AsynchronousDelayCount);
        Assert.Single(delayProvider.Delays);
    }

    [Fact]
    public async Task PutPAsyncUsesOneAffectedLine()
    {
        using StringWriter writer = new();
        RecordingDelayProvider delayProvider = new();

        await TermInfoOutput.PutPAsync(
            "A$<6*>B",
            writer,
            PaddingMode.Delay,
            delayProvider);

        Assert.Equal("AB", writer.ToString());
        TermInfoDelay delay = Assert.Single(delayProvider.Delays);
        Assert.Equal(TimeSpan.FromMilliseconds(6), delay.Duration);
    }

    [Fact]
    public async Task AsyncOutputObservesCancellationBeforeWriting()
    {
        using StringWriter writer = new();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await TermInfoOutput.TPutsAsync(
                "ABC",
                1,
                writer,
                cancellationToken: cancellation.Token));

        Assert.Equal(string.Empty, writer.ToString());
    }

    [Fact]
    public void OrdinaryDollarAndAngleCharactersArePreserved()
    {
        using StringWriter writer = new();

        TermInfoOutput.TPuts(
            "cost=$5 <tag> $",
            1,
            writer);

        Assert.Equal("cost=$5 <tag> $", writer.ToString());
    }

    [Theory]
    [InlineData("$<>")]
    [InlineData("$<.5>")]
    [InlineData("$<5.>")]
    [InlineData("$<5.00>")]
    [InlineData("$<5x>")]
    [InlineData("$<5**>")]
    [InlineData("$<5//>")]
    [InlineData("$<5")]
    public void MalformedPaddingThrowsBeforeAnyOutput(string value)
    {
        using StringWriter writer = new();

        TermInfoPaddingFormatException exception =
            Assert.Throws<TermInfoPaddingFormatException>(
                () => TermInfoOutput.TPuts(
                    $"prefix{value}suffix",
                    1,
                    writer));

        Assert.True(exception.Position >= 0);
        Assert.Equal(string.Empty, writer.ToString());
    }

    [Fact]
    public void BothPaddingSuffixesMayAppearTogether()
    {
        using StringWriter writer = new();
        RecordingDelayProvider delayProvider = new();

        TermInfoOutput.TPuts(
            "$<3/*>",
            2,
            writer,
            PaddingMode.Delay,
            delayProvider);

        TermInfoDelay delay = Assert.Single(delayProvider.Delays);
        Assert.Equal(TimeSpan.FromMilliseconds(6), delay.Duration);
        Assert.True(delay.IsMandatory);
    }

    [Fact]
    public void NegativeAffectedLineCountIsRejected()
    {
        using StringWriter writer = new();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => TermInfoOutput.TPuts(
                "ABC",
                -1,
                writer));
    }

    [Fact]
    public void UnknownPaddingModeIsRejected()
    {
        using StringWriter writer = new();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => TermInfoOutput.TPuts(
                "ABC",
                1,
                writer,
                (PaddingMode)999));
    }

    [Fact]
    public void NonWritableStreamIsRejected()
    {
        using MemoryStream stream =
            new(Array.Empty<byte>(), writable: false);

        Assert.Throws<ArgumentException>(
            () => TermInfoOutput.TPuts(
                "ABC",
                1,
                stream,
                Encoding.ASCII));
    }

    [Fact]
    public void TermInfoDelayRejectsNegativeDuration()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new TermInfoDelay(
                TimeSpan.FromMilliseconds(-1),
                false));
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
