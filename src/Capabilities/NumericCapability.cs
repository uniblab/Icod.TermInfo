namespace Icod.TermInfo;

/// <summary>
/// Identifies a numeric terminal capability.
/// </summary>
public enum NumericCapability
{
    /// <summary>
    /// The terminal profile's default column count (<c>cols</c>).
    /// </summary>
    Columns,

    /// <summary>
    /// The terminal profile's default line count (<c>lines</c>).
    /// </summary>
    Lines,

    /// <summary>
    /// The number of supported colors (<c>colors</c>).
    /// </summary>
    Colors,

    /// <summary>
    /// The number of supported color pairs (<c>pairs</c>).
    /// </summary>
    ColorPairs,

    /// <summary>
    /// The number of spaces between hardware tab stops (<c>it</c>).
    /// </summary>
    InitialTabWidth,

    /// <summary>
    /// The terminal's virtual-terminal number (<c>vt</c>).
    /// </summary>
    VirtualTerminal,

    /// <summary>
    /// Bit mask of video attributes unavailable while colors are active
    /// (<c>ncv</c>).
    /// </summary>
    NoColorVideo,

    /// <summary>
    /// Standard terminfo capability <c>lm</c> (<c>lines_of_memory</c>).
    /// </summary>
    LinesOfMemory,

    /// <summary>
    /// Standard terminfo capability <c>xmc</c> (<c>magic_cookie_glitch</c>).
    /// </summary>
    MagicCookieGlitch,

    /// <summary>
    /// Standard terminfo capability <c>pb</c> (<c>padding_baud_rate</c>).
    /// </summary>
    PaddingBaudRate,

    /// <summary>
    /// Standard terminfo capability <c>wsl</c> (<c>width_status_line</c>).
    /// </summary>
    WidthStatusLine,

    /// <summary>
    /// Standard terminfo capability <c>nlab</c> (<c>num_labels</c>).
    /// </summary>
    NumLabels,

    /// <summary>
    /// Standard terminfo capability <c>lh</c> (<c>label_height</c>).
    /// </summary>
    LabelHeight,

    /// <summary>
    /// Standard terminfo capability <c>lw</c> (<c>label_width</c>).
    /// </summary>
    LabelWidth,

    /// <summary>
    /// Standard terminfo capability <c>ma</c> (<c>max_attributes</c>).
    /// </summary>
    MaxAttributes,

    /// <summary>
    /// Standard terminfo capability <c>wnum</c> (<c>maximum_windows</c>).
    /// </summary>
    MaximumWindows,

    /// <summary>
    /// Standard terminfo capability <c>bufsz</c> (<c>buffer_capacity</c>).
    /// </summary>
    BufferCapacity,

    /// <summary>
    /// Standard terminfo capability <c>spinv</c> (<c>dot_vert_spacing</c>).
    /// </summary>
    DotVertSpacing,

    /// <summary>
    /// Standard terminfo capability <c>spinh</c> (<c>dot_horz_spacing</c>).
    /// </summary>
    DotHorzSpacing,

    /// <summary>
    /// Standard terminfo capability <c>maddr</c> (<c>max_micro_address</c>).
    /// </summary>
    MaxMicroAddress,

    /// <summary>
    /// Standard terminfo capability <c>mjump</c> (<c>max_micro_jump</c>).
    /// </summary>
    MaxMicroJump,

    /// <summary>
    /// Standard terminfo capability <c>mcs</c> (<c>micro_col_size</c>).
    /// </summary>
    MicroColSize,

    /// <summary>
    /// Standard terminfo capability <c>mls</c> (<c>micro_line_size</c>).
    /// </summary>
    MicroLineSize,

    /// <summary>
    /// Standard terminfo capability <c>npins</c> (<c>number_of_pins</c>).
    /// </summary>
    NumberOfPins,

    /// <summary>
    /// Standard terminfo capability <c>orc</c> (<c>output_res_char</c>).
    /// </summary>
    OutputResChar,

    /// <summary>
    /// Standard terminfo capability <c>orl</c> (<c>output_res_line</c>).
    /// </summary>
    OutputResLine,

    /// <summary>
    /// Standard terminfo capability <c>orhi</c> (<c>output_res_horz_inch</c>).
    /// </summary>
    OutputResHorzInch,

    /// <summary>
    /// Standard terminfo capability <c>orvi</c> (<c>output_res_vert_inch</c>).
    /// </summary>
    OutputResVertInch,

    /// <summary>
    /// Standard terminfo capability <c>cps</c> (<c>print_rate</c>).
    /// </summary>
    PrintRate,

    /// <summary>
    /// Standard terminfo capability <c>widcs</c> (<c>wide_char_size</c>).
    /// </summary>
    WideCharSize,

    /// <summary>
    /// Standard terminfo capability <c>btns</c> (<c>buttons</c>).
    /// </summary>
    Buttons,

    /// <summary>
    /// Standard terminfo capability <c>bitwin</c> (<c>bit_image_entwining</c>).
    /// </summary>
    BitImageEntwining,

    /// <summary>
    /// Standard terminfo capability <c>bitype</c> (<c>bit_image_type</c>).
    /// </summary>
    BitImageType,

    /// <summary>
    /// Standard terminfo capability <c>OTug</c> (<c>magic_cookie_glitch_ul</c>).
    /// </summary>
    MagicCookieGlitchUl,

    /// <summary>
    /// Standard terminfo capability <c>OTdC</c> (<c>carriage_return_delay</c>).
    /// </summary>
    CarriageReturnDelay,

    /// <summary>
    /// Standard terminfo capability <c>OTdN</c> (<c>new_line_delay</c>).
    /// </summary>
    NewLineDelay,

    /// <summary>
    /// Standard terminfo capability <c>OTdB</c> (<c>backspace_delay</c>).
    /// </summary>
    BackspaceDelay,

    /// <summary>
    /// Standard terminfo capability <c>OTdT</c> (<c>horizontal_tab_delay</c>).
    /// </summary>
    HorizontalTabDelay,

    /// <summary>
    /// Standard terminfo capability <c>OTkn</c> (<c>number_of_function_keys</c>).
    /// </summary>
    NumberOfFunctionKeys,
}
