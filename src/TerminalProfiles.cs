namespace Icod.TermInfo;

/// <summary>
/// Provides the terminal profiles built into <c>Icod.TermInfo</c>.
/// </summary>
public static class TerminalProfiles
{
    /// <summary>
    /// Gets the lowest-common-denominator <c>dumb</c> terminal profile.
    /// </summary>
    public static TerminalDescription Dumb { get; } =
        DumbTerminalProfile.Create();
}
