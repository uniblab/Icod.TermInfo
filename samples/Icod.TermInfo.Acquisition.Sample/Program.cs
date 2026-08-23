using Icod.TermInfo;

if (args.Length == 0
    || string.Equals(
        args[0],
        "--help",
        StringComparison.Ordinal)
    || string.Equals(
        args[0],
        "-h",
        StringComparison.Ordinal))
{
    PrintUsage();
    return;
}

try
{
    switch (args[0])
    {
        case "parse":
            RequireArgumentCount(
                args,
                expected: 2);
            Describe(
                "caller-supplied bytes",
                CompiledTermInfoParser.Parse(
                    File.ReadAllBytes(
                        Path.GetFullPath(
                            args[1]))));
            break;

        case "directory":
            RequireArgumentCount(
                args,
                expected: 3);
            LoadFromProvider(
                "explicit directory",
                new DirectoryTerminalDescriptionProvider(
                    args[1]),
                args[2]);
            break;

        case "system":
            RequireArgumentCount(
                args,
                expected: 2);
            LoadFromProvider(
                "system discovery",
                new SystemTerminalDescriptionProvider(),
                args[1]);
            break;

        case "restricted":
            RequireArgumentCount(
                args,
                expected: 2);
            LoadFromProvider(
                "restricted system discovery",
                new SystemTerminalDescriptionProvider(
                    new SystemTerminalDescriptionProviderOptions(
                        useEnvironment: false,
                        useUserDatabase: false,
                        useSystemDatabases: false)),
                args[1]);
            break;

        case "fallback":
            RequireArgumentCount(
                args,
                expected: 2);
            LoadFromProvider(
                "system then built-in fallback",
                new TerminalDatabase(
                    new ITerminalDescriptionProvider[]
                    {
                        new SystemTerminalDescriptionProvider(),
                        TerminalDatabase.BuiltIn,
                    }),
                args[1]);
            break;

        default:
            Console.Error.WriteLine(
                $"Unknown command '{args[0]}'.");
            PrintUsage();
            Environment.ExitCode = 2;
            break;
    }
}
catch (Exception exception)
    when (exception is ArgumentException
        or IOException
        or UnauthorizedAccessException
        or FormatException
        or NotSupportedException
        or KeyNotFoundException)
{
    Console.Error.WriteLine(
        $"{exception.GetType().Name}: {exception.Message}");
    Environment.ExitCode = 2;
}

static void LoadFromProvider(
    string source,
    ITerminalDescriptionProvider provider,
    string name)
{
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(provider);
    ArgumentNullException.ThrowIfNull(name);

    if (!provider.TryLoad(
            name,
            out TerminalDescription? terminal))
    {
        Console.WriteLine(
            $"Source: {source}");
        Console.WriteLine(
            $"Result: clean miss for '{name}'");
        Environment.ExitCode = 1;
        return;
    }

    Describe(
        source,
        terminal);
}

static void Describe(
    string source,
    TerminalDescription terminal)
{
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(terminal);

    Console.WriteLine(
        $"Source: {source}");
    Console.WriteLine(
        $"Name: {terminal.Name}");
    Console.WriteLine(
        $"Description: {terminal.Description ?? "(none)"}");
    Console.WriteLine(
        $"Aliases: {FormatAliases(terminal.Aliases)}");
    Console.WriteLine(
        $"Columns: {FormatNumber(terminal.GetNumber(NumericCapability.Columns))}");
    Console.WriteLine(
        $"Lines: {FormatNumber(terminal.GetNumber(NumericCapability.Lines))}");
    Console.WriteLine(
        $"Colors: {FormatNumber(terminal.GetNumber(NumericCapability.Colors))}");
    Console.WriteLine(
        $"Standard booleans: {terminal.BooleanCapabilities.Count}");
    Console.WriteLine(
        $"Standard numerics: {terminal.NumericCapabilities.Count}");
    Console.WriteLine(
        $"Standard strings: {terminal.StringCapabilities.Count}");
    Console.WriteLine(
        $"Extended capabilities: {terminal.ExtendedCapabilities.Count}");
}

static string FormatAliases(
    IReadOnlyList<string> aliases)
{
    ArgumentNullException.ThrowIfNull(aliases);

    if (aliases.Count == 0)
    {
        return "(none)";
    }

    return string.Join(
        ", ",
        aliases);
}

static string FormatNumber(
    int? value)
{
    return value?.ToString()
        ?? "(absent)";
}

static void RequireArgumentCount(
    string[] arguments,
    int expected)
{
    ArgumentNullException.ThrowIfNull(arguments);

    if (arguments.Length != expected)
    {
        throw new ArgumentException(
            $"Command '{arguments[0]}' expects {expected - 1} argument(s).");
    }
}

static void PrintUsage()
{
    Console.WriteLine(
        "Icod.TermInfo compiled-database acquisition sample");
    Console.WriteLine();
    Console.WriteLine(
        "Commands:");
    Console.WriteLine(
        "  parse <compiled-file>");
    Console.WriteLine(
        "  directory <root> <terminal-name>");
    Console.WriteLine(
        "  system <terminal-name>");
    Console.WriteLine(
        "  restricted <terminal-name>");
    Console.WriteLine(
        "  fallback <terminal-name>");
}
