namespace Icod.TermInfo.Inspection;

/// <summary>
/// Describes one immutable location in the ordered system terminfo discovery
/// snapshot.
/// </summary>
public sealed class TermInfoDatabaseLocation {
	internal TermInfoDatabaseLocation(
		TermInfoDatabaseLocationKind kind,
		string? path
	) {
		if (kind == TermInfoDatabaseLocationKind.EncodedTermInfo) {
			if (path is not null) {
				throw new ArgumentException(
					"An encoded TERMINFO location cannot expose a filesystem path.",
					nameof(path)
				);
			}
		} else {
			ArgumentException.ThrowIfNullOrWhiteSpace(path);

			if (!System.IO.Path.IsPathFullyQualified(path)) {
				throw new ArgumentException(
					"A terminfo database directory path must be fully qualified.",
					nameof(path)
				);
			}
		}

		Kind = kind;
		Path = path;
	}

	/// <summary>
	/// Gets the discovery-source kind.
	/// </summary>
	public TermInfoDatabaseLocationKind Kind {
		get;
	}

	/// <summary>
	/// Gets the normalized absolute directory path, or <see langword="null"/>
	/// when <see cref="Kind"/> is <see cref="TermInfoDatabaseLocationKind.EncodedTermInfo"/>.
	/// </summary>
	/// <remarks>
	/// Encoded <c>TERMINFO</c> bytes are intentionally not exposed by inspection.
	/// </remarks>
	public string? Path {
		get;
	}
}
