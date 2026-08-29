using System.Globalization;

namespace Icod.TermInfo.Inspection;

public static partial class TermInfoDatabaseInspector {
	private const int CatalogFileBufferSize = 4096;

	/// <summary>
	/// Inspects one explicit conventional terminfo database root.
	/// </summary>
	/// <param name="root">
	/// The explicit database root. The path is normalized to an absolute path
	/// and does not need to exist.
	/// </param>
	/// <param name="parserOptions">
	/// Optional compiled-entry resource limits. The values are snapshotted for
	/// the complete inspection.
	/// </param>
	/// <returns>
	/// An immutable catalog containing successfully parsed physical entries,
	/// deterministic non-fatal issues, duplicate canonical identities, and the
	/// observed root storage state.
	/// </returns>
	/// <remarks>
	/// Enumeration is limited to immediate literal first-character and
	/// two-digit hexadecimal subdirectories. Symbolic-link, junction, and
	/// reparse-point children are reported and skipped rather than followed.
	/// Hashed or other non-directory stores are reported as unsupported.
	/// </remarks>
	public static TermInfoDatabaseCatalog InspectDirectory(
		string root,
		CompiledTermInfoParserOptions? parserOptions = null
	) {
		return InspectDirectory(
			root,
			parserOptions,
			CancellationToken.None
		);
	}

	/// <summary>
	/// Inspects one explicit conventional terminfo database root with
	/// cancellation support.
	/// </summary>
	/// <param name="root">
	/// The explicit database root. The path is normalized to an absolute path
	/// and does not need to exist.
	/// </param>
	/// <param name="parserOptions">
	/// Optional compiled-entry resource limits. The values are snapshotted for
	/// the complete inspection.
	/// </param>
	/// <param name="cancellationToken">
	/// A token which can cancel directory traversal and entry reads.
	/// </param>
	/// <returns>
	/// An immutable catalog containing successfully parsed physical entries,
	/// deterministic non-fatal issues, duplicate canonical identities, and the
	/// observed root storage state.
	/// </returns>
	public static TermInfoDatabaseCatalog InspectDirectory(
		string root,
		CompiledTermInfoParserOptions? parserOptions,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull(root);

		if (string.IsNullOrWhiteSpace(root)) {
			throw new ArgumentException(
				"The terminfo database root cannot be empty or whitespace.",
				nameof(root)
			);
		}

		cancellationToken.ThrowIfCancellationRequested();

		string normalizedRoot =
			Path.GetFullPath(root);
		CompiledTermInfoParserOptions effectiveParserOptions =
			SnapshotParserOptions(parserOptions);

		FileAttributes rootAttributes;
		try {
			rootAttributes =
				File.GetAttributes(normalizedRoot);
		}
		catch (FileNotFoundException) {
			return CreateEmptyCatalog(
				normalizedRoot,
				TermInfoDatabaseCatalogKind.Missing
			);
		}
		catch (DirectoryNotFoundException) {
			return CreateEmptyCatalog(
				normalizedRoot,
				TermInfoDatabaseCatalogKind.Missing
			);
		}
		catch (Exception exception) when (IsCatalogIoException(exception)) {
			return CreateUnavailableCatalog(
				normalizedRoot,
				exception,
				"terminfo database root"
			);
		}

		if ((rootAttributes & FileAttributes.Directory) == 0) {
			return CreateEmptyCatalog(
				normalizedRoot,
				TermInfoDatabaseCatalogKind.UnsupportedStore
			);
		}

		return InspectConventionalDirectory(
			normalizedRoot,
			effectiveParserOptions,
			cancellationToken
		);
	}

