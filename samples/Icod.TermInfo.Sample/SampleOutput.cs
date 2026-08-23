namespace Icod.TermInfo.Sample;

internal static class SampleOutput
{
    internal static void EmitDemonstration(TerminalDescription terminal)
    {
        ArgumentNullException.ThrowIfNull(terminal);

        string? clear =
            terminal.GetString(StringCapability.ClearScreen);
        if (clear is not null)
        {
            TermInfoOutput.PutP(clear, Console.Out);
        }

        if (terminal.GetString(StringCapability.CursorAddress) is not null)
        {
            string move =
                terminal.Expand(
                    StringCapability.CursorAddress,
                    0,
                    0);

            TermInfoOutput.PutP(move, Console.Out);
        }

        string? bold =
            terminal.GetString(StringCapability.EnterBoldMode);
        string? normal =
            terminal.GetString(StringCapability.ExitAttributeMode);

        if (bold is not null)
        {
            TermInfoOutput.PutP(bold, Console.Out);
        }

        TerminalColorSupport color =
            TerminalColors.GetColorSupport(terminal);

        if (color.Model == TerminalColorModel.DirectRgb
            && color.HasForegroundSelector)
        {
            TermInfoOutput.PutP(
                TerminalColors.ExpandForeground(
                    terminal,
                    new TerminalRgbColor(0x80, 0x40, 0xC0)),
                Console.Out);
        }
        else if (color.IndexedColorCount >= 8
            && color.HasForegroundSelector)
        {
            TermInfoOutput.PutP(
                TerminalColors.ExpandForeground(terminal, 1),
                Console.Out);
        }

        Console.Write(
            "Icod.TermInfo terminal-control demonstration");

        if (normal is not null)
        {
            TermInfoOutput.PutP(normal, Console.Out);
        }

        Console.WriteLine();
    }
}
