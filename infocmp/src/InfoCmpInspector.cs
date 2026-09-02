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
		if ( options.IsComparison || options.IsSynthesis || options.IsPlanning ) {
			throw new ArgumentException(
				"Comparison, synthesis, or planning options cannot be rendered as one terminal.",
				nameof( options )
			);
		}

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

		InfoCmpTerminal? terminal =
			await AcquireAsync(
				requestedName,
				options.DatabaseDirectory,
				stderr,
				cancellationToken
			).ConfigureAwait( false );
		if ( terminal is null ) {
			return CommandExitCodes.Failure;
		}

		cancellationToken.ThrowIfCancellationRequested();
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
					terminal.Description,
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

	internal static async Task<int> PlanAsync(
		InfoCmpOptions options,
		Stream stdout,
		Stream stderr,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( options );
		ArgumentNullException.ThrowIfNull( stdout );
		ArgumentNullException.ThrowIfNull( stderr );
		cancellationToken.ThrowIfCancellationRequested();
		if ( !options.IsPlanning ) {
			throw new ArgumentException(
				"Relative-source planning options are required.",
				nameof( options )
			);
		}

		string targetName = options.TerminalNames[ 0 ];
		InfoCmpTerminal? target =
			await AcquireAsync(
				targetName,
				options.DatabaseDirectory,
				stderr,
				cancellationToken
			).ConfigureAwait( false );
		if ( target is null ) {
			return CommandExitCodes.Failure;
		}

		List<TerminalDescriptionSourceSynthesisParent> candidates = [];
		for ( int index = 1; index < options.TerminalNames.Count; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			string requestedName = options.TerminalNames[ index ];
			InfoCmpTerminal? candidate =
				await AcquireAsync(
					requestedName,
					options.ComparisonDatabaseDirectory,
					stderr,
					cancellationToken
				).ConfigureAwait( false );
			if ( candidate is null ) {
				return CommandExitCodes.Failure;
			}
			candidates.Add(
				new TerminalDescriptionSourceSynthesisParent(
					requestedName,
					candidate.Description
				)
			);
		}

		TerminalDescriptionSourcePlan plan;
		try {
			TerminalDescriptionSourceSynthesisOptions synthesisOptions =
				new(
					options.LineWidth,
					options.Layout,
					options.CapabilityOrder,
					options.MaximumSelectedParentCount,
					options.IncludeExtendedCapabilities
				);
			TerminalDescriptionSourcePlanningOptions planningOptions =
				new(
					synthesisOptions,
					maximumCandidateCount:
						TerminalDescriptionSourcePlanningOptions.DefaultMaximumCandidateCount,
					maximumSelectedParentCount:
						options.MaximumSelectedParentCount,
					maximumEvaluatedPlanCount:
						options.MaximumEvaluatedPlanCount,
					allowNonExhaustiveResult:
						options.AllowNonExhaustiveResult
				);
			plan =
				TerminalDescriptionSourcePlanner.Plan(
					target.Description,
					candidates,
					planningOptions,
					cancellationToken
				);
		} catch ( Exception exception ) when (
			exception is ArgumentException
			or InvalidOperationException
		) {
			await InfoCmpDiagnosticWriter.WriteErrorAsync(
				stderr,
				"INFOCMP0004",
				targetName,
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
			return CommandExitCodes.Failure;
		}

		await WriteAsync(
			stdout,
			plan.Source,
			cancellationToken
		).ConfigureAwait( false );
		return CommandExitCodes.Success;
	}

	internal static async Task<int> SynthesizeAsync(
		InfoCmpOptions options,
		Stream stdout,
		Stream stderr,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( options );
		ArgumentNullException.ThrowIfNull( stdout );
		ArgumentNullException.ThrowIfNull( stderr );
		cancellationToken.ThrowIfCancellationRequested();
		if ( !options.IsSynthesis ) {
			throw new ArgumentException(
				"Relative synthesis options are required.",
				nameof( options )
			);
		}

		string targetName = options.TerminalNames[ 0 ];
		InfoCmpTerminal? target =
			await AcquireAsync(
				targetName,
				options.DatabaseDirectory,
				stderr,
				cancellationToken
			).ConfigureAwait( false );
		if ( target is null ) {
			return CommandExitCodes.Failure;
		}

		List<TerminalDescriptionSourceSynthesisParent> parents = [];
		for ( int index = 1; index < options.TerminalNames.Count; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			string requestedName = options.TerminalNames[ index ];
			InfoCmpTerminal? parent =
				await AcquireAsync(
					requestedName,
					options.ComparisonDatabaseDirectory,
					stderr,
					cancellationToken
				).ConfigureAwait( false );
			if ( parent is null ) {
				return CommandExitCodes.Failure;
			}
			parents.Add(
				new TerminalDescriptionSourceSynthesisParent(
					requestedName,
					parent.Description
				)
			);
		}

		string rendered;
		try {
			TerminalDescriptionSourceSynthesisOptions synthesisOptions =
				new(
					options.LineWidth,
					options.Layout,
					options.CapabilityOrder,
					TerminalDescriptionSourceSynthesisOptions.DefaultMaximumParentCount,
					options.IncludeExtendedCapabilities
				);
			rendered =
				TerminalDescriptionSourceSynthesizer.Synthesize(
					target.Description,
					parents,
					synthesisOptions
				);
		} catch ( Exception exception ) when (
			exception is ArgumentException
			or InvalidOperationException
		) {
			await InfoCmpDiagnosticWriter.WriteErrorAsync(
				stderr,
				"INFOCMP0004",
				targetName,
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

	internal static async Task<int> CompareAsync(
		InfoCmpOptions options,
		Stream stdout,
		Stream stderr,
		CancellationToken cancellationToken
	) {
		ArgumentNullException.ThrowIfNull( options );
		ArgumentNullException.ThrowIfNull( stdout );
		ArgumentNullException.ThrowIfNull( stderr );
		cancellationToken.ThrowIfCancellationRequested();
		if ( !options.IsComparison ) {
			throw new ArgumentException(
				"At least two terminal operands are required for comparison.",
				nameof( options )
			);
		}

		List<InfoCmpTerminal> terminals = [];
		for ( int index = 0; index < options.TerminalNames.Count; index++ ) {
			cancellationToken.ThrowIfCancellationRequested();
			string requestedName = options.TerminalNames[ index ];
			string? databaseDirectory =
				index == 0
					? options.DatabaseDirectory
					: options.ComparisonDatabaseDirectory;
			InfoCmpTerminal? terminal =
				await AcquireAsync(
					requestedName,
					databaseDirectory,
					stderr,
					cancellationToken
				).ConfigureAwait( false );
			if ( terminal is null ) {
				return CommandExitCodes.Failure;
			}
			terminals.Add( terminal );
		}

		cancellationToken.ThrowIfCancellationRequested();
		string rendered =
			InfoCmpComparisonRenderer.Render(
				options,
				terminals
			);
		await WriteAsync(
			stdout,
			rendered,
			cancellationToken
		).ConfigureAwait( false );
		return CommandExitCodes.Success;
	}

	private static async Task<InfoCmpTerminal?> AcquireAsync(
		string requestedName,
		string? databaseDirectory,
		Stream stderr,
		CancellationToken cancellationToken
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( requestedName );
		ArgumentNullException.ThrowIfNull( stderr );
		cancellationToken.ThrowIfCancellationRequested();

		ITerminalDescriptionProvider provider;
		string displayLabel;
		try {
			if ( databaseDirectory is not null ) {
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
				databaseDirectory ?? "system terminfo search",
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
			return null;
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
				return null;
			}
		} catch ( Exception exception ) when ( IsOperationalException( exception ) ) {
			await InfoCmpDiagnosticWriter.WriteErrorAsync(
				stderr,
				"INFOCMP0003",
				requestedName,
				exception.Message,
				cancellationToken
			).ConfigureAwait( false );
			return null;
		}

		TermInfoInspectionResult inspected =
			result
			?? throw new InvalidOperationException(
				"Inspection succeeded without a result."
			);
		return new InfoCmpTerminal(
			requestedName,
			inspected.Terminal
		);
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
