using System.Text;
using Icod.CommandFramework.Diagnostics;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.InfoCmp.Tests;

[Collection( EnvironmentSensitiveCollection.Name )]
public sealed class RS06RelativeSynthesisTests {
	private const string HistoricalDevelopmentVersion = "1.7.0-Alpha-6";

	[Fact]
	public void ImplementationRecordPreservesRs06History() {
		string root = FindRepositoryRoot();
		string implementation = File.ReadAllText(
			System.IO.Path.Combine(
				root,
				"docs",
				"1.7.0-RS06-INFOCMP-RELATIVE-SYNTHESIS.md"
			)
		);

		Assert.Contains( HistoricalDevelopmentVersion, implementation );
		Assert.Contains( "-c -u", implementation, StringComparison.Ordinal );
		Assert.Contains( "RS07", implementation );
	}

	[Fact]
	public async Task RelativeSynthesisUsesSeparateDatabasesAndPreservesReferences() {
		string targetRoot = CreateTemporaryDirectory();
		string parentRoot = CreateTemporaryDirectory();
		try {
			TerminalDescription left =
				new TerminalDescriptionBuilder( "rs06-left" )
					.AddAlias( "rs06-left-alias" )
					.SetDescription( "RS06 left parent" )
					.SetNumber( NumericCapability.Columns, 80 )
					.SetString( StringCapability.Bell, "bell" )
					.Build();
			TerminalDescription right =
				new TerminalDescriptionBuilder( "rs06-right" )
					.SetDescription( "RS06 right parent" )
					.SetNumber( NumericCapability.Lines, 24 )
					.Build();
			TerminalDescription target =
				new TerminalDescriptionBuilder( "rs06-child" )
					.SetDescription( "RS06 child" )
					.SetNumber( NumericCapability.Columns, 132 )
					.SetNumber( NumericCapability.Lines, 24 )
					.SetString( StringCapability.Bell, "bell" )
					.Build();
			Publish( targetRoot, target );
			Publish( parentRoot, left );
			Publish( parentRoot, right );

			CommandResult result = await RunAsync(
				"-A",
				targetRoot,
				"-B",
				parentRoot,
				"-u",
				target.Name,
				"rs06-left-alias",
				right.Name
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal( string.Empty, result.Stderr );
			Assert.StartsWith(
				"rs06-child|RS06 child,\n",
				result.Stdout,
				StringComparison.Ordinal
			);
			Assert.Contains( "    cols#132,\n", result.Stdout, StringComparison.Ordinal );
			int leftUse = result.Stdout.IndexOf(
				"    use=rs06-left-alias,\n",
				StringComparison.Ordinal
			);
			int rightUse = result.Stdout.IndexOf(
				"    use=rs06-right,\n",
				StringComparison.Ordinal
			);
			Assert.True( leftUse >= 0 );
			Assert.True( rightUse > leftUse );
			AssertSemanticRoundTrip(
				target,
				[ left, right ],
				result.Stdout
			);
		} finally {
			DeleteTemporaryDirectory( targetRoot );
			DeleteTemporaryDirectory( parentRoot );
		}
	}

	[Fact]
	public async Task CommonPlusUseIsAcceptedAsSynthesisSynonym() {
		string root = CreateTemporaryDirectory();
		try {
			TerminalDescription parent =
				new TerminalDescriptionBuilder( "rs06-synonym-parent" )
					.SetDescription( "RS06 synonym parent" )
					.SetNumber( NumericCapability.Columns, 80 )
					.Build();
			TerminalDescription target =
				new TerminalDescriptionBuilder( "rs06-synonym-child" )
					.SetDescription( "RS06 synonym child" )
					.SetNumber( NumericCapability.Columns, 132 )
					.Build();
			Publish( root, parent );
			Publish( root, target );

			CommandResult direct = await RunAsync(
				"-A",
				root,
				"-B",
				root,
				"-u",
				target.Name,
				parent.Name
			);
			CommandResult compatible = await RunAsync(
				"-A",
				root,
				"-B",
				root,
				"-c",
				"-u",
				target.Name,
				parent.Name
			);

			Assert.Equal( CommandExitCodes.Success, direct.Status );
			Assert.Equal( direct.Status, compatible.Status );
			Assert.Equal( direct.Stdout, compatible.Stdout );
			Assert.Equal( direct.Stderr, compatible.Stderr );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Theory]
	[InlineData( "-d" )]
	[InlineData( "-n" )]
	[InlineData( "-q" )]
	public async Task IncompatibleComparisonOptionsAreUsageErrors(
		string option
	) {
		CommandResult result = await RunAsync(
			option,
			"-u",
			"target",
			"parent"
		);

		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Equal( string.Empty, result.Stdout );
		Assert.NotEqual( string.Empty, result.Stderr );
	}

	[Fact]
	public async Task RelativeSynthesisRequiresAtLeastOneParent() {
		CommandResult result = await RunAsync(
			"-u",
			"target"
		);

		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Contains(
			"at least one parent",
			result.Stderr,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public async Task RelativeSynthesisRejectsDuplicateParentReferences() {
		CommandResult result = await RunAsync(
			"-u",
			"target",
			"parent",
			"parent"
		);

		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Contains(
			"duplicated",
			result.Stderr,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public async Task MissingParentIsControlledOperationalFailure() {
		string targetRoot = CreateTemporaryDirectory();
		string parentRoot = CreateTemporaryDirectory();
		try {
			TerminalDescription target =
				new TerminalDescriptionBuilder( "rs06-missing-child" )
					.SetDescription( "RS06 missing child" )
					.Build();
			Publish( targetRoot, target );

			CommandResult result = await RunAsync(
				"-A",
				targetRoot,
				"-B",
				parentRoot,
				"-u",
				target.Name,
				"missing-parent"
			);

			Assert.Equal( CommandExitCodes.Failure, result.Status );
			Assert.Equal( string.Empty, result.Stdout );
			Assert.Contains( "INFOCMP0002 error", result.Stderr, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( targetRoot );
			DeleteTemporaryDirectory( parentRoot );
		}
	}

	[Fact]
	public async Task ExtendedLocalDeltaRequiresX() {
		string root = CreateTemporaryDirectory();
		try {
			TerminalDescription parent =
				new TerminalDescriptionBuilder( "rs06-x-parent" )
					.SetDescription( "RS06 x parent" )
					.Build();
			TerminalDescription target =
				new TerminalDescriptionBuilder( "rs06-x-child" )
					.SetDescription( "RS06 x child" )
					.SetExtendedNumber( "XLocal", 7 )
					.Build();
			Publish( root, parent );
			Publish( root, target );

			CommandResult filtered = await RunAsync(
				"-A",
				root,
				"-B",
				root,
				"-u",
				target.Name,
				parent.Name
			);
			CommandResult extended = await RunAsync(
				"-A",
				root,
				"-B",
				root,
				"-x",
				"-u",
				target.Name,
				parent.Name
			);

			Assert.Equal( CommandExitCodes.Failure, filtered.Status );
			Assert.Equal( string.Empty, filtered.Stdout );
			Assert.Contains( "INFOCMP0004 error", filtered.Stderr, StringComparison.Ordinal );
			Assert.Equal( CommandExitCodes.Success, extended.Status );
			Assert.Contains( "    XLocal#7,\n", extended.Stdout, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Theory]
	[InlineData( "-0" )]
	[InlineData( "-1" )]
	public async Task RelativeSynthesisAcceptsSourceLayoutControls(
		string option
	) {
		string root = CreateTemporaryDirectory();
		try {
			TerminalDescription parent =
				new TerminalDescriptionBuilder( "rs06-layout-parent" )
					.SetDescription( "RS06 layout parent" )
					.Build();
			TerminalDescription target =
				new TerminalDescriptionBuilder( "rs06-layout-child" )
					.SetDescription( "RS06 layout child" )
					.SetNumber( NumericCapability.Columns, 132 )
					.Build();
			Publish( root, parent );
			Publish( root, target );

			CommandResult result = await RunAsync(
				"-A",
				root,
				"-B",
				root,
				option,
				"-u",
				target.Name,
				parent.Name
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal( string.Empty, result.Stderr );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task WidthAndSortControlsRemainAvailableInRelativeMode() {
		string root = CreateTemporaryDirectory();
		try {
			TerminalDescription parent =
				new TerminalDescriptionBuilder( "rs06-control-parent" )
					.SetDescription( "RS06 control parent" )
					.Build();
			TerminalDescription target =
				new TerminalDescriptionBuilder( "rs06-control-child" )
					.SetDescription( "RS06 control child" )
					.SetNumber( NumericCapability.Colors, 256 )
					.SetNumber( NumericCapability.Columns, 132 )
					.Build();
			Publish( root, parent );
			Publish( root, target );

			CommandResult result = await RunAsync(
				"-A",
				root,
				"-B",
				root,
				"-w",
				"40",
				"-s",
				"i",
				"-u",
				target.Name,
				parent.Name
			);

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Equal( string.Empty, result.Stderr );
			Assert.Contains( "cols#132", result.Stdout, StringComparison.Ordinal );
			Assert.Contains( "colors#256", result.Stdout, StringComparison.Ordinal );
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	private static void AssertSemanticRoundTrip(
		TerminalDescription target,
		IReadOnlyList<TerminalDescription> parents,
		string relativeSource
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( parents );
		ArgumentNullException.ThrowIfNull( relativeSource );

		StringBuilder source = new();
		source.Append( relativeSource );
		foreach ( TerminalDescription parent in parents ) {
			source.Append( TerminalDescriptionSourceRenderer.Render( parent ) );
		}
		TermInfoSourceParseResult parsed = TermInfoSourceParser.Parse(
			source.ToString(),
			"rs06-command-roundtrip.ti"
		);
		Assert.False( parsed.HasErrors );
		TermInfoSourceResolveResult resolved = TermInfoSourceResolver.Resolve(
			parsed.Document,
			target.Name
		);
		Assert.False( resolved.HasErrors );
		Assert.NotNull( resolved.Entry );
		TermInfoComparisonResult comparison = TerminalDescriptionComparer.Compare(
			target,
			resolved.Entry!.ToTerminalDescription()
		);
		Assert.True(
			comparison.AreEqual,
			string.Join(
				Environment.NewLine,
				comparison.Differences.Select(
					difference => difference.ToString()
				)
			)
		);
	}

	private static void Publish(
		string root,
		TerminalDescription description
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentNullException.ThrowIfNull( description );

		CompiledTermInfoDatabaseWriter.Write(
			root,
			description
		);
	}

	private static async Task<CommandResult> RunAsync(
		params string[] args
	) {
		ArgumentNullException.ThrowIfNull( args );

		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		int status = await Command.RunAsync(
			args,
			stdin,
			stdout,
			stderr
		);

		return new CommandResult(
			status,
			Encoding.UTF8.GetString( stdout.ToArray() ),
			Encoding.UTF8.GetString( stderr.ToArray() )
		);
	}

	private static string CreateTemporaryDirectory() {
		string path = System.IO.Path.Combine(
			System.IO.Path.GetTempPath(),
			$"icod-terminfo-infocmp-rs06-{Guid.NewGuid():N}"
		);
		Directory.CreateDirectory( path );
		return path;
	}

	private static void DeleteTemporaryDirectory(
		string path
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( path );

		try {
			Directory.Delete(
				path,
				recursive: true
			);
		} catch ( IOException ) {
		} catch ( UnauthorizedAccessException ) {
		}
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( AppContext.BaseDirectory );
		while ( current is not null ) {
			if ( File.Exists(
				System.IO.Path.Combine( current.FullName, "Icod.TermInfo.sln" )
			) ) {
				return current.FullName;
			}
			current = current.Parent;
		}

		throw new DirectoryNotFoundException(
			"Could not locate the repository root."
		);
	}

	private sealed record CommandResult(
		int Status,
		string Stdout,
		string Stderr
	);
}
