using System.Globalization;
using System.Text;
using Icod.TermInfo.Source;

namespace Icod.TermInfo.Tic;

internal sealed class TicDiagnostic {
	internal TicDiagnostic(
		string code,
		bool isError,
		string message,
		string sourceName,
		int? line,
		int? column,
		int offset
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( code );
		ArgumentNullException.ThrowIfNull( message );
		ArgumentException.ThrowIfNullOrWhiteSpace( sourceName );
		if ( offset < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( offset )
			);
		}

		Code = code;
		IsError = isError;
		Message = message;
		SourceName = sourceName;
		Line = line;
		Column = column;
		Offset = offset;
	}

	internal string Code {
		get;
	}

	internal bool IsError {
		get;
	}

	internal string Message {
		get;
	}

	internal string SourceName {
		get;
	}

	internal int? Line {
		get;
	}

	internal int? Column {
		get;
	}

	internal int Offset {
		get;
	}
}

internal static class TicDiagnosticWriter {
	private const string CommandName = "tic";

	internal static TicDiagnostic FromSource(
		TermInfoSourceDiagnostic diagnostic,
		string fallbackSourceName
	) {
		ArgumentNullException.ThrowIfNull( diagnostic );
		ArgumentException.ThrowIfNullOrWhiteSpace( fallbackSourceName );

		return new TicDiagnostic(
			diagnostic.Code,
			diagnostic.Severity == TermInfoSourceDiagnosticSeverity.Error,
			diagnostic.Message,
			diagnostic.Span?.SourceName ?? fallbackSourceName,
			diagnostic.Span?.Line,
			diagnostic.Span?.Column,
			diagnostic.Span?.Offset ?? int.MaxValue
		);
	}

	internal static TicDiagnostic Error(
		string code,
		string message,
		string sourceName
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( code );
		ArgumentNullException.ThrowIfNull( message );
		ArgumentException.ThrowIfNullOrWhiteSpace( sourceName );

		return new TicDiagnostic(
			code,
			isError: true,
			message,
			sourceName,
			line: null,
			column: null,
			offset: int.MaxValue
		);
	}

	internal static TicDiagnostic Error(
		string code,
		string message,
		TermInfoSourceSpan span
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( code );
		ArgumentNullException.ThrowIfNull( message );
		ArgumentNullException.ThrowIfNull( span );

		return new TicDiagnostic(
			code,
			isError: true,
			message,
			span.SourceName ?? "<source>",
			span.Line,
			span.Column,
			span.Offset
		);
	}

	internal static async Task WriteAsync(
		Stream stderr,
		IEnumerable<TicDiagnostic> diagnostics,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stderr );
		ArgumentNullException.ThrowIfNull( diagnostics );

		TicDiagnostic[] ordered =
			diagnostics
				.Select(
					( diagnostic, ordinal ) =>
						new {
							Diagnostic = diagnostic,
							Ordinal = ordinal,
						}
				)
				.GroupBy(
					item =>
						string.Join(
							"\u001f",
							item.Diagnostic.Code,
							item.Diagnostic.IsError ? "1" : "0",
							item.Diagnostic.SourceName,
							item.Diagnostic.Line?.ToString(
								CultureInfo.InvariantCulture
							) ?? string.Empty,
							item.Diagnostic.Column?.ToString(
								CultureInfo.InvariantCulture
							) ?? string.Empty,
							item.Diagnostic.Offset.ToString(
								CultureInfo.InvariantCulture
							),
							item.Diagnostic.Message
						),
					StringComparer.Ordinal
				)
				.Select( group => group.First() )
				.OrderBy( item => item.Diagnostic.SourceName, StringComparer.Ordinal )
				.ThenBy( item => item.Diagnostic.Offset )
				.ThenBy( item => item.Ordinal )
				.Select( item => item.Diagnostic )
				.ToArray();

		using StreamWriter writer =
			new(
				stderr,
				new UTF8Encoding( false ),
				bufferSize: 1024,
				leaveOpen: true
			);
		foreach ( TicDiagnostic diagnostic in ordered ) {
			cancellationToken.ThrowIfCancellationRequested();

			string location =
				diagnostic.Line.HasValue
					? string.Concat(
						diagnostic.SourceName,
						":",
						diagnostic.Line.Value.ToString( CultureInfo.InvariantCulture ),
						":",
						diagnostic.Column?.ToString( CultureInfo.InvariantCulture )
							?? string.Empty
					)
					: diagnostic.SourceName
			;
			string severity =
				diagnostic.IsError
					? "error"
					: "warning"
			;
			string text =
				$"{CommandName}: {location}: {diagnostic.Code} {severity}: {diagnostic.Message}{Environment.NewLine}";

			await writer.WriteAsync(
				text.AsMemory(),
				cancellationToken
			).ConfigureAwait( false );
		}
		await writer.FlushAsync(
			cancellationToken
		).ConfigureAwait( false );
	}
}
