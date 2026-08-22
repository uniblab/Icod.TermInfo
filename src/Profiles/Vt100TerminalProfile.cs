namespace Icod.TermInfo;

internal static class Vt100TerminalProfile
{
    internal static TerminalDescription Create()
    {
        return new TerminalDescriptionBuilder("vt100")
            .SetDescription("DEC VT100 (w/advanced video)")
            .AddAlias("vt100-am")
            .ApplyVt100Core()
            .Build();
    }
}
