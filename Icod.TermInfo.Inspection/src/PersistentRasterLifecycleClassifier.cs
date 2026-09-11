namespace Icod.TermInfo.Inspection;

/// <summary>
/// Classifies immutable persistent-raster lifecycle evidence into one deterministic
/// protocol-neutral support profile.
/// </summary>
public static class PersistentRasterLifecycleClassifier {
	/// <summary>
	/// Classifies raw lifecycle evidence using explicit per-subject provenance
	/// precedence while retaining the complete canonical evidence snapshot.
	/// </summary>
	/// <param name="evidence">The raw positive and negative lifecycle evidence.</param>
	/// <param name="options">Optional deterministic evidence resource bounds.</param>
	/// <returns>An immutable classified lifecycle profile.</returns>
	/// <exception cref="ArgumentException">
	/// The evidence sequence contains an invalid element or exceeds the configured
	/// evidence bound.
	/// </exception>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="evidence"/> is <see langword="null"/>.
	/// </exception>
	public static PersistentRasterLifecycleProfile Classify(
		IEnumerable<PersistentRasterLifecycleEvidence> evidence,
		PersistentRasterLifecycleEvidenceOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( evidence );

		IReadOnlyList<PersistentRasterLifecycleEvidence> snapshot =
			PersistentRasterLifecycleEvidence.Snapshot(
				evidence,
				options
			);
		PersistentRasterLifecycleEvidenceSubject[] subjects =
			Enum.GetValues<PersistentRasterLifecycleEvidenceSubject>();
		PersistentRasterLifecycleSupportStatus[] statuses =
			new PersistentRasterLifecycleSupportStatus[ subjects.Length ];

		foreach ( PersistentRasterLifecycleEvidenceSubject subject in subjects ) {
			statuses[ (int)subject ] =
				ClassifySubject(
					snapshot,
					subject
				);
		}

		return new PersistentRasterLifecycleProfile(
			statuses,
			snapshot
		);
	}

	private static PersistentRasterLifecycleSupportStatus ClassifySubject(
		IReadOnlyList<PersistentRasterLifecycleEvidence> evidence,
		PersistentRasterLifecycleEvidenceSubject subject
	) {
		int highestPrecedence = -1;
		bool hasPositive = false;
		bool hasNegative = false;

		foreach ( PersistentRasterLifecycleEvidence item in evidence ) {
			if ( item.Subject != subject ) {
				continue;
			}

			int precedence =
				GetPrecedence(
					item.Kind
				);
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
		PersistentRasterLifecycleEvidenceKind kind
	) {
		return kind switch {
			PersistentRasterLifecycleEvidenceKind.CapabilityDerived => 0,
			PersistentRasterLifecycleEvidenceKind.Declared => 1,
			PersistentRasterLifecycleEvidenceKind.Verified => 2,
			_ => throw new ArgumentOutOfRangeException(
				nameof( kind ),
				kind,
				"The lifecycle evidence kind must be a defined value."
			),
		};
	}
}
