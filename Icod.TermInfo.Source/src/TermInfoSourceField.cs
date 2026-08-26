namespace Icod.TermInfo.Source;

/// <summary>
/// Represents one unresolved field in a parsed terminfo source entry.
/// </summary>
/// <remarks>
/// <para>
/// Fields remain in source order. S04 does not classify capability names as
/// standard or extended and does not apply cancellation or inheritance.
/// </para>
/// <para>
/// Invalid numeric or string values retain their raw source text and field
/// identity while their decoded value is <see langword="null"/>. The matching
/// diagnostics are carried by <see cref="TermInfoSourceParseResult"/>.
/// </para>
/// </remarks>
public sealed class TermInfoSourceField
{
    internal TermInfoSourceField(
        TermInfoSourceFieldKind kind,
        string? capabilityName,
        string? referenceName,
        int? numericValue,
        string? stringValue,
        string text,
        TermInfoSourceSpan span)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(span);

        Kind = kind;
        CapabilityName = capabilityName;
        ReferenceName = referenceName;
        NumericValue = numericValue;
        StringValue = stringValue;
        Text = text;
        Span = span;
    }

    /// <summary>
    /// Gets the unresolved source-language field kind.
    /// </summary>
    public TermInfoSourceFieldKind Kind { get; }

    /// <summary>
    /// Gets the capability name for capability-bearing fields.
    /// </summary>
    /// <remarks>
    /// This is <see langword="null"/> for <see cref="TermInfoSourceFieldKind.UseReference"/>.
    /// The name has not yet been classified against the runtime capability
    /// catalog.
    /// </remarks>
    public string? CapabilityName { get; }

    /// <summary>
    /// Gets the referenced parent entry name for a <c>use=</c> field.
    /// </summary>
    public string? ReferenceName { get; }

    /// <summary>
    /// Gets the decoded numeric value for a numeric capability.
    /// </summary>
    /// <remarks>
    /// A value of <see langword="null"/> means either that this is not a
    /// numeric field or that numeric-value decoding failed. Diagnostics
    /// distinguish those cases.
    /// </remarks>
    public int? NumericValue { get; }

    /// <summary>
    /// Gets the decoded string value for a string capability.
    /// </summary>
    /// <remarks>
    /// A value of <see langword="null"/> means either that this is not a
    /// string field or that string-value decoding failed. Diagnostics
    /// distinguish those cases.
    /// </remarks>
    public string? StringValue { get; }

    /// <summary>
    /// Gets the exact lexical text of the source field.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the location of the complete field in the original source.
    /// </summary>
    public TermInfoSourceSpan Span { get; }
}
