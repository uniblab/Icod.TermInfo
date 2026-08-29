using System.Globalization;
using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Inspection;

namespace Icod.TermInfo.Tic;

internal static class TicPublisher {
	private const string DestinationCode = "TIC0006";
	private const string PublicationCode = "TIC0007";

	internal static async Task<int> PublishAsync(
		TicOptions options,
		Stream stdin,
		Stream stderr,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( options );
		ArgumentNullException.ThrowIfNull( stdin );
		ArgumentNullException.ThrowIfNull( stderr );

		TicSourceValidationResult validation =
			await TicSourceValidator.AnalyzeAsync(
				options,
				stdin,
				cancellationToken
			).ConfigureAwait( false );

		await TicDiagnosticWriter.WriteAsync(
			stderr,
			validation.Diagnostics,
			cancellationToken
		).ConfigureAwait( false );
		if ( validation.HasErrors ) {
			return CommandExitCodes.Failure;
		}

		TicDestinationResolution destination =
			await ResolveDestinationAsync(
				options,
				stderr,
				cancellationToken
			).ConfigureAwait( false );
		if ( destination.Error is not null ) {
			return CommandExitCodes.Failure;
		}

		string root =
			destination.Path
			?? throw new InvalidOperationException(
				"Destination resolution succeeded without returning a path."
			);

		cancellationToken.ThrowIfCancellationRequested();

		try {
			CompiledTermInfoDatabaseWriter.Write(
				root,
				validation.Descriptions,
				writerOptions: null,
				databaseOptions: new CompiledTermInfoDatabaseWriterOptions(
					overwriteExisting: options.Force
				)
			);
		} catch ( IOException exception ) {
			return await WritePublicationFailureAsync(
				stderr,
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
		} catch ( UnauthorizedAccessException exception ) {
			return await WritePublicationFailureAsync(
				stderr,
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
		} catch ( ArgumentException exception ) {
			return await WritePublicationFailureAsync(
				stderr,
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
		} catch ( InvalidOperationException exception ) {
			return await WritePublicationFailureAsync(
				stderr,
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
		} catch ( NotSupportedException exception ) {
			return await WritePublicationFailureAsync(
				stderr,
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
		}

		if ( options.Summary ) {
			await WriteSummaryAsync(
				stderr,
				root,
				validation.Descriptions.Count,
				TicDiagnosticWriter.CountWarnings( validation.Diagnostics ),
				cancellationToken
			).ConfigureAwait( false );
		}

		return CommandExitCodes.Success;
	}

	private static async Task<TicDestinationResolution> ResolveDestinationAsync(
		TicOptions options,
		Stream stderr,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( options );
		ArgumentNullException.ThrowIfNull( stderr );

		try {
			TicDestinationResolution destination =
				options.OutputDirectory is string explicitDirectory
					? TicDestinationResolver.ResolveExplicit( explicitDirectory )
					: TicDestinationResolver.ResolveDefault(
						TermInfoDatabaseInspector.GetSystemLocations()
					)
			;

			if ( destination.Error is string destinationError ) {
				await WriteCommandErrorAsync(
					stderr,
					DestinationCode,
					destinationError,
					cancellationToken
				).ConfigureAwait( false );
			}

			return destination;
		} catch ( IOException exception ) {
			return await WriteDestinationFailureAsync(
				stderr,
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
		} catch ( UnauthorizedAccessException exception ) {
			return await WriteDestinationFailureAsync(
				stderr,
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
		} catch ( ArgumentException exception ) {
			return await WriteDestinationFailureAsync(
				stderr,
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
		} catch ( InvalidOperationException exception ) {
			return await WriteDestinationFailureAsync(
				stderr,
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
		} catch ( NotSupportedException exception ) {
			return await WriteDestinationFailureAsync(
				stderr,
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
		}
	}

	private static async Task<TicDestinationResolution> WriteDestinationFailureAsync(
		Stream stderr,
		string message,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stderr );
		ArgumentNullException.ThrowIfNull( message );

		await WriteCommandErrorAsync(
			stderr,
			DestinationCode,
			$"output destination could not be resolved: {message}",
			cancellationToken
		).ConfigureAwait( false );

		return TicDestinationResolution.FromError( message );
	}

	private static async Task<int> WritePublicationFailureAsync(
		Stream stderr,
		string message,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stderr );
		ArgumentNullException.ThrowIfNull( message );

		await WriteCommandErrorAsync(
			stderr,
			PublicationCode,
			$"database publication failed: {message}",
			cancellationToken
		).ConfigureAwait( false );
		return CommandExitCodes.Failure;
	}

	private static async Task WriteCommandErrorAsync(
		Stream stderr,
		string code,
		string message,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stderr );
		ArgumentException.ThrowIfNullOrWhiteSpace( code );
		ArgumentNullException.ThrowIfNull( message );

		await TicDiagnosticWriter.WriteAsync(
			stderr,
			[
				TicDiagnosticWriter.Error(
					code,
					message,
					"<output>"
				),
			],
			cancellationToken
		).ConfigureAwait( false );
	}

	private static async Task WriteSummaryAsync(
		Stream stderr,
		string root,
		int entryCount,
		int warningCount,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( stderr );
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		if ( entryCount < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( entryCount )
			);
		}
		if ( warningCount < 0 ) {
			throw new ArgumentOutOfRangeException(
				nameof( warningCount )
			);
		}

		using StreamWriter writer =
			new(
				stderr,
				new UTF8Encoding( false ),
				bufferSize: 1024,
				leaveOpen: true
			);
		await writer.WriteAsync(
			$"tic: output: {root}{Environment.NewLine}".AsMemory(),
			cancellationToken
		).ConfigureAwait( false );
		await writer.WriteAsync(
			$"tic: compiled entries: {entryCount.ToString( CultureInfo.InvariantCulture )}{Environment.NewLine}".AsMemory(),
			cancellationToken
		).ConfigureAwait( false );
		await writer.WriteAsync(
			$"tic: warnings: {warningCount.ToString( CultureInfo.InvariantCulture )}{Environment.NewLine}".AsMemory(),
			cancellationToken
		).ConfigureAwait( false );
		await writer.FlushAsync(
			cancellationToken
		).ConfigureAwait( false );
	}
}

internal sealed class TicDestinationResolution {
	private TicDestinationResolution(
		string? path,
		string? error
	) {
		Path = path;
		Error = error;
	}

	internal string? Path {
		get;
	}

	internal string? Error {
		get;
	}

	internal static TicDestinationResolution FromPath(
		string path
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( path );

		return new TicDestinationResolution(
			path,
			null
		);
	}

	internal static TicDestinationResolution FromError(
		string error
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( error );

		return new TicDestinationResolution(
			null,
			error
		);
	}
}

internal readonly struct TicDestinationCandidate {
	internal TicDestinationCandidate(
		TermInfoDatabaseLocationKind kind,
		string? path
	) {
		Kind = kind;
		Path = path;
	}

	internal TermInfoDatabaseLocationKind Kind {
		get;
	}

	internal string? Path {
		get;
	}
}

internal static class TicDestinationResolver {
	internal static TicDestinationResolution ResolveExplicit(
		string directory
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( directory );

		return TicDestinationResolution.FromPath(
			System.IO.Path.GetFullPath( directory )
		);
	}

	internal static TicDestinationResolution ResolveDefault(
		IReadOnlyList<TermInfoDatabaseLocation> locations
	) {
		ArgumentNullException.ThrowIfNull( locations );

		return ResolveDefault(
			locations
				.Select(
					location =>
						new TicDestinationCandidate(
							location.Kind,
							location.Path
						)
				)
				.ToArray()
		);
	}

	internal static TicDestinationResolution ResolveDefault(
		IReadOnlyList<TicDestinationCandidate> locations
	) {
		ArgumentNullException.ThrowIfNull( locations );

		foreach ( TicDestinationCandidate location in locations ) {
			if (
				location.Kind == TermInfoDatabaseLocationKind.TermInfoDirectory
				&& location.Path is string termInfoPath
			) {
				return TicDestinationResolution.FromPath( termInfoPath );
			}
		}

		foreach ( TicDestinationCandidate location in locations ) {
			if (
				location.Kind == TermInfoDatabaseLocationKind.UserDatabase
				&& location.Path is string userPath
			) {
				return TicDestinationResolution.FromPath( userPath );
			}
		}

		return TicDestinationResolution.FromError(
			"no safe default terminfo output directory is available; specify one with '-o'"
		);
	}
}
