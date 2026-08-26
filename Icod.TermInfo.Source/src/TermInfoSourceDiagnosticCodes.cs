namespace Icod.TermInfo.Source;

/// <summary>
/// Defines stable machine-readable diagnostic codes emitted by the terminfo
/// source-language layer.
/// </summary>
public static class TermInfoSourceDiagnosticCodes
{
    /// <summary>
    /// The supplied source exceeds the configured maximum character count.
    /// </summary>
    public const string MaximumSourceLengthExceeded = "TIS0001";

    /// <summary>
    /// An indented capability field appears before any entry header.
    /// </summary>
    public const string OrphanedCapabilityField = "TIS0002";

    /// <summary>
    /// A source field reaches the end of input without its terminating comma.
    /// </summary>
    public const string MissingFieldTerminator = "TIS0003";

    /// <summary>
    /// A capability operator appears without a capability name.
    /// </summary>
    public const string MissingCapabilityName = "TIS0004";

    /// <summary>
    /// An entry header has an empty canonical terminal name.
    /// </summary>
    public const string EmptyTerminalName = "TIS0005";

    /// <summary>
    /// An entry header contains an empty alias or descriptive-name component.
    /// </summary>
    public const string EmptyHeaderComponent = "TIS0006";

    /// <summary>
    /// A <c>use=</c> field does not identify a parent terminal entry.
    /// </summary>
    public const string MissingUseReference = "TIS0007";

    /// <summary>
    /// Two field separators occur without a field between them.
    /// </summary>
    public const string EmptyField = "TIS0008";

    /// <summary>
    /// A cancelled capability contains non-whitespace text after <c>@</c>.
    /// </summary>
    public const string UnexpectedTextAfterCancellation = "TIS0009";
}
