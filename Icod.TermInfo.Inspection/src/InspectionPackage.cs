using Icod.TermInfo;
using Icod.TermInfo.Source;

namespace Icod.TermInfo.Inspection;

/// <summary>
/// Provides an internal assembly anchor for the I01 Inspection package foundation.
/// </summary>
internal static class InspectionPackage {
	/// <summary>
	/// Gets the Runtime contract type used by the Inspection package.
	/// </summary>
	internal static Type RuntimeContract =>
		typeof( TerminalDescription );

	/// <summary>
	/// Gets the Source contract type used by the Inspection package.
	/// </summary>
	internal static Type SourceContract =>
		typeof( TermInfoSourceEntry );
}
