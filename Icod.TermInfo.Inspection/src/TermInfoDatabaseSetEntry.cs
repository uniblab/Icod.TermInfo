namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one constituent catalog in an immutable ordered terminfo database
/// set.
/// </summary>
public sealed class TermInfoDatabaseSetEntry {
	internal TermInfoDatabaseSetEntry(
		int index,
		TermInfoDatabaseCatalog catalog
	) {
		if (index < 0) {
			throw new ArgumentOutOfRangeException(nameof(index));
		}
		ArgumentNullException.ThrowIfNull(catalog);

		Index = index;
		Catalog = catalog;
	}

	/// <summary>
	/// Gets the zero-based caller-order index of this database.
	/// </summary>
	public int Index {
		get;
	}

	/// <summary>
	/// Gets the frozen 1.9 catalog snapshot without reinterpretation.
	/// </summary>
	public TermInfoDatabaseCatalog Catalog {
		get;
	}

	/// <summary>
	/// Gets whether this constituent is a conventional directory with no catalog
	/// inspection issues.
	/// </summary>
	public bool IsComplete =>
		Catalog.Kind == TermInfoDatabaseCatalogKind.ConventionalDirectory
		&& !Catalog.HasIssues;
}
