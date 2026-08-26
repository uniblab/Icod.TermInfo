namespace Icod.TermInfo.Source;

/// <summary>
/// Identifies one unresolved field in a parsed terminfo source entry.
/// </summary>
/// <remarks>
/// S04 deliberately preserves the source-language field kind without deciding
/// whether a capability name is standard or extended. Capability catalog
/// classification begins in S05.
/// </remarks>
public enum TermInfoSourceFieldKind
{
    /// <summary>
    /// A Boolean capability declaration.
    /// </summary>
    BooleanCapability = 0,

    /// <summary>
    /// A numeric capability declaration.
    /// </summary>
    NumericCapability = 1,

    /// <summary>
    /// A string capability declaration.
    /// </summary>
    StringCapability = 2,

    /// <summary>
    /// A capability cancellation declaration.
    /// </summary>
    CancelledCapability = 3,

    /// <summary>
    /// A <c>use=</c> inheritance reference.
    /// </summary>
    UseReference = 4,

    /// <summary>
    /// An ncurses-compatible field disabled by a leading period.
    /// </summary>
    DisabledCapability = 5,
}
