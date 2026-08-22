using Icod.TermInfo;
using Icod.TermInfo.Sample;

bool describeOnly =
    args.Contains("--describe-only", StringComparer.Ordinal);

TerminalDescription terminal = ResolveTerminal(args);

Console.WriteLine($"Profile: {terminal.Name}");

if (TryResolveSize(terminal, out TerminalSize size, out string source))
{
    Console.WriteLine($"Size ({source}): {size.Columns}x{size.Rows}");
}
else
{
    Console.WriteLine("Size: unknown");
}

DescribeProfile(terminal);

TerminalDatabase customDatabase =
    new(
        new ITerminalDescriptionProvider[]
        {
            new ExampleTerminalDescriptionProvider(),
        });

Console.WriteLine(
    $"Custom provider example available: {customDatabase.TryLoad("example-terminal", out _)}");

if (describeOnly)
{
    Console.WriteLine("Describe-only mode: no terminal-control strings were emitted.");
    return;
}

if (TerminalEnvironment.IsOutputRedirected)
{
    Console.WriteLine(
        "Output is redirected; terminal-control demonstration skipped.");
    return;
}

using IDisposable? windowsVt =
    WindowsVirtualTerminal.TryEnableOutput();

if (OperatingSystem.IsWindows() && (windowsVt is null))
{
    Console.WriteLine(
        "Windows virtual-terminal processing is unavailable; terminal-control demonstration skipped.");
    return;
}

EmitDemonstration(terminal);

static TerminalDescription ResolveTerminal(string[] arguments)
{
    ArgumentNullException.ThrowIfNull(arguments);

    for (int i = 0; i < arguments.Length; i++)
    {
        if (!string.Equals(
                arguments[i],
                "--profile",
                StringComparison.Ordinal))
        {
            continue;
        }

        if (i + 1 >= arguments.Length)
        {
            throw new ArgumentException(
                "--profile requires a built-in terminal name.",
                nameof(arguments));
        }

        return TerminalDatabase.BuiltIn.Load(arguments[i + 1]);
    }

    return TerminalEnvironment.Resolve(
        TerminalDatabase.BuiltIn,
        TerminalProfiles.Dumb);
}

static bool TryResolveSize(
    TerminalDescription terminal,
    out TerminalSize size,
    out string source)
{
    ArgumentNullException.ThrowIfNull(terminal);

    if (TerminalEnvironment.TryGetLiveSize(out size))
    {
        source = "live";
        return true;
    }

    if (TerminalEnvironment.TryGetEnvironmentSize(out size))
    {
        source = "environment";
        return true;
    }

    if (TerminalEnvironment.TryGetProfileSize(terminal, out size))
    {
        source = "profile";
        return true;
    }

    source = string.Empty;
    return false;
}

static void DescribeProfile(TerminalDescription terminal)
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
        string expansion = TerminalColors.ExpandForeground(terminal, index);
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
        terminal.GetString(StringCapability.EnterCursorAddressingMode) is not null
        && terminal.GetString(StringCapability.ExitCursorAddressingMode) is not null;
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

static void EmitDemonstration(TerminalDescription terminal)
{
    ArgumentNullException.ThrowIfNull(terminal);

    string? clear = terminal.GetString(StringCapability.ClearScreen);
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

    string? bold = terminal.GetString(StringCapability.EnterBoldMode);
    string? normal = terminal.GetString(StringCapability.ExitAttributeMode);

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

    Console.Write("Icod.TermInfo terminal-control demonstration");

    if (normal is not null)
    {
        TermInfoOutput.PutP(normal, Console.Out);
    }

    Console.WriteLine();
}

static string FormatNullable(int? value)
{
    return value?.ToString() ?? "absent";
}

static string EscapeForDisplay(string value)
{
    ArgumentNullException.ThrowIfNull(value);

    return value
        .Replace("\u001b", "\\E", StringComparison.Ordinal)
        .Replace("\r", "\\r", StringComparison.Ordinal)
        .Replace("\n", "\\n", StringComparison.Ordinal)
        .Replace("\t", "\\t", StringComparison.Ordinal);
}
