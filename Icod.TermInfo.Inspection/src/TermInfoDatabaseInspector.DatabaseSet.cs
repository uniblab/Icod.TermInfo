namespace Icod.TermInfo.Inspection;

public static partial class TermInfoDatabaseInspector {
	/// <summary>
	/// Inspects explicit terminfo database roots exactly once in caller order and
	/// aggregates their frozen catalogs into an immutable database set.
	/// </summary>
	public static TermInfoDatabaseSet InspectSet(
		IEnumerable<string> roots,
		TermInfoDatabaseSetOptions? options = null,
		CompiledTermInfoParserOptions? parserOptions = null,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull(roots);
		cancellationToken.ThrowIfCancellationRequested();

		TermInfoDatabaseSetOptions effectiveOptions =
			options ?? new TermInfoDatabaseSetOptions();
		string[] rootArray = roots.ToArray();
		if (rootArray.Length > effectiveOptions.MaximumDatabaseCount) {
			throw new ArgumentException(
				$"The database-set request exceeds the configured maximum of {effectiveOptions.MaximumDatabaseCount} databases.",
				nameof(roots)
			);
		}
		if (rootArray.Any(string.IsNullOrWhiteSpace)) {
			throw new ArgumentException(
				"Database-set roots cannot contain null, empty, or whitespace values.",
				nameof(roots)
			);
		}

		List<TermInfoDatabaseCatalog> catalogs = [];
		foreach (string root in rootArray) {
			cancellationToken.ThrowIfCancellationRequested();
			catalogs.Add(
				InspectDirectory(
					root,
					parserOptions,
					cancellationToken
				)
			);
		}

		return CreateSet(
			catalogs,
			effectiveOptions,
			cancellationToken
		);
	}

	/// <summary>
	/// Aggregates already-inspected frozen 1.9 catalogs without performing any
	/// filesystem I/O or reinspection.
	/// </summary>
	public static TermInfoDatabaseSet CreateSet(
		IEnumerable<TermInfoDatabaseCatalog> catalogs,
		TermInfoDatabaseSetOptions? options = null,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull(catalogs);
		cancellationToken.ThrowIfCancellationRequested();

		TermInfoDatabaseSetOptions effectiveOptions =
			options ?? new TermInfoDatabaseSetOptions();
		TermInfoDatabaseCatalog[] catalogArray = catalogs.ToArray();
		if (catalogArray.Length > effectiveOptions.MaximumDatabaseCount) {
			throw new ArgumentException(
				$"The database-set request exceeds the configured maximum of {effectiveOptions.MaximumDatabaseCount} databases.",
				nameof(catalogs)
			);
		}
		if (catalogArray.Any(catalog => catalog is null)) {
			throw new ArgumentException(
				"A database-set catalog collection cannot contain null.",
				nameof(catalogs)
			);
		}

		List<TermInfoDatabaseSetEntry> setEntries = [];
		List<TermInfoDatabaseSetIssue> setIssues = [];
		Dictionary<string, List<TermInfoDatabaseSetOccurrence>> identityOccurrences =
			new(StringComparer.Ordinal);
		int totalEntryCount = 0;

		for (int databaseIndex = 0; databaseIndex < catalogArray.Length; databaseIndex++) {
			cancellationToken.ThrowIfCancellationRequested();
			TermInfoDatabaseCatalog catalog = catalogArray[databaseIndex];
			if (
				catalog.Entries.Count
				> effectiveOptions.MaximumTotalEntryCount - totalEntryCount
			) {
				throw new ArgumentException(
					$"The database-set request exceeds the configured maximum of {effectiveOptions.MaximumTotalEntryCount} aggregate physical entries.",
					nameof(catalogs)
				);
			}

			setEntries.Add(
				new TermInfoDatabaseSetEntry(
					databaseIndex,
					catalog
				)
			);
			for (int entryIndex = 0; entryIndex < catalog.Entries.Count; entryIndex++) {
				cancellationToken.ThrowIfCancellationRequested();
				TermInfoDatabaseCatalogEntry catalogEntry = catalog.Entries[entryIndex];
				TermInfoDatabaseSetOccurrence occurrence =
					new(
						databaseIndex,
						entryIndex,
						catalogEntry
					);
				if (!identityOccurrences.TryGetValue(
					catalogEntry.Name,
					out List<TermInfoDatabaseSetOccurrence>? occurrences
				)) {
					occurrences = [];
					identityOccurrences.Add(
						catalogEntry.Name,
						occurrences
					);
				}
				occurrences.Add(occurrence);
			}
			totalEntryCount += catalog.Entries.Count;

			for (int issueIndex = 0; issueIndex < catalog.Issues.Count; issueIndex++) {
				cancellationToken.ThrowIfCancellationRequested();
				setIssues.Add(
					new TermInfoDatabaseSetIssue(
						databaseIndex,
						issueIndex,
						catalog.Issues[issueIndex]
					)
				);
			}
		}
		cancellationToken.ThrowIfCancellationRequested();

		TermInfoDatabaseSetIdentity[] identities =
			identityOccurrences
				.OrderBy(
					pair => pair.Key,
					StringComparer.Ordinal
				)
				.Select(
					pair => new TermInfoDatabaseSetIdentity(
						pair.Key,
						pair.Value
					)
				)
				.ToArray();

		return new TermInfoDatabaseSet(
			setEntries,
			identities,
			setIssues,
			totalEntryCount
		);
	}
}
