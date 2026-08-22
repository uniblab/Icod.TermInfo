namespace Icod.TermInfo;

internal static class XtermIndexedTerminalProfile
{
    // Baseline: ncurses development terminfo.src revision 1.1267
    // (2026-08-14), matching the xterm baseline used by T15/T15¾.
    internal static TerminalDescription Create16Color()
    {
        return new TerminalDescriptionBuilder("xterm-16color")
            .SetDescription("xterm with 16 colors like aixterm")
            .ApplyXtermCommon()
            .ApplyXtermSixteenColor()
            .ApplyXtermKeys()
            .ApplyXtermModernMetadata()
            .Build();
    }

    internal static TerminalDescription Create88Color()
    {
        return new TerminalDescriptionBuilder("xterm-88color")
            .SetDescription("xterm with 88 colors")
            .ApplyXtermCommon()
            .ApplyXtermExtendedIndexed(88, 7744)
            .ApplyXtermPaletteControls()
            .ApplyXtermKeys()
            .ApplyXtermModernMetadata()
            .Build();
    }

    internal static TerminalDescription Create256Color()
    {
        return new TerminalDescriptionBuilder("xterm-256color")
            .SetDescription("xterm with 256 colors")
            .ApplyXtermCommon()
            .ApplyXtermExtendedIndexed(256, 65536)
            .ApplyXtermPaletteControls()
            .ApplyXtermKeys()
            .ApplyXtermModernMetadata()
            .Build();
    }
}
