namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one immutable ordered semantic operation in a persistent-raster
/// lifecycle plan.
/// </summary>
public sealed class PersistentRasterLifecyclePlanStep {
	internal PersistentRasterLifecyclePlanStep(
		int sequenceIndex,
		PersistentRasterLifecycleOperation operation,
		bool requiresRuntimeVerification
	) {
		if ( sequenceIndex < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( sequenceIndex ),
				sequenceIndex,
				"The plan-step sequence index cannot be negative."
			);
		}
		if ( !Enum.IsDefined( operation ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( operation ),
				operation,
				"The lifecycle operation must be a defined value."
			);
		}

		SequenceIndex = sequenceIndex;
		Operation = operation;
		RequiresRuntimeVerification = requiresRuntimeVerification;
	}

	/// <summary>Gets the zero-based deterministic plan-step position.</summary>
	public int SequenceIndex {
		get;
	}

	/// <summary>Gets the protocol-neutral semantic operation.</summary>
	public PersistentRasterLifecycleOperation Operation {
		get;
	}

	/// <summary>
	/// Gets whether this step depends on one or more unknown or contradicted
	/// semantic capabilities that must be verified by the runtime consumer.
	/// </summary>
	public bool RequiresRuntimeVerification {
		get;
	}
}
