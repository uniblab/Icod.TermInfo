namespace Icod.TermInfo;

internal static class XtermDirectTerminalProfile
{
    // Baseline: ncurses development terminfo.src revision 1.1267
    // (2026-08-14), matching the xterm baseline used by T15-T16.
    internal static TerminalDescription Create()
    {
        return new TerminalDescriptionBuilder("xterm-direct")
            .ApplyXtermCommon()
            .ApplyXtermDirectEightColor()
            .ApplyXtermKeys()
            .ApplyXtermModernMetadata()
            .Build();
    }

    internal static TerminalDescription Create16Color()
    {
        return new TerminalDescriptionBuilder("xterm-direct16")
            .ApplyXtermCommon()
            .ApplyXtermDirectSixteenColor()
            .ApplyXtermKeys()
            .ApplyXtermModernMetadata()
            .Build();
    }

    internal static TerminalDescription Create256Color()
    {
        return new TerminalDescriptionBuilder("xterm-direct256")
            .ApplyXtermCommon()
            .ApplyXtermDirect256Color()
            .ApplyXtermKeys()
            .ApplyXtermModernMetadata()
            .Build();
    }
}
