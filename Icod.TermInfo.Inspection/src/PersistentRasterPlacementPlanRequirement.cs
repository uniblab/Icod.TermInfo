namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one requested advanced persistent-raster placement semantic and its
/// classified support state.
/// </summary>
public sealed class PersistentRasterPlacementPlanRequirement {
	internal PersistentRasterPlacementPlanRequirement(
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

		Subject = subject;
		SupportStatus = supportStatus;
		RequiresRuntimeVerification =
			supportStatus == PersistentRasterLifecycleSupportStatus.Unknown;
	}

	/// <summary>Gets the advanced placement semantic being required.</summary>
	public PersistentRasterPlacementSubject Subject {
		get;
	}

	/// <summary>Gets the classified support state for the required semantic.</summary>
	public PersistentRasterLifecycleSupportStatus SupportStatus {
		get;
	}

	/// <summary>
	/// Gets whether this requirement can be strengthened by runtime verification.
	/// </summary>
	public bool RequiresRuntimeVerification {
		get;
	}
}
