namespace Icod.TermInfo;

/// <summary>
/// Identifies a string terminal capability.
/// </summary>
public enum StringCapability
{
    /// <summary>
    /// Audible bell (<c>bel</c>).
    /// </summary>
    Bell,

    /// <summary>
    /// Carriage return (<c>cr</c>).
    /// </summary>
    CarriageReturn,

    /// <summary>
    /// Move the cursor down one line (<c>cud1</c>).
    /// </summary>
    CursorDownOne,

    /// <summary>
    /// Scroll forward one line (<c>ind</c>).
    /// </summary>
    ScrollForward,

    /// <summary>
    /// Clear the screen (<c>clear</c>).
    /// </summary>
    ClearScreen,

    /// <summary>
    /// Address the cursor by row and column (<c>cup</c>).
    /// </summary>
    CursorAddress,

    /// <summary>
    /// Enter bold mode (<c>bold</c>).
    /// </summary>
    EnterBoldMode,

    /// <summary>
    /// Exit all attributes (<c>sgr0</c>).
    /// </summary>
    ExitAttributeMode,

    /// <summary>
    /// Set an ANSI-compatible foreground color (<c>setaf</c>).
    /// </summary>
    SetForegroundColor,

    /// <summary>
    /// Set an ANSI-compatible background color (<c>setab</c>).
    /// </summary>
    SetBackgroundColor,
}
