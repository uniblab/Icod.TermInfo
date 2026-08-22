namespace Icod.TermInfo;

/// <summary>
/// Describes one resolved terminfo output delay.
/// </summary>
public readonly record struct TermInfoDelay
{
    /// <summary>
    /// Initializes a resolved terminfo delay.
    /// </summary>
    public TermInfoDelay(
        TimeSpan duration,
        bool isMandatory)
    {
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                "A terminal delay cannot be negative.");
        }

        Duration = duration;
        IsMandatory = isMandatory;
    }

    /// <summary>
    /// Gets the resolved delay duration.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Gets whether the source padding directive used the mandatory
    /// <c>/</c> suffix.
    /// </summary>
    public bool IsMandatory { get; }
}
