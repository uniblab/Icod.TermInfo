namespace Icod.TermInfo;

internal static class DumbTerminalProfile
{
    internal static TerminalDescription Create()
    {
        return new TerminalDescription(
            name: "dumb",
            aliases: Array.Empty<string>(),
            booleanCapabilities:
            [
                BooleanCapability.AutoRightMargin,
            ],
            numericCapabilities: new Dictionary<NumericCapability, int>
            {
                [NumericCapability.Columns] = 80,
            },
            stringCapabilities: new Dictionary<StringCapability, string>
            {
                [StringCapability.Bell] = "\a",
                [StringCapability.CarriageReturn] = "\r",
                [StringCapability.CursorDownOne] = "\n",
                [StringCapability.ScrollForward] = "\n",
            });
    }
}
