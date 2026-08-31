using System.Globalization;
using System.Reflection;
using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Termcap;

namespace Icod.TermInfo.CapToInfo;

/// <summary>
/// Implements the managed <c>captoinfo</c> conversion command.
/// </summary>
public static class Command {
	private const string CommandName = "captoinfo";

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
			if ( IsSingleArgument( args, "--help" ) || IsSingleArgument( args, "-h" ) ) {
				await WriteAsync(
					stdout,
					GetHelpText(),
					cancellationToken
				).ConfigureAwait( false );
				return CommandExitCodes.Success;
			}
			if ( IsSingleArgument( args, "--version" ) || IsSingleArgument( args, "-V" ) ) {
				await WriteAsync(
					stdout,
					$"{CommandName} (Icod.TermInfo) {GetSemanticVersion()}{Environment.NewLine}",
					cancellationToken
				).ConfigureAwait( false );
				return CommandExitCodes.Success;
			}

			CommandOptionsParseResult parsedOptions =
				ParseOptions( args );
			if ( parsedOptions.Error is string usageError ) {
				await WriteUsageErrorAsync(
					stderr,
					usageError,
					cancellationToken
				).ConfigureAwait( false );
				return CommandExitCodes.UsageError;
			}

			CommandOptions options =
				parsedOptions.Options
				?? throw new InvalidOperationException(
					"The captoinfo option parser returned neither options nor an error."
				);

			if ( options.Operands.Count == 0 ) {
				return await ConvertEnvironmentEntryAsync(
					options,
					stdout,
					stderr,
					cancellationToken
				).ConfigureAwait( false );
			}

			bool standardInputConsumed = false;
			foreach ( string operand in options.Operands ) {
				cancellationToken.ThrowIfCancellationRequested();

				if ( operand == "-" ) {
					if ( standardInputConsumed ) {
						await WriteUsageErrorAsync(
							stderr,
							"standard input may be specified at most once",
							cancellationToken
						).ConfigureAwait( false );
						return CommandExitCodes.UsageError;
					}
					standardInputConsumed = true;

					using TextReader reader =
						CreateReader(
							stdin
						);
					int status =
						await ConvertReaderAsync(
							reader,
							"<stdin>",
							options,
							stdout,
							stderr,
							cancellationToken
						).ConfigureAwait( false );
					if ( status != CommandExitCodes.Success ) {
						return status;
					}
					continue;
				}

				try {
					using TextReader reader =
						CreateReader(
							operand
						);
					int status =
						await ConvertReaderAsync(
							reader,
							operand,
							options,
							stdout,
							stderr,
							cancellationToken
						).ConfigureAwait( false );
					if ( status != CommandExitCodes.Success ) {
						return status;
					}
				} catch ( Exception exception ) when ( IsOperationalException( exception ) ) {
					await WriteOperationalErrorAsync(
						stderr,
						operand,
						exception.Message,
						cancellationToken
					).ConfigureAwait( false );
					return CommandExitCodes.Failure;
				}
			}

