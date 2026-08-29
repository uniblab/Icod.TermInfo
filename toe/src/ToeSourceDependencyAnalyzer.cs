using System.Globalization;
using System.Text;
using Icod.TermInfo.Source;

namespace Icod.TermInfo.Toe;

internal enum ToeSourceDependencyMode {
	Forward = 0,
	Reverse = 1,
}

internal sealed class ToeSourceDependencyResult {
	internal ToeSourceDependencyResult(
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

	internal string Stdout { get; }

	internal string Stderr { get; }

	internal bool HasOperationalFailure { get; }
}

internal static class ToeSourceDependencyAnalyzer {
	private const string InputCode = "TOE0006";
	private static readonly UTF8Encoding StrictUtf8 =
		new(
			encoderShouldEmitUTF8Identifier: false,
			throwOnInvalidBytes: true
		);

	internal static async Task<ToeSourceDependencyResult> AnalyzeAsync(
		string sourcePath,
		ToeSourceDependencyMode mode,
		CancellationToken cancellationToken
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( sourcePath );

		(string? source, string? inputError) =
			await ReadSourceAsync(
				sourcePath,
				cancellationToken
			).ConfigureAwait( false );
		if ( inputError is not null ) {
			return new ToeSourceDependencyResult(
				string.Empty,
				CreateInputDiagnostic(
					sourcePath,
					inputError
				),
				hasOperationalFailure: true
			);
		}

		cancellationToken.ThrowIfCancellationRequested();

		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				source
					?? throw new InvalidOperationException(
						"Source acquisition succeeded without returning source text."
					),
				sourcePath
			);
		var diagnostics = new List<TermInfoSourceDiagnostic>();
		diagnostics.AddRange( parsed.Diagnostics );

		if ( parsed.HasErrors ) {
			return new ToeSourceDependencyResult(
				string.Empty,
				FormatDiagnostics( diagnostics ),
				hasOperationalFailure: true
			);
		}

		Dictionary<string, TermInfoSourceEntry> identities =
			BuildIdentityMap( parsed.Document );
		IReadOnlyList<DependencyEdge> edges =
			BuildEdges(
				parsed.Document,
				identities,
				cancellationToken
			);

		var diagnosticKeys = new HashSet<string>(
			diagnostics.Select( CreateDiagnosticKey ),
			StringComparer.Ordinal
		);
		foreach ( TermInfoSourceEntry entry in parsed.Document.Entries ) {
			cancellationToken.ThrowIfCancellationRequested();

			TermInfoSourceResolveResult resolved =
				TermInfoSourceResolver.Resolve(
					parsed.Document,
					entry.CanonicalName
				);
			foreach ( TermInfoSourceDiagnostic diagnostic in resolved.Diagnostics ) {
				string key = CreateDiagnosticKey( diagnostic );
				if ( diagnosticKeys.Add( key ) ) {
					diagnostics.Add( diagnostic );
				}
			}
		}

		string output = (mode == ToeSourceDependencyMode.Forward)
			? FormatForward( edges )
			: FormatReverse(
				parsed.Document,
				edges
			)
		;
		bool hasErrors = diagnostics.Any(
			diagnostic =>
				diagnostic.Severity
					== TermInfoSourceDiagnosticSeverity.Error
		);

		return new ToeSourceDependencyResult(
			output,
			FormatDiagnostics( diagnostics ),
			hasErrors
		);
	}

	private static Dictionary<string, TermInfoSourceEntry> BuildIdentityMap(
		TermInfoSourceDocument document
	) {
		ArgumentNullException.ThrowIfNull( document );

		var identities = new Dictionary<string, TermInfoSourceEntry>(
			StringComparer.Ordinal
		);
		foreach ( TermInfoSourceEntry entry in document.Entries ) {
			identities.TryAdd(
				entry.CanonicalName,
				entry
			);
			foreach ( string alias in entry.Aliases ) {
				identities.TryAdd(
					alias,
					entry
				);
			}
		}

		return identities;
	}

	private static IReadOnlyList<DependencyEdge> BuildEdges(
		TermInfoSourceDocument document,
		IReadOnlyDictionary<string, TermInfoSourceEntry> identities,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( document );
		ArgumentNullException.ThrowIfNull( identities );

		var edges = new List<DependencyEdge>();
		foreach ( TermInfoSourceEntry entry in document.Entries ) {
			cancellationToken.ThrowIfCancellationRequested();

			var seenParents = new HashSet<string>( StringComparer.Ordinal );
			foreach ( TermInfoSourceField field in entry.Fields ) {
				string? referenceName = field.ReferenceName;
				if (
					field.Kind != TermInfoSourceFieldKind.UseReference
					|| string.IsNullOrWhiteSpace( referenceName )
				) {
					continue;
				}

				TermInfoSourceEntry? parent = identities.TryGetValue(
					referenceName,
					out TermInfoSourceEntry? resolvedParent
				)
					? resolvedParent
					: null;
				string parentName = parent?.CanonicalName ?? referenceName;
				if ( !seenParents.Add( parentName ) ) {
					continue;
				}

				edges.Add(
					new DependencyEdge(
						entry.CanonicalName,
						parentName,
						parent
					)
				);
			}
		}

		return edges;
	}

