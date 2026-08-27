namespace Icod.TermInfo.Compiler;

/// <summary>
/// Controls publication of compiled entries into a conventional terminfo
/// directory tree.
/// </summary>
public sealed class CompiledTermInfoDatabaseWriterOptions {
	/// <summary>
	/// Initializes the default database writer policy.
	/// </summary>
	public CompiledTermInfoDatabaseWriterOptions()
		: this(
			overwriteExisting: false
		) {
	}

	/// <summary>
	/// Initializes the database writer policy.
	/// </summary>
	/// <param name="overwriteExisting">
	/// <see langword="true"/> to replace existing compiled entry files;
	/// otherwise an existing destination causes the write to fail.
	/// </param>
	public CompiledTermInfoDatabaseWriterOptions(
		bool overwriteExisting
	) {
		OverwriteExisting = overwriteExisting;
	}

	/// <summary>
	/// Gets whether existing compiled entry files may be replaced.
	/// </summary>
	public bool OverwriteExisting { get; }
}
