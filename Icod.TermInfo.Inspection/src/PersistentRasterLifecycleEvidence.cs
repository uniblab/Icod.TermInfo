namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one immutable protocol-neutral positive or negative lifecycle
/// evidence assertion together with deterministic provenance.
/// </summary>
public sealed class PersistentRasterLifecycleEvidence {
	/// <summary>
	/// Initializes one immutable lifecycle evidence assertion.
	/// </summary>
	/// <param name="subject">The semantic lifecycle dimension being asserted.</param>
	/// <param name="isPositive">
	/// <see langword="true"/> for positive evidence; <see langword="false"/> for
	/// negative evidence.
	/// </param>
	/// <param name="kind">The provenance class of the assertion.</param>
	/// <param name="sourceLabel">
	/// An opaque caller- or inspector-owned provenance label. The label is
	/// preserved exactly and is compared ordinally for deterministic ordering.
	/// </param>
	/// <param name="sourceOrdinal">
	/// A non-negative deterministic provenance ordinal supplied by the evidence
	/// producer.
	/// </param>
	public PersistentRasterLifecycleEvidence(
		PersistentRasterLifecycleEvidenceSubject subject,
		bool isPositive,
		PersistentRasterLifecycleEvidenceKind kind,
		string sourceLabel,
		int sourceOrdinal
	) {
		if ( !Enum.IsDefined( subject ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( subject ),
				subject,
				"The lifecycle evidence subject must be a defined value."
			);
		}
		if ( !Enum.IsDefined( kind ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( kind ),
				kind,
				"The lifecycle evidence kind must be a defined value."
			);
		}
		ArgumentException.ThrowIfNullOrWhiteSpace( sourceLabel );
		if ( sourceOrdinal < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( sourceOrdinal ),
				sourceOrdinal,
				"The lifecycle evidence source ordinal cannot be negative."
			);
		}

		Subject = subject;
		IsPositive = isPositive;
		Kind = kind;
		SourceLabel = sourceLabel;
		SourceOrdinal = sourceOrdinal;
	}

	/// <summary>
	/// Gets the lifecycle dimension about which this evidence makes an assertion.
	/// </summary>
	public PersistentRasterLifecycleEvidenceSubject Subject {
		get;
	}

	/// <summary>
	/// Gets whether this assertion is positive rather than negative.
	/// </summary>
	public bool IsPositive {
		get;
	}

	/// <summary>
	/// Gets the provenance class of this assertion.
	/// </summary>
	public PersistentRasterLifecycleEvidenceKind Kind {
		get;
	}

	/// <summary>
	/// Gets the exact opaque provenance label supplied by the evidence producer.
	/// </summary>
	public string SourceLabel {
		get;
	}

	/// <summary>
	/// Gets the non-negative deterministic provenance ordinal supplied by the
	/// evidence producer.
	/// </summary>
	public int SourceOrdinal {
		get;
	}

	/// <summary>
	/// Copies, bounds, and canonically orders raw lifecycle evidence without
	/// classifying it into a final support judgment.
	/// </summary>
	/// <param name="evidence">The evidence assertions to snapshot.</param>
	/// <param name="options">Optional deterministic resource bounds.</param>
	/// <returns>
	/// An immutable snapshot ordered by source ordinal, subject, evidence kind,
	/// polarity, and source label using ordinal string comparison.
	/// </returns>
	public static IReadOnlyList<PersistentRasterLifecycleEvidence> Snapshot(
		IEnumerable<PersistentRasterLifecycleEvidence> evidence,
		PersistentRasterLifecycleEvidenceOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( evidence );

		PersistentRasterLifecycleEvidenceOptions effectiveOptions =
			options ?? new PersistentRasterLifecycleEvidenceOptions();
		List<PersistentRasterLifecycleEvidence> items = [];
		foreach ( PersistentRasterLifecycleEvidence item in evidence ) {
			if ( item is null ) {
				throw new ArgumentException(
					"A lifecycle evidence collection cannot contain null.",
					nameof( evidence )
				);
			}
			if ( items.Count >= effectiveOptions.MaximumEvidenceCount ) {
				throw new ArgumentException(
					$"The lifecycle evidence request exceeds the configured maximum of {effectiveOptions.MaximumEvidenceCount} assertions.",
					nameof( evidence )
				);
			}

			items.Add( item );
		}

		PersistentRasterLifecycleEvidence[] ordered = items
			.OrderBy( item => item.SourceOrdinal )
			.ThenBy( item => (int)item.Subject )
			.ThenBy( item => (int)item.Kind )
			.ThenBy( item => item.IsPositive )
			.ThenBy( item => item.SourceLabel, StringComparer.Ordinal )
			.ToArray();
		return Array.AsReadOnly( ordered );
	}
}
