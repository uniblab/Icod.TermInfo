namespace Icod.TermInfo.Inspection;

/// <summary>
/// Selects the physical layout used when rendering an effective terminal
/// description as terminfo source.
/// </summary>
public enum TerminalDescriptionSourceLayout {
	/// <summary>
	/// Uses canonical multi-line source formatting with deterministic wrapping.
	/// </summary>
	Canonical = 0,

	/// <summary>
	/// Emits the complete entry on one logical source line without wrapping.
	/// </summary>
	SingleLine = 1,

	/// <summary>
	/// Emits the header on one line and each capability on exactly one following
	/// line without continuation wrapping.
	/// </summary>
	OneCapabilityPerLine = 2,
}
