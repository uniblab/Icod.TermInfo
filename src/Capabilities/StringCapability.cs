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

    /// <summary>
    /// Back tab (<c>cbt</c>).
    /// </summary>
    BackTab,

    /// <summary>
    /// Enter blink mode (<c>blink</c>).
    /// </summary>
    EnterBlinkMode,

    /// <summary>
    /// Enter dim mode (<c>dim</c>).
    /// </summary>
    EnterDimMode,

    /// <summary>
    /// Change the scrolling region (<c>csr</c>).
    /// </summary>
    ChangeScrollRegion,

    /// <summary>
    /// Move the cursor left by a parameterized count (<c>cub</c>).
    /// </summary>
    CursorLeft,

    /// <summary>
    /// Move the cursor left one column (<c>cub1</c>).
    /// </summary>
    CursorLeftOne,

    /// <summary>
    /// Move the cursor down by a parameterized count (<c>cud</c>).
    /// </summary>
    CursorDown,

    /// <summary>
    /// Move the cursor right by a parameterized count (<c>cuf</c>).
    /// </summary>
    CursorRight,

    /// <summary>
    /// Move the cursor right one column (<c>cuf1</c>).
    /// </summary>
    CursorRightOne,

    /// <summary>
    /// Move the cursor up by a parameterized count (<c>cuu</c>).
    /// </summary>
    CursorUp,

    /// <summary>
    /// Move the cursor up one line (<c>cuu1</c>).
    /// </summary>
    CursorUpOne,

    /// <summary>
    /// Delete a parameterized number of characters (<c>dch</c>).
    /// </summary>
    DeleteCharacters,

    /// <summary>
    /// Delete one character (<c>dch1</c>).
    /// </summary>
    DeleteCharacter,

    /// <summary>
    /// Delete a parameterized number of lines (<c>dl</c>).
    /// </summary>
    DeleteLines,

    /// <summary>
    /// Delete one line (<c>dl1</c>).
    /// </summary>
    DeleteLine,

    /// <summary>
    /// Clear from the cursor to the end of the screen (<c>ed</c>).
    /// </summary>
    ClearToEndOfScreen,

    /// <summary>
    /// Clear from the cursor to the end of the line (<c>el</c>).
    /// </summary>
    ClearToEndOfLine,

    /// <summary>
    /// Clear from the beginning of the line to the cursor (<c>el1</c>).
    /// </summary>
    ClearToBeginningOfLine,

    /// <summary>
    /// Move the cursor home (<c>home</c>).
    /// </summary>
    CursorHome,

    /// <summary>
    /// Address the cursor column (<c>hpa</c>).
    /// </summary>
    ColumnAddress,

    /// <summary>
    /// Move to the next hardware tab stop (<c>ht</c>).
    /// </summary>
    Tab,

    /// <summary>
    /// Set a hardware tab stop (<c>hts</c>).
    /// </summary>
    SetTab,

    /// <summary>
    /// Insert a parameterized number of characters (<c>ich</c>).
    /// </summary>
    InsertCharacters,

    /// <summary>
    /// Insert one character (<c>ich1</c>).
    /// </summary>
    InsertCharacter,

    /// <summary>
    /// Insert a parameterized number of lines (<c>il</c>).
    /// </summary>
    InsertLines,

    /// <summary>
    /// Insert one line (<c>il1</c>).
    /// </summary>
    InsertLine,

    /// <summary>
    /// Enter invisible or secure display mode (<c>invis</c>).
    /// </summary>
    EnterInvisibleMode,

    /// <summary>
    /// Restore the original color pair (<c>op</c>).
    /// </summary>
    OriginalColorPair,

    /// <summary>
    /// Restore a previously saved cursor position (<c>rc</c>).
    /// </summary>
    RestoreCursor,

    /// <summary>
    /// Enter reverse-video mode (<c>rev</c>).
    /// </summary>
    EnterReverseMode,

    /// <summary>
    /// Scroll backward one line (<c>ri</c>).
    /// </summary>
    ScrollReverse,

    /// <summary>
    /// Exit alternate-character-set mode (<c>rmacs</c>).
    /// </summary>
    ExitAlternateCharacterSetMode,

    /// <summary>
    /// Disable automatic margins (<c>rmam</c>).
    /// </summary>
    ExitAutomaticMargins,

    /// <summary>
    /// Leave application keypad mode (<c>rmkx</c>).
    /// </summary>
    ExitKeypadMode,

    /// <summary>
    /// Exit standout mode (<c>rmso</c>).
    /// </summary>
    ExitStandoutMode,

    /// <summary>
    /// Exit underline mode (<c>rmul</c>).
    /// </summary>
    ExitUnderlineMode,

    /// <summary>
    /// Save the cursor position (<c>sc</c>).
    /// </summary>
    SaveCursor,

    /// <summary>
    /// Set multiple video attributes (<c>sgr</c>).
    /// </summary>
    SetAttributes,

    /// <summary>
    /// Enter alternate-character-set mode (<c>smacs</c>).
    /// </summary>
    EnterAlternateCharacterSetMode,

    /// <summary>
    /// Enable automatic margins (<c>smam</c>).
    /// </summary>
    EnterAutomaticMargins,

    /// <summary>
    /// Enter application keypad mode (<c>smkx</c>).
    /// </summary>
    EnterKeypadMode,

    /// <summary>
    /// Enter standout mode (<c>smso</c>).
    /// </summary>
    EnterStandoutMode,

    /// <summary>
    /// Enter underline mode (<c>smul</c>).
    /// </summary>
    EnterUnderlineMode,

    /// <summary>
    /// Address the cursor row (<c>vpa</c>).
    /// </summary>
    RowAddress,

    /// <summary>
    /// Alternate-character-set map (<c>acsc</c>).
    /// </summary>
    AlternateCharacterSet,

    /// <summary>
    /// Enable the alternate character set (<c>enacs</c>).
    /// </summary>
    EnableAlternateCharacterSet,

    /// <summary>
    /// Backspace key sequence (<c>kbs</c>).
    /// </summary>
    KeyBackspace,

    /// <summary>
    /// Cursor-down key sequence (<c>kcud1</c>).
    /// </summary>
    KeyCursorDown,

    /// <summary>
    /// Cursor-left key sequence (<c>kcub1</c>).
    /// </summary>
    KeyCursorLeft,

    /// <summary>
    /// Cursor-right key sequence (<c>kcuf1</c>).
    /// </summary>
    KeyCursorRight,

    /// <summary>
    /// Cursor-up key sequence (<c>kcuu1</c>).
    /// </summary>
    KeyCursorUp,

    /// <summary>
    /// Home key sequence (<c>khome</c>).
    /// </summary>
    KeyHome,

    /// <summary>
    /// Function/PF key 1 sequence (<c>kf1</c>).
    /// </summary>
    KeyF1,

    /// <summary>
    /// Function/PF key 2 sequence (<c>kf2</c>).
    /// </summary>
    KeyF2,

    /// <summary>
    /// Function/PF key 3 sequence (<c>kf3</c>).
    /// </summary>
    KeyF3,

    /// <summary>
    /// Function/PF key 4 sequence (<c>kf4</c>).
    /// </summary>
    KeyF4,

    /// <summary>
    /// Reset string 2 (<c>rs2</c>).
    /// </summary>
    ResetString2,

    /// <summary>
    /// Erase a parameterized number of characters (<c>ech</c>).
    /// </summary>
    EraseCharacters,

    /// <summary>
    /// Clear all hardware tab stops (<c>tbc</c>).
    /// </summary>
    ClearAllTabs,
}
