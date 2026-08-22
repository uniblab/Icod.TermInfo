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

    /// <summary>
    /// Find key sequence (<c>kfnd</c>).
    /// </summary>
    KeyFind,

    /// <summary>
    /// Help key sequence (<c>khlp</c>).
    /// </summary>
    KeyHelp,

    /// <summary>
    /// Redo key sequence (<c>krdo</c>).
    /// </summary>
    KeyRedo,

    /// <summary>
    /// Select key sequence (<c>kslt</c>).
    /// </summary>
    KeySelect,

    /// <summary>
    /// Standard terminfo capability <c>cmdch</c> (<c>command_character</c>).
    /// </summary>
    CommandCharacter,

    /// <summary>
    /// Standard terminfo capability <c>mrcup</c> (<c>cursor_mem_address</c>).
    /// </summary>
    CursorMemAddress,

    /// <summary>
    /// Standard terminfo capability <c>ll</c> (<c>cursor_to_ll</c>).
    /// </summary>
    CursorToLl,

    /// <summary>
    /// Standard terminfo capability <c>dsl</c> (<c>dis_status_line</c>).
    /// </summary>
    DisStatusLine,

    /// <summary>
    /// Standard terminfo capability <c>hd</c> (<c>down_half_line</c>).
    /// </summary>
    DownHalfLine,

    /// <summary>
    /// Standard terminfo capability <c>smdc</c> (<c>enter_delete_mode</c>).
    /// </summary>
    EnterDeleteMode,

    /// <summary>
    /// Standard terminfo capability <c>prot</c> (<c>enter_protected_mode</c>).
    /// </summary>
    EnterProtectedMode,

    /// <summary>
    /// Standard terminfo capability <c>rmdc</c> (<c>exit_delete_mode</c>).
    /// </summary>
    ExitDeleteMode,

    /// <summary>
    /// Standard terminfo capability <c>ff</c> (<c>form_feed</c>).
    /// </summary>
    FormFeed,

    /// <summary>
    /// Standard terminfo capability <c>fsl</c> (<c>from_status_line</c>).
    /// </summary>
    FromStatusLine,

    /// <summary>
    /// Standard terminfo capability <c>if</c> (<c>init_file</c>).
    /// </summary>
    InitFile,

    /// <summary>
    /// Standard terminfo capability <c>ip</c> (<c>insert_padding</c>).
    /// </summary>
    InsertPadding,

    /// <summary>
    /// Standard terminfo capability <c>ktbc</c> (<c>key_catab</c>).
    /// </summary>
    KeyCatab,

    /// <summary>
    /// Standard terminfo capability <c>kclr</c> (<c>key_clear</c>).
    /// </summary>
    KeyClear,

    /// <summary>
    /// Standard terminfo capability <c>kctab</c> (<c>key_ctab</c>).
    /// </summary>
    KeyCtab,

    /// <summary>
    /// Standard terminfo capability <c>kdl1</c> (<c>key_dl</c>).
    /// </summary>
    KeyDl,

    /// <summary>
    /// Standard terminfo capability <c>krmir</c> (<c>key_eic</c>).
    /// </summary>
    KeyEic,

    /// <summary>
    /// Standard terminfo capability <c>kel</c> (<c>key_eol</c>).
    /// </summary>
    KeyEol,

    /// <summary>
    /// Standard terminfo capability <c>ked</c> (<c>key_eos</c>).
    /// </summary>
    KeyEos,

    /// <summary>
    /// Standard terminfo capability <c>kf0</c> (<c>key_f0</c>).
    /// </summary>
    KeyF0,

    /// <summary>
    /// Standard terminfo capability <c>kil1</c> (<c>key_il</c>).
    /// </summary>
    KeyIl,

    /// <summary>
    /// Standard terminfo capability <c>kll</c> (<c>key_ll</c>).
    /// </summary>
    KeyLl,

    /// <summary>
    /// Standard terminfo capability <c>khts</c> (<c>key_stab</c>).
    /// </summary>
    KeyStab,

    /// <summary>
    /// Standard terminfo capability <c>lf0</c> (<c>lab_f0</c>).
    /// </summary>
    LabF0,

    /// <summary>
    /// Standard terminfo capability <c>lf1</c> (<c>lab_f1</c>).
    /// </summary>
    LabF1,

    /// <summary>
    /// Standard terminfo capability <c>lf10</c> (<c>lab_f10</c>).
    /// </summary>
    LabF10,

    /// <summary>
    /// Standard terminfo capability <c>lf2</c> (<c>lab_f2</c>).
    /// </summary>
    LabF2,

    /// <summary>
    /// Standard terminfo capability <c>lf3</c> (<c>lab_f3</c>).
    /// </summary>
    LabF3,

    /// <summary>
    /// Standard terminfo capability <c>lf4</c> (<c>lab_f4</c>).
    /// </summary>
    LabF4,

    /// <summary>
    /// Standard terminfo capability <c>lf5</c> (<c>lab_f5</c>).
    /// </summary>
    LabF5,

    /// <summary>
    /// Standard terminfo capability <c>lf6</c> (<c>lab_f6</c>).
    /// </summary>
    LabF6,

    /// <summary>
    /// Standard terminfo capability <c>lf7</c> (<c>lab_f7</c>).
    /// </summary>
    LabF7,

    /// <summary>
    /// Standard terminfo capability <c>lf8</c> (<c>lab_f8</c>).
    /// </summary>
    LabF8,

    /// <summary>
    /// Standard terminfo capability <c>lf9</c> (<c>lab_f9</c>).
    /// </summary>
    LabF9,

    /// <summary>
    /// Standard terminfo capability <c>pad</c> (<c>pad_char</c>).
    /// </summary>
    PadChar,

    /// <summary>
    /// Standard terminfo capability <c>pfkey</c> (<c>pkey_key</c>).
    /// </summary>
    PkeyKey,

    /// <summary>
    /// Standard terminfo capability <c>pfloc</c> (<c>pkey_local</c>).
    /// </summary>
    PkeyLocal,

    /// <summary>
    /// Standard terminfo capability <c>pfx</c> (<c>pkey_xmit</c>).
    /// </summary>
    PkeyXmit,

    /// <summary>
    /// Standard terminfo capability <c>rf</c> (<c>reset_file</c>).
    /// </summary>
    ResetFile,

    /// <summary>
    /// Standard terminfo capability <c>wind</c> (<c>set_window</c>).
    /// </summary>
    SetWindow,

    /// <summary>
    /// Standard terminfo capability <c>tsl</c> (<c>to_status_line</c>).
    /// </summary>
    ToStatusLine,

    /// <summary>
    /// Standard terminfo capability <c>uc</c> (<c>underline_char</c>).
    /// </summary>
    UnderlineChar,

    /// <summary>
    /// Standard terminfo capability <c>hu</c> (<c>up_half_line</c>).
    /// </summary>
    UpHalfLine,

    /// <summary>
    /// Standard terminfo capability <c>iprog</c> (<c>init_prog</c>).
    /// </summary>
    InitProg,

    /// <summary>
    /// Standard terminfo capability <c>mc5p</c> (<c>prtr_non</c>).
    /// </summary>
    PrtrNon,

    /// <summary>
    /// Standard terminfo capability <c>rmp</c> (<c>char_padding</c>).
    /// </summary>
    CharPadding,

    /// <summary>
    /// Standard terminfo capability <c>pln</c> (<c>plab_norm</c>).
    /// </summary>
    PlabNorm,

    /// <summary>
    /// Standard terminfo capability <c>smxon</c> (<c>enter_xon_mode</c>).
    /// </summary>
    EnterXonMode,

    /// <summary>
    /// Standard terminfo capability <c>rmxon</c> (<c>exit_xon_mode</c>).
    /// </summary>
    ExitXonMode,

    /// <summary>
    /// Standard terminfo capability <c>xonc</c> (<c>xon_character</c>).
    /// </summary>
    XonCharacter,

    /// <summary>
    /// Standard terminfo capability <c>xoffc</c> (<c>xoff_character</c>).
    /// </summary>
    XoffCharacter,

    /// <summary>
    /// Standard terminfo capability <c>smln</c> (<c>label_on</c>).
    /// </summary>
    LabelOn,

    /// <summary>
    /// Standard terminfo capability <c>rmln</c> (<c>label_off</c>).
    /// </summary>
    LabelOff,

    /// <summary>
    /// Standard terminfo capability <c>kcan</c> (<c>key_cancel</c>).
    /// </summary>
    KeyCancel,

    /// <summary>
    /// Standard terminfo capability <c>kclo</c> (<c>key_close</c>).
    /// </summary>
    KeyClose,

    /// <summary>
    /// Standard terminfo capability <c>kcmd</c> (<c>key_command</c>).
    /// </summary>
    KeyCommand,

    /// <summary>
    /// Standard terminfo capability <c>kcpy</c> (<c>key_copy</c>).
    /// </summary>
    KeyCopy,

    /// <summary>
    /// Standard terminfo capability <c>kcrt</c> (<c>key_create</c>).
    /// </summary>
    KeyCreate,

    /// <summary>
    /// Standard terminfo capability <c>kext</c> (<c>key_exit</c>).
    /// </summary>
    KeyExit,

    /// <summary>
    /// Standard terminfo capability <c>kmrk</c> (<c>key_mark</c>).
    /// </summary>
    KeyMark,

    /// <summary>
    /// Standard terminfo capability <c>kmsg</c> (<c>key_message</c>).
    /// </summary>
    KeyMessage,

    /// <summary>
    /// Standard terminfo capability <c>kmov</c> (<c>key_move</c>).
    /// </summary>
    KeyMove,

    /// <summary>
    /// Standard terminfo capability <c>knxt</c> (<c>key_next</c>).
    /// </summary>
    KeyNext,

    /// <summary>
    /// Standard terminfo capability <c>kopn</c> (<c>key_open</c>).
    /// </summary>
    KeyOpen,

    /// <summary>
    /// Standard terminfo capability <c>kopt</c> (<c>key_options</c>).
    /// </summary>
    KeyOptions,

    /// <summary>
    /// Standard terminfo capability <c>kprv</c> (<c>key_previous</c>).
    /// </summary>
    KeyPrevious,

    /// <summary>
    /// Standard terminfo capability <c>kprt</c> (<c>key_print</c>).
    /// </summary>
    KeyPrint,

    /// <summary>
    /// Standard terminfo capability <c>kref</c> (<c>key_reference</c>).
    /// </summary>
    KeyReference,

    /// <summary>
    /// Standard terminfo capability <c>krfr</c> (<c>key_refresh</c>).
    /// </summary>
    KeyRefresh,

    /// <summary>
    /// Standard terminfo capability <c>krpl</c> (<c>key_replace</c>).
    /// </summary>
    KeyReplace,

    /// <summary>
    /// Standard terminfo capability <c>krst</c> (<c>key_restart</c>).
    /// </summary>
    KeyRestart,

    /// <summary>
    /// Standard terminfo capability <c>kres</c> (<c>key_resume</c>).
    /// </summary>
    KeyResume,

    /// <summary>
    /// Standard terminfo capability <c>ksav</c> (<c>key_save</c>).
    /// </summary>
    KeySave,

    /// <summary>
    /// Standard terminfo capability <c>kspd</c> (<c>key_suspend</c>).
    /// </summary>
    KeySuspend,

    /// <summary>
    /// Standard terminfo capability <c>kund</c> (<c>key_undo</c>).
    /// </summary>
    KeyUndo,

    /// <summary>
    /// Standard terminfo capability <c>kBEG</c> (<c>key_sbeg</c>).
    /// </summary>
    KeySbeg,

    /// <summary>
    /// Standard terminfo capability <c>kCAN</c> (<c>key_scancel</c>).
    /// </summary>
    KeyScancel,

    /// <summary>
    /// Standard terminfo capability <c>kCMD</c> (<c>key_scommand</c>).
    /// </summary>
    KeyScommand,

    /// <summary>
    /// Standard terminfo capability <c>kCPY</c> (<c>key_scopy</c>).
    /// </summary>
    KeyScopy,

    /// <summary>
    /// Standard terminfo capability <c>kCRT</c> (<c>key_screate</c>).
    /// </summary>
    KeyScreate,

    /// <summary>
    /// Standard terminfo capability <c>kDL</c> (<c>key_sdl</c>).
    /// </summary>
    KeySdl,

    /// <summary>
    /// Standard terminfo capability <c>kEOL</c> (<c>key_seol</c>).
    /// </summary>
    KeySeol,

    /// <summary>
    /// Standard terminfo capability <c>kEXT</c> (<c>key_sexit</c>).
    /// </summary>
    KeySexit,

    /// <summary>
    /// Standard terminfo capability <c>kFND</c> (<c>key_sfind</c>).
    /// </summary>
    KeySfind,

    /// <summary>
    /// Standard terminfo capability <c>kHLP</c> (<c>key_shelp</c>).
    /// </summary>
    KeyShelp,

    /// <summary>
    /// Standard terminfo capability <c>kMSG</c> (<c>key_smessage</c>).
    /// </summary>
    KeySmessage,

    /// <summary>
    /// Standard terminfo capability <c>kMOV</c> (<c>key_smove</c>).
    /// </summary>
    KeySmove,

    /// <summary>
    /// Standard terminfo capability <c>kOPT</c> (<c>key_soptions</c>).
    /// </summary>
    KeySoptions,

    /// <summary>
    /// Standard terminfo capability <c>kPRT</c> (<c>key_sprint</c>).
    /// </summary>
    KeySprint,

    /// <summary>
    /// Standard terminfo capability <c>kRDO</c> (<c>key_sredo</c>).
    /// </summary>
    KeySredo,

    /// <summary>
    /// Standard terminfo capability <c>kRPL</c> (<c>key_sreplace</c>).
    /// </summary>
    KeySreplace,

    /// <summary>
    /// Standard terminfo capability <c>kRES</c> (<c>key_srsume</c>).
    /// </summary>
    KeySrsume,

    /// <summary>
    /// Standard terminfo capability <c>kSAV</c> (<c>key_ssave</c>).
    /// </summary>
    KeySsave,

    /// <summary>
    /// Standard terminfo capability <c>kSPD</c> (<c>key_ssuspend</c>).
    /// </summary>
    KeySsuspend,

    /// <summary>
    /// Standard terminfo capability <c>kUND</c> (<c>key_sundo</c>).
    /// </summary>
    KeySundo,

    /// <summary>
    /// Standard terminfo capability <c>rfi</c> (<c>req_for_input</c>).
    /// </summary>
    ReqForInput,

    /// <summary>
    /// Standard terminfo capability <c>kf25</c> (<c>key_f25</c>).
    /// </summary>
    KeyF25,

    /// <summary>
    /// Standard terminfo capability <c>kf26</c> (<c>key_f26</c>).
    /// </summary>
    KeyF26,

    /// <summary>
    /// Standard terminfo capability <c>kf27</c> (<c>key_f27</c>).
    /// </summary>
    KeyF27,

    /// <summary>
    /// Standard terminfo capability <c>kf28</c> (<c>key_f28</c>).
    /// </summary>
    KeyF28,

    /// <summary>
    /// Standard terminfo capability <c>kf29</c> (<c>key_f29</c>).
    /// </summary>
    KeyF29,

    /// <summary>
    /// Standard terminfo capability <c>kf30</c> (<c>key_f30</c>).
    /// </summary>
    KeyF30,

    /// <summary>
    /// Standard terminfo capability <c>kf31</c> (<c>key_f31</c>).
    /// </summary>
    KeyF31,

    /// <summary>
    /// Standard terminfo capability <c>kf32</c> (<c>key_f32</c>).
    /// </summary>
    KeyF32,

    /// <summary>
    /// Standard terminfo capability <c>kf33</c> (<c>key_f33</c>).
    /// </summary>
    KeyF33,

    /// <summary>
    /// Standard terminfo capability <c>kf34</c> (<c>key_f34</c>).
    /// </summary>
    KeyF34,

    /// <summary>
    /// Standard terminfo capability <c>kf35</c> (<c>key_f35</c>).
    /// </summary>
    KeyF35,

    /// <summary>
    /// Standard terminfo capability <c>kf36</c> (<c>key_f36</c>).
    /// </summary>
    KeyF36,

    /// <summary>
    /// Standard terminfo capability <c>kf37</c> (<c>key_f37</c>).
    /// </summary>
    KeyF37,

    /// <summary>
    /// Standard terminfo capability <c>kf38</c> (<c>key_f38</c>).
    /// </summary>
    KeyF38,

    /// <summary>
    /// Standard terminfo capability <c>kf39</c> (<c>key_f39</c>).
    /// </summary>
    KeyF39,

    /// <summary>
    /// Standard terminfo capability <c>kf40</c> (<c>key_f40</c>).
    /// </summary>
    KeyF40,

    /// <summary>
    /// Standard terminfo capability <c>kf41</c> (<c>key_f41</c>).
    /// </summary>
    KeyF41,

    /// <summary>
    /// Standard terminfo capability <c>kf42</c> (<c>key_f42</c>).
    /// </summary>
    KeyF42,

    /// <summary>
    /// Standard terminfo capability <c>kf43</c> (<c>key_f43</c>).
    /// </summary>
    KeyF43,

    /// <summary>
    /// Standard terminfo capability <c>kf44</c> (<c>key_f44</c>).
    /// </summary>
    KeyF44,

    /// <summary>
    /// Standard terminfo capability <c>kf45</c> (<c>key_f45</c>).
    /// </summary>
    KeyF45,

    /// <summary>
    /// Standard terminfo capability <c>kf46</c> (<c>key_f46</c>).
    /// </summary>
    KeyF46,

    /// <summary>
    /// Standard terminfo capability <c>kf47</c> (<c>key_f47</c>).
    /// </summary>
    KeyF47,

    /// <summary>
    /// Standard terminfo capability <c>kf48</c> (<c>key_f48</c>).
    /// </summary>
    KeyF48,

    /// <summary>
    /// Standard terminfo capability <c>kf49</c> (<c>key_f49</c>).
    /// </summary>
    KeyF49,

    /// <summary>
    /// Standard terminfo capability <c>kf50</c> (<c>key_f50</c>).
    /// </summary>
    KeyF50,

    /// <summary>
    /// Standard terminfo capability <c>kf51</c> (<c>key_f51</c>).
    /// </summary>
    KeyF51,

    /// <summary>
    /// Standard terminfo capability <c>kf52</c> (<c>key_f52</c>).
    /// </summary>
    KeyF52,

    /// <summary>
    /// Standard terminfo capability <c>kf53</c> (<c>key_f53</c>).
    /// </summary>
    KeyF53,

    /// <summary>
    /// Standard terminfo capability <c>kf54</c> (<c>key_f54</c>).
    /// </summary>
    KeyF54,

    /// <summary>
    /// Standard terminfo capability <c>kf55</c> (<c>key_f55</c>).
    /// </summary>
    KeyF55,

    /// <summary>
    /// Standard terminfo capability <c>kf56</c> (<c>key_f56</c>).
    /// </summary>
    KeyF56,

    /// <summary>
    /// Standard terminfo capability <c>kf57</c> (<c>key_f57</c>).
    /// </summary>
    KeyF57,

    /// <summary>
    /// Standard terminfo capability <c>kf58</c> (<c>key_f58</c>).
    /// </summary>
    KeyF58,

    /// <summary>
    /// Standard terminfo capability <c>kf59</c> (<c>key_f59</c>).
    /// </summary>
    KeyF59,

    /// <summary>
    /// Standard terminfo capability <c>kf60</c> (<c>key_f60</c>).
    /// </summary>
    KeyF60,

    /// <summary>
    /// Standard terminfo capability <c>kf61</c> (<c>key_f61</c>).
    /// </summary>
    KeyF61,

    /// <summary>
    /// Standard terminfo capability <c>kf62</c> (<c>key_f62</c>).
    /// </summary>
    KeyF62,

    /// <summary>
    /// Standard terminfo capability <c>kf63</c> (<c>key_f63</c>).
    /// </summary>
    KeyF63,

    /// <summary>
    /// Standard terminfo capability <c>mgc</c> (<c>clear_margins</c>).
    /// </summary>
    ClearMargins,

    /// <summary>
    /// Standard terminfo capability <c>smgl</c> (<c>set_left_margin</c>).
    /// </summary>
    SetLeftMargin,

    /// <summary>
    /// Standard terminfo capability <c>smgr</c> (<c>set_right_margin</c>).
    /// </summary>
    SetRightMargin,

    /// <summary>
    /// Standard terminfo capability <c>fln</c> (<c>label_format</c>).
    /// </summary>
    LabelFormat,

    /// <summary>
    /// Standard terminfo capability <c>sclk</c> (<c>set_clock</c>).
    /// </summary>
    SetClock,

    /// <summary>
    /// Standard terminfo capability <c>dclk</c> (<c>display_clock</c>).
    /// </summary>
    DisplayClock,

    /// <summary>
    /// Standard terminfo capability <c>rmclk</c> (<c>remove_clock</c>).
    /// </summary>
    RemoveClock,

    /// <summary>
    /// Standard terminfo capability <c>cwin</c> (<c>create_window</c>).
    /// </summary>
    CreateWindow,

    /// <summary>
    /// Standard terminfo capability <c>wingo</c> (<c>goto_window</c>).
    /// </summary>
    GotoWindow,

    /// <summary>
    /// Standard terminfo capability <c>hup</c> (<c>hangup</c>).
    /// </summary>
    Hangup,

    /// <summary>
    /// Standard terminfo capability <c>dial</c> (<c>dial_phone</c>).
    /// </summary>
    DialPhone,

    /// <summary>
    /// Standard terminfo capability <c>qdial</c> (<c>quick_dial</c>).
    /// </summary>
    QuickDial,

    /// <summary>
    /// Standard terminfo capability <c>tone</c> (<c>tone</c>).
    /// </summary>
    Tone,

    /// <summary>
    /// Standard terminfo capability <c>pulse</c> (<c>pulse</c>).
    /// </summary>
    Pulse,

    /// <summary>
    /// Standard terminfo capability <c>hook</c> (<c>flash_hook</c>).
    /// </summary>
    FlashHook,

    /// <summary>
    /// Standard terminfo capability <c>pause</c> (<c>fixed_pause</c>).
    /// </summary>
    FixedPause,

    /// <summary>
    /// Standard terminfo capability <c>wait</c> (<c>wait_tone</c>).
    /// </summary>
    WaitTone,

    /// <summary>
    /// Standard terminfo capability <c>u0</c> (<c>user0</c>).
    /// </summary>
    User0,

    /// <summary>
    /// Standard terminfo capability <c>u1</c> (<c>user1</c>).
    /// </summary>
    User1,

    /// <summary>
    /// Standard terminfo capability <c>u2</c> (<c>user2</c>).
    /// </summary>
    User2,

    /// <summary>
    /// Standard terminfo capability <c>u3</c> (<c>user3</c>).
    /// </summary>
    User3,

    /// <summary>
    /// Standard terminfo capability <c>u4</c> (<c>user4</c>).
    /// </summary>
    User4,

    /// <summary>
    /// Standard terminfo capability <c>u5</c> (<c>user5</c>).
    /// </summary>
    User5,

    /// <summary>
    /// Standard terminfo capability <c>u6</c> (<c>user6</c>).
    /// </summary>
    User6,

    /// <summary>
    /// Standard terminfo capability <c>u7</c> (<c>user7</c>).
    /// </summary>
    User7,

    /// <summary>
    /// Standard terminfo capability <c>u8</c> (<c>user8</c>).
    /// </summary>
    User8,

    /// <summary>
    /// Standard terminfo capability <c>u9</c> (<c>user9</c>).
    /// </summary>
    User9,

    /// <summary>
    /// Standard terminfo capability <c>initp</c> (<c>initialize_pair</c>).
    /// </summary>
    InitializePair,

    /// <summary>
    /// Standard terminfo capability <c>scp</c> (<c>set_color_pair</c>).
    /// </summary>
    SetColorPair,

    /// <summary>
    /// Standard terminfo capability <c>cpi</c> (<c>change_char_pitch</c>).
    /// </summary>
    ChangeCharPitch,

    /// <summary>
    /// Standard terminfo capability <c>lpi</c> (<c>change_line_pitch</c>).
    /// </summary>
    ChangeLinePitch,

    /// <summary>
    /// Standard terminfo capability <c>chr</c> (<c>change_res_horz</c>).
    /// </summary>
    ChangeResHorz,

    /// <summary>
    /// Standard terminfo capability <c>cvr</c> (<c>change_res_vert</c>).
    /// </summary>
    ChangeResVert,

    /// <summary>
    /// Standard terminfo capability <c>defc</c> (<c>define_char</c>).
    /// </summary>
    DefineChar,

    /// <summary>
    /// Standard terminfo capability <c>swidm</c> (<c>enter_doublewide_mode</c>).
    /// </summary>
    EnterDoublewideMode,

    /// <summary>
    /// Standard terminfo capability <c>sdrfq</c> (<c>enter_draft_quality</c>).
    /// </summary>
    EnterDraftQuality,

    /// <summary>
    /// Standard terminfo capability <c>slm</c> (<c>enter_leftward_mode</c>).
    /// </summary>
    EnterLeftwardMode,

    /// <summary>
    /// Standard terminfo capability <c>smicm</c> (<c>enter_micro_mode</c>).
    /// </summary>
    EnterMicroMode,

    /// <summary>
    /// Standard terminfo capability <c>snlq</c> (<c>enter_near_letter_quality</c>).
    /// </summary>
    EnterNearLetterQuality,

    /// <summary>
    /// Standard terminfo capability <c>snrmq</c> (<c>enter_normal_quality</c>).
    /// </summary>
    EnterNormalQuality,

    /// <summary>
    /// Standard terminfo capability <c>sshm</c> (<c>enter_shadow_mode</c>).
    /// </summary>
    EnterShadowMode,

    /// <summary>
    /// Standard terminfo capability <c>ssubm</c> (<c>enter_subscript_mode</c>).
    /// </summary>
    EnterSubscriptMode,

    /// <summary>
    /// Standard terminfo capability <c>ssupm</c> (<c>enter_superscript_mode</c>).
    /// </summary>
    EnterSuperscriptMode,

    /// <summary>
    /// Standard terminfo capability <c>sum</c> (<c>enter_upward_mode</c>).
    /// </summary>
    EnterUpwardMode,

    /// <summary>
    /// Standard terminfo capability <c>rwidm</c> (<c>exit_doublewide_mode</c>).
    /// </summary>
    ExitDoublewideMode,

    /// <summary>
    /// Standard terminfo capability <c>rlm</c> (<c>exit_leftward_mode</c>).
    /// </summary>
    ExitLeftwardMode,

    /// <summary>
    /// Standard terminfo capability <c>rmicm</c> (<c>exit_micro_mode</c>).
    /// </summary>
    ExitMicroMode,

    /// <summary>
    /// Standard terminfo capability <c>rshm</c> (<c>exit_shadow_mode</c>).
    /// </summary>
    ExitShadowMode,

    /// <summary>
    /// Standard terminfo capability <c>rsubm</c> (<c>exit_subscript_mode</c>).
    /// </summary>
    ExitSubscriptMode,

    /// <summary>
    /// Standard terminfo capability <c>rsupm</c> (<c>exit_superscript_mode</c>).
    /// </summary>
    ExitSuperscriptMode,

    /// <summary>
    /// Standard terminfo capability <c>rum</c> (<c>exit_upward_mode</c>).
    /// </summary>
    ExitUpwardMode,

    /// <summary>
    /// Standard terminfo capability <c>mhpa</c> (<c>micro_column_address</c>).
    /// </summary>
    MicroColumnAddress,

    /// <summary>
    /// Standard terminfo capability <c>mcud1</c> (<c>micro_down</c>).
    /// </summary>
    MicroDown,

    /// <summary>
    /// Standard terminfo capability <c>mcub1</c> (<c>micro_left</c>).
    /// </summary>
    MicroLeft,

    /// <summary>
    /// Standard terminfo capability <c>mcuf1</c> (<c>micro_right</c>).
    /// </summary>
    MicroRight,

    /// <summary>
    /// Standard terminfo capability <c>mvpa</c> (<c>micro_row_address</c>).
    /// </summary>
    MicroRowAddress,

    /// <summary>
    /// Standard terminfo capability <c>mcuu1</c> (<c>micro_up</c>).
    /// </summary>
    MicroUp,

    /// <summary>
    /// Standard terminfo capability <c>porder</c> (<c>order_of_pins</c>).
    /// </summary>
    OrderOfPins,

    /// <summary>
    /// Standard terminfo capability <c>mcud</c> (<c>parm_down_micro</c>).
    /// </summary>
    ParmDownMicro,

    /// <summary>
    /// Standard terminfo capability <c>mcub</c> (<c>parm_left_micro</c>).
    /// </summary>
    ParmLeftMicro,

    /// <summary>
    /// Standard terminfo capability <c>mcuf</c> (<c>parm_right_micro</c>).
    /// </summary>
    ParmRightMicro,

    /// <summary>
    /// Standard terminfo capability <c>mcuu</c> (<c>parm_up_micro</c>).
    /// </summary>
    ParmUpMicro,

    /// <summary>
    /// Standard terminfo capability <c>scs</c> (<c>select_char_set</c>).
    /// </summary>
    SelectCharSet,

    /// <summary>
    /// Standard terminfo capability <c>smgb</c> (<c>set_bottom_margin</c>).
    /// </summary>
    SetBottomMargin,

    /// <summary>
    /// Standard terminfo capability <c>smgbp</c> (<c>set_bottom_margin_parm</c>).
    /// </summary>
    SetBottomMarginParm,

    /// <summary>
    /// Standard terminfo capability <c>smglp</c> (<c>set_left_margin_parm</c>).
    /// </summary>
    SetLeftMarginParm,

    /// <summary>
    /// Standard terminfo capability <c>smgrp</c> (<c>set_right_margin_parm</c>).
    /// </summary>
    SetRightMarginParm,

    /// <summary>
    /// Standard terminfo capability <c>smgt</c> (<c>set_top_margin</c>).
    /// </summary>
    SetTopMargin,

    /// <summary>
    /// Standard terminfo capability <c>smgtp</c> (<c>set_top_margin_parm</c>).
    /// </summary>
    SetTopMarginParm,

    /// <summary>
    /// Standard terminfo capability <c>sbim</c> (<c>start_bit_image</c>).
    /// </summary>
    StartBitImage,

    /// <summary>
    /// Standard terminfo capability <c>scsd</c> (<c>start_char_set_def</c>).
    /// </summary>
    StartCharSetDef,

    /// <summary>
    /// Standard terminfo capability <c>rbim</c> (<c>stop_bit_image</c>).
    /// </summary>
    StopBitImage,

    /// <summary>
    /// Standard terminfo capability <c>rcsd</c> (<c>stop_char_set_def</c>).
    /// </summary>
    StopCharSetDef,

    /// <summary>
    /// Standard terminfo capability <c>subcs</c> (<c>subscript_characters</c>).
    /// </summary>
    SubscriptCharacters,

    /// <summary>
    /// Standard terminfo capability <c>supcs</c> (<c>superscript_characters</c>).
    /// </summary>
    SuperscriptCharacters,

    /// <summary>
    /// Standard terminfo capability <c>docr</c> (<c>these_cause_cr</c>).
    /// </summary>
    TheseCauseCr,

    /// <summary>
    /// Standard terminfo capability <c>zerom</c> (<c>zero_motion</c>).
    /// </summary>
    ZeroMotion,

    /// <summary>
    /// Standard terminfo capability <c>csnm</c> (<c>char_set_names</c>).
    /// </summary>
    CharSetNames,

    /// <summary>
    /// Standard terminfo capability <c>minfo</c> (<c>mouse_info</c>).
    /// </summary>
    MouseInfo,

    /// <summary>
    /// Standard terminfo capability <c>reqmp</c> (<c>req_mouse_pos</c>).
    /// </summary>
    ReqMousePos,

    /// <summary>
    /// Standard terminfo capability <c>getm</c> (<c>get_mouse</c>).
    /// </summary>
    GetMouse,

    /// <summary>
    /// Standard terminfo capability <c>pfxl</c> (<c>pkey_plab</c>).
    /// </summary>
    PkeyPlab,

    /// <summary>
    /// Standard terminfo capability <c>devt</c> (<c>device_type</c>).
    /// </summary>
    DeviceType,

    /// <summary>
    /// Standard terminfo capability <c>csin</c> (<c>code_set_init</c>).
    /// </summary>
    CodeSetInit,

    /// <summary>
    /// Standard terminfo capability <c>s0ds</c> (<c>set0_des_seq</c>).
    /// </summary>
    Set0DesSeq,

    /// <summary>
    /// Standard terminfo capability <c>s1ds</c> (<c>set1_des_seq</c>).
    /// </summary>
    Set1DesSeq,

    /// <summary>
    /// Standard terminfo capability <c>s2ds</c> (<c>set2_des_seq</c>).
    /// </summary>
    Set2DesSeq,

    /// <summary>
    /// Standard terminfo capability <c>s3ds</c> (<c>set3_des_seq</c>).
    /// </summary>
    Set3DesSeq,

    /// <summary>
    /// Standard terminfo capability <c>smglr</c> (<c>set_lr_margin</c>).
    /// </summary>
    SetLrMargin,

    /// <summary>
    /// Standard terminfo capability <c>smgtb</c> (<c>set_tb_margin</c>).
    /// </summary>
    SetTbMargin,

    /// <summary>
    /// Standard terminfo capability <c>birep</c> (<c>bit_image_repeat</c>).
    /// </summary>
    BitImageRepeat,

    /// <summary>
    /// Standard terminfo capability <c>binel</c> (<c>bit_image_newline</c>).
    /// </summary>
    BitImageNewline,

    /// <summary>
    /// Standard terminfo capability <c>bicr</c> (<c>bit_image_carriage_return</c>).
    /// </summary>
    BitImageCarriageReturn,

    /// <summary>
    /// Standard terminfo capability <c>colornm</c> (<c>color_names</c>).
    /// </summary>
    ColorNames,

    /// <summary>
    /// Standard terminfo capability <c>defbi</c> (<c>define_bit_image_region</c>).
    /// </summary>
    DefineBitImageRegion,

    /// <summary>
    /// Standard terminfo capability <c>endbi</c> (<c>end_bit_image_region</c>).
    /// </summary>
    EndBitImageRegion,

    /// <summary>
    /// Standard terminfo capability <c>setcolor</c> (<c>set_color_band</c>).
    /// </summary>
    SetColorBand,

    /// <summary>
    /// Standard terminfo capability <c>slines</c> (<c>set_page_length</c>).
    /// </summary>
    SetPageLength,

    /// <summary>
    /// Standard terminfo capability <c>dispc</c> (<c>display_pc_char</c>).
    /// </summary>
    DisplayPcChar,

    /// <summary>
    /// Standard terminfo capability <c>smpch</c> (<c>enter_pc_charset_mode</c>).
    /// </summary>
    EnterPcCharsetMode,

    /// <summary>
    /// Standard terminfo capability <c>rmpch</c> (<c>exit_pc_charset_mode</c>).
    /// </summary>
    ExitPcCharsetMode,

    /// <summary>
    /// Standard terminfo capability <c>smsc</c> (<c>enter_scancode_mode</c>).
    /// </summary>
    EnterScancodeMode,

    /// <summary>
    /// Standard terminfo capability <c>rmsc</c> (<c>exit_scancode_mode</c>).
    /// </summary>
    ExitScancodeMode,

    /// <summary>
    /// Standard terminfo capability <c>pctrm</c> (<c>pc_term_options</c>).
    /// </summary>
    PcTermOptions,

    /// <summary>
    /// Standard terminfo capability <c>scesc</c> (<c>scancode_escape</c>).
    /// </summary>
    ScancodeEscape,

    /// <summary>
    /// Standard terminfo capability <c>scesa</c> (<c>alt_scancode_esc</c>).
    /// </summary>
    AltScancodeEsc,

    /// <summary>
    /// Standard terminfo capability <c>ehhlm</c> (<c>enter_horizontal_hl_mode</c>).
    /// </summary>
    EnterHorizontalHlMode,

    /// <summary>
    /// Standard terminfo capability <c>elhlm</c> (<c>enter_left_hl_mode</c>).
    /// </summary>
    EnterLeftHlMode,

    /// <summary>
    /// Standard terminfo capability <c>elohlm</c> (<c>enter_low_hl_mode</c>).
    /// </summary>
    EnterLowHlMode,

    /// <summary>
    /// Standard terminfo capability <c>erhlm</c> (<c>enter_right_hl_mode</c>).
    /// </summary>
    EnterRightHlMode,

    /// <summary>
    /// Standard terminfo capability <c>ethlm</c> (<c>enter_top_hl_mode</c>).
    /// </summary>
    EnterTopHlMode,

    /// <summary>
    /// Standard terminfo capability <c>evhlm</c> (<c>enter_vertical_hl_mode</c>).
    /// </summary>
    EnterVerticalHlMode,

    /// <summary>
    /// Standard terminfo capability <c>sgr1</c> (<c>set_a_attributes</c>).
    /// </summary>
    SetAAttributes,

    /// <summary>
    /// Standard terminfo capability <c>slength</c> (<c>set_pglen_inch</c>).
    /// </summary>
    SetPglenInch,

    /// <summary>
    /// Standard terminfo capability <c>OTi2</c> (<c>termcap_init2</c>).
    /// </summary>
    TermcapInit2,

    /// <summary>
    /// Standard terminfo capability <c>OTrs</c> (<c>termcap_reset</c>).
    /// </summary>
    TermcapReset,

    /// <summary>
    /// Standard terminfo capability <c>OTnl</c> (<c>linefeed_if_not_lf</c>).
    /// </summary>
    LinefeedIfNotLf,

    /// <summary>
    /// Standard terminfo capability <c>OTbc</c> (<c>backspace_if_not_bs</c>).
    /// </summary>
    BackspaceIfNotBs,

    /// <summary>
    /// Standard terminfo capability <c>OTko</c> (<c>other_non_function_keys</c>).
    /// </summary>
    OtherNonFunctionKeys,

    /// <summary>
    /// Standard terminfo capability <c>OTma</c> (<c>arrow_key_map</c>).
    /// </summary>
    ArrowKeyMap,

    /// <summary>
    /// Standard terminfo capability <c>OTG2</c> (<c>acs_ulcorner</c>).
    /// </summary>
    AcsUlcorner,

    /// <summary>
    /// Standard terminfo capability <c>OTG3</c> (<c>acs_llcorner</c>).
    /// </summary>
    AcsLlcorner,

    /// <summary>
    /// Standard terminfo capability <c>OTG1</c> (<c>acs_urcorner</c>).
    /// </summary>
    AcsUrcorner,

    /// <summary>
    /// Standard terminfo capability <c>OTG4</c> (<c>acs_lrcorner</c>).
    /// </summary>
    AcsLrcorner,

    /// <summary>
    /// Standard terminfo capability <c>OTGR</c> (<c>acs_ltee</c>).
    /// </summary>
    AcsLtee,

    /// <summary>
    /// Standard terminfo capability <c>OTGL</c> (<c>acs_rtee</c>).
    /// </summary>
    AcsRtee,

    /// <summary>
    /// Standard terminfo capability <c>OTGU</c> (<c>acs_btee</c>).
    /// </summary>
    AcsBtee,

    /// <summary>
    /// Standard terminfo capability <c>OTGD</c> (<c>acs_ttee</c>).
    /// </summary>
    AcsTtee,

    /// <summary>
    /// Standard terminfo capability <c>OTGH</c> (<c>acs_hline</c>).
    /// </summary>
    AcsHline,

    /// <summary>
    /// Standard terminfo capability <c>OTGV</c> (<c>acs_vline</c>).
    /// </summary>
    AcsVline,

    /// <summary>
    /// Standard terminfo capability <c>OTGC</c> (<c>acs_plus</c>).
    /// </summary>
    AcsPlus,

    /// <summary>
    /// Standard terminfo capability <c>box1</c> (<c>box_chars_1</c>).
    /// </summary>
    BoxChars1,
}
