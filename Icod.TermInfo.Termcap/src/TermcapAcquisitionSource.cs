namespace Icod.TermInfo.Termcap;

/// <summary>
/// Describes the configured source which supplied an acquired root termcap
/// entry.
/// </summary>
public sealed class TermcapAcquisitionSource
{
	internal TermcapAcquisitionSource(
		TermcapAcquisitionSourceKind kind,
		string identifier
	) {
		if ( !Enum.IsDefined( typeof( TermcapAcquisitionSourceKind ), kind ) ) {
			throw new ArgumentOutOfRangeException( nameof( kind ) );
		}
		ArgumentException.ThrowIfNullOrWhiteSpace( identifier );

		Kind = kind;
		Identifier = identifier;
	}

	/// <summary>Gets the configured source category.</summary>
	public TermcapAcquisitionSourceKind Kind { get; }

	/// <summary>
	/// Gets the deterministic source identifier used for diagnostics. Inline
	/// acquisition uses a symbolic name; database acquisition uses the configured
	/// path.
	/// </summary>
	public string Identifier { get; }
}
