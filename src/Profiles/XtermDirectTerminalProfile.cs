namespace Icod.TermInfo;

internal static class XtermDirectTerminalProfile
{
    // Baseline: ncurses development terminfo.src revision 1.1267
    // (2026-08-14), matching the xterm baseline used by T15-T16.
    internal static TerminalDescription Create()
    {
        return new TerminalDescriptionBuilder("xterm-direct")
            .SetDescription("xterm with direct-color indexing")
            .ApplyXtermCommon()
            .ApplyXtermDirectEightColor()
            .ApplyXtermKeys()
            .ApplyXtermModernMetadata()
            .Build();
    }

    internal static TerminalDescription Create16Color()
    {
        return new TerminalDescriptionBuilder("xterm-direct16")
            .SetDescription("xterm with direct-colors and 16 indexed colors")
            .ApplyXtermCommon()
            .ApplyXtermDirectSixteenColor()
            .ApplyXtermKeys()
            .ApplyXtermModernMetadata()
            .Build();
    }

    internal static TerminalDescription Create256Color()
    {
        return new TerminalDescriptionBuilder("xterm-direct256")
            .SetDescription("xterm with direct-colors and 256 indexed colors")
            .ApplyXtermCommon()
            .ApplyXtermDirect256Color()
            .ApplyXtermKeys()
            .ApplyXtermModernMetadata()
            .Build();
    }
}
