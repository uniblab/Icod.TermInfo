using System.Globalization;
using Icod.TermInfo;

namespace Icod.TermInfo.Compiler;

/// <summary>
/// Compiles or publishes terminfo entries into one caller-supplied conventional
/// terminfo directory tree.
/// </summary>
/// <remarks>
/// The writer never discovers or modifies a process-global or system terminfo
/// database. All output is rooted beneath the explicit <c>root</c> argument.
/// </remarks>
public static class CompiledTermInfoDatabaseWriter {
	private const int FileBufferSize = 4096;

	/// <summary>
	/// Publishes a successful source-compilation result.
	/// </summary>
	/// <param name="root">The explicit terminfo database root.</param>
	/// <param name="compilation">The completed source compilation.</param>
	/// <param name="options">Optional database publication policy.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="root"/> or <paramref name="compilation"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="root"/> is empty or whitespace.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// <paramref name="compilation"/> contains an error diagnostic.
	/// </exception>
	/// <exception cref="IOException">
	/// A destination cannot be published safely or overwrite policy rejects an
	/// existing destination.
	/// </exception>
	public static void Write(
		string root,
		TermInfoSourceCompilationResult compilation,
		CompiledTermInfoDatabaseWriterOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( root );
		ArgumentNullException.ThrowIfNull( compilation );
		ValidateRoot( root );

		if ( compilation.HasErrors ) {
			throw new InvalidOperationException(
				"A source compilation containing errors cannot be published."
			);
		}

		WriteCore(
			Path.GetFullPath( root ),
			compilation.Entries,
			options ?? new CompiledTermInfoDatabaseWriterOptions()
		);
	}

	/// <summary>
	/// Compiles and publishes one resolved terminal description.
	/// </summary>
	/// <param name="root">The explicit terminfo database root.</param>
	/// <param name="description">The resolved terminal description.</param>
	/// <param name="writerOptions">Optional compiled representation policy.</param>
	/// <param name="databaseOptions">Optional database publication policy.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="root"/> or <paramref name="description"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="root"/> is empty or whitespace, or a terminal identity
	/// cannot be used safely as a database file name.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// The description cannot be represented by the requested compiled format.
	/// </exception>
	/// <exception cref="IOException">
	/// A destination cannot be published safely or overwrite policy rejects an
	/// existing destination.
	/// </exception>
	public static void Write(
		string root,
		TerminalDescription description,
		CompiledTermInfoWriterOptions? writerOptions = null,
		CompiledTermInfoDatabaseWriterOptions? databaseOptions = null
	) {
		ArgumentNullException.ThrowIfNull( root );
		ArgumentNullException.ThrowIfNull( description );
		ValidateRoot( root );

		WriteDescriptionsCore(
			Path.GetFullPath( root ),
			new[] { description },
			writerOptions ?? new CompiledTermInfoWriterOptions(),
			databaseOptions ?? new CompiledTermInfoDatabaseWriterOptions()
		);
	}

	/// <summary>
	/// Compiles and publishes a sequence of resolved terminal descriptions.
	/// </summary>
	/// <param name="root">The explicit terminfo database root.</param>
	/// <param name="descriptions">The resolved descriptions to publish.</param>
	/// <param name="writerOptions">Optional compiled representation policy.</param>
	/// <param name="databaseOptions">Optional database publication policy.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="root"/> or <paramref name="descriptions"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="root"/> is empty or whitespace, a description is
	/// <see langword="null"/>, or a terminal identity cannot be used safely as a
	/// database file name.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// A description cannot be represented by the requested compiled format or
	/// multiple identities map to the same destination.
	/// </exception>
	/// <exception cref="IOException">
	/// A destination cannot be published safely or overwrite policy rejects an
	/// existing destination.
	/// </exception>
	public static void Write(
		string root,
		IEnumerable<TerminalDescription> descriptions,
		CompiledTermInfoWriterOptions? writerOptions = null,
		CompiledTermInfoDatabaseWriterOptions? databaseOptions = null
	) {
		ArgumentNullException.ThrowIfNull( root );
		ArgumentNullException.ThrowIfNull( descriptions );
		ValidateRoot( root );

		WriteDescriptionsCore(
			Path.GetFullPath( root ),
			descriptions,
			writerOptions ?? new CompiledTermInfoWriterOptions(),
			databaseOptions ?? new CompiledTermInfoDatabaseWriterOptions()
		);
	}

