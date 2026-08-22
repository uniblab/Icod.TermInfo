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

    /// <summary>
    /// Moving the cursor is safe while insert mode is active (<c>mir</c>).
    /// </summary>
    MoveInsertMode,

    /// <summary>
    /// Erasing characters uses the current background color (<c>bce</c>).
    /// </summary>
    BackColorErase,

    /// <summary>
    /// The terminal can redefine its color palette (<c>ccc</c>).
    /// </summary>
    CanChangeColor,

    /// <summary>
    /// Color initialization uses HLS values rather than RGB values (<c>hls</c>).
    /// </summary>
    HueLightnessSaturation,

    /// <summary>
    /// The terminal has a meta key (<c>km</c>).
    /// </summary>
    HasMetaKey,

    /// <summary>
    /// The terminal does not require a pad character (<c>npc</c>).
    /// </summary>
    NoPadCharacter,
}
