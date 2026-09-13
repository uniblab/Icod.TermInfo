namespace Icod.TermInfo.Inspection;

/// <summary>
/// Classifies immutable advanced persistent-raster placement evidence into one
/// deterministic protocol-neutral support profile.
/// </summary>
public static class PersistentRasterPlacementClassifier {
	/// <summary>
	/// Classifies raw placement evidence using explicit per-subject provenance
	/// precedence while retaining the complete canonical evidence snapshot.
	/// </summary>
	/// <param name="evidence">The raw positive and negative placement evidence.</param>
	/// <param name="options">Optional deterministic evidence resource bounds.</param>
	/// <returns>An immutable classified placement profile.</returns>
	/// <exception cref="ArgumentException">
	/// The evidence sequence contains an invalid element or exceeds the configured
	/// evidence bound.
	/// </exception>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="evidence"/> is <see langword="null"/>.
	/// </exception>
	public static PersistentRasterPlacementProfile Classify(
		IEnumerable<PersistentRasterPlacementEvidence> evidence,
		PersistentRasterPlacementEvidenceOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( evidence );

		IReadOnlyList<PersistentRasterPlacementEvidence> snapshot =
			PersistentRasterPlacementEvidence.Snapshot(
				evidence,
				options
			);
		PersistentRasterPlacementSubject[] subjects =
			Enum.GetValues<PersistentRasterPlacementSubject>();
		PersistentRasterLifecycleSupportStatus[] statuses =
			new PersistentRasterLifecycleSupportStatus[ subjects.Length ];

		foreach ( PersistentRasterPlacementSubject subject in subjects ) {
			statuses[ (int)subject ] = ClassifySubject(
				snapshot,
				subject
			);
		}

		return new PersistentRasterPlacementProfile(
			statuses,
			snapshot
		);
	}

	private static PersistentRasterLifecycleSupportStatus ClassifySubject(
		IReadOnlyList<PersistentRasterPlacementEvidence> evidence,
		PersistentRasterPlacementSubject subject
	) {
		int highestPrecedence = -1;
		bool hasPositive = false;
		bool hasNegative = false;

		foreach ( PersistentRasterPlacementEvidence item in evidence ) {
			if ( item.Subject != subject ) {
				continue;
			}

			int precedence = GetPrecedence( item.Kind );
			if ( precedence > highestPrecedence ) {
				highestPrecedence = precedence;
				hasPositive = item.IsPositive;
				hasNegative = !item.IsPositive;
			} else {
				if ( precedence == highestPrecedence ) {
					if ( item.IsPositive ) {
						hasPositive = true;
					} else {
						hasNegative = true;
					}
				}
			}
		}

		if ( highestPrecedence < 0 ) {
			return PersistentRasterLifecycleSupportStatus.Unknown;
		}
		if ( hasPositive && hasNegative ) {
			return PersistentRasterLifecycleSupportStatus.Contradicted;
		}
		if ( hasPositive ) {
			return PersistentRasterLifecycleSupportStatus.Supported;
		}

		return PersistentRasterLifecycleSupportStatus.Unsupported;
	}

	private static int GetPrecedence(
		PersistentRasterPlacementEvidenceKind kind
	) {
		return kind switch {
			PersistentRasterPlacementEvidenceKind.CapabilityDerived => 0,
			PersistentRasterPlacementEvidenceKind.Declared => 1,
			PersistentRasterPlacementEvidenceKind.Verified => 2,
			_ => throw new ArgumentOutOfRangeException(
				nameof( kind ),
				kind,
				"The placement evidence kind must be a defined value."
			),
		};
	}
}
