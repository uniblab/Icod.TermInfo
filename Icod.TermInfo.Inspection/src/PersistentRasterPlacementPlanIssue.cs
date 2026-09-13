namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one non-supported advanced persistent-raster placement requirement
/// discovered during deterministic planning.
/// </summary>
public sealed class PersistentRasterPlacementPlanIssue {
	internal PersistentRasterPlacementPlanIssue(
		PersistentRasterPlacementSubject subject,
		PersistentRasterLifecycleSupportStatus supportStatus
	) {
		if ( !Enum.IsDefined( subject ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( subject ),
				subject,
				"The placement subject must be a defined value."
			);
		}
		if ( !Enum.IsDefined( supportStatus ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( supportStatus ),
				supportStatus,
				"The lifecycle support status must be a defined value."
			);
		}
		if ( supportStatus == PersistentRasterLifecycleSupportStatus.Supported ) {
			throw new ArgumentException(
				"A supported placement requirement is not a planning issue.",
				nameof( supportStatus )
			);
		}

		Subject = subject;
		SupportStatus = supportStatus;
		RequiresRuntimeVerification =
			supportStatus == PersistentRasterLifecycleSupportStatus.Unknown;
	}

	/// <summary>Gets the advanced placement semantic affected by this issue.</summary>
	public PersistentRasterPlacementSubject Subject {
		get;
	}

	/// <summary>Gets the classified support state that caused this issue.</summary>
	public PersistentRasterLifecycleSupportStatus SupportStatus {
		get;
	}

	/// <summary>
	/// Gets whether the issue can be strengthened by runtime verification rather
	/// than representing semantic impossibility.
	/// </summary>
	public bool RequiresRuntimeVerification {
		get;
	}
}
