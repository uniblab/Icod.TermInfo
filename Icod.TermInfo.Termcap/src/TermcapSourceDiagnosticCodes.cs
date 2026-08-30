namespace Icod.TermInfo.Termcap;

/// <summary>
/// Defines stable diagnostic codes emitted by the termcap source parser.
/// </summary>
public static class TermcapSourceDiagnosticCodes
{
	/// <summary>The configured maximum source length was exceeded.</summary>
	public const string MaximumSourceLengthExceeded = "TCAP0001";
	/// <summary>A terminal description does not contain the header-terminating colon.</summary>
	public const string MissingHeaderTerminator = "TCAP0002";
	/// <summary>A terminal-name component is empty.</summary>
	public const string EmptyTerminalName = "TCAP0003";
	/// <summary>A capability does not use a valid two-character termcap name.</summary>
	public const string InvalidCapabilityName = "TCAP0004";
	/// <summary>A capability field does not use a supported termcap field form.</summary>
	public const string MalformedCapability = "TCAP0005";
	/// <summary>A numeric capability has no value after its number marker.</summary>
	public const string MissingNumericValue = "TCAP0006";
	/// <summary>A numeric capability contains an invalid decimal, octal, or hexadecimal value.</summary>
	public const string InvalidNumericValue = "TCAP0007";
	/// <summary>A numeric capability exceeds the supported signed 32-bit range.</summary>
	public const string NumericValueOutOfRange = "TCAP0008";
	/// <summary>A string ends with an incomplete backslash escape.</summary>
	public const string IncompleteBackslashEscape = "TCAP0009";
	/// <summary>A string uses an unrecognized backslash escape.</summary>
	public const string UnknownStringEscape = "TCAP0010";
	/// <summary>A string ends with an incomplete control-character escape.</summary>
	public const string IncompleteControlEscape = "TCAP0011";
	/// <summary>A string contains a NUL character which cannot be represented safely by termcap APIs.</summary>
	public const string EmbeddedNullCharacter = "TCAP0012";
	/// <summary>A <c>tc=</c> inheritance reference has no target name.</summary>
	public const string MissingReferenceName = "TCAP0013";
	/// <summary>A <c>tc=</c> inheritance reference is not the final capability.</summary>
	public const string ReferenceMustBeLast = "TCAP0014";
	/// <summary>A terminal description omits the conventional trailing colon.</summary>
	public const string MissingTrailingColon = "TCAP0015";
	/// <summary>An octal string escape exceeds one-byte termcap string semantics.</summary>
	public const string OctalEscapeOutOfRange = "TCAP0016";
}
