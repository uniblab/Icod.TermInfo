namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one immutable protocol-neutral runtime observation about an
/// advanced persistent-raster placement semantic.
/// </summary>
public sealed class PersistentRasterRuntimePlacementObservation {
	/// <summary>
	/// Initializes one immutable placement runtime observation.
	/// </summary>
	/// <param name="subject">The advanced placement semantic being observed.</param>
	/// <param name="outcome">The caller-owned runtime observation outcome.</param>
	/// <param name="sourceLabel">
	/// A bounded caller-owned provenance label preserved exactly and compared
	/// ordinally for deterministic ordering.
	/// </param>
	/// <param name="sourceOrdinal">
	/// A non-negative deterministic source-local ordinal supplied by the runtime
	/// verifier.
	/// </param>
	public PersistentRasterRuntimePlacementObservation(
		PersistentRasterPlacementSubject subject,
		PersistentRasterRuntimeObservationOutcome outcome,
		string sourceLabel,
		int sourceOrdinal
	) {
		if ( !Enum.IsDefined( subject ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( subject ),
				subject,
				"The placement runtime-observation subject must be a defined value."
			);
		}
		if ( !Enum.IsDefined( outcome ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( outcome ),
				outcome,
				"The runtime-observation outcome must be a defined value."
			);
		}
		ArgumentException.ThrowIfNullOrWhiteSpace( sourceLabel );
		if (
			sourceLabel.Length
				> PersistentRasterRuntimeObservationOptions.MaximumSourceLabelLength
		) {
			throw new ArgumentException(
				$"The runtime-observation source label cannot exceed {PersistentRasterRuntimeObservationOptions.MaximumSourceLabelLength} UTF-16 code units.",
				nameof( sourceLabel )
			);
		}
		if ( sourceOrdinal < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( sourceOrdinal ),
				sourceOrdinal,
				"The placement runtime-observation source ordinal cannot be negative."
			);
		}

		Subject = subject;
		Outcome = outcome;
		SourceLabel = sourceLabel;
		SourceOrdinal = sourceOrdinal;
	}

	/// <summary>Gets the advanced placement semantic being observed.</summary>
	public PersistentRasterPlacementSubject Subject {
		get;
	}

	/// <summary>Gets the caller-owned runtime observation outcome.</summary>
	public PersistentRasterRuntimeObservationOutcome Outcome {
		get;
	}

	/// <summary>Gets the exact bounded provenance label supplied by the caller.</summary>
	public string SourceLabel {
		get;
	}

	/// <summary>
	/// Gets the non-negative source-local ordinal supplied by the runtime verifier.
	/// </summary>
	public int SourceOrdinal {
		get;
	}
}
