namespace Icod.TermInfo;

/// <summary>
/// Controls how terminfo padding directives are handled during output.
/// </summary>
public enum PaddingMode
{
    /// <summary>
    /// Remove padding directives without delaying output.
    /// </summary>
    Ignore,

    /// <summary>
    /// Honor padding directives using the configured delay provider.
    /// </summary>
    Delay,
}
