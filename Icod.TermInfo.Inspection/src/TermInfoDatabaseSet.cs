namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents an immutable deterministic aggregation of caller-ordered explicit
/// terminfo database catalogs.
/// </summary>
public sealed class TermInfoDatabaseSet {
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
}
