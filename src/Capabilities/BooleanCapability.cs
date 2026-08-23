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

    /// <summary>
    /// Standard terminfo capability <c>bw</c> (<c>auto_left_margin</c>).
    /// </summary>
    AutoLeftMargin,

    /// <summary>
    /// Standard terminfo capability <c>xsb</c> (<c>no_esc_ctlc</c>).
    /// </summary>
    NoEscCtlc,

    /// <summary>
    /// Standard terminfo capability <c>xhp</c> (<c>ceol_standout_glitch</c>).
    /// </summary>
    CeolStandoutGlitch,

    /// <summary>
    /// Standard terminfo capability <c>eo</c> (<c>erase_overstrike</c>).
    /// </summary>
    EraseOverstrike,

    /// <summary>
    /// Standard terminfo capability <c>hc</c> (<c>hard_copy</c>).
    /// </summary>
    HardCopy,

    /// <summary>
    /// Standard terminfo capability <c>hs</c> (<c>has_status_line</c>).
    /// </summary>
    HasStatusLine,

    /// <summary>
    /// Standard terminfo capability <c>in</c> (<c>insert_null_glitch</c>).
    /// </summary>
    InsertNullGlitch,

    /// <summary>
    /// Standard terminfo capability <c>da</c> (<c>memory_above</c>).
    /// </summary>
    MemoryAbove,

    /// <summary>
    /// Standard terminfo capability <c>db</c> (<c>memory_below</c>).
    /// </summary>
    MemoryBelow,

    /// <summary>
    /// Standard terminfo capability <c>os</c> (<c>over_strike</c>).
    /// </summary>
    OverStrike,

    /// <summary>
    /// Standard terminfo capability <c>eslok</c> (<c>status_line_esc_ok</c>).
    /// </summary>
    StatusLineEscOk,

    /// <summary>
    /// Standard terminfo capability <c>xt</c> (<c>dest_tabs_magic_smso</c>).
    /// </summary>
    DestTabsMagicSmso,

    /// <summary>
    /// Standard terminfo capability <c>hz</c> (<c>tilde_glitch</c>).
    /// </summary>
    TildeGlitch,

    /// <summary>
    /// Standard terminfo capability <c>ul</c> (<c>transparent_underline</c>).
    /// </summary>
    TransparentUnderline,

    /// <summary>
    /// Standard terminfo capability <c>nxon</c> (<c>needs_xon_xoff</c>).
    /// </summary>
    NeedsXonXoff,

    /// <summary>
    /// Standard terminfo capability <c>mc5i</c> (<c>prtr_silent</c>).
    /// </summary>
    PrtrSilent,

    /// <summary>
    /// Standard terminfo capability <c>chts</c> (<c>hard_cursor</c>).
    /// </summary>
    HardCursor,

    /// <summary>
    /// Standard terminfo capability <c>nrrmc</c> (<c>non_rev_rmcup</c>).
    /// </summary>
    NonRevRmcup,

    /// <summary>
    /// Standard terminfo capability <c>ndscr</c> (<c>non_dest_scroll_region</c>).
    /// </summary>
    NonDestScrollRegion,

    /// <summary>
    /// Standard terminfo capability <c>xhpa</c> (<c>col_addr_glitch</c>).
    /// </summary>
    ColAddrGlitch,

    /// <summary>
    /// Standard terminfo capability <c>crxm</c> (<c>cr_cancels_micro_mode</c>).
    /// </summary>
    CrCancelsMicroMode,

    /// <summary>
    /// Standard terminfo capability <c>daisy</c> (<c>has_print_wheel</c>).
    /// </summary>
    HasPrintWheel,

    /// <summary>
    /// Standard terminfo capability <c>xvpa</c> (<c>row_addr_glitch</c>).
    /// </summary>
    RowAddrGlitch,

    /// <summary>
    /// Standard terminfo capability <c>sam</c> (<c>semi_auto_right_margin</c>).
    /// </summary>
    SemiAutoRightMargin,

    /// <summary>
    /// Standard terminfo capability <c>cpix</c> (<c>cpi_changes_res</c>).
    /// </summary>
    CpiChangesRes,

    /// <summary>
    /// Standard terminfo capability <c>lpix</c> (<c>lpi_changes_res</c>).
    /// </summary>
    LpiChangesRes,

    /// <summary>
    /// Standard terminfo capability <c>OTbs</c> (<c>backspaces_with_bs</c>).
    /// </summary>
    BackspacesWithBs,

    /// <summary>
    /// Standard terminfo capability <c>OTns</c> (<c>crt_no_scrolling</c>).
    /// </summary>
    CrtNoScrolling,

    /// <summary>
    /// Standard terminfo capability <c>OTnc</c> (<c>no_correctly_working_cr</c>).
    /// </summary>
    NoCorrectlyWorkingCr,

    /// <summary>
    /// Standard terminfo capability <c>OTMT</c> (<c>gnu_has_meta_key</c>).
    /// </summary>
    GnuHasMetaKey,

    /// <summary>
    /// Standard terminfo capability <c>OTNL</c> (<c>linefeed_is_newline</c>).
    /// </summary>
    LinefeedIsNewline,

    /// <summary>
    /// Standard terminfo capability <c>OTpt</c> (<c>has_hardware_tabs</c>).
    /// </summary>
    HasHardwareTabs,

    /// <summary>
    /// Standard terminfo capability <c>OTxr</c> (<c>return_does_clr_eol</c>).
    /// </summary>
    ReturnDoesClrEol,
}