	private static TermInfoDatabaseCatalog InspectConventionalDirectory(
		string root,
		CompiledTermInfoParserOptions parserOptions,
		CancellationToken cancellationToken
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace(root);
		ArgumentNullException.ThrowIfNull(parserOptions);

		List<TermInfoDatabaseCatalogEntry> entries = [];
		List<TermInfoDatabaseCatalogIssue> issues = [];

		string[] directories;
		try {
			directories =
				Directory.GetDirectories(root);
		}
		catch (DirectoryNotFoundException) {
			return CreateEmptyCatalog(
				root,
				TermInfoDatabaseCatalogKind.Missing
			);
		}
		catch (Exception exception) when (IsCatalogIoException(exception)) {
			return CreateUnavailableCatalog(
				root,
				exception,
				"terminfo database root"
			);
		}

		foreach (
			string directory
			in directories
				.OrderBy(
					path => Path.GetFileName(path),
					StringComparer.Ordinal
				)
		) {
			cancellationToken.ThrowIfCancellationRequested();

			string directoryName =
				GetRequiredFileName(directory);
			if (!IsConventionalCatalogDirectoryName(directoryName)) {
				continue;
			}

			FileAttributes directoryAttributes;
			try {
				directoryAttributes =
					File.GetAttributes(directory);
			}
			catch (Exception exception) when (IsCatalogIoException(exception)) {
				issues.Add(
					CreateFileSystemIssue(
						directory,
						exception,
						"terminfo database subdirectory"
					)
				);
				continue;
			}

			if (IsCatalogReparsePoint(directoryAttributes)) {
				issues.Add(
					new TermInfoDatabaseCatalogIssue(
						TermInfoDatabaseCatalogIssueKind.LinkSkipped,
						directory,
						"The terminfo database subdirectory is a link or reparse point and was not traversed."
					)
				);
				continue;
			}

			string[] files;
			try {
				files =
					Directory.GetFiles(directory);
			}
			catch (Exception exception) when (IsCatalogIoException(exception)) {
				issues.Add(
					CreateFileSystemIssue(
						directory,
						exception,
						"terminfo database subdirectory"
					)
				);
				continue;
			}

			foreach (
				string path
				in files.OrderBy(
					value => value,
					StringComparer.Ordinal
				)
			) {
				cancellationToken.ThrowIfCancellationRequested();

				InspectCandidate(
					directoryName,
					path,
					parserOptions,
					entries,
					issues,
					cancellationToken
				);
			}
		}

		TermInfoDatabaseCatalogEntry[] orderedEntries =
			entries
				.OrderBy(
					entry => entry.Name,
					StringComparer.Ordinal
				)
				.ThenBy(
					entry => entry.Path,
					StringComparer.Ordinal
				)
				.ToArray();

		TermInfoDatabaseCatalogIssue[] orderedIssues =
			issues
				.OrderBy(
					issue => issue.Path,
					StringComparer.Ordinal
				)
				.ThenBy(issue => issue.Kind)
				.ThenBy(
					issue => issue.Message,
					StringComparer.Ordinal
				)
				.ToArray();

		string[] duplicateCanonicalNames =
			orderedEntries
				.GroupBy(
					entry => entry.Name,
					StringComparer.Ordinal
				)
				.Where(group => group.Count() > 1)
				.Select(group => group.Key)
				.OrderBy(
					name => name,
					StringComparer.Ordinal
				)
				.ToArray();

		return new TermInfoDatabaseCatalog(
			root,
			TermInfoDatabaseCatalogKind.ConventionalDirectory,
			orderedEntries,
			orderedIssues,
			duplicateCanonicalNames
		);
	}

