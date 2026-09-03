namespace Icod.TermInfo.Inspection;

/// <summary>
/// Identifies one physical catalog occurrence of a canonical terminal identity
/// in an ordered database set.
/// </summary>
public sealed class TermInfoDatabaseSetOccurrence {
	internal TermInfoDatabaseSetOccurrence(
		int databaseIndex,
		int catalogEntryIndex,
		TermInfoDatabaseCatalogEntry entry
	) {
		if (databaseIndex < 0) {
			throw new ArgumentOutOfRangeException(nameof(databaseIndex));
		}
		if (catalogEntryIndex < 0) {
			throw new ArgumentOutOfRangeException(nameof(catalogEntryIndex));
		}
		ArgumentNullException.ThrowIfNull(entry);

		DatabaseIndex = databaseIndex;
		CatalogEntryIndex = catalogEntryIndex;
		Entry = entry;
	}

	/// <summary>
	/// Gets the zero-based caller-order database index.
	/// </summary>
	public int DatabaseIndex {
		get;
	}

	/// <summary>
	/// Gets the zero-based entry index within the frozen constituent catalog.
	/// </summary>
	public int CatalogEntryIndex {
		get;
	}

	/// <summary>
	/// Gets the original immutable catalog entry.
	/// </summary>
	public TermInfoDatabaseCatalogEntry Entry {
		get;
	}

	/// <summary>
	/// Gets the canonical terminal identity declared by the occurrence.
	/// </summary>
	public string Name =>
		Entry.Name;

	/// <summary>
	/// Gets the aliases declared by this physical occurrence. Aliases remain
	/// occurrence evidence and are not promoted to canonical database-set
	/// identities.
	/// </summary>
	public IReadOnlyList<string> Aliases =>
		Entry.Aliases;
}
