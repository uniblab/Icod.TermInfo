using System.Text;

namespace Icod.TermInfo.Sample;

internal static class SampleDescription
{
    internal static void DescribeSemanticCompletionApis(
        TerminalDescription terminal)
    {
        ArgumentNullException.ThrowIfNull(terminal);

        Console.WriteLine(
            $"Standard catalog: booleans={StandardCapabilityCatalog.BooleanCapabilities.Count}, numerics={StandardCapabilityCatalog.NumericCapabilities.Count}, strings={StandardCapabilityCatalog.StringCapabilities.Count}");
        Console.WriteLine(
            $"Present standard capabilities: booleans={terminal.BooleanCapabilities.Count}, numerics={terminal.NumericCapabilities.Count}, strings={terminal.StringCapabilities.Count}; extended={terminal.ExtendedCapabilities.Count}");

        StandardCapabilityMetadata<StringCapability> cupMetadata =
            StandardCapabilityCatalog.GetMetadata(
                StringCapability.CursorAddress);
        Console.WriteLine(
            $"Catalog sample: {cupMetadata.ShortName}/{cupMetadata.LongName}, binary-index={cupMetadata.BinaryIndex}");

        TermInfoParameterProgram reusableProgram =
            TermInfoParameterProgram.Parse("%p1%{1}%+%d");
        Console.WriteLine(
            $"Reusable parameter program sample: 41 -> {reusableProgram.Expand(41)}");

        if (terminal.TryGetExtendedString("XM", out _))
        {
            string mouseEnable =
                terminal.ExpandExtendedString("XM", 1);
            Console.WriteLine(
                $"Extended expansion sample XM(1): {EscapeForDisplay(mouseEnable)}");
        }

        using MemoryStream byteStream = new();
        TermInfoOutput.TPuts(
            "\u0080",
            affectedLines: 1,
            byteStream,
            Encoding.Latin1);
        Console.WriteLine(
            $"Latin-1 capability byte sample: 0x{byteStream.ToArray()[0]:X2}");

        TerminalDescription xonExample =
            new TerminalDescriptionBuilder("sample-xon")
                .SetBoolean(BooleanCapability.XonXoff)
                .Build();
        using StringWriter paddingWriter = new();
        TermInfoOutput.TPuts(
            "before$<1>after",
            affectedLines: 1,
            paddingWriter,
            new TermInfoOutputOptions(xonExample));
        Console.WriteLine(
            $"Terminal-aware padding sample: {paddingWriter}");

        TerminalDescription winConsole =
            TerminalDatabase.BuiltIn.Load("winconsole");
        TerminalDescription windowsTerminal =
            TerminalDatabase.BuiltIn.Load("ms-terminal");
        TerminalDescription windowsTerminalDirect =
            TerminalDatabase.BuiltIn.Load("ms-terminal-direct");

        Console.WriteLine(
            $"Windows profiles: {winConsole.Name}, {windowsTerminal.Name}, {windowsTerminalDirect.Name} ({TerminalColors.GetColorSupport(windowsTerminalDirect).Model})");
    }

    internal static void DescribeProfile(TerminalDescription terminal)
    {
        ArgumentNullException.ThrowIfNull(terminal);

        TerminalColorSupport color =
            TerminalColors.GetColorSupport(terminal);

        Console.WriteLine(
            $"Color: {color.Model} / {color.Tier}; raw colors={FormatNullable(color.ColorCount)}; indexed={color.IndexedColorCount}; pairs={FormatNullable(color.ColorPairCount)}");

        if (color.Model == TerminalColorModel.Indexed
            && color.IndexedColorCount > 0
            && color.HasForegroundSelector)
        {
            int index = Math.Min(1, color.IndexedColorCount - 1);
            string expansion =
                TerminalColors.ExpandForeground(terminal, index);
            Console.WriteLine(
                $"Indexed foreground sample: {EscapeForDisplay(expansion)}");
        }
        else if (color.Model == TerminalColorModel.DirectRgb
            && color.HasForegroundSelector)
        {
            string expansion =
                TerminalColors.ExpandForeground(
                    terminal,
                    new TerminalRgbColor(0x12, 0x34, 0x56));
            Console.WriteLine(
                $"Direct RGB foreground sample: {EscapeForDisplay(expansion)}");
        }

        bool hasFullScreenPrimitives =
            terminal.GetString(
                StringCapability.EnterCursorAddressingMode) is not null
            && terminal.GetString(
                StringCapability.ExitCursorAddressingMode) is not null;
        bool hasCursorVisibility =
            terminal.GetString(StringCapability.CursorInvisible) is not null
            && terminal.GetString(StringCapability.CursorNormal) is not null;

        Console.WriteLine(
            $"Cursor-addressing lifecycle primitives: {hasFullScreenPrimitives}");
        Console.WriteLine(
            $"Cursor-visibility primitives: {hasCursorVisibility}");

        bool hasBracketedPaste =
            terminal.TryGetExtendedString("BE", out _)
            && terminal.TryGetExtendedString("BD", out _)
            && terminal.TryGetExtendedString("PS", out _)
            && terminal.TryGetExtendedString("PE", out _);
        bool hasFocus =
            terminal.TryGetExtendedString("fe", out _)
            && terminal.TryGetExtendedString("fd", out _);
        bool hasMouse =
            terminal.GetString(StringCapability.KeyMouse) is not null
            && terminal.TryGetExtendedString("XM", out _)
            && terminal.TryGetExtendedString("xm", out _);

        Console.WriteLine(
            $"Descriptive metadata: mouse={hasMouse}, focus={hasFocus}, bracketed-paste={hasBracketedPaste}");
    }

    private static string FormatNullable(int? value)
    {
        return value?.ToString() ?? "absent";
    }

    private static string EscapeForDisplay(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value
            .Replace("\u001b", "\\E", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);
    }
}
