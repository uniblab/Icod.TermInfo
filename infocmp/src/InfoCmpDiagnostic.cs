using System.Text;

namespace Icod.TermInfo.InfoCmp;

internal static class InfoCmpDiagnosticWriter {
	private const string CommandName = "infocmp";

	internal static async Task WriteErrorAsync(
		Stream stderr,
		string code,
		string subject,
		string message,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stderr );
		ArgumentException.ThrowIfNullOrWhiteSpace( code );
		ArgumentException.ThrowIfNullOrWhiteSpace( subject );
		ArgumentNullException.ThrowIfNull( message );

		using StreamWriter writer =
			new(
				stderr,
				new UTF8Encoding( false ),
				bufferSize: 1024,
				leaveOpen: true
			);
		await writer.WriteAsync(
			$"{CommandName}: {subject}: {code} error: {message}{Environment.NewLine}".AsMemory(),
			cancellationToken
		).ConfigureAwait( false );
		await writer.FlushAsync(
			cancellationToken
		).ConfigureAwait( false );
	}
}
