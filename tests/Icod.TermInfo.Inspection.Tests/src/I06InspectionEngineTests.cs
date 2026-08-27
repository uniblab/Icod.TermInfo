using System.Diagnostics.CodeAnalysis;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class I06InspectionEngineTests {
	[Fact]
	public void InspectionTarget_RetainsExplicitProviderNameAndCallerLabel() {
		ITerminalDescriptionProvider provider =
			TerminalDatabase.BuiltIn;
		TermInfoInspectionTarget target =
			new(
				provider,
				"xterm",
				"built-in xterm"
			);

		Assert.Same( provider, target.Provider );
		Assert.Equal( "xterm", target.RequestedName );
		Assert.Equal( "built-in xterm", target.DisplayLabel );
		Assert.Equal( "built-in xterm", target.DisplayName );

		TermInfoInspectionTarget unlabeled =
			new(
				provider,
				"xterm-256color"
			);
		Assert.Null( unlabeled.DisplayLabel );
		Assert.Equal( "xterm-256color", unlabeled.DisplayName );
	}

	[Fact]
	public void InspectionTarget_RejectsInvalidArguments() {
		ITerminalDescriptionProvider provider =
			TerminalDatabase.BuiltIn;

		Assert.Throws<ArgumentNullException>(
			() =>
				new TermInfoInspectionTarget(
					null!,
					"xterm"
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				new TermInfoInspectionTarget(
					provider,
					null!
				)
		);
		Assert.Throws<ArgumentException>(
			() =>
				new TermInfoInspectionTarget(
					provider,
					"   "
				)
		);
		Assert.Throws<ArgumentException>(
			() =>
				new TermInfoInspectionTarget(
					provider,
					"xterm",
					"\t"
				)
		);
	}

	[Fact]
	public void TryInspect_CleanMissRemainsDistinguishable() {
		CountingProvider provider =
			new(
				acceptedName: "present",
				terminal: null
			);
		TermInfoInspectionTarget target =
			new(
				provider,
				"missing",
				"custom provider"
			);

		bool found =
			TermInfoInspectionEngine.TryInspect(
				target,
				out TermInfoInspectionResult? result
			);

		Assert.False( found );
		Assert.Null( result );
		Assert.Equal( 1, provider.LoadCount );
		Assert.Equal( "missing", provider.LastRequestedName );
	}

	[Fact]
	public void Inspect_CleanMissThrowsKeyNotFoundWithoutMaskingProviderContract() {
		CountingProvider provider =
			new(
				acceptedName: "present",
				terminal: null
			);
		TermInfoInspectionTarget target =
			new(
				provider,
				"missing",
				"explicit database"
			);

		KeyNotFoundException exception =
			Assert.Throws<KeyNotFoundException>(
				() => TermInfoInspectionEngine.Inspect( target )
			);

		Assert.Contains( "missing", exception.Message );
		Assert.Contains( "explicit database", exception.Message );
		Assert.Equal( 1, provider.LoadCount );
	}

	[Fact]
	public void Inspect_ProviderFailuresPropagateUnchanged() {
		InvalidOperationException expected =
			new(
				"provider failure"
			);
		ThrowingProvider provider =
			new(
				expected
			);
		TermInfoInspectionTarget target =
			new(
				provider,
				"xterm"
			);

		InvalidOperationException actual =
			Assert.Throws<InvalidOperationException>(
				() => TermInfoInspectionEngine.Inspect( target )
			);

		Assert.Same( expected, actual );
		Assert.Equal( 1, provider.LoadCount );
	}

	[Fact]
	public void Inspect_RetainsRequestedAliasSeparatelyFromCanonicalTerminalIdentity() {
		TerminalDescription terminal =
			new TerminalDescriptionBuilder( "canonical-terminal" )
				.SetDescription( "I06 alias acquisition" )
				.AddAlias( "requested-alias" )
				.Build();
		InMemoryTerminalDescriptionProvider provider =
			new(
				new[] {
					terminal,
				}
			);
		TermInfoInspectionTarget target =
			new(
				provider,
				"requested-alias",
				"caller label"
			);

		TermInfoInspectionResult result =
			TermInfoInspectionEngine.Inspect(
				target
			);

		Assert.Same( target, result.Target );
		Assert.Same( terminal, result.Terminal );
		Assert.Equal( "requested-alias", result.Target.RequestedName );
		Assert.Equal( "canonical-terminal", result.Terminal.Name );
		Assert.Equal( "caller label", result.Target.DisplayName );
	}

	[Fact]
	public void Render_TargetAndAcquiredResultUseCanonicalEffectiveRenderer() {
		TerminalDescription terminal =
			TerminalProfiles.Xterm;
		CountingProvider provider =
			new(
				"xterm",
				terminal
			);
		TermInfoInspectionTarget target =
			new(
				provider,
				"xterm"
			);

		string renderedFromTarget =
			TermInfoInspectionEngine.Render(
				target
			);
		Assert.Equal(
			TerminalDescriptionSourceRenderer.Render( terminal ),
			renderedFromTarget
		);
		Assert.Equal( 1, provider.LoadCount );

		TermInfoInspectionResult acquired =
			TermInfoInspectionEngine.Inspect(
				target
			);
		Assert.Equal( 2, provider.LoadCount );
		string renderedFromResult =
			TermInfoInspectionEngine.Render(
				acquired
			);
		Assert.Equal( renderedFromTarget, renderedFromResult );
		Assert.Equal( 2, provider.LoadCount );
	}

	[Fact]
	public void Compare_ExplicitTargetsRetainsBothIdentitiesAndLoadsEachOnce() {
		TerminalDescription leftTerminal =
			new TerminalDescriptionBuilder( "left-terminal" )
				.SetDescription( "I06 left" )
				.AddAlias( "left-request" )
				.Build();
		TerminalDescription rightTerminal =
			new TerminalDescriptionBuilder( "right-terminal" )
				.SetDescription( "I06 right" )
				.AddAlias( "right-request" )
				.Build();
		CountingProvider leftProvider =
			new(
				"left-request",
				leftTerminal
			);
		CountingProvider rightProvider =
			new(
				"right-request",
				rightTerminal
			);
		TermInfoInspectionTarget left =
			new(
				leftProvider,
				"left-request",
				"left source"
			);
		TermInfoInspectionTarget right =
			new(
				rightProvider,
				"right-request",
				"right source"
			);

		TermInfoInspectionComparison comparison =
			TermInfoInspectionEngine.Compare(
				left,
				right
			);

		Assert.Same( left, comparison.Left.Target );
		Assert.Same( right, comparison.Right.Target );
		Assert.Same( leftTerminal, comparison.Left.Terminal );
		Assert.Same( rightTerminal, comparison.Right.Terminal );
		Assert.False( comparison.AreEqual );
		Assert.False( comparison.Comparison.AreEqual );
		Assert.Equal( 1, leftProvider.LoadCount );
		Assert.Equal( 1, rightProvider.LoadCount );
		Assert.Equal( "left source", comparison.Left.Target.DisplayName );
		Assert.Equal( "right source", comparison.Right.Target.DisplayName );
	}

	[Fact]
	public void Compare_AcquiredResultsDoesNotReacquireProviders() {
		TerminalDescription terminal =
			TerminalProfiles.Xterm;
		CountingProvider leftProvider =
			new(
				"xterm",
				terminal
			);
		CountingProvider rightProvider =
			new(
				"xterm",
				terminal
			);
		TermInfoInspectionResult left =
			TermInfoInspectionEngine.Inspect(
				new TermInfoInspectionTarget(
					leftProvider,
					"xterm"
				)
			);
		TermInfoInspectionResult right =
			TermInfoInspectionEngine.Inspect(
				new TermInfoInspectionTarget(
					rightProvider,
					"xterm"
				)
			);

		TermInfoInspectionComparison comparison =
			TermInfoInspectionEngine.Compare(
				left,
				right
			);

		Assert.True( comparison.AreEqual );
		Assert.Same( left, comparison.Left );
		Assert.Same( right, comparison.Right );
		Assert.Equal( 1, leftProvider.LoadCount );
		Assert.Equal( 1, rightProvider.LoadCount );
	}

	[Fact]
	public void Compare_BuiltInXtermAgainstCallerProviderRequiresNoApplicationGlue() {
		ITerminalDescriptionProvider builtIn =
			TerminalDatabase.BuiltIn;
		InMemoryTerminalDescriptionProvider callerProvider =
			new(
				new[] {
					TerminalProfiles.Xterm,
				}
			);
		TermInfoInspectionTarget left =
			new(
				builtIn,
				"xterm",
				"built-in"
			);
		TermInfoInspectionTarget right =
			new(
				callerProvider,
				"xterm",
				"caller provider"
			);

		TermInfoInspectionComparison comparison =
			TermInfoInspectionEngine.Compare(
				left,
				right
			);

		Assert.True( comparison.AreEqual );
		Assert.Equal( "built-in", comparison.Left.Target.DisplayName );
		Assert.Equal( "caller provider", comparison.Right.Target.DisplayName );
	}

	[Fact]
	public void Compare_Xterm256ColorAgainstScreen256ColorIsARegularExplicitComparison() {
		TerminalDescription screen =
			new TerminalDescriptionBuilder( "screen-256color" )
				.SetDescription( "I06 screen fixture" )
				.Build();
		InMemoryTerminalDescriptionProvider screenProvider =
			new(
				new[] {
					screen,
				}
			);
		TermInfoInspectionTarget left =
			new(
				TerminalDatabase.BuiltIn,
				"xterm-256color",
				"xterm-256color"
			);
		TermInfoInspectionTarget right =
			new(
				screenProvider,
				"screen-256color",
				"screen-256color"
			);

		TermInfoInspectionComparison comparison =
			TermInfoInspectionEngine.Compare(
				left,
				right
			);

		Assert.False( comparison.AreEqual );
		Assert.Equal( "xterm-256color", comparison.Left.Target.RequestedName );
		Assert.Equal( "screen-256color", comparison.Right.Target.RequestedName );
		Assert.NotEmpty( comparison.Comparison.Differences );
	}

	[Fact]
	public void TargetsCanRepresentSystemAndSeparateDirectorySourcesWithoutInventedProvenance() {
		SystemTerminalDescriptionProvider systemProvider =
			new();
		DirectoryTerminalDescriptionProvider firstDirectory =
			new(
				Path.Combine(
					Path.GetTempPath(),
					"icod-i06-first"
				)
			);
		DirectoryTerminalDescriptionProvider secondDirectory =
			new(
				Path.Combine(
					Path.GetTempPath(),
					"icod-i06-second"
				)
			);

		TermInfoInspectionTarget system =
			new(
				systemProvider,
				"xterm",
				"system"
			);
		TermInfoInspectionTarget explicitDirectory =
			new(
				firstDirectory,
				"xterm",
				"database A"
			);
		TermInfoInspectionTarget otherDirectory =
			new(
				secondDirectory,
				"xterm",
				"database B"
			);

		Assert.IsType<SystemTerminalDescriptionProvider>( system.Provider );
		Assert.IsType<DirectoryTerminalDescriptionProvider>( explicitDirectory.Provider );
		Assert.IsType<DirectoryTerminalDescriptionProvider>( otherDirectory.Provider );
		Assert.Equal( "system", system.DisplayName );
		Assert.Equal( "database A", explicitDirectory.DisplayName );
		Assert.Equal( "database B", otherDirectory.DisplayName );
		Assert.NotEqual( firstDirectory.Root, secondDirectory.Root );
	}

	[Fact]
	public void Engine_PublicEntryPointsRejectNullArguments() {
		TermInfoInspectionTarget target =
			new(
				TerminalDatabase.BuiltIn,
				"xterm"
			);
		TermInfoInspectionResult result =
			TermInfoInspectionEngine.Inspect(
				target
			);

		Assert.Throws<ArgumentNullException>(
			() =>
				TermInfoInspectionEngine.TryInspect(
					null!,
					out _
				)
		);
		Assert.Throws<ArgumentNullException>(
			() => TermInfoInspectionEngine.Inspect( null! )
		);
		Assert.Throws<ArgumentNullException>(
			() => TermInfoInspectionEngine.Render( (TermInfoInspectionTarget)null! )
		);
		Assert.Throws<ArgumentNullException>(
			() => TermInfoInspectionEngine.Render( (TermInfoInspectionResult)null! )
		);
		Assert.Throws<ArgumentNullException>(
			() => TermInfoInspectionEngine.Compare( null!, target )
		);
		Assert.Throws<ArgumentNullException>(
			() => TermInfoInspectionEngine.Compare( target, null! )
		);
		Assert.Throws<ArgumentNullException>(
			() => TermInfoInspectionEngine.Compare( null!, result )
		);
		Assert.Throws<ArgumentNullException>(
			() => TermInfoInspectionEngine.Compare( result, null! )
		);
	}

	private sealed class CountingProvider : ITerminalDescriptionProvider {
		private readonly string _acceptedName;
		private readonly TerminalDescription? _terminal;

		internal CountingProvider(
			string acceptedName,
			TerminalDescription? terminal
		) {
			ArgumentException.ThrowIfNullOrWhiteSpace( acceptedName );

			_acceptedName = acceptedName;
			_terminal = terminal;
		}

		internal int LoadCount { get; private set; }

		internal string? LastRequestedName { get; private set; }

		public bool TryLoad(
			string name,
			[NotNullWhen( true )] out TerminalDescription? terminal
		) {
			ArgumentNullException.ThrowIfNull( name );

			LoadCount++;
			LastRequestedName = name;

			if ( _terminal is null
				|| !string.Equals(
					name,
					_acceptedName,
					StringComparison.Ordinal
				) ) {
				terminal = null;
				return false;
			}

			terminal = _terminal;
			return true;
		}
	}

	private sealed class ThrowingProvider : ITerminalDescriptionProvider {
		private readonly Exception _exception;

		internal ThrowingProvider(
			Exception exception
		) {
			ArgumentNullException.ThrowIfNull( exception );

			_exception = exception;
		}

		internal int LoadCount { get; private set; }

		public bool TryLoad(
			string name,
			[NotNullWhen( true )] out TerminalDescription? terminal
		) {
			ArgumentNullException.ThrowIfNull( name );

			LoadCount++;
			terminal = null;
			throw _exception;
		}
	}
}
