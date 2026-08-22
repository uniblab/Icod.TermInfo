using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo;

/// <summary>
/// Resolves terminal descriptions by canonical name or alias.
/// </summary>
public sealed class TerminalDatabase
{
    private readonly IReadOnlyDictionary<string, TerminalDescription> _terminals;

    private TerminalDatabase(
        IEnumerable<TerminalDescription> terminals)
    {
        ArgumentNullException.ThrowIfNull(terminals);

        Dictionary<string, TerminalDescription> byName =
            new(StringComparer.Ordinal);

        foreach (TerminalDescription terminal in terminals)
        {
            ArgumentNullException.ThrowIfNull(terminal);

            AddName(byName, terminal.Name, terminal);

            foreach (string alias in terminal.Aliases)
            {
                AddName(byName, alias, terminal);
            }
        }

        _terminals = byName;
    }

    /// <summary>
    /// Gets the immutable database of profiles supplied with the package.
    /// </summary>
    public static TerminalDatabase BuiltIn { get; } =
        new(
        [
            TerminalProfiles.Dumb,
        ]);

    /// <summary>
    /// Loads a terminal profile by canonical name or alias.
    /// </summary>
    /// <exception cref="KeyNotFoundException">
    /// No built-in terminal profile has the requested name.
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
    public bool TryLoad(
        string name,
        [NotNullWhen(true)] out TerminalDescription? terminal)
    {
        ValidateTerminalName(name);

        return _terminals.TryGetValue(name, out terminal);
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

    private static void AddName(
        IDictionary<string, TerminalDescription> terminals,
        string name,
        TerminalDescription terminal)
    {
        ArgumentNullException.ThrowIfNull(terminals);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(terminal);

        if (terminals.ContainsKey(name))
        {
            throw new ArgumentException(
                $"Duplicate terminal name or alias '{name}'.",
                nameof(name));
        }

        terminals.Add(name, terminal);
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
