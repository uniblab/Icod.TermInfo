using Icod.TermInfo;
using Icod.TermInfo.Sample;

TerminalDescription terminal =
    TerminalEnvironment.Resolve(
        TerminalDatabase.BuiltIn,
        TerminalProfiles.Dumb);

Console.WriteLine($"Profile: {terminal.Name}");

if (TryResolveSize(terminal, out TerminalSize size, out string source))
{
    Console.WriteLine($"Size ({source}): {size.Columns}x{size.Rows}");
}
else
{
    Console.WriteLine("Size: unknown");
}

TerminalDatabase customDatabase =
    new(
        new ITerminalDescriptionProvider[]
        {
            new ExampleTerminalDescriptionProvider(),
        });

Console.WriteLine(
    $"Custom provider example available: {customDatabase.TryLoad("example-terminal", out _)}");

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

static void EmitDemonstration(TerminalDescription terminal)
{
    ArgumentNullException.ThrowIfNull(terminal);

    string? clear = terminal.GetString(StringCapability.ClearScreen);
    if (clear is not null)
    {
        TermInfoOutput.PutP(clear, Console.Out);
    }

    string? cursorAddress =
        terminal.GetString(StringCapability.CursorAddress);
    if (cursorAddress is not null)
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

    int? colors = terminal.GetNumber(NumericCapability.Colors);
    string? setForeground =
        terminal.GetString(StringCapability.SetForegroundColor);

    if ((colors is not null)
        && (colors.Value >= 8)
        && (setForeground is not null))
    {
        string red =
            terminal.Expand(
                StringCapability.SetForegroundColor,
                1);

        TermInfoOutput.PutP(red, Console.Out);
    }

    Console.Write("Icod.TermInfo terminal-control demonstration");

    if (normal is not null)
    {
        TermInfoOutput.PutP(normal, Console.Out);
    }

    Console.WriteLine();
}
