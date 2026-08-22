namespace Icod.TermInfo;

internal static class XtermTerminalProfile
{
    // Baseline: ncurses development terminfo.src revision 1.1267
    // (2026-08-14), where xterm -> xterm-new -> xterm-p370.
    internal static TerminalDescription Create()
    {
        return new TerminalDescriptionBuilder("xterm")
            .ApplyXtermCommon()
            .ApplyXtermBasicEightColor()
            .ApplyXtermKeys()
            .ApplyXtermModernMetadata()
            .Build();
    }
}