			return CommandExitCodes.Success;
		} catch ( OperationCanceledException ) {
			return CommandExitCodes.Canceled;
		} catch ( Exception exception ) when ( IsOperationalException( exception ) ) {
			await WriteOperationalErrorAsync(
				stderr,
				"input",
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
			return CommandExitCodes.Failure;
		}
	}

	private static async Task<int> ConvertEnvironmentEntryAsync(
		CommandOptions options,
		Stream stdout,
		Stream stderr,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( options );
		ArgumentNullException.ThrowIfNull( stdout );
		ArgumentNullException.ThrowIfNull( stderr );

		SystemTermcapEnvironmentProvider environmentProvider = new();
		string? terminalName =
			environmentProvider.GetEnvironmentVariable( "TERM" );
		if ( string.IsNullOrWhiteSpace( terminalName ) ) {
			await WriteOperationalErrorAsync(
				stderr,
				"TERM",
				"TERM is not set; supply a termcap file operand or '-' for standard input",
				cancellationToken
			).ConfigureAwait( false );
			return CommandExitCodes.Failure;
		}

		TermcapAcquisitionOptions acquisitionOptions =
			TermcapAcquisitionOptions.FromEnvironment(
				environmentProvider,
				new SystemTermcapFileProvider(),
				TermcapDefaultPathPolicy.Ncurses
			);
		TermcapAcquisitionResult acquired =
			TermcapAcquirer.Acquire(
				terminalName,
				acquisitionOptions
			);

		await WriteSourceDiagnosticsAsync(
			stderr,
			acquired.SourceDiagnostics,
			cancellationToken
		).ConfigureAwait( false );
		await WriteConversionDiagnosticsAsync(
			stderr,
			acquired.ConversionDiagnostics,
			cancellationToken
		).ConfigureAwait( false );

		if ( !acquired.IsSuccess || acquired.Description is null ) {
			if ( acquired.SourceDiagnostics.Count == 0
				&& acquired.ConversionDiagnostics.Count == 0 ) {
				await WriteOperationalErrorAsync(
					stderr,
					terminalName,
					"no matching termcap entry was found",
					cancellationToken
				).ConfigureAwait( false );
			}
			return CommandExitCodes.Failure;
		}

		return await WriteTermInfoAsync(
			acquired.Description,
			options,
			stdout,
			stderr,
			cancellationToken
		).ConfigureAwait( false );
	}

	private static async Task<int> ConvertReaderAsync(
		TextReader reader,
		string sourceName,
		CommandOptions options,
		Stream stdout,
		Stream stderr,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( reader );
		ArgumentException.ThrowIfNullOrWhiteSpace( sourceName );
		ArgumentNullException.ThrowIfNull( options );
		ArgumentNullException.ThrowIfNull( stdout );
		ArgumentNullException.ThrowIfNull( stderr );

		TermcapSourceParseResult parsed =
			TermcapSourceParser.Parse(
				reader,
				sourceName
			);
		await WriteSourceDiagnosticsAsync(
			stderr,
			parsed.Diagnostics,
			cancellationToken
		).ConfigureAwait( false );
		if ( parsed.HasErrors ) {
			return CommandExitCodes.Failure;
		}

		foreach ( TermcapSourceEntry sourceEntry in parsed.Document.Entries ) {
			cancellationToken.ThrowIfCancellationRequested();
			if ( sourceEntry.Names.Count == 0 ) {
				continue;
			}

			TermcapSourceResolveResult resolved =
				TermcapSourceResolver.Resolve(
					parsed.Document,
					sourceEntry.Names[0]
				);
			await WriteSourceDiagnosticsAsync(
				stderr,
				resolved.Diagnostics,
				cancellationToken
			).ConfigureAwait( false );
			if ( resolved.HasErrors || resolved.Entry is null ) {
				return CommandExitCodes.Failure;
			}

			TermcapConversionResult converted =
				TermcapConverter.Convert(
					resolved.Entry
				);
			await WriteConversionDiagnosticsAsync(
				stderr,
				converted.Diagnostics,
				cancellationToken
			).ConfigureAwait( false );
			if ( converted.HasErrors || converted.Description is null ) {
				return CommandExitCodes.Failure;
			}

			int status =
				await WriteTermInfoAsync(
					converted.Description,
					options,
					stdout,
					stderr,
					cancellationToken
				).ConfigureAwait( false );
			if ( status != CommandExitCodes.Success ) {
				return status;
			}
		}

		return CommandExitCodes.Success;
	}

	private static async Task<int> WriteTermInfoAsync(
		TerminalDescription description,
		CommandOptions options,
		Stream stdout,
		Stream stderr,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( description );
		ArgumentNullException.ThrowIfNull( options );
		ArgumentNullException.ThrowIfNull( stdout );
		ArgumentNullException.ThrowIfNull( stderr );

		string text;
		try {
			text =
				TerminalDescriptionSourceRenderer.Render(
					description,
					new TerminalDescriptionSourceRendererOptions(
						options.LineWidth
					)
				);
		} catch ( InvalidOperationException exception ) {
			await WriteOperationalErrorAsync(
				stderr,
				description.Name,
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
			return CommandExitCodes.Failure;
		}

		await WriteAsync(
			stdout,
			text,
			cancellationToken
		).ConfigureAwait( false );
		return CommandExitCodes.Success;
	}

	private static CommandOptionsParseResult ParseOptions(
		IReadOnlyList<string> args
	) {
		ArgumentNullException.ThrowIfNull( args );

		int lineWidth = 80;
		List<string> operands = [];
		bool endOptions = false;

		for ( int index = 0; index < args.Count; index++ ) {
			string argument = args[index];
			if ( !endOptions && argument == "--" ) {
				endOptions = true;
				continue;
			}
			if ( !endOptions && argument == "-w" ) {
				if ( index + 1 >= args.Count ) {
					return CommandOptionsParseResult.Fail(
						"option '-w' requires a width"
					);
				}
				index++;
				if ( !TryParseLineWidth( args[index], out lineWidth ) ) {
					return CommandOptionsParseResult.Fail(
						$"invalid line width '{args[index]}'"
					);
				}
				continue;
			}
			if (
				!endOptions
				&& argument.StartsWith( "-w", StringComparison.Ordinal )
				&& argument.Length > 2
			) {
				if ( !TryParseLineWidth( argument[2..], out lineWidth ) ) {
					return CommandOptionsParseResult.Fail(
						$"invalid line width '{argument[2..]}'"
					);
				}
				continue;
			}
			if (
				!endOptions
				&& argument.StartsWith( "-", StringComparison.Ordinal )
				&& argument != "-"
			) {
				return CommandOptionsParseResult.Fail(
					$"unsupported option '{argument}'"
				);
			}
			operands.Add( argument );
		}

		if ( operands.Count( operand => operand == "-" ) > 1 ) {
			return CommandOptionsParseResult.Fail(
				"standard input may be specified at most once"
			);
		}

		return CommandOptionsParseResult.Succeed(
			new CommandOptions(
				lineWidth,
				operands
			)
		);
	}

	private static bool TryParseLineWidth(
		string value,
		out int lineWidth
	) {
		ArgumentNullException.ThrowIfNull( value );

		return int.TryParse(
			value,
			NumberStyles.None,
			CultureInfo.InvariantCulture,
			out lineWidth
		)
			&& lineWidth >= 16
			&& lineWidth <= TermcapRenderOptions.MaximumSupportedLineLength;
	}

	private static TextReader CreateReader(
		Stream stream
	) {
		ArgumentNullException.ThrowIfNull( stream );

		return new StreamReader(
			stream,
			new UTF8Encoding(
				encoderShouldEmitUTF8Identifier: false,
				throwOnInvalidBytes: true
			),
			detectEncodingFromByteOrderMarks: true,
			bufferSize: 4096,
			leaveOpen: true
		);
	}

	private static TextReader CreateReader(
		string path
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( path );

		return new StreamReader(
			path,
			new UTF8Encoding(
				encoderShouldEmitUTF8Identifier: false,
				throwOnInvalidBytes: true
			),
			detectEncodingFromByteOrderMarks: true
		);
	}

	private static async Task WriteSourceDiagnosticsAsync(
		Stream stderr,
		IEnumerable<TermcapSourceDiagnostic> diagnostics,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stderr );
		ArgumentNullException.ThrowIfNull( diagnostics );

		foreach ( TermcapSourceDiagnostic diagnostic in diagnostics ) {
			cancellationToken.ThrowIfCancellationRequested();
			string location =
				FormatLocation(
					diagnostic.Span
				);
			await WriteAsync(
				stderr,
				$"{location}{diagnostic.Code} {FormatSeverity( diagnostic.Severity )}: {diagnostic.Message}{Environment.NewLine}",
				cancellationToken
			).ConfigureAwait( false );
		}
	}

	private static async Task WriteConversionDiagnosticsAsync(
		Stream stderr,
		IEnumerable<TermcapConversionDiagnostic> diagnostics,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stderr );
		ArgumentNullException.ThrowIfNull( diagnostics );

		foreach ( TermcapConversionDiagnostic diagnostic in diagnostics ) {
			cancellationToken.ThrowIfCancellationRequested();
			await WriteAsync(
				stderr,
				$"{FormatLocation( diagnostic.Span )}{diagnostic.Code} {FormatSeverity( diagnostic.Severity )}: {diagnostic.Message}{Environment.NewLine}",
				cancellationToken
			).ConfigureAwait( false );
		}
	}

	private static string FormatLocation(
		TermcapSourceSpan? span
	) {
		if ( span is null ) {
			return string.Empty;
		}

		string sourceName =
			span.SourceName
			?? "<termcap>";
		return $"{sourceName}:{span.Line}:{span.Column}: ";
	}

	private static string FormatSeverity(
		TermcapSourceDiagnosticSeverity severity
	) {
		return severity switch {
			TermcapSourceDiagnosticSeverity.Warning => "warning",
			TermcapSourceDiagnosticSeverity.Error => "error",
			_ => severity.ToString().ToLowerInvariant(),
		};
	}

	private static string FormatSeverity(
		TermcapConversionDiagnosticSeverity severity
	) {
		return severity switch {
			TermcapConversionDiagnosticSeverity.Warning => "warning",
			TermcapConversionDiagnosticSeverity.Error => "error",
			_ => severity.ToString().ToLowerInvariant(),
		};
	}

	private static async Task WriteOperationalErrorAsync(
		Stream stderr,
		string subject,
		string message,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stderr );
		ArgumentException.ThrowIfNullOrWhiteSpace( subject );
		ArgumentException.ThrowIfNullOrWhiteSpace( message );

		await WriteAsync(
			stderr,
			$"{CommandName}: {subject}: {message}{Environment.NewLine}",
			cancellationToken
		).ConfigureAwait( false );
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
			$"{CommandName}: {detail}.{Environment.NewLine}Try '{CommandName} --help' for more information.{Environment.NewLine}",
			cancellationToken
		).ConfigureAwait( false );
	}

	private static bool IsOperationalException(
		Exception exception
	) {
		ArgumentNullException.ThrowIfNull( exception );

		return exception is ArgumentException
			or IOException
			or UnauthorizedAccessException
			or DecoderFallbackException
			or NotSupportedException
			or InvalidOperationException;
	}

	private static bool IsSingleArgument(
		IReadOnlyList<string> args,
		string value
	) {
		ArgumentNullException.ThrowIfNull( args );
		ArgumentNullException.ThrowIfNull( value );

		return args.Count == 1
			&& string.Equals(
				args[0],
				value,
				StringComparison.Ordinal
			);
	}

	private static string GetHelpText() {
		return $"Usage: {CommandName} [OPTION]... [FILE]...{Environment.NewLine}"
			+ Environment.NewLine
			+ $"Convert conventional termcap descriptions to effective terminfo source.{Environment.NewLine}"
			+ $"FILE '-' reads standard input. With no FILE, TERM plus the explicit TC06 TERMCAP/TERMPATH/default acquisition policy is used.{Environment.NewLine}"
			+ Environment.NewLine
			+ $"Options:{Environment.NewLine}"
			+ $"  -w WIDTH        request deterministic output wrapping width (16..{TermcapRenderOptions.MaximumSupportedLineLength}){Environment.NewLine}"
			+ $"  -h, --help     display this help and exit{Environment.NewLine}"
			+ $"  -V, --version  output the coordinated suite version and exit{Environment.NewLine}"
			+ $"      --          end option processing{Environment.NewLine}"
			+ Environment.NewLine
			+ $"Output is effective resolved state; tc= ancestry, comments, disabled fields, and source formatting are not reconstructed.{Environment.NewLine}";
	}

	private static string GetSemanticVersion() {
		string informationalVersion =
			typeof( Command )
				.Assembly
				.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
				?.InformationalVersion
				?? "0.0.0";
		int metadataSeparator =
			informationalVersion.IndexOf(
				'+',
				StringComparison.Ordinal
			);

		return ( metadataSeparator < 0 )
			? informationalVersion
			: informationalVersion[..metadataSeparator]
		;
	}

	private static async Task WriteAsync(
		Stream stream,
		string text,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stream );
		ArgumentNullException.ThrowIfNull( text );

		using StreamWriter writer =
			new(
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

	private sealed class CommandOptions {
		internal CommandOptions(
			int lineWidth,
			IEnumerable<string> operands
		) {
			if (
				lineWidth < 16
				|| lineWidth > TermcapRenderOptions.MaximumSupportedLineLength
			) {
				throw new ArgumentOutOfRangeException( nameof( lineWidth ) );
			}
			ArgumentNullException.ThrowIfNull( operands );

			LineWidth = lineWidth;
			Operands = operands.ToArray();
		}

		internal int LineWidth { get; }

		internal IReadOnlyList<string> Operands { get; }
	}

	private sealed class CommandOptionsParseResult {
		private CommandOptionsParseResult(
			CommandOptions? options,
			string? error
		) {
			Options = options;
			Error = error;
		}

		internal CommandOptions? Options { get; }

		internal string? Error { get; }

		internal static CommandOptionsParseResult Succeed(
			CommandOptions options
		) {
			ArgumentNullException.ThrowIfNull( options );
			return new CommandOptionsParseResult(
				options,
				null
			);
		}

		internal static CommandOptionsParseResult Fail(
			string error
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( error );
			return new CommandOptionsParseResult(
				null,
				error
			);
		}
	}
}
