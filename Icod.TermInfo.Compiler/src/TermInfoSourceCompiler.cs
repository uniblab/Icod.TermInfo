using Icod.TermInfo.Source;

namespace Icod.TermInfo.Compiler;

/// <summary>
/// Compiles terminfo source through the Source parser and inheritance resolver
/// into independently loadable conventional compiled entries.
/// </summary>
/// <remarks>
/// Source parsing, capability classification, cancellation, and
/// <c>use=</c> inheritance remain owned by <c>Icod.TermInfo.Source</c>.
/// This type only composes those semantics with
/// <see cref="CompiledTermInfoWriter"/>.
/// </remarks>
public static class TermInfoSourceCompiler {
	/// <summary>
	/// Compiles a complete terminfo source document.
	/// </summary>
	/// <param name="source">The complete source text.</param>
	/// <param name="sourceName">
	/// An optional source identity retained by diagnostics.
	/// </param>
	/// <param name="lexerOptions">
	/// Optional source lexer and resource-limit settings.
	/// </param>
	/// <param name="resolverOptions">
	/// Optional inheritance resolver settings.
	/// </param>
	/// <param name="writerOptions">
	/// Optional compiled representation policy.
	/// </param>
	/// <returns>
	/// Successfully compiled entries and source diagnostics.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="source"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// A resolved terminal description cannot be represented exactly by the
	/// selected compiled format.
	/// </exception>
	public static TermInfoSourceCompilationResult Compile(
		string source,
		string? sourceName = null,
		TermInfoSourceLexerOptions? lexerOptions = null,
		TermInfoSourceResolverOptions? resolverOptions = null,
		CompiledTermInfoWriterOptions? writerOptions = null
	) {
		ArgumentNullException.ThrowIfNull( source );

		return CompileCore(
			TermInfoSourceParser.Parse(
				source,
				sourceName,
				lexerOptions
			),
			resolverOptions,
			writerOptions
		);
	}

	/// <summary>
	/// Reads and compiles a complete terminfo source document.
	/// </summary>
	/// <param name="reader">The source reader.</param>
	/// <param name="sourceName">
	/// An optional source identity retained by diagnostics.
	/// </param>
	/// <param name="lexerOptions">
	/// Optional source lexer and resource-limit settings.
	/// </param>
	/// <param name="resolverOptions">
	/// Optional inheritance resolver settings.
	/// </param>
	/// <param name="writerOptions">
	/// Optional compiled representation policy.
	/// </param>
	/// <returns>
	/// Successfully compiled entries and source diagnostics.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="reader"/> is <see langword="null"/>.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// A resolved terminal description cannot be represented exactly by the
	/// selected compiled format.
	/// </exception>
	public static TermInfoSourceCompilationResult Compile(
		TextReader reader,
		string? sourceName = null,
		TermInfoSourceLexerOptions? lexerOptions = null,
		TermInfoSourceResolverOptions? resolverOptions = null,
		CompiledTermInfoWriterOptions? writerOptions = null
	) {
		ArgumentNullException.ThrowIfNull( reader );

		return CompileCore(
			TermInfoSourceParser.Parse(
				reader,
				sourceName,
				lexerOptions
			),
			resolverOptions,
			writerOptions
		);
	}

	private static TermInfoSourceCompilationResult CompileCore(
		TermInfoSourceParseResult parseResult,
		TermInfoSourceResolverOptions? resolverOptions,
		CompiledTermInfoWriterOptions? writerOptions
	) {
		ArgumentNullException.ThrowIfNull( parseResult );

		List<TermInfoSourceDiagnostic> diagnostics =
			[.. parseResult.Diagnostics];
		List<CompiledTermInfoSourceEntry> entries = [];

		if ( parseResult.HasErrors ) {
			return new TermInfoSourceCompilationResult(
				entries,
				OrderDiagnostics( diagnostics )
			);
		}

		CompiledTermInfoWriterOptions effectiveWriterOptions =
			writerOptions
			?? new CompiledTermInfoWriterOptions();

		foreach ( TermInfoSourceEntry sourceEntry in parseResult.Document.Entries ) {
			TermInfoSourceResolveResult resolved =
				TermInfoSourceResolver.Resolve(
					parseResult.Document,
					sourceEntry.CanonicalName,
					resolverOptions
				);
			diagnostics.AddRange( resolved.Diagnostics );

			if ( resolved.HasErrors
				|| resolved.Entry is null ) {
				continue;
			}

			byte[] data =
				CompiledTermInfoWriter.Write(
					resolved.Entry.ToTerminalDescription(),
					effectiveWriterOptions
				);
			entries.Add(
				new CompiledTermInfoSourceEntry(
					resolved.Entry.SourceEntry,
					data
				)
			);
		}

		return new TermInfoSourceCompilationResult(
			entries,
			OrderDiagnostics( diagnostics )
		);
	}

	private static IReadOnlyList<TermInfoSourceDiagnostic> OrderDiagnostics(
		IEnumerable<TermInfoSourceDiagnostic> diagnostics
	) {
		ArgumentNullException.ThrowIfNull( diagnostics );

		return diagnostics
			.Select(
				( diagnostic, ordinal ) =>
					new {
						Diagnostic = diagnostic,
						Ordinal = ordinal,
					}
			)
			.OrderBy(
				item =>
					item.Diagnostic.Span?.SourceName
					?? string.Empty,
				StringComparer.Ordinal
			)
			.ThenBy(
				item =>
					item.Diagnostic.Span?.Offset
					?? int.MaxValue
			)
			.ThenBy(
				item =>
					item.Diagnostic.Span?.Length
					?? int.MaxValue
			)
			.ThenBy( item => item.Ordinal )
			.Select( item => item.Diagnostic )
			.ToArray();
	}
}
