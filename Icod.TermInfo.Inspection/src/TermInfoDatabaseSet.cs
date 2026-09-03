namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents an immutable deterministic aggregation of caller-ordered explicit
/// terminfo database catalogs.
/// </summary>
public sealed partial class TermInfoDatabaseSet {
	internal TermInfoDatabaseSet(
		IEnumerable<TermInfoDatabaseSetEntry> entries,
		IEnumerable<TermInfoDatabaseSetIdentity> identities,
		IEnumerable<TermInfoDatabaseSetIssue> issues,
		int totalEntryCount
	) {
		ArgumentNullException.ThrowIfNull(entries);
		ArgumentNullException.ThrowIfNull(identities);
		ArgumentNullException.ThrowIfNull(issues);
		if (totalEntryCount < 0) {
			throw new ArgumentOutOfRangeException(nameof(totalEntryCount));
		}

		TermInfoDatabaseSetEntry[] entryArray = entries.ToArray();
		TermInfoDatabaseSetIdentity[] identityArray = identities.ToArray();
		TermInfoDatabaseSetIssue[] issueArray = issues.ToArray();
		if (entryArray.Any(entry => entry is null)) {
			throw new ArgumentException(
				"A database-set entry collection cannot contain null.",
				nameof(entries)
			);
		}
		if (identityArray.Any(identity => identity is null)) {
			throw new ArgumentException(
				"A database-set identity collection cannot contain null.",
				nameof(identities)
			);
		}
		if (issueArray.Any(issue => issue is null)) {
			throw new ArgumentException(
				"A database-set issue collection cannot contain null.",
				nameof(issues)
			);
		}

		Entries = Array.AsReadOnly(entryArray);
		Identities = Array.AsReadOnly(identityArray);
		Issues = Array.AsReadOnly(issueArray);
		TotalEntryCount = totalEntryCount;
		IsComplete = entryArray.All(entry => entry.IsComplete);
	}

	/// <summary>
	/// Gets constituent catalogs in exact caller order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetEntry> Entries {
		get;
	}

	/// <summary>
	/// Gets canonical identities in ordinal name order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetIdentity> Identities {
		get;
	}

	/// <summary>
	/// Gets frozen constituent catalog issues in database order and then catalog
	/// issue order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetIssue> Issues {
		get;
	}

	/// <summary>
	/// Gets the total number of successfully parsed physical entries represented
	/// across all constituent catalogs.
	/// </summary>
	public int TotalEntryCount {
		get;
	}

	/// <summary>
	/// Gets whether every requested constituent is a conventional directory with
	/// no catalog issues. The empty database set is complete.
	/// </summary>
	public bool IsComplete {
		get;
	}

	/// <summary>
	/// Resolves one exact canonical terminal name against caller-selected database
	/// precedence without treating aliases as canonical identities.
	/// </summary>
	/// <param name="name">The exact canonical terminal name.</param>
	/// <returns>Structured precedence and incomplete-input evidence.</returns>
	public TermInfoDatabaseSetLookupResult LookupCanonicalName(
		string name
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		int[] incompleteDatabaseIndices =
			Entries
				.Where( entry => !entry.IsComplete )
				.Select( entry => entry.Index )
				.ToArray();
		TermInfoDatabaseSetIdentity? identity = FindIdentity( name );
		if ( identity is null ) {
			if ( incompleteDatabaseIndices.Length == 0 ) {
				return new TermInfoDatabaseSetLookupResult(
					name,
					TermInfoDatabaseSetLookupStatus.NotObserved,
					Array.Empty<TermInfoDatabaseSetOccurrence>(),
					null,
					Array.Empty<TermInfoDatabaseSetOccurrence>(),
					Array.Empty<int>(),
					Array.Empty<int>()
				);
			}

			return new TermInfoDatabaseSetLookupResult(
				name,
				TermInfoDatabaseSetLookupStatus.Indeterminate,
				Array.Empty<TermInfoDatabaseSetOccurrence>(),
				null,
				Array.Empty<TermInfoDatabaseSetOccurrence>(),
				incompleteDatabaseIndices,
				incompleteDatabaseIndices
			);
		}

		TermInfoDatabaseSetOccurrence[] occurrences =
			identity.Occurrences.ToArray();
		int firstObservedDatabaseIndex = occurrences[ 0 ].DatabaseIndex;
		int[] blockingDatabaseIndices =
			incompleteDatabaseIndices
				.Where( index => index <= firstObservedDatabaseIndex )
				.ToArray();
		if ( blockingDatabaseIndices.Length != 0 ) {
			return new TermInfoDatabaseSetLookupResult(
				name,
				TermInfoDatabaseSetLookupStatus.Indeterminate,
				occurrences,
				null,
				Array.Empty<TermInfoDatabaseSetOccurrence>(),
				incompleteDatabaseIndices,
				blockingDatabaseIndices
			);
		}

		return new TermInfoDatabaseSetLookupResult(
			name,
			TermInfoDatabaseSetLookupStatus.WinnerKnown,
			occurrences,
			occurrences[ 0 ],
			occurrences.Skip( 1 ),
			incompleteDatabaseIndices,
			Array.Empty<int>()
		);
	}

	private TermInfoDatabaseSetIdentity? FindIdentity(
		string name
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		int low = 0;
		int high = Identities.Count - 1;
		while ( low <= high ) {
			int middle = low + ( ( high - low ) / 2 );
			TermInfoDatabaseSetIdentity candidate = Identities[ middle ];
			int comparison = StringComparer.Ordinal.Compare( candidate.Name, name );
			if ( comparison == 0 ) {
				return candidate;
			}
			if ( comparison < 0 ) {
				low = middle + 1;
			} else {
				high = middle - 1;
			}
		}

		return null;
	}
}
