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
			ToeSourceCommandLineResult sourceMode =
				ToeCommandLine.ParseSourceMode( args );
			if ( sourceMode.IsSourceMode ) {
				if ( sourceMode.Error is string sourceUsageError ) {
					await WriteUsageErrorAsync(
						stderr,
						sourceUsageError,
						cancellationToken
					).ConfigureAwait( false );
					return CommandExitCodes.UsageError;
				}

				ToeSourceDependencyResult dependency =
					await ToeSourceDependencyAnalyzer.AnalyzeAsync(
						sourceMode.SourcePath
							?? throw new InvalidOperationException(
								"The toe source-mode parser returned no source path."
							),
						sourceMode.Mode,
						cancellationToken
					).ConfigureAwait( false );

				if ( dependency.Stdout.Length != 0 ) {
					await WriteAsync(
						stdout,
						dependency.Stdout,
						cancellationToken
					).ConfigureAwait( false );
				}
				if ( dependency.Stderr.Length != 0 ) {
					await WriteAsync(
						stderr,
						dependency.Stderr,
						cancellationToken
					).ConfigureAwait( false );
				}

				return dependency.HasOperationalFailure
					? CommandExitCodes.Failure
					: CommandExitCodes.Success
				;
			}

			ToeCommandLineNormalizationResult normalized =
				ToeCommandLine.NormalizeListing( args );
			if ( normalized.Error is string normalizationError ) {
				await WriteUsageErrorAsync(
					stderr,
					normalizationError,
					cancellationToken
				).ConfigureAwait( false );
				return CommandExitCodes.UsageError;
			}
			args =
				normalized.Arguments
				?? throw new InvalidOperationException(
					"The toe command-line normalizer returned neither arguments nor an error."
				);

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

			if ( options.Json ) {
				return await RenderCatalogAsync(
					options.Directories[ 0 ],
					stdout,
					stderr,
					cancellationToken
				).ConfigureAwait( false );
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

	private static async Task<int> RenderCatalogAsync(
		string directory,
		Stream stdout,
		Stream stderr,
		CancellationToken cancellationToken
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( directory );
		ArgumentNullException.ThrowIfNull( stdout );
		ArgumentNullException.ThrowIfNull( stderr );
		cancellationToken.ThrowIfCancellationRequested();

		string rendered;
		try {
			TermInfoDatabaseCatalog catalog =
				TermInfoDatabaseInspector.InspectDirectory(
					directory,
					parserOptions: null,
					cancellationToken: cancellationToken
				);
			rendered =
				TermInfoJsonRenderer.Render(
					catalog,
					new TermInfoJsonRendererOptions(),
					cancellationToken
				)
				+ "\n";
		} catch ( Exception exception ) when ( IsOperationalException( exception ) ) {
			var diagnostics = new StringBuilder();
			AppendDiagnostic(
				diagnostics,
				"TOE0005",
				directory,
				exception.Message
			);
			await WriteAsync(
				stderr,
				diagnostics.ToString(),
				cancellationToken
			).ConfigureAwait( false );
			return CommandExitCodes.Failure;
		}

		await WriteAsync(
			stdout,
			rendered,
			cancellationToken
		).ConfigureAwait( false );
		return CommandExitCodes.Success;
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
		Dictionary<string, ToeDuplicateReference>? duplicateReferences =
			options.AllDatabases && options.SortByName
				? new Dictionary<string, ToeDuplicateReference>( StringComparer.Ordinal )
				: null;

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

					var namesInCurrentRoot = duplicateReferences is null
						? null
						: new HashSet<string>( StringComparer.Ordinal );
					foreach ( TermInfoDatabaseCatalogEntry entry in entries ) {
						cancellationToken.ThrowIfCancellationRequested();

						output
							.Append( entry.Name )
							.Append( '\t' )
							.Append( entry.Description ?? string.Empty )
							.Append( Environment.NewLine );

						if (
							duplicateReferences is null
							|| !(namesInCurrentRoot?.Add( entry.Name ) ?? false)
						) {
							continue;
						}

						if (
							duplicateReferences.TryGetValue(
								entry.Name,
								out ToeDuplicateReference? first
							)
						) {
							if (
								string.Equals(
									first.Root,
									catalog.Root,
									StringComparison.Ordinal
								)
							) {
								continue;
							}

							bool areEqual = TerminalDescriptionComparer.Compare(
								first.Terminal,
								entry.Terminal
							).AreEqual;
							output
								.Append( "# Icod duplicate " )
								.Append( entry.Name )
								.Append( ": semantically " )
								.Append( areEqual ? "equal to " : "different from " )
								.Append( first.Root )
								.Append( Environment.NewLine );
						} else {
							duplicateReferences.Add(
								entry.Name,
								new ToeDuplicateReference(
									entry.Terminal,
									catalog.Root
								)
							);
						}
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
			.Append( subject )
			.Append( ": " )
			.Append( code )
			.Append( " error: " )
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
		bool json = false;
		bool jsonSpecified = false;
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

				case "--json":
					if ( jsonSpecified ) {
						options = ToeOptions.Empty;
						error = "option '--json' may be specified only once";
						return false;
					}
					json = true;
					jsonSpecified = true;
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
					error = $"source dependency option '{argument}' is a standalone mode and cannot be combined with listing options or directory operands";
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

		if ( json ) {
			if ( allDatabases || showHeadings || sortByName ) {
				options = ToeOptions.Empty;
				error = "options '-a', '-h', and '-s' cannot be combined with '--json'";
				return false;
			}
			if ( directories.Count != 1 ) {
				options = ToeOptions.Empty;
				error = "option '--json' requires exactly one explicit directory operand";
				return false;
			}
		}

		options = new ToeOptions(
			allDatabases,
			showHeadings,
			sortByName,
			json,
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
			+ $"       {CommandName} --json directory{Environment.NewLine}"
			+ $"       {CommandName} -u file{Environment.NewLine}"
			+ $"       {CommandName} -U file{Environment.NewLine}"
			+ $"       {CommandName} -D{Environment.NewLine}"
			+ $"       {CommandName} -V | --version{Environment.NewLine}"
			+ Environment.NewLine
			+ "List parsed terminal descriptions from conventional terminfo databases or analyze terminfo source dependencies."
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
			+ "  -s              sort entries by canonical terminal name; with -a, mark semantic duplicates"
			+ Environment.NewLine
			+ "      --json      inspect exactly one explicit directory and emit its databaseCatalog document"
			+ Environment.NewLine
			+ "  -u file         list forward use= dependencies in source order"
			+ Environment.NewLine
			+ "  -U file         list reverse use= dependencies deterministically"
			+ Environment.NewLine
			+ "  -D              print Runtime database discovery locations and exit"
			+ Environment.NewLine
			+ "  -V, --version   print version information and exit"
			+ Environment.NewLine
			+ "      --help      display this help and exit"
			+ Environment.NewLine
			+ Environment.NewLine
			+ "Unambiguous listing options may be clustered; -u/-U accept attached source paths; use -- before a directory or source filename beginning with '-'."
			+ Environment.NewLine
			+ "JSON mode rejects listing presentation switches and writes one document followed by exactly one LF."
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
			json: false,
			directories: Array.Empty<string>()
		);

		public ToeOptions(
			bool allDatabases,
			bool showHeadings,
			bool sortByName,
			bool json,
			IReadOnlyList<string> directories
		) {
			ArgumentNullException.ThrowIfNull( directories );

			AllDatabases = allDatabases;
			ShowHeadings = showHeadings;
			SortByName = sortByName;
			Json = json;
			Directories = directories;
		}

		public bool AllDatabases { get; }

		public bool ShowHeadings { get; }

		public bool SortByName { get; }

		public bool Json { get; }

		public IReadOnlyList<string> Directories { get; }
	}

	private sealed class ToeDuplicateReference {
		public ToeDuplicateReference(
			TerminalDescription terminal,
			string root
		) {
			ArgumentNullException.ThrowIfNull( terminal );
			ArgumentException.ThrowIfNullOrWhiteSpace( root );

			Terminal = terminal;
			Root = root;
		}

		public TerminalDescription Terminal { get; }

		public string Root { get; }
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
