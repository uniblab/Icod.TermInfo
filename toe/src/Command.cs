using System.Reflection;
using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo.Inspection;

namespace Icod.TermInfo.Toe;

/// <summary>
/// Implements the managed <c>toe</c> command.
/// </summary>
public static class Command {
	private const string CommandName = "toe";

	/// <summary>
	/// Runs the command with caller-owned standard streams.
	/// </summary>
	/// <param name="args">Command-line arguments excluding the executable name.</param>
	/// <param name="stdin">Standard input.</param>
	/// <param name="stdout">Standard output.</param>
	/// <param name="stderr">Standard error.</param>
	/// <param name="cancellationToken">Cancellation request.</param>
	/// <returns>The process exit status.</returns>
	public static async Task<int> RunAsync(
		string[] args,
		Stream stdin,
		Stream stdout,
		Stream stderr,
		CancellationToken cancellationToken = default
	) {
		ArgumentNullException.ThrowIfNull( args );
		ArgumentNullException.ThrowIfNull( stdin );
		ArgumentNullException.ThrowIfNull( stdout );
		ArgumentNullException.ThrowIfNull( stderr );

		if ( cancellationToken.IsCancellationRequested ) {
			return CommandExitCodes.Canceled;
		}

		try {
			if (
				args.Length == 1
				&& string.Equals(
					args[ 0 ],
					"--help",
					StringComparison.Ordinal
				)
			) {
				await WriteAsync(
					stdout,
					GetHelpText(),
					cancellationToken
				).ConfigureAwait( false );
				return CommandExitCodes.Success;
			}

			if (
				args.Length == 1
				&& (
					string.Equals(
						args[ 0 ],
						"-V",
						StringComparison.Ordinal
					)
					|| string.Equals(
						args[ 0 ],
						"--version",
						StringComparison.Ordinal
					)
				)
			) {
				await WriteAsync(
					stdout,
					$"{CommandName} (Icod.TermInfo) {GetSemanticVersion()}{Environment.NewLine}",
					cancellationToken
				).ConfigureAwait( false );
				return CommandExitCodes.Success;
			}

			if (
				args.Length == 1
				&& string.Equals(
					args[ 0 ],
					"-D",
					StringComparison.Ordinal
				)
			) {
				return await WriteDatabaseLocationsAsync(
					stdout,
					stderr,
					cancellationToken
				).ConfigureAwait( false );
			}

			if (
				!TryParseOptions(
					args,
					out ToeOptions options,
					out string usageError
				)
			) {
				await WriteUsageErrorAsync(
					stderr,
					usageError,
					cancellationToken
				).ConfigureAwait( false );
				return CommandExitCodes.UsageError;
			}

			ToeListingResult listing = BuildListing(
				options,
				cancellationToken
			);

			if ( listing.Stdout.Length != 0 ) {
				await WriteAsync(
					stdout,
					listing.Stdout,
					cancellationToken
				).ConfigureAwait( false );
			}

			if ( listing.Stderr.Length != 0 ) {
				await WriteAsync(
					stderr,
					listing.Stderr,
					cancellationToken
				).ConfigureAwait( false );
			}

			return listing.HasOperationalFailure
				? CommandExitCodes.Failure
				: CommandExitCodes.Success
			;
		} catch ( OperationCanceledException ) {
			return CommandExitCodes.Canceled;
		}
	}

