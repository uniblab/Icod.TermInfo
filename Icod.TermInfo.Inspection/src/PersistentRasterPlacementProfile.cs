namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one immutable protocol-neutral classification of advanced
/// persistent-raster placement semantics together with the complete canonical
/// evidence snapshot.
/// </summary>
public sealed class PersistentRasterPlacementProfile {
	internal PersistentRasterPlacementProfile(
		IReadOnlyList<PersistentRasterLifecycleSupportStatus> statuses,
		IReadOnlyList<PersistentRasterPlacementEvidence> evidence
	) {
		ArgumentNullException.ThrowIfNull( statuses );
		ArgumentNullException.ThrowIfNull( evidence );

		int expectedStatusCount =
			Enum.GetValues<PersistentRasterPlacementSubject>().Length;
		if ( statuses.Count != expectedStatusCount ) {
			throw new ArgumentException(
				$"A placement profile must contain exactly {expectedStatusCount} support states.",
				nameof( statuses )
			);
		}
		foreach ( PersistentRasterLifecycleSupportStatus status in statuses ) {
			if ( !Enum.IsDefined( status ) ) {
				throw new ArgumentException(
					"A placement profile cannot contain an undefined support state.",
					nameof( statuses )
				);
			}
		}

		SourceRectangle =
			statuses[ (int)PersistentRasterPlacementSubject.SourceRectangle ];
		SignedZOrder =
			statuses[ (int)PersistentRasterPlacementSubject.SignedZOrder ];
		Evidence = Array.AsReadOnly( evidence.ToArray() );
	}

	/// <summary>
	/// Gets the classified support state for pixel-space source-rectangle placement.
	/// </summary>
	public PersistentRasterLifecycleSupportStatus SourceRectangle {
		get;
	}

	/// <summary>
	/// Gets the classified support state for signed z-order / stacking-order
	/// placement.
	/// </summary>
	public PersistentRasterLifecycleSupportStatus SignedZOrder {
		get;
	}

	/// <summary>
	/// Gets the complete canonical immutable evidence snapshot used to classify
	/// this profile, including lower-precedence and contradictory evidence.
	/// </summary>
	public IReadOnlyList<PersistentRasterPlacementEvidence> Evidence {
		get;
	}

	/// <summary>
	/// Gets the classified support state for one advanced placement semantic.
	/// </summary>
	/// <param name="subject">The placement semantic to query.</param>
	/// <returns>The classified support state for <paramref name="subject"/>.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <paramref name="subject"/> is not a defined placement subject.
	/// </exception>
	public PersistentRasterLifecycleSupportStatus GetStatus(
		PersistentRasterPlacementSubject subject
	) {
		if ( !Enum.IsDefined( subject ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( subject ),
				subject,
				"The placement subject must be a defined value."
			);
		}

		return subject switch {
			PersistentRasterPlacementSubject.SourceRectangle => SourceRectangle,
			PersistentRasterPlacementSubject.SignedZOrder => SignedZOrder,
			_ => throw new ArgumentOutOfRangeException(
				nameof( subject ),
				subject,
				"The placement subject must be a defined value."
			),
		};
	}
}
