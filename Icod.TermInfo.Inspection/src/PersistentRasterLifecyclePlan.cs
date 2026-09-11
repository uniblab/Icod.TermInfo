namespace Icod.TermInfo.Inspection;

/// <summary>
/// Contains one immutable deterministic persistent-raster lifecycle planning
/// result.
/// </summary>
public sealed class PersistentRasterLifecyclePlan {
	internal PersistentRasterLifecyclePlan(
		PersistentRasterLifecyclePlanStatus status,
		IEnumerable<PersistentRasterLifecyclePlanStep> steps,
		IEnumerable<PersistentRasterLifecyclePlanIssue> issues
	) {
		ArgumentNullException.ThrowIfNull( steps );
		ArgumentNullException.ThrowIfNull( issues );
		if ( !Enum.IsDefined( status ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( status ),
				status,
				"The lifecycle plan status must be a defined value."
			);
		}

		PersistentRasterLifecyclePlanStep[] stepArray =
			steps.ToArray();
		PersistentRasterLifecyclePlanIssue[] issueArray =
			issues.ToArray();
		if ( stepArray.Any( step => step is null ) ) {
			throw new ArgumentException(
				"The lifecycle plan step sequence cannot contain null.",
				nameof( steps )
			);
		}
		if ( issueArray.Any( issue => issue is null ) ) {
			throw new ArgumentException(
				"The lifecycle plan issue sequence cannot contain null.",
				nameof( issues )
			);
		}
		for ( int index = 0; index < stepArray.Length; index++ ) {
			if ( stepArray[ index ].SequenceIndex != index ) {
				throw new ArgumentException(
					"Lifecycle plan step indices must be contiguous and zero-based.",
					nameof( steps )
				);
			}
		}

		switch ( status ) {
			case PersistentRasterLifecyclePlanStatus.Success:
				if ( issueArray.Length != 0 ) {
					throw new ArgumentException(
						"A successful lifecycle plan cannot contain issues.",
						nameof( issues )
					);
				}
				break;
			case PersistentRasterLifecyclePlanStatus.Indeterminate:
				if (
					stepArray.Length == 0
					|| !issueArray.Any(
						issue => issue.RequiresRuntimeVerification
					)
				) {
					throw new ArgumentException(
						"An indeterminate lifecycle plan requires executable steps and at least one runtime-verification issue."
					);
				}
				break;
			case PersistentRasterLifecyclePlanStatus.Impossible:
				if (
					stepArray.Length != 0
					|| !issueArray.Any(
						issue =>
							issue.SupportStatus
								== PersistentRasterLifecycleSupportStatus.Unsupported
					)
				) {
					throw new ArgumentException(
						"An impossible lifecycle plan must contain no executable steps and at least one unsupported requirement."
					);
				}
				break;
		}

		Status = status;
		Steps = Array.AsReadOnly( stepArray );
		Issues = Array.AsReadOnly( issueArray );
		RequiresRuntimeVerification =
			status == PersistentRasterLifecyclePlanStatus.Indeterminate;
	}

	/// <summary>Gets the deterministic planning outcome.</summary>
	public PersistentRasterLifecyclePlanStatus Status {
		get;
	}

	/// <summary>Gets the ordered semantic operations when execution remains admissible.</summary>
	public IReadOnlyList<PersistentRasterLifecyclePlanStep> Steps {
		get;
	}

	/// <summary>Gets the structured non-supported requirements.</summary>
	public IReadOnlyList<PersistentRasterLifecyclePlanIssue> Issues {
		get;
	}

	/// <summary>
	/// Gets whether the plan is admissible only after consumer-owned runtime
	/// verification strengthens one or more uncertain requirements.
	/// </summary>
	public bool RequiresRuntimeVerification {
		get;
	}
}
