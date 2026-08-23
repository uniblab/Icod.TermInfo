namespace Icod.TermInfo;

internal static class WindowsTerminalProfile
{
    // ncurses terminfo.src revision 1.1267 (2026-08-14), entries
    // ms-terminal, ms-terminal-direct, and ms+terminal.
    internal static TerminalDescription Create()
    {
        return new TerminalDescriptionBuilder("ms-terminal")
            .SetDescription("Windows terminal")
            .ApplyWindowsTerminalInheritedCapabilities()
            .ApplyWindowsTerminalIndexedColor()
            .ApplyWindowsTerminalSourceEntry()
            .Build();
    }

    internal static TerminalDescription CreateDirect()
    {
        return new TerminalDescriptionBuilder("ms-terminal-direct")
            .SetDescription("Windows terminal with direct-colors")
            .ApplyWindowsTerminalInheritedCapabilities()
            .ApplyXtermDirectEightColor()
            .ApplyWindowsTerminalSourceEntry()
            .Build();
    }
}