	private static void InspectCandidate(
		string directoryName,
		string path,
		CompiledTermInfoParserOptions parserOptions,
		ICollection<TermInfoDatabaseCatalogEntry> entries,
		ICollection<TermInfoDatabaseCatalogIssue> issues,
		CancellationToken cancellationToken
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace(directoryName);
		ArgumentException.ThrowIfNullOrWhiteSpace(path);
		ArgumentNullException.ThrowIfNull(parserOptions);
		ArgumentNullException.ThrowIfNull(entries);
		ArgumentNullException.ThrowIfNull(issues);

		FileAttributes attributes;
		try {
			attributes =
				File.GetAttributes(path);
		}
		catch (Exception exception) when (IsCatalogIoException(exception)) {
			issues.Add(
				CreateFileSystemIssue(
					path,
					exception,
					"compiled terminfo candidate"
				)
			);
			return;
		}

		if (IsCatalogReparsePoint(attributes)) {
			issues.Add(
				new TermInfoDatabaseCatalogIssue(
					TermInfoDatabaseCatalogIssueKind.LinkSkipped,
					path,
					"The compiled terminfo candidate is a link or reparse point and was not followed."
				)
			);
			return;
		}

		TerminalDescription terminal;
		try {
			terminal =
				ReadCatalogTerminal(
					path,
					parserOptions,
					cancellationToken
				);
		}
		catch (CompiledTermInfoFormatException exception) {
			issues.Add(
				new TermInfoDatabaseCatalogIssue(
					TermInfoDatabaseCatalogIssueKind.MalformedEntry,
					path,
					exception.Message
				)
			);
			return;
		}
		catch (Exception exception) when (IsCatalogIoException(exception)) {
			issues.Add(
				CreateFileSystemIssue(
					path,
					exception,
					"compiled terminfo candidate"
				)
			);
			return;
		}

		entries.Add(
			new TermInfoDatabaseCatalogEntry(
				path,
				terminal
			)
		);

		string fileName =
			GetRequiredFileName(path);
		if (!IsConventionallyPlaced(
				directoryName,
				fileName,
				terminal
			)) {
			issues.Add(
				new TermInfoDatabaseCatalogIssue(
					TermInfoDatabaseCatalogIssueKind.InvalidPlacement,
					path,
					"The compiled terminfo candidate is not conventionally placed for an identity declared by the parsed entry."
				)
			);
		}
	}

	private static TerminalDescription ReadCatalogTerminal(
		string path,
		CompiledTermInfoParserOptions parserOptions,
		CancellationToken cancellationToken
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace(path);
		ArgumentNullException.ThrowIfNull(parserOptions);

		using FileStream stream =
			new(
				path,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read,
				CatalogFileBufferSize,
				FileOptions.SequentialScan
			);

		long length =
			stream.Length;
		if (length > parserOptions.MaximumEntrySize) {
			throw new CompiledTermInfoFormatException(
				"The compiled entry is "
				+ $"{length} bytes, exceeding the configured maximum of "
				+ $"{parserOptions.MaximumEntrySize} bytes."
			);
		}

		byte[] entry =
			new byte[(int)length];
		int offset = 0;

		while (offset < entry.Length) {
			cancellationToken.ThrowIfCancellationRequested();

			int read =
				stream.Read(
					entry,
					offset,
					entry.Length - offset
				);
			if (read == 0) {
				throw new IOException(
					$"Compiled terminfo entry '{path}' changed length while it was being read."
				);
			}

			offset += read;
		}

		cancellationToken.ThrowIfCancellationRequested();

		if (stream.ReadByte() != -1) {
			throw new IOException(
				$"Compiled terminfo entry '{path}' changed length while it was being read."
			);
		}

		cancellationToken.ThrowIfCancellationRequested();

		return CompiledTermInfoParser.Parse(
			entry,
			parserOptions
		);
	}

	private static CompiledTermInfoParserOptions SnapshotParserOptions(
		CompiledTermInfoParserOptions? parserOptions
	) {
		CompiledTermInfoParserOptions effectiveOptions =
			parserOptions
			?? new CompiledTermInfoParserOptions();

		return new CompiledTermInfoParserOptions(
			effectiveOptions.MaximumEntrySize
		);
	}

	private static string GetRequiredFileName(
		string path
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace(path);

		string? fileName =
			Path.GetFileName(path);
		if (string.IsNullOrEmpty(fileName)) {
			throw new InvalidOperationException(
				$"The filesystem path '{path}' does not identify a file or directory name."
			);
		}

		return fileName;
	}

