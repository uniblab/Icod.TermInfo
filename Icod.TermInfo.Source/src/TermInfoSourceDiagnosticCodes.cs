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

    /// <summary>
    /// A numeric capability contains no value after <c>#</c>.
    /// </summary>
    public const string MissingNumericValue = "TIS0010";

    /// <summary>
    /// A numeric capability contains a malformed value for its selected base.
    /// </summary>
    public const string InvalidNumericValue = "TIS0011";

    /// <summary>
    /// A numeric capability exceeds the supported signed 32-bit range.
    /// </summary>
    public const string NumericValueOutOfRange = "TIS0012";

    /// <summary>
    /// A backslash source escape ends before an escaped character is present.
    /// </summary>
    public const string IncompleteBackslashEscape = "TIS0013";

    /// <summary>
    /// A backslash source escape is not one of the defined terminfo escapes.
    /// </summary>
    public const string UnknownStringEscape = "TIS0014";

    /// <summary>
    /// A caret control-character escape is incomplete.
    /// </summary>
    public const string IncompleteControlEscape = "TIS0015";

    /// <summary>
    /// A caret control-character target lies outside printable ASCII.
    /// </summary>
    public const string InvalidControlEscape = "TIS0016";

    /// <summary>
    /// A digit <c>8</c> or <c>9</c> occurs inside an octal source escape.
    /// </summary>
    public const string NonOctalDigitInStringEscape = "TIS0017";

    /// <summary>
    /// A physical newline continues a string on an unindented source line.
    /// </summary>
    public const string UnindentedStringContinuation = "TIS0018";

    /// <summary>
    /// A source string contains a literal NUL character.
    /// </summary>
    public const string EmbeddedNullCharacter = "TIS0019";
}