	private static ToeListingResult BuildListing(
		ToeOptions options,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( options );

		bool explicitDirectories = options.Directories.Count != 0;
		var roots = new List<string>();
		var diagnostics = new StringBuilder();

		if ( explicitDirectories ) {
			roots.AddRange( options.Directories );
		} else {
			try {
				IReadOnlyList<TermInfoDatabaseLocation> locations = TermInfoDatabaseInspector.GetSystemLocations();

				foreach ( TermInfoDatabaseLocation location in locations ) {
					if ( location.Path is not null ) {
						roots.Add( location.Path );
					}
				}
			} catch ( Exception exception ) when ( IsOperationalException( exception ) ) {
				AppendDiagnostic(
					diagnostics,
					"TOE0001",
					"database discovery",
					exception.Message
				);
				return new ToeListingResult(
					string.Empty,
					diagnostics.ToString(),
					hasOperationalFailure: true
				);
			}
		}

		var output = new StringBuilder();
		bool hasOperationalFailure = false;
		bool stopAfterFirstConventional = !explicitDirectories
			&& !options.AllDatabases;

		foreach ( string root in roots ) {
			cancellationToken.ThrowIfCancellationRequested();

			TermInfoDatabaseCatalog catalog;
			try {
				catalog = TermInfoDatabaseInspector.InspectDirectory(
					root,
					parserOptions: null,
					cancellationToken: cancellationToken
				);
			} catch ( Exception exception ) when ( IsOperationalException( exception ) ) {
				AppendDiagnostic(
					diagnostics,
					"TOE0005",
					root,
					exception.Message
				);
				hasOperationalFailure = true;
				continue;
			}

			switch ( catalog.Kind ) {
				case TermInfoDatabaseCatalogKind.Missing:
					if ( explicitDirectories ) {
						AppendDiagnostic(
							diagnostics,
							"TOE0002",
							catalog.Root,
							"requested database root does not exist"
						);
						hasOperationalFailure = true;
					}
					break;

				case TermInfoDatabaseCatalogKind.UnsupportedStore:
					AppendDiagnostic(
						diagnostics,
						"TOE0003",
						catalog.Root,
						"database root is not a supported conventional directory"
					);
					hasOperationalFailure = true;
					break;

				case TermInfoDatabaseCatalogKind.Unavailable:
					if ( catalog.Issues.Count == 0 ) {
						AppendDiagnostic(
							diagnostics,
							"TOE0004",
							catalog.Root,
							"database root is unavailable"
						);
					} else {
						AppendCatalogIssues(
							diagnostics,
							catalog.Issues
						);
					}
					hasOperationalFailure = true;
					break;

				case TermInfoDatabaseCatalogKind.ConventionalDirectory:
					if ( options.ShowHeadings ) {
						output
							.Append( "# " )
							.Append( catalog.Root )
							.Append( Environment.NewLine );
					}

					IEnumerable<TermInfoDatabaseCatalogEntry> entries = catalog.Entries;
					if ( options.SortByName ) {
						entries = entries
							.OrderBy(
								entry => entry.Name,
								StringComparer.Ordinal
							)
							.ThenBy(
								entry => entry.Path,
								StringComparer.Ordinal
							);
					}

					foreach ( TermInfoDatabaseCatalogEntry entry in entries ) {
						cancellationToken.ThrowIfCancellationRequested();

						output
							.Append( entry.Name )
							.Append( '\t' )
							.Append( entry.Description ?? string.Empty )
							.Append( Environment.NewLine );
					}

					if ( catalog.Issues.Count != 0 ) {
						AppendCatalogIssues(
							diagnostics,
							catalog.Issues
						);
						hasOperationalFailure = true;
					}

					if ( stopAfterFirstConventional ) {
						return new ToeListingResult(
							output.ToString(),
							diagnostics.ToString(),
							hasOperationalFailure
						);
					}
					break;

				default:
					throw new InvalidOperationException(
						$"Unsupported database catalog kind '{catalog.Kind}'."
					);
			}
		}

		return new ToeListingResult(
			output.ToString(),
			diagnostics.ToString(),
			hasOperationalFailure
		);
	}

	private static void AppendCatalogIssues(
		StringBuilder diagnostics,
		IReadOnlyList<TermInfoDatabaseCatalogIssue> issues
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );
		ArgumentNullException.ThrowIfNull( issues );

