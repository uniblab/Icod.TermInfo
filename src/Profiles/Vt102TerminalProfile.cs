namespace Icod.TermInfo;

internal static class Vt102TerminalProfile
{
    internal static TerminalDescription Create()
    {
        return new TerminalDescriptionBuilder("vt102")
            .SetDescription("DEC VT102")
            .ApplyVt100Core()
            .ApplyVt102Editing()
            .Build();
    }
}
