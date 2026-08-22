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

    /// <summary>
    /// Enter the mode used for cursor-addressed applications (<c>smcup</c>).
    /// </summary>
    EnterCursorAddressingMode,

    /// <summary>
    /// Leave the mode used for cursor-addressed applications (<c>rmcup</c>).
    /// </summary>
    ExitCursorAddressingMode,

    /// <summary>
    /// Make the cursor invisible (<c>civis</c>).
    /// </summary>
    CursorInvisible,

    /// <summary>
    /// Restore the normal cursor appearance (<c>cnorm</c>).
    /// </summary>
    CursorNormal,

    /// <summary>
    /// Make the cursor very visible (<c>cvvis</c>).
    /// </summary>
    CursorVeryVisible,

    /// <summary>
    /// Produce a visible bell or screen flash (<c>flash</c>).
    /// </summary>
    FlashScreen,

    /// <summary>
    /// Move to the beginning of the next line (<c>nel</c>).
    /// </summary>
    NewLine,

    /// <summary>
    /// Scroll forward by a parameterized number of lines (<c>indn</c>).
    /// </summary>
    ScrollForwardLines,

    /// <summary>
    /// Scroll backward by a parameterized number of lines (<c>rin</c>).
    /// </summary>
    ScrollReverseLines,

    /// <summary>
    /// Enter insert mode (<c>smir</c>).
    /// </summary>
    EnterInsertMode,

    /// <summary>
    /// Exit insert mode (<c>rmir</c>).
    /// </summary>
    ExitInsertMode,

    /// <summary>
    /// Enter meta mode (<c>smm</c>).
    /// </summary>
    EnterMetaMode,

    /// <summary>
    /// Exit meta mode (<c>rmm</c>).
    /// </summary>
    ExitMetaMode,

    /// <summary>
    /// Enter italic mode (<c>sitm</c>).
    /// </summary>
    EnterItalicMode,

    /// <summary>
    /// Exit italic mode (<c>ritm</c>).
    /// </summary>
    ExitItalicMode,

    /// <summary>
    /// Initialize or redefine one color (<c>initc</c>).
    /// </summary>
    InitializeColor,

    /// <summary>
    /// Restore the terminal's original colors (<c>oc</c>).
    /// </summary>
    OriginalColors,

    /// <summary>
    /// Set a foreground color using the legacy terminfo selector (<c>setf</c>).
    /// </summary>
    SetLegacyForegroundColor,

    /// <summary>
    /// Set a background color using the legacy terminfo selector (<c>setb</c>).
    /// </summary>
    SetLegacyBackgroundColor,

    /// <summary>
    /// Initialization string 1 (<c>is1</c>).
    /// </summary>
    InitString1,

    /// <summary>
    /// Initialization string 2 (<c>is2</c>).
    /// </summary>
    InitString2,

    /// <summary>
    /// Initialization string 3 (<c>is3</c>).
    /// </summary>
    InitString3,

    /// <summary>
    /// Reset string 1 (<c>rs1</c>).
    /// </summary>
    ResetString1,

    /// <summary>
    /// Reset string 3 (<c>rs3</c>).
    /// </summary>
    ResetString3,

    /// <summary>
    /// Mouse-report key prefix (<c>kmous</c>).
    /// </summary>
    KeyMouse,

    /// <summary>
    /// Lock terminal memory above the cursor (<c>meml</c>).
    /// </summary>
    MemoryLock,

    /// <summary>
    /// Unlock terminal memory (<c>memu</c>).
    /// </summary>
    MemoryUnlock,

    /// <summary>
    /// Repeat a character a parameterized number of times (<c>rep</c>).
    /// </summary>
    RepeatCharacter,

    /// <summary>
    /// Print the current screen (<c>mc0</c>).
    /// </summary>
    PrintScreen,

    /// <summary>
    /// Turn the printer off (<c>mc4</c>).
    /// </summary>
    PrinterOff,

    /// <summary>
    /// Turn the printer on (<c>mc5</c>).
    /// </summary>
    PrinterOn,

    /// <summary>
    /// Back-tab key sequence (<c>kcbt</c>).
    /// </summary>
    KeyBackTab,

    /// <summary>
    /// Beginning key sequence (<c>kbeg</c>).
    /// </summary>
    KeyBegin,

    /// <summary>
    /// Delete-character key sequence (<c>kdch1</c>).
    /// </summary>
    KeyDeleteCharacter,

    /// <summary>
    /// End key sequence (<c>kend</c>).
    /// </summary>
    KeyEnd,

    /// <summary>
    /// Enter key sequence (<c>kent</c>).
    /// </summary>
    KeyEnter,

    /// <summary>
    /// Insert-character key sequence (<c>kich1</c>).
    /// </summary>
    KeyInsertCharacter,

    /// <summary>
    /// Next-page key sequence (<c>knp</c>).
    /// </summary>
    KeyNextPage,

    /// <summary>
    /// Previous-page key sequence (<c>kpp</c>).
    /// </summary>
    KeyPreviousPage,

    /// <summary>
    /// Function key 5 sequence (<c>kf5</c>).
    /// </summary>
    KeyF5,

    /// <summary>
    /// Function key 6 sequence (<c>kf6</c>).
    /// </summary>
    KeyF6,

    /// <summary>
    /// Function key 7 sequence (<c>kf7</c>).
    /// </summary>
    KeyF7,

    /// <summary>
    /// Function key 8 sequence (<c>kf8</c>).
    /// </summary>
    KeyF8,

    /// <summary>
    /// Function key 9 sequence (<c>kf9</c>).
    /// </summary>
    KeyF9,

    /// <summary>
    /// Function key 10 sequence (<c>kf10</c>).
    /// </summary>
    KeyF10,

    /// <summary>
    /// Function key 11 sequence (<c>kf11</c>).
    /// </summary>
    KeyF11,

    /// <summary>
    /// Function key 12 sequence (<c>kf12</c>).
    /// </summary>
    KeyF12,

    /// <summary>
    /// Function key 13 sequence (<c>kf13</c>).
    /// </summary>
    KeyF13,

    /// <summary>
    /// Function key 14 sequence (<c>kf14</c>).
    /// </summary>
    KeyF14,

    /// <summary>
    /// Function key 15 sequence (<c>kf15</c>).
    /// </summary>
    KeyF15,

    /// <summary>
    /// Function key 16 sequence (<c>kf16</c>).
    /// </summary>
    KeyF16,

    /// <summary>
    /// Function key 17 sequence (<c>kf17</c>).
    /// </summary>
    KeyF17,

    /// <summary>
    /// Function key 18 sequence (<c>kf18</c>).
    /// </summary>
    KeyF18,

    /// <summary>
    /// Function key 19 sequence (<c>kf19</c>).
    /// </summary>
    KeyF19,

    /// <summary>
    /// Function key 20 sequence (<c>kf20</c>).
    /// </summary>
    KeyF20,

    /// <summary>
    /// Function key 21 sequence (<c>kf21</c>).
    /// </summary>
    KeyF21,

    /// <summary>
    /// Function key 22 sequence (<c>kf22</c>).
    /// </summary>
    KeyF22,

    /// <summary>
    /// Function key 23 sequence (<c>kf23</c>).
    /// </summary>
    KeyF23,

    /// <summary>
    /// Function key 24 sequence (<c>kf24</c>).
    /// </summary>
    KeyF24,

    /// <summary>
    /// Upper-left keypad key sequence (<c>ka1</c>).
    /// </summary>
    KeyA1,

    /// <summary>
    /// Upper-right keypad key sequence (<c>ka3</c>).
    /// </summary>
    KeyA3,

    /// <summary>
    /// Center keypad key sequence (<c>kb2</c>).
    /// </summary>
    KeyB2,

    /// <summary>
    /// Lower-left keypad key sequence (<c>kc1</c>).
    /// </summary>
    KeyC1,

    /// <summary>
    /// Lower-right keypad key sequence (<c>kc3</c>).
    /// </summary>
    KeyC3,

    /// <summary>
    /// Scroll-forward key sequence (<c>kind</c>).
    /// </summary>
    KeyScrollForward,

    /// <summary>
    /// Scroll-backward key sequence (<c>kri</c>).
    /// </summary>
    KeyScrollReverse,

    /// <summary>
    /// Shifted delete-character key sequence (<c>kDC</c>).
    /// </summary>
    KeyShiftDeleteCharacter,

    /// <summary>
    /// Shifted End key sequence (<c>kEND</c>).
    /// </summary>
    KeyShiftEnd,

    /// <summary>
    /// Shifted Home key sequence (<c>kHOM</c>).
    /// </summary>
    KeyShiftHome,

    /// <summary>
    /// Shifted insert-character key sequence (<c>kIC</c>).
    /// </summary>
    KeyShiftInsertCharacter,

    /// <summary>
    /// Shifted left-arrow key sequence (<c>kLFT</c>).
    /// </summary>
    KeyShiftLeft,

    /// <summary>
    /// Shifted next-page key sequence (<c>kNXT</c>).
    /// </summary>
    KeyShiftNextPage,

    /// <summary>
    /// Shifted previous-page key sequence (<c>kPRV</c>).
    /// </summary>
    KeyShiftPreviousPage,

    /// <summary>
    /// Shifted right-arrow key sequence (<c>kRIT</c>).
    /// </summary>
    KeyShiftRight,
}
