using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo.Inspection;

namespace Icod.TermInfo.InfoCmp;

internal static class InfoCmpInspector {
	internal static async Task<int> RenderAsync(
		InfoCmpOptions options,
		Stream stdout,
		Stream stderr,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( options );
		ArgumentNullException.ThrowIfNull( stdout );
		ArgumentNullException.ThrowIfNull( stderr );
		cancellationToken.ThrowIfCancellationRequested();

		string? requestedName =
			options.TerminalName
			?? Environment.GetEnvironmentVariable( "TERM" );
		if ( string.IsNullOrWhiteSpace( requestedName ) ) {
			await InfoCmpDiagnosticWriter.WriteErrorAsync(
				stderr,
				"INFOCMP0001",
				"TERM",
				"no terminal operand was supplied and TERM is missing or empty",
				cancellationToken
			).ConfigureAwait( false );
			return CommandExitCodes.Failure;
		}

		ITerminalDescriptionProvider provider;
		string displayLabel;
		try {
			if ( options.DatabaseDirectory is string databaseDirectory ) {
				DirectoryTerminalDescriptionProvider directoryProvider =
					new( databaseDirectory );
				provider = directoryProvider;
				displayLabel = directoryProvider.Root;
			} else {
				provider = new SystemTerminalDescriptionProvider();
				displayLabel = "system terminfo search";
			}
		} catch ( Exception exception ) when ( IsOperationalException( exception ) ) {
			await InfoCmpDiagnosticWriter.WriteErrorAsync(
				stderr,
				"INFOCMP0003",
				options.DatabaseDirectory ?? "system terminfo search",
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
			return CommandExitCodes.Failure;
		}

		TermInfoInspectionResult? result;
		try {
			TermInfoInspectionTarget target =
				new(
					provider,
					requestedName,
					displayLabel
				);
			if ( !TermInfoInspectionEngine.TryInspect(
					target,
					out result
				) ) {
				await InfoCmpDiagnosticWriter.WriteErrorAsync(
					stderr,
					"INFOCMP0002",
					requestedName,
					$"terminal is not available from {displayLabel}",
					cancellationToken
				).ConfigureAwait( false );
				return CommandExitCodes.Failure;
			}
		} catch ( Exception exception ) when ( IsOperationalException( exception ) ) {
			await InfoCmpDiagnosticWriter.WriteErrorAsync(
				stderr,
				"INFOCMP0003",
				requestedName,
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
			return CommandExitCodes.Failure;
		}

		cancellationToken.ThrowIfCancellationRequested();
		TermInfoInspectionResult inspected =
			result
			?? throw new InvalidOperationException(
				"Inspection succeeded without a result."
			);

		string rendered;
		try {
			TerminalDescriptionSourceRendererOptions rendererOptions =
				new(
					options.LineWidth,
					options.Layout,
					options.CapabilityOrder,
					options.IncludeExtendedCapabilities
				);
			rendered =
				TerminalDescriptionSourceRenderer.Render(
					inspected.Terminal,
					rendererOptions
				);
		} catch ( InvalidOperationException exception ) {
			await InfoCmpDiagnosticWriter.WriteErrorAsync(
				stderr,
				"INFOCMP0004",
				requestedName,
				exception.Message,
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

	private static bool IsOperationalException(
		Exception exception
	) {
		ArgumentNullException.ThrowIfNull( exception );

		return exception is ArgumentException
			or IOException
			or UnauthorizedAccessException
			or NotSupportedException
			or FormatException
			or InvalidOperationException;
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
}