		foreach ( TermInfoDatabaseCatalogIssue issue in issues ) {
			AppendDiagnostic(
				diagnostics,
				"TOE0004",
				$"{issue.Kind}: {issue.Path}",
				issue.Message
			);
		}
	}

	private static void AppendDiagnostic(
		StringBuilder diagnostics,
		string code,
		string subject,
		string message
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );
		ArgumentException.ThrowIfNullOrWhiteSpace( code );
		ArgumentNullException.ThrowIfNull( subject );
		ArgumentNullException.ThrowIfNull( message );

		diagnostics
			.Append( CommandName )
			.Append( ": " )
			.Append( code )
			.Append( ": " )
			.Append( subject )
			.Append( ": " )
			.Append( message )
			.Append( Environment.NewLine );
	}

	private static bool TryParseOptions(
		string[] args,
		out ToeOptions options,
		out string error
	) {
		ArgumentNullException.ThrowIfNull( args );

		bool allDatabases = false;
		bool showHeadings = false;
		bool sortByName = false;
		bool operandsOnly = false;
		var directories = new List<string>();

		foreach ( string argument in args ) {
			if ( operandsOnly ) {
				if ( argument.Length == 0 ) {
					options = ToeOptions.Empty;
					error = "directory operands must not be empty";
					return false;
				}

				directories.Add( argument );
				continue;
			}

			if ( string.Equals( argument, "--", StringComparison.Ordinal ) ) {
				operandsOnly = true;
				continue;
			}

			switch ( argument ) {
				case "-a":
					allDatabases = true;
					break;

				case "-h":
					showHeadings = true;
					break;

				case "-s":
					sortByName = true;
					break;

				case "-D":
				case "-V":
				case "--version":
				case "--help":
					options = ToeOptions.Empty;
					error = $"option '{argument}' must be used alone";
					return false;

				case "-u":
				case "-U":
					options = ToeOptions.Empty;
					error = $"source dependency option '{argument}' is introduced by T09 and is not available in T08";
					return false;

				default:
					if ( argument.StartsWith( "-", StringComparison.Ordinal ) ) {
						options = ToeOptions.Empty;
						error = $"unsupported option '{argument}'";
						return false;
					}

					if ( argument.Length == 0 ) {
						options = ToeOptions.Empty;
						error = "directory operands must not be empty";
						return false;
					}

					directories.Add( argument );
					break;
			}
		}

		options = new ToeOptions(
			allDatabases,
			showHeadings,
			sortByName,
			directories.ToArray()
		);
		error = string.Empty;
		return true;
	}

	private static async Task<int> WriteDatabaseLocationsAsync(
		Stream stdout,
		Stream stderr,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stdout );
		ArgumentNullException.ThrowIfNull( stderr );

		IReadOnlyList<TermInfoDatabaseLocation> locations;
		try {
			locations = TermInfoDatabaseInspector.GetSystemLocations();
		} catch ( Exception exception ) when ( IsOperationalException( exception ) ) {
			var diagnostics = new StringBuilder();
			AppendDiagnostic(
				diagnostics,
				"TOE0001",
				"database discovery",
				exception.Message
			);
			await WriteAsync(
				stderr,
				diagnostics.ToString(),
				cancellationToken
			).ConfigureAwait( false );
			return CommandExitCodes.Failure;
		}

		var output = new StringBuilder();
		foreach ( TermInfoDatabaseLocation location in locations ) {
			cancellationToken.ThrowIfCancellationRequested();

			output
				.Append( location.Kind )
				.Append( '\t' )
				.Append( location.Path ?? "<encoded>" )
				.Append( Environment.NewLine );
		}

		if ( output.Length != 0 ) {
			await WriteAsync(
				stdout,
				output.ToString(),
				cancellationToken
			).ConfigureAwait( false );
		}
		return CommandExitCodes.Success;
	}

	private static async Task WriteUsageErrorAsync(
		Stream stderr,
		string detail,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stderr );
		ArgumentException.ThrowIfNullOrWhiteSpace( detail );

		await WriteAsync(
			stderr,
			$"{CommandName}: {detail}.{Environment.NewLine}"
				+ $"Try '{CommandName} --help' for more information.{Environment.NewLine}",
			cancellationToken
		).ConfigureAwait( false );
	}

	private static string GetHelpText() {
		return $"Usage: {CommandName} [options] [directory ...]{Environment.NewLine}"
			+ $"       {CommandName} -D{Environment.NewLine}"
			+ $"       {CommandName} -V | --version{Environment.NewLine}"
			+ Environment.NewLine
			+ "List parsed terminal descriptions from conventional terminfo databases."
			+ Environment.NewLine
			+ Environment.NewLine
			+ "With directory operands, inspect exactly those roots in operand order."
			+ Environment.NewLine
			+ "Without directory operands, use Runtime discovery order and inspect the first applicable conventional database."
			+ Environment.NewLine
			+ Environment.NewLine
			+ "Options:"
			+ Environment.NewLine
			+ "  -a              inspect all discovered conventional databases"
			+ Environment.NewLine
			+ "  -h              write a heading naming each inspected database"
			+ Environment.NewLine
			+ "  -s              sort entries by canonical terminal name"
			+ Environment.NewLine
			+ "  -D              print Runtime database discovery locations and exit"
			+ Environment.NewLine
			+ "  -V, --version   print version information and exit"
			+ Environment.NewLine
			+ "      --help      display this help and exit"
			+ Environment.NewLine;
	}

	private static string GetSemanticVersion() {
		string informationalVersion = typeof( Command )
			.Assembly
			.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
			?.InformationalVersion
			?? "0.0.0";
		int metadataSeparator = informationalVersion.IndexOf(
			'+',
			StringComparison.Ordinal
		);

		return (metadataSeparator < 0)
			? informationalVersion
			: informationalVersion[ ..metadataSeparator ]
		;
	}

	private static bool IsOperationalException( Exception exception ) {
		ArgumentNullException.ThrowIfNull( exception );

		return exception is ArgumentException
			or IOException
			or UnauthorizedAccessException
			or NotSupportedException
			or InvalidOperationException;
	}

	private static async Task WriteAsync(
		Stream stream,
		string text,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stream );
		ArgumentNullException.ThrowIfNull( text );

		using StreamWriter writer = new(
			stream,
			new UTF8Encoding( false ),
			bufferSize: 1024,
			leaveOpen: true
		);
		await writer.WriteAsync(
			text.AsMemory(),
			cancellationToken
		).ConfigureAwait( false );
		await writer.FlushAsync(
			cancellationToken
		).ConfigureAwait( false );
	}

	private sealed class ToeOptions {
		public static ToeOptions Empty { get; } = new(
			allDatabases: false,
			showHeadings: false,
			sortByName: false,
			directories: Array.Empty<string>()
		);

		public ToeOptions(
			bool allDatabases,
			bool showHeadings,
			bool sortByName,
			IReadOnlyList<string> directories
		) {
			ArgumentNullException.ThrowIfNull( directories );

			AllDatabases = allDatabases;
			ShowHeadings = showHeadings;
			SortByName = sortByName;
			Directories = directories;
		}

		public bool AllDatabases { get; }

		public bool ShowHeadings { get; }

		public bool SortByName { get; }

		public IReadOnlyList<string> Directories { get; }
	}

	private sealed class ToeListingResult {
		public ToeListingResult(
			string stdout,
			string stderr,
			bool hasOperationalFailure
		) {
			ArgumentNullException.ThrowIfNull( stdout );
			ArgumentNullException.ThrowIfNull( stderr );

			Stdout = stdout;
			Stderr = stderr;
			HasOperationalFailure = hasOperationalFailure;
		}

		public string Stdout { get; }

		public string Stderr { get; }

		public bool HasOperationalFailure { get; }
	}
}
