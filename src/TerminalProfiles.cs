namespace Icod.TermInfo;

/// <summary>
/// Provides the terminal profiles built into <c>Icod.TermInfo</c>.
/// </summary>
public static class TerminalProfiles
{
    /// <summary>
    /// Gets the selected modern <c>xterm</c> core profile.
    /// </summary>
    public static TerminalDescription Xterm { get; } =
        XtermTerminalProfile.Create();

    /// <summary>
    /// Gets the modern <c>xterm-16color</c> indexed-color profile.
    /// </summary>
    public static TerminalDescription Xterm16Color { get; } =
        XtermIndexedTerminalProfile.Create16Color();

    /// <summary>
    /// Gets the modern <c>xterm-88color</c> indexed-color profile.
    /// </summary>
    public static TerminalDescription Xterm88Color { get; } =
        XtermIndexedTerminalProfile.Create88Color();

    /// <summary>
    /// Gets the modern <c>xterm-256color</c> indexed-color profile.
    /// </summary>
    public static TerminalDescription Xterm256Color { get; } =
        XtermIndexedTerminalProfile.Create256Color();

    /// <summary>
    /// Gets the color-capable ANSI/PC-terminal profile.
    /// </summary>
    public static TerminalDescription Ansi { get; } =
        AnsiTerminalProfile.Create();

    /// <summary>
    /// Gets the DEC VT100 profile with the advanced-video option.
    /// </summary>
    public static TerminalDescription Vt100 { get; } =
        Vt100TerminalProfile.Create();

    /// <summary>
    /// Gets the canonical DEC VT102 profile.
    /// </summary>
    public static TerminalDescription Vt102 { get; } =
        Vt102TerminalProfile.Create();

    /// <summary>
    /// Gets the canonical seven-bit DEC VT220 profile.
    /// </summary>
    public static TerminalDescription Vt220 { get; } =
        Vt220TerminalProfile.Create();

    /// <summary>
    /// Gets the lowest-common-denominator <c>dumb</c> terminal profile.
    /// </summary>
    public static TerminalDescription Dumb { get; } =
        DumbTerminalProfile.Create();
}
