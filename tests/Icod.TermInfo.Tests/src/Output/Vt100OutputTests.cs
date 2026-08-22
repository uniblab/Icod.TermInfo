using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class Vt100OutputTests
{
    [Fact]
    public void DefaultOutputRemovesVt100ClearScreenPadding()
    {
        using StringWriter writer = new();

        TermInfoOutput.PutP(
            TerminalProfiles.Vt100.GetRequiredString(
                StringCapability.ClearScreen),
            writer);

        Assert.Equal("\x1b[H\x1b[J", writer.ToString());
        Assert.DoesNotContain("$<", writer.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void DelayModeHonorsVt100ClearScreenPadding()
    {
        using StringWriter writer = new();
        RecordingDelayProvider delayProvider = new();

        TermInfoOutput.PutP(
            TerminalProfiles.Vt100.GetRequiredString(
                StringCapability.ClearScreen),
            writer,
            PaddingMode.Delay,
            delayProvider);

        Assert.Equal("\x1b[H\x1b[J", writer.ToString());

        TermInfoDelay delay = Assert.Single(delayProvider.Delays);
        Assert.Equal(TimeSpan.FromMilliseconds(50), delay.Duration);
        Assert.False(delay.IsMandatory);
    }

    [Fact]
    public void ExpandedVt100CursorAddressCanBeEmittedWithPadding()
    {
        using StringWriter writer = new();
        RecordingDelayProvider delayProvider = new();

        string value =
            TerminalProfiles.Vt100.Expand(
                StringCapability.CursorAddress,
                10,
                20);

        TermInfoOutput.TPuts(
            value,
            1,
            writer,
            PaddingMode.Delay,
            delayProvider);

        Assert.Equal("\x1b[11;21H", writer.ToString());

        TermInfoDelay delay = Assert.Single(delayProvider.Delays);
        Assert.Equal(TimeSpan.FromMilliseconds(5), delay.Duration);
    }

    [Fact]
    public void ExpandedVt100AttributesCanBeEmittedWithoutPaddingText()
    {
        using StringWriter writer = new();
        RecordingDelayProvider delayProvider = new();

        string value =
            TerminalProfiles.Vt100.Expand(
                StringCapability.SetAttributes,
                0,
                1,
                0,
                1,
                0,
                1,
                0,
                0,
                1);

        TermInfoOutput.TPuts(
            value,
            1,
            writer,
            PaddingMode.Delay,
            delayProvider);

        Assert.DoesNotContain("$<", writer.ToString(), StringComparison.Ordinal);

        TermInfoDelay delay = Assert.Single(delayProvider.Delays);
        Assert.Equal(TimeSpan.FromMilliseconds(2), delay.Duration);
    }

    private sealed class RecordingDelayProvider : ITermInfoDelayProvider
    {
        internal List<TermInfoDelay> Delays { get; } = [];

        public void Delay(TermInfoDelay delay)
        {
            Delays.Add(delay);
        }

        public ValueTask DelayAsync(
            TermInfoDelay delay,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Delays.Add(delay);
            return ValueTask.CompletedTask;
        }
    }
}
