namespace Icod.TermInfo.Inspection;

/// <summary>
/// Locates one frozen catalog issue within an ordered terminfo database set.
/// </summary>
public sealed class TermInfoDatabaseSetIssue {
	internal TermInfoDatabaseSetIssue(
		int databaseIndex,
		int catalogIssueIndex,
		TermInfoDatabaseCatalogIssue issue
	) {
		if (databaseIndex < 0) {
			throw new ArgumentOutOfRangeException(nameof(databaseIndex));
		}
		if (catalogIssueIndex < 0) {
			throw new ArgumentOutOfRangeException(nameof(catalogIssueIndex));
		}
		ArgumentNullException.ThrowIfNull(issue);

		DatabaseIndex = databaseIndex;
		CatalogIssueIndex = catalogIssueIndex;
		Issue = issue;
	}

	/// <summary>
	/// Gets the zero-based caller-order database index.
	/// </summary>
	public int DatabaseIndex {
		get;
	}

	/// <summary>
	/// Gets the zero-based issue index within the frozen constituent catalog.
	/// </summary>
	public int CatalogIssueIndex {
		get;
	}

	/// <summary>
	/// Gets the original frozen catalog issue.
	/// </summary>
	public TermInfoDatabaseCatalogIssue Issue {
		get;
	}
}