	/// <summary>
	/// Publishes a sequence of independently compiled source entries.
	/// </summary>
	/// <param name="root">The explicit terminfo database root.</param>
	/// <param name="entries">The compiled entries to publish.</param>
	/// <param name="options">Optional database publication policy.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="root"/> or <paramref name="entries"/> is
	/// <see langword="null"/>.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// <paramref name="root"/> is empty or whitespace, an entry is
	/// <see langword="null"/>, or an entry name cannot be used safely as a
	/// database file name.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// Multiple entries or aliases map to the same destination.
	/// </exception>
	/// <exception cref="IOException">
	/// A destination cannot be published safely or overwrite policy rejects an
	/// existing destination.
	/// </exception>
	public static void Write(
		string root,
		IEnumerable<CompiledTermInfoSourceEntry> entries,
		CompiledTermInfoDatabaseWriterOptions? options = null
	) {
		ArgumentNullException.ThrowIfNull( root );
		ArgumentNullException.ThrowIfNull( entries );
		ValidateRoot( root );

		WriteCore(
			Path.GetFullPath( root ),
			entries,
			options ?? new CompiledTermInfoDatabaseWriterOptions()
		);
	}

	private static void WriteCore(
		string root,
		IEnumerable<CompiledTermInfoSourceEntry> entries,
		CompiledTermInfoDatabaseWriterOptions options
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentNullException.ThrowIfNull( entries );
		ArgumentNullException.ThrowIfNull( options );

		PublishCore(
			root,
			PreparePublications(
				root,
				entries
			),
			options
		);
	}

	private static void WriteDescriptionsCore(
		string root,
		IEnumerable<TerminalDescription> descriptions,
		CompiledTermInfoWriterOptions writerOptions,
		CompiledTermInfoDatabaseWriterOptions databaseOptions
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentNullException.ThrowIfNull( descriptions );
		ArgumentNullException.ThrowIfNull( writerOptions );
		ArgumentNullException.ThrowIfNull( databaseOptions );

		PublishCore(
			root,
			PreparePublications(
				root,
				descriptions,
				writerOptions
			),
			databaseOptions
		);
	}

	private static void PublishCore(
		string root,
		Publication[] publications,
		CompiledTermInfoDatabaseWriterOptions options
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentNullException.ThrowIfNull( publications );
		ArgumentNullException.ThrowIfNull( options );

		if ( publications.Length == 0 ) {
			return;
		}

		if ( File.Exists( root ) ) {
			throw new IOException(
				$"The terminfo output root '{root}' identifies a file."
			);
		}

		PreflightDestinations(
			publications,
			options.OverwriteExisting
		);
		EnsurePublicationDirectories( publications );

		StagedPublication[] staged =
			StagePublications( publications );

		try {
			foreach ( StagedPublication item in staged ) {
				File.Move(
					item.TemporaryPath,
					item.DestinationPath,
					options.OverwriteExisting
				);
			}
		}
		finally {
			foreach ( StagedPublication item in staged ) {
				TryDeleteTemporaryFile(
					item.TemporaryPath
				);
			}
		}
	}

	private static Publication[] PreparePublications(
		string root,
		IEnumerable<CompiledTermInfoSourceEntry> entries
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentNullException.ThrowIfNull( entries );

		List<Publication> publications = [];
		HashSet<string> destinations =
			new( GetPathComparer() );

		foreach ( CompiledTermInfoSourceEntry? entry in entries ) {
			if ( entry is null ) {
				throw new ArgumentException(
					"The compiled entry sequence cannot contain null.",
					nameof( entries )
				);
			}

			byte[] data = entry.Data;
			AddPublication(
				root,
				entry.CanonicalName,
				data,
				destinations,
				publications,
				nameof( entries )
			);

			foreach ( string alias in entry.Aliases ) {
				AddPublication(
					root,
					alias,
					data,
					destinations,
					publications,
					nameof( entries )
				);
			}
		}

		return publications.ToArray();
	}

	private static Publication[] PreparePublications(
		string root,
		IEnumerable<TerminalDescription> descriptions,
		CompiledTermInfoWriterOptions writerOptions
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentNullException.ThrowIfNull( descriptions );
		ArgumentNullException.ThrowIfNull( writerOptions );

		List<Publication> publications = [];
		HashSet<string> destinations =
			new( GetPathComparer() );

		foreach ( TerminalDescription? description in descriptions ) {
			if ( description is null ) {
				throw new ArgumentException(
					"The terminal description sequence cannot contain null.",
					nameof( descriptions )
				);
			}

			byte[] data =
				CompiledTermInfoWriter.Write(
					description,
					writerOptions
				);
			AddPublication(
				root,
				description.Name,
				data,
				destinations,
				publications,
				nameof( descriptions )
			);

			foreach ( string alias in description.Aliases ) {
				AddPublication(
					root,
					alias,
					data,
					destinations,
					publications,
					nameof( descriptions )
				);
			}
		}

		return publications.ToArray();
	}

