using Icod.TermInfo;

TerminalDescription terminal = TerminalDatabase.BuiltIn.Load("dumb");

Console.WriteLine($"Profile: {terminal.Name}");
Console.WriteLine(
    $"Columns: {terminal.GetNumber(NumericCapability.Columns)?.ToString() ?? "unknown"}");

string expanded = TermInfoParameterExpander.Expand(
    "cursor example: ESC[%i%p1%d;%p2%dH",
    4,
    12);

Console.WriteLine(expanded);
