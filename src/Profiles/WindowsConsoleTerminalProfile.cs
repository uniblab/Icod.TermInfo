namespace Icod.TermInfo;

internal static class WindowsConsoleTerminalProfile
{
    // ncurses terminfo.src revision 1.1267 (2026-08-14), entry winconsole.
    internal static TerminalDescription Create()
    {
        return new TerminalDescriptionBuilder("winconsole")
            .SetDescription("Windows 10 new console")
            .ApplyWindowsConsoleInheritedCapabilities()
            .ApplyWindowsConsoleSourceEntry()
            .Build();
    }
}
