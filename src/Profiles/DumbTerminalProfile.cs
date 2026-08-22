namespace Icod.TermInfo;

internal static class DumbTerminalProfile
{
    internal static TerminalDescription Create()
    {
        return new TerminalDescriptionBuilder("dumb")
            .SetBoolean(BooleanCapability.AutoRightMargin)
            .SetNumber(NumericCapability.Columns, 80)
            .SetString(StringCapability.Bell, "\a")
            .SetString(StringCapability.CarriageReturn, "\r")
            .SetString(StringCapability.CursorDownOne, "\n")
            .SetString(StringCapability.ScrollForward, "\n")
            .Build();
    }
}
