namespace Icod.TermInfo.Compiler;

/// <summary>
/// Selects the conventional compiled-terminfo numeric representation emitted by
/// <see cref="CompiledTermInfoWriter"/>.
/// </summary>
public enum CompiledTermInfoFormat {
	/// <summary>
	/// Prefer legacy <c>0432</c> and select wide <c>01036</c> only when a
	/// representable present numeric value requires it.
	/// </summary>
	Automatic = 0,

	/// <summary>
	/// Emit legacy <c>0432</c> exactly, failing when the description requires the
	/// wide numeric representation.
	/// </summary>
	Legacy = 1,

	/// <summary>
	/// Emit wide-numeric <c>01036</c> exactly, including when all present numeric
	/// values would fit the legacy representation.
	/// </summary>
	Wide = 2,
}