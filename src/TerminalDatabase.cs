using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo;

/// <summary>
/// Resolves terminal descriptions from an ordered set of providers.
/// </summary>
public sealed class TerminalDatabase
{
    private readonly IReadOnlyList<ITerminalDescriptionProvider> _providers;

    /// <summary>
    /// Initializes a database from providers consulted in the supplied order.
    /// </summary>
    public TerminalDatabase(
        IEnumerable<ITerminalDescriptionProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);

        ITerminalDescriptionProvider[] providerArray = providers.ToArray();
        for (int i = 0; i < providerArray.Length; i++)
        {
            if (providerArray[i] is null)
            {
                throw new ArgumentException(
                    "Terminal providers cannot contain null entries.",
                    nameof(providers));
            }
        }

        _providers = Array.AsReadOnly(providerArray);
    }

    /// <summary>
    /// Gets the immutable database of profiles supplied with the package.
    /// </summary>
    public static TerminalDatabase BuiltIn { get; } =
        new(
            new ITerminalDescriptionProvider[]
            {
                new InMemoryTerminalDescriptionProvider(
                    new[]
                    {
                        TerminalProfiles.Xterm256Color,
                        TerminalProfiles.Xterm88Color,
                        TerminalProfiles.Xterm16Color,
                        TerminalProfiles.Xterm,
                        TerminalProfiles.Vt220,
                        TerminalProfiles.Vt102,
                        TerminalProfiles.Ansi,
                        TerminalProfiles.Vt100,
                        TerminalProfiles.Dumb,
                    }),
            });

    /// <summary>
    /// Loads a terminal profile by canonical name or alias.
    /// </summary>
    /// <exception cref="KeyNotFoundException">
    /// No configured provider has the requested terminal profile.
    /// </exception>
    public TerminalDescription Load(string name)
    {
        ValidateTerminalName(name);

        if (TryLoad(name, out TerminalDescription? terminal))
        {
            return terminal;
        }

        throw new KeyNotFoundException(
            $"Terminal profile '{name}' is not available.");
    }

    /// <summary>
    /// Attempts to load a terminal profile by canonical name or alias.
    /// </summary>
    /// <remarks>
    /// Providers are consulted in constructor order. The first provider which
    /// resolves the requested name wins.
    /// </remarks>
    public bool TryLoad(
        string name,
        [NotNullWhen(true)] out TerminalDescription? terminal)
    {
        ValidateTerminalName(name);

        foreach (ITerminalDescriptionProvider provider in _providers)
        {
            if (provider.TryLoad(name, out terminal))
            {
                if (terminal is null)
                {
                    throw new InvalidOperationException(
                        $"Terminal provider '{provider.GetType().FullName}' returned success without a terminal description.");
                }

                return true;
            }
        }

        terminal = null;
        return false;
    }

    /// <summary>
    /// Resolves a requested name and returns an explicit fallback if the name is
    /// absent or unsupported.
    /// </summary>
    public TerminalDescription Resolve(
        string? requestedName,
        TerminalDescription fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);

        if (string.IsNullOrWhiteSpace(requestedName))
        {
            return fallback;
        }

        if (TryLoad(requestedName, out TerminalDescription? terminal))
        {
            return terminal;
        }

        return fallback;
    }

    private static void ValidateTerminalName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "The terminal name cannot be empty or whitespace.",
                nameof(name));
        }
    }
}