	private static void AddPublication(
		string root,
		string name,
		byte[] data,
		ISet<string> destinations,
		ICollection<Publication> publications,
		string parameterName
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentNullException.ThrowIfNull( name );
		ArgumentNullException.ThrowIfNull( data );
		ArgumentNullException.ThrowIfNull( destinations );
		ArgumentNullException.ThrowIfNull( publications );
		ArgumentException.ThrowIfNullOrWhiteSpace( parameterName );

		ValidateTerminalName(
			name,
			parameterName
		);
		string destinationPath =
			GetDestinationPath(
				root,
				name
			);

		if ( !destinations.Add( destinationPath ) ) {
			throw new InvalidOperationException(
				$"Multiple compiled identities map to '{destinationPath}'."
			);
		}

		publications.Add(
			new Publication(
				destinationPath,
				data
			)
		);
	}

	private static string GetDestinationPath(
		string root,
		string name
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		string directoryName =
			((byte)name[0]).ToString(
				"x2",
				CultureInfo.InvariantCulture
			);
		string destinationPath =
			Path.GetFullPath(
				Path.Combine(
					root,
					directoryName,
					name
				)
			);
		string relativePath =
			Path.GetRelativePath(
				root,
				destinationPath
			);

		if ( Path.IsPathRooted( relativePath )
			|| string.Equals(
				relativePath,
				"..",
				StringComparison.Ordinal
			)
			|| relativePath.StartsWith(
				$"..{Path.DirectorySeparatorChar}",
				StringComparison.Ordinal
			)
			|| relativePath.StartsWith(
				$"..{Path.AltDirectorySeparatorChar}",
				StringComparison.Ordinal
			) ) {
			throw new ArgumentException(
				"The terminal name escapes the requested output root.",
				nameof( name )
			);
		}

		return destinationPath;
	}

	private static void PreflightDestinations(
		IEnumerable<Publication> publications,
		bool overwriteExisting
	) {
		ArgumentNullException.ThrowIfNull( publications );

		foreach ( Publication publication in publications ) {
			if ( Directory.Exists( publication.DestinationPath ) ) {
				throw new IOException(
					$"The compiled entry destination '{publication.DestinationPath}' identifies a directory."
				);
			}

			if ( !File.Exists( publication.DestinationPath ) ) {
				continue;
			}

			RejectReparsePoint(
				new FileInfo(
					publication.DestinationPath
				)
			);

			if ( !overwriteExisting ) {
				throw new IOException(
					$"The compiled entry destination '{publication.DestinationPath}' already exists."
				);
			}
		}
	}

	private static void EnsurePublicationDirectories(
		IEnumerable<Publication> publications
	) {
		ArgumentNullException.ThrowIfNull( publications );

		HashSet<string> directories =
			new( GetPathComparer() );

		foreach ( Publication publication in publications ) {
			string directory =
				Path.GetDirectoryName(
					publication.DestinationPath
				)
				?? throw new InvalidOperationException(
					"The compiled entry destination has no parent directory."
				);
			directories.Add( directory );
		}

		foreach ( string directory in directories ) {
			if ( Directory.Exists( directory ) ) {
				RejectReparsePoint(
					new DirectoryInfo( directory )
				);
				continue;
			}

			Directory.CreateDirectory( directory );
			RejectReparsePoint(
				new DirectoryInfo( directory )
			);
		}
	}

	private static StagedPublication[] StagePublications(
		IEnumerable<Publication> publications
	) {
		ArgumentNullException.ThrowIfNull( publications );

		List<StagedPublication> staged = [];

		try {
			foreach ( Publication publication in publications ) {
				string directory =
					Path.GetDirectoryName(
						publication.DestinationPath
					)
					?? throw new InvalidOperationException(
						"The compiled entry destination has no parent directory."
					);
				string temporaryPath =
					Path.Combine(
						directory,
						$".icod-terminfo-{Guid.NewGuid():N}.tmp"
					);

				try {
					using FileStream stream =
						new(
							temporaryPath,
							FileMode.CreateNew,
							FileAccess.Write,
							FileShare.None,
							FileBufferSize,
							FileOptions.WriteThrough
						);
					stream.Write( publication.Data );
					stream.Flush(
						flushToDisk: true
					);
				}
				catch {
					TryDeleteTemporaryFile(
						temporaryPath
					);
					throw;
				}

				staged.Add(
					new StagedPublication(
						temporaryPath,
						publication.DestinationPath
					)
				);
			}

			return staged.ToArray();
		}
		catch {
			foreach ( StagedPublication item in staged ) {
				TryDeleteTemporaryFile(
					item.TemporaryPath
				);
			}
			throw;
		}
	}

	private static void RejectReparsePoint(
		FileSystemInfo info
	) {
		ArgumentNullException.ThrowIfNull( info );

		info.Refresh();
		if ( ( info.Attributes & FileAttributes.ReparsePoint ) != 0
			|| info.LinkTarget is not null ) {
			throw new IOException(
				$"Refusing to publish through reparse point '{info.FullName}'."
			);
		}
	}

