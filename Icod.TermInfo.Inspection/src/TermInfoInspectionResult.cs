namespace Icod.TermInfo.Inspection;

/// <summary>
/// Couples one successfully acquired terminal description with the explicit
/// inspection target that produced it.
/// </summary>
public sealed class TermInfoInspectionResult {
	internal TermInfoInspectionResult(
		TermInfoInspectionTarget target,
		TerminalDescription terminal
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( terminal );

		Target = target;
		Terminal = terminal;
	}

	/// <summary>
	/// Gets the explicit provider/name target used for acquisition.
	/// </summary>
	public TermInfoInspectionTarget Target { get; }

	/// <summary>
	/// Gets the acquired immutable effective terminal description.
	/// </summary>
	public TerminalDescription Terminal { get; }
}
