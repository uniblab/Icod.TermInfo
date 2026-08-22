namespace Icod.TermInfo;

/// <summary>
/// Provides a convenient classification of a terminal's advertised color depth.
/// </summary>
public enum TerminalColorTier
{
    /// <summary>
    /// No usable color selection is advertised.
    /// </summary>
    Monochrome,

    /// <summary>
    /// Four indexed colors are advertised.
    /// </summary>
    Color4,

    /// <summary>
    /// Eight indexed colors are advertised.
    /// </summary>
    Color8,

    /// <summary>
    /// Sixteen indexed colors are advertised.
    /// </summary>
    Color16,

    /// <summary>
    /// Two hundred fifty-six indexed colors are advertised.
    /// </summary>
    Color256,

    /// <summary>
    /// Direct RGB color with eight bits per red, green, and blue channel is
    /// advertised across the full 24-bit range.
    /// </summary>
    TrueColor,

    /// <summary>
    /// An indexed palette with another positive size is advertised.
    /// </summary>
    OtherIndexed,

    /// <summary>
    /// A direct RGB layout other than full 8/8/8 true color is advertised.
    /// </summary>
    OtherDirectRgb,
}
