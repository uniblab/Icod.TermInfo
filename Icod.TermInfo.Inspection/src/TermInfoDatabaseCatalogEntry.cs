namespace Icod.TermInfo.Inspection;

/// <summary>
/// Represents one successfully parsed physical file in a conventional terminfo
/// database root.
/// </summary>
public sealed class TermInfoDatabaseCatalogEntry {
	internal TermInfoDatabaseCatalogEntry(
		string path,
		TerminalDescription terminal
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace(path);
		ArgumentNullException.ThrowIfNull(terminal);

		if (!System.IO.Path.IsPathFullyQualified(path)) {
			throw new ArgumentException(
				"A catalog entry path must be fully qualified.",
				nameof(path)
			);
		}

		Path = path;
		Terminal = terminal;
	}

	/// <summary>
	/// Gets the normalized absolute path of the compiled entry file.
	/// </summary>
	public string Path {
		get;
	}

	/// <summary>
	/// Gets the parsed immutable terminal description.
	/// </summary>
	public TerminalDescription Terminal {
		get;
	}

	/// <summary>
	/// Gets the canonical terminal name represented by the compiled entry.
	/// </summary>
	public string Name =>
		Terminal.Name;

	/// <summary>
	/// Gets the aliases represented by the compiled entry.
	/// </summary>
	public IReadOnlyList<string> Aliases =>
		Terminal.Aliases;

	/// <summary>
	/// Gets the terminal description, when present.
	/// </summary>
	public string? Description =>
		Terminal.Description;
}
