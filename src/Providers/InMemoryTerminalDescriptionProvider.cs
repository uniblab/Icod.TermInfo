using System.Diagnostics.CodeAnalysis;

namespace Icod.TermInfo;

/// <summary>
/// Resolves a fixed, immutable set of terminal descriptions from memory.
/// </summary>
public sealed class InMemoryTerminalDescriptionProvider : ITerminalDescriptionProvider
{
    private readonly IReadOnlyDictionary<string, TerminalDescription> _terminals;

    /// <summary>
    /// Initializes a provider from the specified terminal descriptions.
    /// </summary>
    public InMemoryTerminalDescriptionProvider(
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

    /// <inheritdoc/>
    public bool TryLoad(
        string name,
        [NotNullWhen(true)] out TerminalDescription? terminal)
    {
        ValidateTerminalName(name);
        return _terminals.TryGetValue(name, out terminal);
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
                nameof(terminals));
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
