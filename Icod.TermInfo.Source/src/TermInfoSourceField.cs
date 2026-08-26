using Icod.TermInfo;

namespace Icod.TermInfo.Source;

/// <summary>
/// Represents one unresolved field in a parsed terminfo source entry.
/// </summary>
/// <remarks>
/// <para>
/// Fields remain in source order. S06 keeps these declarations intact for
/// provenance while an internal semantic state applies local values and
/// cancellation tombstones. S07 consumes <c>use=</c> declarations through the
/// inheritance resolver without mutating this unresolved representation.
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
        TermInfoSourceSpan span,
        TermInfoSourceCapabilityClassification? capabilityClassification = null,
        string? canonicalCapabilityName = null,
        TermInfoCapabilityValueKind? standardValueKind = null,
        BooleanCapability? standardBooleanCapability = null,
        NumericCapability? standardNumericCapability = null,
        StringCapability? standardStringCapability = null)
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
        CapabilityClassification = capabilityClassification;
        CanonicalCapabilityName = canonicalCapabilityName;
        StandardValueKind = standardValueKind;
        StandardBooleanCapability = standardBooleanCapability;
        StandardNumericCapability = standardNumericCapability;
        StandardStringCapability = standardStringCapability;
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
    /// The spelling is the normalized source spelling and may be a standard
    /// long name rather than the canonical short name.
    /// </remarks>
    public string? CapabilityName { get; }

    /// <summary>
    /// Gets the S05 classification for a capability-bearing field.
    /// </summary>
    /// <remarks>
    /// This is <see langword="null"/> for <c>use=</c> references, which are not
    /// capability declarations.
    /// </remarks>
    public TermInfoSourceCapabilityClassification? CapabilityClassification { get; }

    /// <summary>
    /// Gets the canonical standard short name or the accepted extended name.
    /// </summary>
    /// <remarks>
    /// This is <see langword="null"/> for invalid/reserved names and for
    /// <c>use=</c> references.
    /// </remarks>
    public string? CanonicalCapabilityName { get; }

    /// <summary>
    /// Gets the standard capability value kind when classification resolved to
    /// the runtime standard catalog.
    /// </summary>
    public TermInfoCapabilityValueKind? StandardValueKind { get; }

    /// <summary>
    /// Gets the exact runtime Boolean capability identity for a standard
    /// Boolean source name.
    /// </summary>
    public BooleanCapability? StandardBooleanCapability { get; }

    /// <summary>
    /// Gets the exact runtime numeric capability identity for a standard
    /// numeric source name.
    /// </summary>
    public NumericCapability? StandardNumericCapability { get; }

    /// <summary>
    /// Gets the exact runtime string capability identity for a standard string
    /// source name.
    /// </summary>
    public StringCapability? StandardStringCapability { get; }

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
