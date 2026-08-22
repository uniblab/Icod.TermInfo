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
    /// Gets the lowest-common-denominator <c>dumb</c> terminal profile.
    /// </summary>
    public static TerminalDescription Dumb { get; } =
        DumbTerminalProfile.Create();
}
