namespace Icod.TermInfo;

internal static class Vt220TerminalProfile
{
    // Baseline: ncurses development terminfo.src revision 1.1267
    // (2026-08-14), canonical 7-bit vt220/vt200.
    internal static TerminalDescription Create()
    {
        return new TerminalDescriptionBuilder("vt220")
            .AddAlias("vt200")
            .ApplyVt220Core()
            .ApplyVt220CursorVisibility()
            .Build();
    }
}
