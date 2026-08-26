namespace Icod.TermInfo.Source;

/// <summary>
/// Represents one parsed but unresolved terminfo source entry.
/// </summary>
/// <remarks>
/// The entry is intentionally pre-resolution state. Capability classification,
/// cancellation application, <c>use=</c> inheritance, and
/// <c>TerminalDescription</c> materialization occur in later tranches.
/// </remarks>
public sealed class TermInfoSourceEntry
{
    internal TermInfoSourceEntry(
        string canonicalName,
        IEnumerable<string> aliases,
        string? description,
        IEnumerable<TermInfoSourceField> fields,
        TermInfoSourceSpan span)
    {
        ArgumentNullException.ThrowIfNull(canonicalName);
        ArgumentNullException.ThrowIfNull(aliases);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(span);

        CanonicalName = canonicalName;
        Aliases = aliases.ToArray();
        Description = description;
        Fields = fields.ToArray();
        Span = span;
    }

    /// <summary>
    /// Gets the canonical terminal name from the entry header.
    /// </summary>
    public string CanonicalName { get; }

    /// <summary>
    /// Gets alternate terminal names in source order.
    /// </summary>
    public IReadOnlyList<string> Aliases { get; }

    /// <summary>
    /// Gets the descriptive header component when one is present.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets unresolved entry fields in source order.
    /// </summary>
    public IReadOnlyList<TermInfoSourceField> Fields { get; }

    /// <summary>
    /// Gets the source span from the canonical name through the final semantic
    /// field or header component retained for this entry.
    /// </summary>
    public TermInfoSourceSpan Span { get; }
}
