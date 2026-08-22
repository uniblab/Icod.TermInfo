namespace Icod.TermInfo;

/// <summary>
/// Performs delays requested by the terminfo output layer.
/// </summary>
public interface ITermInfoDelayProvider
{
    /// <summary>
    /// Performs a synchronous delay.
    /// </summary>
    void Delay(TermInfoDelay delay);

    /// <summary>
    /// Performs an asynchronous delay.
    /// </summary>
    ValueTask DelayAsync(
        TermInfoDelay delay,
        CancellationToken cancellationToken = default);
}