	private static string FormatForward(
		IReadOnlyList<DependencyEdge> edges
	) {
		ArgumentNullException.ThrowIfNull( edges );

		var output = new StringBuilder();
		foreach ( DependencyEdge edge in edges ) {
			output
				.Append( edge.ChildName )
				.Append( '\t' )
				.Append( edge.ParentName )
				.Append( Environment.NewLine );
		}
		return output.ToString();
	}

	private static string FormatReverse(
		TermInfoSourceDocument document,
		IReadOnlyList<DependencyEdge> edges
	) {
		ArgumentNullException.ThrowIfNull( document );
		ArgumentNullException.ThrowIfNull( edges );

		var output = new StringBuilder();
		foreach ( TermInfoSourceEntry parent in document.Entries ) {
			foreach (
				DependencyEdge edge
				in edges.Where(
					candidate => ReferenceEquals(
						candidate.Parent,
						parent
					)
				)
			) {
				output
					.Append( parent.CanonicalName )
					.Append( '\t' )
					.Append( edge.ChildName )
					.Append( Environment.NewLine );
			}
		}
		return output.ToString();
	}

	private static string FormatDiagnostics(
		IReadOnlyList<TermInfoSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );

		var output = new StringBuilder();
		foreach ( TermInfoSourceDiagnostic diagnostic in diagnostics ) {
			TermInfoSourceSpan? span = diagnostic.Span;
			string location = span is null
				? "source"
				: string.Concat(
					span.SourceName ?? "source",
					":",
					span.Line.ToString( CultureInfo.InvariantCulture ),
					":",
					span.Column.ToString( CultureInfo.InvariantCulture )
				)
			;
			output
				.Append( "toe: " )
				.Append( location )
				.Append( ": " )
				.Append( diagnostic.Code )
				.Append( ' ' )
				.Append(
					diagnostic.Severity == TermInfoSourceDiagnosticSeverity.Error
						? "error"
						: "warning"
				)
				.Append( ": " )
				.Append( diagnostic.Message )
				.Append( Environment.NewLine );
		}
		return output.ToString();
	}

	private static string CreateDiagnosticKey(
		TermInfoSourceDiagnostic diagnostic
	) {
		ArgumentNullException.ThrowIfNull( diagnostic );

		TermInfoSourceSpan? span = diagnostic.Span;
		return string.Join(
			'\u001f',
			diagnostic.Code,
			((int)diagnostic.Severity).ToString( CultureInfo.InvariantCulture ),
			diagnostic.Message,
			span?.SourceName ?? string.Empty,
			span?.Offset.ToString( CultureInfo.InvariantCulture ) ?? string.Empty,
			span?.Length.ToString( CultureInfo.InvariantCulture ) ?? string.Empty
		);
	}

	private static string CreateInputDiagnostic(
		string sourcePath,
		string message
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( sourcePath );
		ArgumentNullException.ThrowIfNull( message );

		return $"toe: {sourcePath}: {InputCode} error: {message}{Environment.NewLine}";
	}

	private static async Task<(string? Source, string? Error)> ReadSourceAsync(
		string sourcePath,
		CancellationToken cancellationToken
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( sourcePath );

		try {
			await using FileStream file = new(
				sourcePath,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read,
				bufferSize: 4096,
				FileOptions.Asynchronous | FileOptions.SequentialScan
			);
			using StreamReader reader = new(
				file,
				StrictUtf8,
				detectEncodingFromByteOrderMarks: false,
				bufferSize: 4096,
				leaveOpen: false
			);
			string source =
				await reader.ReadToEndAsync(
					cancellationToken
				).ConfigureAwait( false );

			return (
				source.Length != 0
					&& source[ 0 ] == '\uFEFF'
						? source[ 1.. ]
						: source,
				null
			);
		} catch ( DecoderFallbackException ) {
			return (
				null,
				"input is not valid UTF-8"
			);
		} catch ( IOException exception ) {
			return (null, exception.Message);
		} catch ( UnauthorizedAccessException exception ) {
			return (null, exception.Message);
		} catch ( ArgumentException exception ) {
			return (null, exception.Message);
		} catch ( NotSupportedException exception ) {
			return (null, exception.Message);
		}
	}

	private sealed class DependencyEdge {
		internal DependencyEdge(
			string childName,
			string parentName,
			TermInfoSourceEntry? parent
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( childName );
			ArgumentException.ThrowIfNullOrWhiteSpace( parentName );

			ChildName = childName;
			ParentName = parentName;
			Parent = parent;
		}

		internal string ChildName { get; }

		internal string ParentName { get; }

		internal TermInfoSourceEntry? Parent { get; }
	}
}
