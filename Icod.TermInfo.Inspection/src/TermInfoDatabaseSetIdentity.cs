namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one canonical terminal identity observed across an ordered
/// database set.
/// </summary>
public sealed class TermInfoDatabaseSetIdentity {
	internal TermInfoDatabaseSetIdentity(
		string name,
		IEnumerable<TermInfoDatabaseSetOccurrence> occurrences
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(occurrences);

		TermInfoDatabaseSetOccurrence[] occurrenceArray =
			occurrences.ToArray();
		if (occurrenceArray.Length == 0) {
			throw new ArgumentException(
				"A database-set identity must contain at least one occurrence.",
				nameof(occurrences)
			);
		}
		if (occurrenceArray.Any(occurrence => occurrence is null)) {
			throw new ArgumentException(
				"A database-set identity occurrence collection cannot contain null.",
				nameof(occurrences)
			);
		}
		if (
			occurrenceArray.Any(
				occurrence => !string.Equals(
					occurrence.Name,
					name,
					StringComparison.Ordinal
				)
			)
		) {
			throw new ArgumentException(
				"Every occurrence must declare the database-set canonical identity.",
				nameof(occurrences)
			);
		}

		Name = name;
		Occurrences = Array.AsReadOnly(occurrenceArray);
	}

	/// <summary>
	/// Gets the canonical terminal name, compared and ordered ordinally.
	/// </summary>
	public string Name {
		get;
	}

	/// <summary>
	/// Gets physical occurrences in database order and then constituent catalog
	/// entry order.
	/// </summary>
	public IReadOnlyList<TermInfoDatabaseSetOccurrence> Occurrences {
		get;
	}
}
