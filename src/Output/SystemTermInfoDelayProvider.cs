namespace Icod.TermInfo;

internal sealed class SystemTermInfoDelayProvider : ITermInfoDelayProvider
{
    internal static SystemTermInfoDelayProvider Instance { get; } = new();

    private SystemTermInfoDelayProvider()
    {
    }

    public void Delay(TermInfoDelay delay)
    {
        if (delay.Duration > TimeSpan.Zero)
        {
            Thread.Sleep(delay.Duration);
        }
    }

    public async ValueTask DelayAsync(
        TermInfoDelay delay,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (delay.Duration > TimeSpan.Zero)
        {
            await Task.Delay(
                delay.Duration,
                cancellationToken).ConfigureAwait(false);
        }
    }
}
