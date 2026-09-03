namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents deterministic ordered-precedence evidence for one canonical
/// terminal name in a terminfo database set.
/// </summary>
public sealed class TermInfoDatabaseSetLookupResult {
	internal TermInfoDatabaseSetLookupResult(
		string name,
		TermInfoDatabaseSetLookupStatus status,
		IEnumerable<TermInfoDatabaseSetOccurrence> occurrences,
		TermInfoDatabaseSetOccurrence? winner,
		IEnumerable<TermInfoDatabaseSetOccurrence> shadowedOccurrences,
		IEnumerable<int> incompleteDatabaseIndices,
		IEnumerable<int> blockingDatabaseIndices
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );
		ArgumentNullException.ThrowIfNull( occurrences );
		ArgumentNullException.ThrowIfNull( shadowedOccurrences );
		ArgumentNullException.ThrowIfNull( incompleteDatabaseIndices );
		ArgumentNullException.ThrowIfNull( blockingDatabaseIndices );

		TermInfoDatabaseSetOccurrence[] occurrenceArray = occurrences.ToArray();
		TermInfoDatabaseSetOccurrence[] shadowArray = shadowedOccurrences.ToArray();
		int[] incompleteArray = incompleteDatabaseIndices.ToArray();
		int[] blockingArray = blockingDatabaseIndices.ToArray();
		if ( occurrenceArray.Any( occurrence => occurrence is null ) ) {
			throw new ArgumentException(
				"Lookup occurrences cannot contain null.",
				nameof( occurrences )
			);
		}
		if ( shadowArray.Any( occurrence => occurrence is null ) ) {
			throw new ArgumentException(
				"Shadow occurrences cannot contain null.",
				nameof( shadowedOccurrences )
			);
		}
		if ( occurrenceArray.Any(
				occurrence => !string.Equals(
					occurrence.Name,
					name,
					StringComparison.Ordinal
				)
			) ) {
			throw new ArgumentException(
				"Every lookup occurrence must declare the requested canonical identity.",
				nameof( occurrences )
			);
		}
		if ( incompleteArray.Any( index => index < 0 )
			|| blockingArray.Any( index => index < 0 ) ) {
			throw new ArgumentOutOfRangeException(
				nameof( incompleteDatabaseIndices ),
				"Database indices cannot be negative."
			);
		}
		if ( incompleteArray.Distinct().Count() != incompleteArray.Length
			|| blockingArray.Distinct().Count() != blockingArray.Length ) {
			throw new ArgumentException(
				"Database-index evidence cannot contain duplicates."
			);
		}
		if ( blockingArray.Except( incompleteArray ).Any() ) {
			throw new ArgumentException(
				"Every blocking database must also be incomplete.",
				nameof( blockingDatabaseIndices )
			);
		}

		switch ( status ) {
			case TermInfoDatabaseSetLookupStatus.NotObserved:
				if ( occurrenceArray.Length != 0
					|| winner is not null
					|| shadowArray.Length != 0
					|| incompleteArray.Length != 0
					|| blockingArray.Length != 0 ) {
					throw new ArgumentException(
						"A conclusive absence cannot contain occurrence or incomplete evidence."
					);
				}
				break;
			case TermInfoDatabaseSetLookupStatus.WinnerKnown:
				if ( occurrenceArray.Length == 0
					|| winner is null
					|| !ReferenceEquals( winner, occurrenceArray[ 0 ] )
					|| blockingArray.Length != 0
					|| shadowArray.Length != occurrenceArray.Length - 1 ) {
					throw new ArgumentException(
						"A known winner must be the first occurrence and all later observed occurrences must be shadows."
					);
				}
				for ( int index = 0; index < shadowArray.Length; index++ ) {
					if ( !ReferenceEquals( shadowArray[ index ], occurrenceArray[ index + 1 ] ) ) {
						throw new ArgumentException(
							"Shadow occurrences must preserve the later occurrence order.",
							nameof( shadowedOccurrences )
						);
					}
				}
				break;
			case TermInfoDatabaseSetLookupStatus.Indeterminate:
				if ( winner is not null
					|| shadowArray.Length != 0
					|| blockingArray.Length == 0 ) {
					throw new ArgumentException(
						"Indeterminate lookup evidence requires at least one blocking incomplete database and cannot claim a winner or shadows."
					);
				}
				break;
			default:
				throw new ArgumentOutOfRangeException( nameof( status ) );
		}

		Name = name;
		Status = status;
		Occurrences = Array.AsReadOnly( occurrenceArray );
		Winner = winner;
		ShadowedOccurrences = Array.AsReadOnly( shadowArray );
		IncompleteDatabaseIndices = Array.AsReadOnly( incompleteArray );
		BlockingDatabaseIndices = Array.AsReadOnly( blockingArray );
	}

	/// <summary>
	/// Gets the exact ordinal canonical name requested by the caller.
	/// </summary>
	public string Name {
		get;
	}

	/// <summary>
	/// Gets the conclusive or indeterminate lookup state.
	/// </summary>
	public TermInfoDatabaseSetLookupStatus Status {
		get;
	}

	/// <summary>
	/// Gets all observed physical occurrences in database and catalog-entry order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetOccurrence> Occurrences {
		get;
	}

	/// <summary>
	/// Gets the first applicable observed occurrence when precedence is conclusive.
	/// </summary>
	public TermInfoDatabaseSetOccurrence? Winner {
		get;
	}

	/// <summary>
	/// Gets later observed occurrences only when a winner is conclusive.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetOccurrence> ShadowedOccurrences {
		get;
	}

	/// <summary>
	/// Gets every incomplete constituent database index in caller order.
	/// </summary>
	public IReadOnlyList<int> IncompleteDatabaseIndices {
		get;
	}

	/// <summary>
	/// Gets incomplete database indices which prevent a conclusive absence or
	/// winner determination.
	/// </summary>
	public IReadOnlyList<int> BlockingDatabaseIndices {
		get;
	}

	/// <summary>
	/// Gets whether at least one physical occurrence was observed.
	/// </summary>
	public bool IsObserved =>
		Occurrences.Count != 0;

	/// <summary>
	/// Gets whether more than one physical occurrence was observed.
	/// </summary>
	public bool HasMultipleOccurrences =>
		Occurrences.Count > 1;

	/// <summary>
	/// Gets whether every database was inspected completely, so the occurrence
	/// list itself is exhaustive for the requested canonical identity.
	/// </summary>
	public bool IsObservationComplete =>
		IncompleteDatabaseIndices.Count == 0;
}
