namespace Icod.TermInfo.Inspection;

/// <summary>
/// Describes one deterministic issue encountered while inspecting a
/// conventional terminfo database root.
/// </summary>
public sealed class TermInfoDatabaseCatalogIssue {
	internal TermInfoDatabaseCatalogIssue(
		TermInfoDatabaseCatalogIssueKind kind,
		string path,
		string message
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace(path);
		ArgumentException.ThrowIfNullOrWhiteSpace(message);

		if (!System.IO.Path.IsPathFullyQualified(path)) {
			throw new ArgumentException(
				"A catalog issue path must be fully qualified.",
				nameof(path)
			);
		}

		Kind = kind;
		Path = path;
		Message = message;
	}

	/// <summary>
	/// Gets the issue category.
	/// </summary>
	public TermInfoDatabaseCatalogIssueKind Kind {
		get;
	}

	/// <summary>
	/// Gets the normalized absolute filesystem path associated with the issue.
	/// </summary>
	public string Path {
		get;
	}

	/// <summary>
	/// Gets the deterministic issue description.
	/// </summary>
	public string Message {
		get;
	}
}
