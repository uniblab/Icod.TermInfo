namespace Icod.TermInfo;

/// <summary>
/// Identifies a boolean terminal capability.
/// </summary>
public enum BooleanCapability
{
    /// <summary>
    /// The terminal automatically wraps at the right margin (<c>am</c>).
    /// </summary>
    AutoRightMargin,

    /// <summary>
    /// The profile describes a generic terminal type (<c>gn</c>).
    /// </summary>
    GenericType,

    /// <summary>
    /// Moving the cursor is safe while standout mode is active (<c>msgr</c>).
    /// </summary>
    MoveStandoutMode,

    /// <summary>
    /// A newline glitch occurs after wrapping at the right margin (<c>xenl</c>).
    /// </summary>
    EatNewlineGlitch,

    /// <summary>
    /// The terminal uses XON/XOFF flow control (<c>xon</c>).
    /// </summary>
    XonXoff,
}
