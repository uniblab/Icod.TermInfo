using Icod.TermInfo;
using Icod.TermInfo.Sample;

bool describeOnly =
    args.Contains("--describe-only", StringComparer.Ordinal);

TerminalDescription terminal =
    SampleTerminalResolver.Resolve(args);

Console.WriteLine($"Profile: {terminal.Name}");
Console.WriteLine($"Description: {terminal.Description ?? "(none)"}");

SampleDescription.DescribeSemanticCompletionApis(terminal);

if (SampleTerminalResolver.TryResolveSize(
        terminal,
        out TerminalSize size,
        out string source))
{
    Console.WriteLine($"Size ({source}): {size.Columns}x{size.Rows}");
}
else
{
    Console.WriteLine("Size: unknown");
}

SampleDescription.DescribeProfile(terminal);

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
    Console.WriteLine(
        "Describe-only mode: no terminal-control strings were emitted.");
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
        "Windows virtual-terminal processing is unavailable; "
        + "terminal-control demonstration skipped.");
    return;
}

SampleOutput.EmitDemonstration(terminal);
