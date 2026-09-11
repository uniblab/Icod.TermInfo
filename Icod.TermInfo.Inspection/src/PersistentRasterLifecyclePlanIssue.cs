namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one structured non-supported requirement discovered during
/// persistent-raster lifecycle planning.
/// </summary>
public sealed class PersistentRasterLifecyclePlanIssue {
	internal PersistentRasterLifecyclePlanIssue(
		PersistentRasterLifecycleOperation operation,
		PersistentRasterLifecycleEvidenceSubject subject,
		PersistentRasterLifecycleSupportStatus supportStatus
	) {
		if ( !Enum.IsDefined( operation ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( operation ),
				operation,
				"The lifecycle operation must be a defined value."
			);
		}
		if ( !Enum.IsDefined( subject ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( subject ),
				subject,
				"The lifecycle evidence subject must be a defined value."
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
				"A supported requirement is not a lifecycle planning issue.",
				nameof( supportStatus )
			);
		}

		Operation = operation;
		Subject = subject;
		SupportStatus = supportStatus;
		RequiresRuntimeVerification =
			supportStatus
				is PersistentRasterLifecycleSupportStatus.Unknown
				or PersistentRasterLifecycleSupportStatus.Contradicted;
	}

	/// <summary>Gets the semantic operation affected by this issue.</summary>
	public PersistentRasterLifecycleOperation Operation {
		get;
	}

	/// <summary>Gets the semantic capability that caused this issue.</summary>
	public PersistentRasterLifecycleEvidenceSubject Subject {
		get;
	}

	/// <summary>Gets the classified support state that caused this issue.</summary>
	public PersistentRasterLifecycleSupportStatus SupportStatus {
		get;
	}

	/// <summary>
	/// Gets whether the issue can be strengthened by runtime verification rather
	/// than representing conclusive semantic impossibility.
	/// </summary>
	public bool RequiresRuntimeVerification {
		get;
	}
}