	private static bool IsConventionallyPlaced(
		string directoryName,
		string fileName,
		TerminalDescription terminal
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace(directoryName);
		ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
		ArgumentNullException.ThrowIfNull(terminal);

		if (!DeclaresIdentity(
				terminal,
				fileName
			)) {
			return false;
		}

		StringComparison pathComparison =
			OperatingSystem.IsWindows()
				? StringComparison.OrdinalIgnoreCase
				: StringComparison.Ordinal
		;

		string literalDirectory =
			fileName[0].ToString();
		if (string.Equals(
				directoryName,
				literalDirectory,
				pathComparison
			)) {
			return true;
		}

		if (fileName[0] > byte.MaxValue) {
			return false;
		}

		string hexadecimalDirectory =
			((byte)fileName[0]).ToString(
				"x2",
				CultureInfo.InvariantCulture
			);

		return string.Equals(
			directoryName,
			hexadecimalDirectory,
			pathComparison
		);
	}

	private static bool DeclaresIdentity(
		TerminalDescription terminal,
		string name
	) {
		ArgumentNullException.ThrowIfNull(terminal);
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		if (string.Equals(
				terminal.Name,
				name,
				StringComparison.Ordinal
			)) {
			return true;
		}

		return terminal.Aliases.Any(
			alias =>
				string.Equals(
					alias,
					name,
					StringComparison.Ordinal
				)
		);
	}

	internal static bool IsConventionalCatalogDirectoryName(
		string name
	) {
		ArgumentNullException.ThrowIfNull(name);

		if (name.Length == 1) {
			return !char.IsSurrogate(name[0]);
		}

		return name.Length == 2
			&& IsHexDigit(name[0])
			&& IsHexDigit(name[1]);
	}

	internal static bool IsCatalogReparsePoint(
		FileAttributes attributes
	) {
		return (attributes & FileAttributes.ReparsePoint) != 0;
	}

	internal static TermInfoDatabaseCatalogIssueKind ClassifyCatalogIoException(
		Exception exception
	) {
		ArgumentNullException.ThrowIfNull(exception);

		return exception is UnauthorizedAccessException
			? TermInfoDatabaseCatalogIssueKind.PermissionFailure
			: TermInfoDatabaseCatalogIssueKind.IoFailure
		;
	}

	private static TermInfoDatabaseCatalogIssue CreateFileSystemIssue(
		string path,
		Exception exception,
		string subject
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace(path);
		ArgumentNullException.ThrowIfNull(exception);
		ArgumentException.ThrowIfNullOrWhiteSpace(subject);

		TermInfoDatabaseCatalogIssueKind kind =
			ClassifyCatalogIoException(exception);
		string message =
			kind == TermInfoDatabaseCatalogIssueKind.PermissionFailure
				? $"Access to the {subject} was denied."
				: $"The {subject} could not be inspected because of an I/O failure."
		;

		return new TermInfoDatabaseCatalogIssue(
			kind,
			path,
			message
		);
	}

	private static TermInfoDatabaseCatalog CreateUnavailableCatalog(
		string root,
		Exception exception,
		string subject
	) {
		return new TermInfoDatabaseCatalog(
			root,
			TermInfoDatabaseCatalogKind.Unavailable,
			Array.Empty<TermInfoDatabaseCatalogEntry>(),
			new[] {
				CreateFileSystemIssue(
					root,
					exception,
					subject
				),
			},
			Array.Empty<string>()
		);
	}

	private static TermInfoDatabaseCatalog CreateEmptyCatalog(
		string root,
		TermInfoDatabaseCatalogKind kind
	) {
		return new TermInfoDatabaseCatalog(
			root,
			kind,
			Array.Empty<TermInfoDatabaseCatalogEntry>(),
			Array.Empty<TermInfoDatabaseCatalogIssue>(),
			Array.Empty<string>()
		);
	}

	private static bool IsCatalogIoException(
		Exception exception
	) {
		return exception is IOException
			|| exception is UnauthorizedAccessException;
	}

	private static bool IsHexDigit(
		char value
	) {
		return (value >= '0' && value <= '9')
			|| (value >= 'a' && value <= 'f')
			|| (value >= 'A' && value <= 'F');
	}
}
