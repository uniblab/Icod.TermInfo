using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Source;

namespace Icod.TermInfo.Tic;

internal static class TicSourceValidator {
	private const string EmptySourceCode = "TIC0001";
	private const string MissingSelectionCode = "TIC0002";
	private const string UnknownExtendedCapabilityCode = "TIC0003";
	private const string RepresentationCode = "TIC0004";
	private const string InputCode = "TIC0005";
	private static readonly UTF8Encoding StrictUtf8 =
		new(
			encoderShouldEmitUTF8Identifier: false,
			throwOnInvalidBytes: true
		);

	internal static async Task<int> ValidateAsync(
		TicOptions options,
		Stream stdin,
		Stream stderr,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( options );
		ArgumentNullException.ThrowIfNull( stdin );
		ArgumentNullException.ThrowIfNull( stderr );

		(string? source, string sourceName, string? inputError) =
			await ReadSourceAsync(
				options.SourceOperand,
				stdin,
				cancellationToken
			).ConfigureAwait( false );

		if ( inputError is not null ) {
			await TicDiagnosticWriter.WriteAsync(
				stderr,
				[
					TicDiagnosticWriter.Error(
						InputCode,
						inputError,
						sourceName
					),
				],
				cancellationToken
			).ConfigureAwait( false );
			return CommandExitCodes.Failure;
		}

		cancellationToken.ThrowIfCancellationRequested();

		TermInfoSourceParseResult parseResult =
			TermInfoSourceParser.Parse(
				source
					?? throw new InvalidOperationException(
						"Source acquisition succeeded without returning source text."
					),
				sourceName
			);
		List<TicDiagnostic> diagnostics =
			parseResult.Diagnostics
				.Select( diagnostic => TicDiagnosticWriter.FromSource( diagnostic, sourceName ) )
				.ToList();

		if ( parseResult.HasErrors ) {
			return await FinishAsync(
				stderr,
				diagnostics,
				cancellationToken
			).ConfigureAwait( false );
		}

		if ( parseResult.Document.Entries.Count == 0 ) {
			diagnostics.Add(
				TicDiagnosticWriter.Error(
					EmptySourceCode,
					"No terminal entries were found in the source document.",
					sourceName
				)
			);
			return await FinishAsync(
				stderr,
				diagnostics,
				cancellationToken
			).ConfigureAwait( false );
		}

		IReadOnlyList<TermInfoSourceEntry> selectedEntries =
			SelectEntries(
				parseResult.Document,
				options.SelectedNames,
				diagnostics,
				sourceName
			);
		if ( diagnostics.Any( diagnostic => diagnostic.IsError ) ) {
			return await FinishAsync(
				stderr,
				diagnostics,
				cancellationToken
			).ConfigureAwait( false );
		}

		if ( !options.AllowUnknownExtensions ) {
			AddUnknownExtensionDiagnostics(
				parseResult.Document,
				selectedEntries,
				diagnostics
			);
		}

		foreach ( TermInfoSourceEntry entry in selectedEntries ) {
			cancellationToken.ThrowIfCancellationRequested();

			TermInfoSourceResolveResult resolved =
				TermInfoSourceResolver.Resolve(
					parseResult.Document,
					entry.CanonicalName
				);
			diagnostics.AddRange(
				resolved.Diagnostics.Select( diagnostic => TicDiagnosticWriter.FromSource( diagnostic, sourceName ) )
			);

			if (
				resolved.HasErrors
				|| resolved.Entry is null
			) {
				continue;
			}

			try {
				_ = CompiledTermInfoWriter.Write(
					resolved.Entry.ToTerminalDescription()
				);
			} catch ( InvalidOperationException exception ) {
				diagnostics.Add(
					TicDiagnosticWriter.Error(
						RepresentationCode,
						$"Entry '{entry.CanonicalName}' cannot be represented by the supported compiled terminfo format: {exception.Message}",
						entry.Span
					)
				);
			}
		}

		return await FinishAsync(
			stderr,
			diagnostics,
			cancellationToken
		).ConfigureAwait( false );
	}

	private static async Task<int> FinishAsync(
		Stream stderr,
		IReadOnlyList<TicDiagnostic> diagnostics,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stderr );
		ArgumentNullException.ThrowIfNull( diagnostics );

		await TicDiagnosticWriter.WriteAsync(
			stderr,
			diagnostics,
			cancellationToken
		).ConfigureAwait( false );

