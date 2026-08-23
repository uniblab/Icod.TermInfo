namespace Icod.TermInfo.Sample;

internal static class SampleTerminalResolver
{
    private static readonly TerminalDatabase DefaultDatabase =
        new(
            new ITerminalDescriptionProvider[]
            {
                new SystemTerminalDescriptionProvider(),
                TerminalDatabase.BuiltIn,
            });

    internal static TerminalDescription Resolve(string[] arguments)
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
            DefaultDatabase,
            TerminalProfiles.Dumb);
    }

    internal static bool TryResolveSize(
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
}
