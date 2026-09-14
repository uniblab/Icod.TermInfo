namespace Icod.TermInfo.Inspection;

/// <summary>
/// Defines explicit caller-owned raster-backend preference policy.
/// </summary>
public sealed class RasterBackendSelectionOptions {
	/// <summary>Gets the largest supported preference-order length.</summary>
	public const int MaximumSupportedPreferenceCount = 2;

	/// <summary>Initializes options with no caller ranking.</summary>
	public RasterBackendSelectionOptions()
		: this( Array.Empty<RasterBackendKind>() ) {
	}

	/// <summary>Initializes options with an explicit ordered backend preference.</summary>
	public RasterBackendSelectionOptions(
		IEnumerable<RasterBackendKind> preferenceOrder
	) {
		ArgumentNullException.ThrowIfNull( preferenceOrder );

		List<RasterBackendKind> snapshot = [];
		HashSet<RasterBackendKind> seen = [];
		foreach ( RasterBackendKind backend in preferenceOrder ) {
			if ( !Enum.IsDefined( backend ) ) {
				throw new ArgumentOutOfRangeException(
					nameof( preferenceOrder ),
					backend,
					"Raster-backend preference values must be defined."
				);
			}
			snapshot.Add( backend );
			if ( snapshot.Count > MaximumSupportedPreferenceCount ) {
				throw new ArgumentException(
					$"Raster-backend preference order cannot exceed {MaximumSupportedPreferenceCount} entries.",
					nameof( preferenceOrder )
				);
			}
			if ( !seen.Add( backend ) ) {
				throw new ArgumentException(
					"Raster-backend preference order cannot contain duplicate backends.",
					nameof( preferenceOrder )
				);
			}
		}

		PreferenceOrder = Array.AsReadOnly( snapshot.ToArray() );
	}

	/// <summary>
	/// Gets the explicit caller preference order. An empty collection means no
	/// caller ranking has been supplied.
	/// </summary>
	public IReadOnlyList<RasterBackendKind> PreferenceOrder {
		get;
	}
}