		return diagnostics.Any( diagnostic => diagnostic.IsError )
			? CommandExitCodes.Failure
			: CommandExitCodes.Success
		;
	}

	private static IReadOnlyList<TermInfoSourceEntry> SelectEntries(
		TermInfoSourceDocument document,
		IReadOnlyList<string> selectedNames,
		ICollection<TicDiagnostic> diagnostics,
		string sourceName
	) {
		ArgumentNullException.ThrowIfNull( document );
		ArgumentNullException.ThrowIfNull( selectedNames );
		ArgumentNullException.ThrowIfNull( diagnostics );
		ArgumentException.ThrowIfNullOrWhiteSpace( sourceName );

		if ( selectedNames.Count == 0 ) {
			return document.Entries;
		}

		HashSet<TermInfoSourceEntry> selected = [];
		foreach ( string name in selectedNames ) {
			TermInfoSourceEntry? entry =
				document.Entries.FirstOrDefault(
					candidate =>
						string.Equals(
							candidate.CanonicalName,
							name,
							StringComparison.Ordinal
						)
						|| candidate.Aliases.Contains(
							name,
							StringComparer.Ordinal
						)
				);
			if ( entry is null ) {
				diagnostics.Add(
					TicDiagnosticWriter.Error(
						MissingSelectionCode,
						$"Requested source entry '{name}' could not be found.",
						sourceName
					)
				);
				continue;
			}

			selected.Add( entry );
		}

		return document.Entries
			.Where( selected.Contains )
			.ToArray();
	}

	private static void AddUnknownExtensionDiagnostics(
		TermInfoSourceDocument document,
		IReadOnlyList<TermInfoSourceEntry> selectedEntries,
		ICollection<TicDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( document );
		ArgumentNullException.ThrowIfNull( selectedEntries );
		ArgumentNullException.ThrowIfNull( diagnostics );

		HashSet<TermInfoSourceEntry> reachable = [];
		foreach ( TermInfoSourceEntry entry in selectedEntries ) {
			CollectReachableEntries(
				document,
				entry,
				reachable
			);
		}

		foreach (
			TermInfoSourceEntry entry
			in document.Entries.Where( reachable.Contains )
		) {
			foreach ( TermInfoSourceField field in entry.Fields ) {
				if (
					field.CapabilityClassification
						!= TermInfoSourceCapabilityClassification.UnknownExtended
				) {
					continue;
				}

				diagnostics.Add(
					TicDiagnosticWriter.Error(
						UnknownExtendedCapabilityCode,
						$"Unknown extended capability '{field.CapabilityName}' requires -x.",
						field.Span
					)
				);
			}
		}
	}

	private static void CollectReachableEntries(
		TermInfoSourceDocument document,
		TermInfoSourceEntry entry,
		ISet<TermInfoSourceEntry> reachable
	) {
		ArgumentNullException.ThrowIfNull( document );
		ArgumentNullException.ThrowIfNull( entry );
		ArgumentNullException.ThrowIfNull( reachable );

		if ( !reachable.Add( entry ) ) {
			return;
		}

		foreach (
			string referenceName
			in entry.Fields
				.Where(
					field =>
						field.Kind == TermInfoSourceFieldKind.UseReference
						&& !string.IsNullOrWhiteSpace( field.ReferenceName )
				)
				.Select( field => field.ReferenceName! )
		) {
			TermInfoSourceEntry? parent =
				document.Entries.FirstOrDefault(
					candidate =>
						string.Equals(
							candidate.CanonicalName,
							referenceName,
							StringComparison.Ordinal
						)
						|| candidate.Aliases.Contains(
							referenceName,
							StringComparer.Ordinal
						)
				);
			if ( parent is not null ) {
				CollectReachableEntries(
					document,
					parent,
					reachable
				);
			}
		}
	}

	private static async Task<(string? Source, string SourceName, string? Error)> ReadSourceAsync(
		string sourceOperand,
		Stream stdin,
		CancellationToken cancellationToken
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( sourceOperand );
		ArgumentNullException.ThrowIfNull( stdin );

		if ( string.Equals( sourceOperand, "-", StringComparison.Ordinal ) ) {
			try {
				return (
					await ReadUtf8Async(
						stdin,
						leaveOpen: true,
						cancellationToken
					).ConfigureAwait( false ),
					"<stdin>",
					null
				);
			} catch ( DecoderFallbackException ) {
				return (
					null,
					"<stdin>",
					"input is not valid UTF-8"
				);
			} catch ( IOException exception ) {
				return (
					null,
					"<stdin>",
					exception.Message
				);
			}
		}

		string sourceName = sourceOperand;
		try {
			await using FileStream file =
				new(
					sourceOperand,
					FileMode.Open,
					FileAccess.Read,
					FileShare.Read,
					bufferSize: 4096,
					FileOptions.Asynchronous | FileOptions.SequentialScan
				);
			return (
				await ReadUtf8Async(
					file,
					leaveOpen: false,
					cancellationToken
				).ConfigureAwait( false ),
				sourceName,
				null
			);
		} catch ( DecoderFallbackException ) {
			return (
				null,
				sourceName,
				"input is not valid UTF-8"
			);
		} catch ( IOException exception ) {
			return (
				null,
				sourceName,
				exception.Message
			);
		} catch ( UnauthorizedAccessException exception ) {
			return (
				null,
				sourceName,
				exception.Message
			);
		} catch ( ArgumentException exception ) {
			return (
				null,
				sourceName,
				exception.Message
			);
		} catch ( NotSupportedException exception ) {
			return (
				null,
				sourceName,
				exception.Message
			);
		}
	}

	private static async Task<string> ReadUtf8Async(
		Stream stream,
		bool leaveOpen,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stream );

		using StreamReader reader =
			new(
				stream,
				StrictUtf8,
				detectEncodingFromByteOrderMarks: false,
				bufferSize: 4096,
				leaveOpen: leaveOpen
			);
		string text =
			await reader.ReadToEndAsync(
				cancellationToken
			).ConfigureAwait( false );

		return (
			text.Length != 0
			&& text[ 0 ] == '\uFEFF'
		)
			? text[ 1.. ]
			: text
		;
	}
}