	private static void TryDeleteTemporaryFile(
		string path
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( path );

		try {
			File.Delete( path );
		}
		catch ( IOException ) {
		}
		catch ( UnauthorizedAccessException ) {
		}
	}

	private static void ValidateRoot(
		string root
	) {
		ArgumentNullException.ThrowIfNull( root );

		if ( string.IsNullOrWhiteSpace( root ) ) {
			throw new ArgumentException(
				"The terminfo output root cannot be empty or whitespace.",
				nameof( root )
			);
		}
	}

	private static void ValidateTerminalName(
		string name,
		string parameterName
	) {
		ArgumentNullException.ThrowIfNull( name );
		ArgumentException.ThrowIfNullOrWhiteSpace( parameterName );

		if ( string.IsNullOrWhiteSpace( name ) ) {
			throw new ArgumentException(
				"The terminal name cannot be empty or whitespace.",
				parameterName
			);
		}

		if ( string.Equals(
				name,
				".",
				StringComparison.Ordinal
			)
			|| string.Equals(
				name,
				"..",
				StringComparison.Ordinal
			)
			|| Path.IsPathRooted( name ) ) {
			throw new ArgumentException(
				"The terminal name must be an exact non-rooted file name.",
				parameterName
			);
		}

		foreach ( char character in name ) {
			if ( character > byte.MaxValue
				|| character == '\0'
				|| character == '/'
				|| character == '\\'
				|| char.IsControl( character )
				|| char.IsSurrogate( character ) ) {
				throw new ArgumentException(
					"The terminal name contains unsafe path syntax.",
					parameterName
				);
			}
		}

		if ( name.IndexOfAny(
				Path.GetInvalidFileNameChars()
			) >= 0 ) {
			throw new ArgumentException(
				"The terminal name contains a character which is invalid in a file name on this platform.",
				parameterName
			);
		}

		if ( OperatingSystem.IsWindows() ) {
			ValidateWindowsTerminalName(
				name,
				parameterName
			);
		}
	}

	private static void ValidateWindowsTerminalName(
		string name,
		string parameterName
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );
		ArgumentException.ThrowIfNullOrWhiteSpace( parameterName );

		if ( name[^1] == '.'
			|| name[^1] == ' ' ) {
			throw new ArgumentException(
				"The terminal name cannot end with a period or space on Windows.",
				parameterName
			);
		}

		int dot = name.IndexOf( '.' );
		string stem =
			( dot < 0 )
				? name
				: name[..dot]
		;

		if ( IsWindowsDeviceStem( stem ) ) {
			throw new ArgumentException(
				"The terminal name conflicts with a reserved Windows device name.",
				parameterName
			);
		}
	}

	private static bool IsWindowsDeviceStem(
		string stem
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( stem );

		if ( stem.Equals(
				"CON",
				StringComparison.OrdinalIgnoreCase
			)
			|| stem.Equals(
				"PRN",
				StringComparison.OrdinalIgnoreCase
			)
			|| stem.Equals(
				"AUX",
				StringComparison.OrdinalIgnoreCase
			)
			|| stem.Equals(
				"NUL",
				StringComparison.OrdinalIgnoreCase
			)
			|| stem.Equals(
				"CONIN$",
				StringComparison.OrdinalIgnoreCase
			)
			|| stem.Equals(
				"CONOUT$",
				StringComparison.OrdinalIgnoreCase
			) ) {
			return true;
		}

		if ( stem.Length != 4 ) {
			return false;
		}

		bool numberedDevice =
			stem.StartsWith(
				"COM",
				StringComparison.OrdinalIgnoreCase
			)
			|| stem.StartsWith(
				"LPT",
				StringComparison.OrdinalIgnoreCase
			);

		return numberedDevice
			&& stem[3] >= '1'
			&& stem[3] <= '9';
	}

	private static StringComparer GetPathComparer() {
		if ( OperatingSystem.IsWindows()
			|| OperatingSystem.IsMacOS() ) {
			return StringComparer.OrdinalIgnoreCase;
		}

		return StringComparer.Ordinal;
	}

	private sealed class Publication {
		internal Publication(
			string destinationPath,
			byte[] data
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( destinationPath );
			ArgumentNullException.ThrowIfNull( data );

			DestinationPath = destinationPath;
			Data = data;
		}

		internal string DestinationPath { get; }

		internal byte[] Data { get; }
	}

	private sealed class StagedPublication {
		internal StagedPublication(
			string temporaryPath,
			string destinationPath
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( temporaryPath );
			ArgumentException.ThrowIfNullOrWhiteSpace( destinationPath );

			TemporaryPath = temporaryPath;
			DestinationPath = destinationPath;
		}

		internal string TemporaryPath { get; }

		internal string DestinationPath { get; }
	}
}
