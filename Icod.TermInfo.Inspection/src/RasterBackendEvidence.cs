namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one immutable assertion about availability of a concrete raster
/// backend.
/// </summary>
public sealed class RasterBackendEvidence {
	/// <summary>Initializes one raster-backend evidence assertion.</summary>
	public RasterBackendEvidence(
		RasterBackendKind backend,
		bool isPositive,
		RasterBackendEvidenceKind kind,
		string sourceLabel,
		int sourceOrdinal
	) {
		if ( !Enum.IsDefined( backend ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( backend ),
				backend,
				"The raster backend must be a defined value."
			);
		}
		if ( !Enum.IsDefined( kind ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( kind ),
				kind,
				"The raster-backend evidence kind must be a defined value."
			);
		}
		ArgumentException.ThrowIfNullOrWhiteSpace( sourceLabel );
		if ( sourceLabel.Length > RasterBackendEvidenceOptions.MaximumSourceLabelLength ) {
			throw new ArgumentOutOfRangeException(
				nameof( sourceLabel ),
				sourceLabel.Length,
				$"The raster-backend evidence source label cannot exceed {RasterBackendEvidenceOptions.MaximumSourceLabelLength} UTF-16 code units."
			);
		}
		if ( sourceOrdinal < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( sourceOrdinal ),
				sourceOrdinal,
				"The raster-backend evidence source ordinal cannot be negative."
			);
		}

		Backend = backend;
		IsPositive = isPositive;
		Kind = kind;
		SourceLabel = sourceLabel;
		SourceOrdinal = sourceOrdinal;
	}

	/// <summary>Gets the concrete backend described by this evidence.</summary>
	public RasterBackendKind Backend {
		get;
	}

	/// <summary>Gets whether this assertion supports backend availability.</summary>
	public bool IsPositive {
		get;
	}

	/// <summary>Gets the evidence provenance class.</summary>
	public RasterBackendEvidenceKind Kind {
		get;
	}

	/// <summary>Gets the exact bounded provenance label.</summary>
	public string SourceLabel {
		get;
	}

	/// <summary>Gets the non-negative deterministic source ordinal.</summary>
	public int SourceOrdinal {
		get;
	}

	/// <summary>
	/// Snapshots, bounds, and canonically orders raster-backend evidence.
	/// </summary>
	public static IReadOnlyList<RasterBackendEvidence> Snapshot(
		IEnumerable<RasterBackendEvidence> evidence,
		RasterBackendEvidenceOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( evidence );
		options ??= new RasterBackendEvidenceOptions();

		List<RasterBackendEvidence> snapshot = [];
		foreach ( RasterBackendEvidence? item in evidence ) {
			if ( item is null ) {
				throw new ArgumentException(
					"Raster-backend evidence cannot contain null elements.",
					nameof( evidence )
				);
			}
			snapshot.Add( item );
			if ( snapshot.Count > options.MaximumEvidenceCount ) {
				throw new ArgumentException(
					$"Raster-backend evidence cannot exceed {options.MaximumEvidenceCount} entries.",
					nameof( evidence )
				);
			}
		}

		RasterBackendEvidence[] ordered = snapshot
			.OrderBy( static item => (int)item.Backend )
			.ThenBy( static item => item.SourceOrdinal )
			.ThenBy( static item => (int)item.Kind )
			.ThenBy( static item => item.IsPositive )
			.ThenBy( static item => item.SourceLabel, StringComparer.Ordinal )
			.ToArray();
		return Array.AsReadOnly( ordered );
	}
}
