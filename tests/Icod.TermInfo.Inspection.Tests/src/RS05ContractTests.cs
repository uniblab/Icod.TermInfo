using System.Globalization;
using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.Compiler;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RS05ContractTests {
	private const string HistoricalDevelopmentVersion = "1.7.0-Alpha-5";

	[Fact]
	public void ImplementationRecordPreservesRs05History() {
		string root = FindRepositoryRoot();
		string implementation =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.7.0-RS05-RELATIVE-RENDERING-AND-SEMANTIC-VERIFICATION.md"
				)
			);

		Assert.Contains( HistoricalDevelopmentVersion, implementation );
		Assert.Contains( "LF", implementation, StringComparison.Ordinal );
		Assert.Contains( "TermInfoSourceCompiler", implementation );
		Assert.Contains( "RS06", implementation );
	}

	[Fact]
	public void CanonicalRenderingPreservesIdentityCancellationsAndBothRoundTrips() {
		TerminalDescription parent =
			new TerminalDescriptionBuilder( "rs05-parent" )
				.SetDescription( "RS05 parent terminal" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetString( StringCapability.Bell, "\a" )
				.SetExtendedString( "XInherited", "parent" )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs05-child" )
				.AddAlias( "rs05-child-alias" )
				.SetDescription( "RS05 child terminal" )
				.SetNumber( NumericCapability.Columns, 132 )
				.SetNumber( NumericCapability.Lines, 24 )
				.SetString( StringCapability.Bell, "\a" )
				.SetString( StringCapability.ClearScreen, "\u001b[H\u001b[2J" )
				.SetExtendedString( "XLocal", "local" )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( parent.Name, parent ),
		];

		string source =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents
			);
		using StringWriter writer = new();
		TerminalDescriptionSourceSynthesizer.Write(
			writer,
			target,
			parents
		);

		Assert.Equal( source, writer.ToString() );
		Assert.StartsWith(
			"rs05-child|rs05-child-alias|RS05 child terminal,\n",
			source
		);
		Assert.Contains( "    am@,\n", source );
		Assert.Contains( "    cols#132,\n", source );
		Assert.Contains( "    lines#24,\n", source );
		Assert.DoesNotContain( "    bel=", source );
		Assert.Contains( "    clear=", source );
		Assert.Contains( "    XInherited@,\n", source );
		Assert.Contains( "    XLocal=local,\n", source );
		Assert.EndsWith( "    use=rs05-parent,\n", source );
		AssertLfOnly( source );
		AssertSourceAndCompilerRoundTrips(
			target,
			parents,
			source
		);
	}

	[Fact]
	public void LayoutWidthOrderingAndEscapingControlsRemainDeterministic() {
		TerminalDescription parent =
			new TerminalDescriptionBuilder( "rs05-layout-parent" )
				.SetDescription( "RS05 layout parent" )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs05-layout-child" )
				.SetDescription( "RS05 layout child" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Colors, 256 )
				.SetNumber( NumericCapability.Columns, 132 )
				.SetNumber( NumericCapability.Lines, 40 )
				.SetString(
					StringCapability.Bell,
					"prefix,\u001b\r\n\\" + new string( 'x', 40 )
				)
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( parent.Name, parent ),
		];
		TerminalDescriptionSourceSynthesisOptions canonicalOptions =
			new(
				32,
				TerminalDescriptionSourceLayout.Canonical,
				TerminalDescriptionSourceCapabilityOrder.TermInfoName,
				TerminalDescriptionSourceSynthesisOptions.DefaultMaximumParentCount
			);
		TerminalDescriptionSourceSynthesisOptions singleLineOptions =
			new(
				32,
				TerminalDescriptionSourceLayout.SingleLine,
				TerminalDescriptionSourceCapabilityOrder.TermInfoName,
				TerminalDescriptionSourceSynthesisOptions.DefaultMaximumParentCount
			);
		TerminalDescriptionSourceSynthesisOptions oneCapabilityOptions =
			new(
				32,
				TerminalDescriptionSourceLayout.OneCapabilityPerLine,
				TerminalDescriptionSourceCapabilityOrder.TermInfoName,
				TerminalDescriptionSourceSynthesisOptions.DefaultMaximumParentCount
			);

		string canonical =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents,
				canonicalOptions
			);
		string singleLine =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents,
				singleLineOptions
			);
		string oneCapability =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents,
				oneCapabilityOptions
			);

		AssertLfOnly( canonical );
		AssertLfOnly( singleLine );
		AssertLfOnly( oneCapability );
		Assert.Contains( "\\,", canonical );
		Assert.Contains( "\\E", canonical );
		Assert.Contains( "\\r", canonical );
		Assert.Contains( "\\n", canonical );
		Assert.Contains( "\\\\", canonical );
		Assert.Contains(
			canonical.Split( '\n' ),
			line => line.StartsWith( "        ", StringComparison.Ordinal )
		);
		Assert.Equal( 1, singleLine.Count( character => character == '\n' ) );
		Assert.DoesNotContain(
			oneCapability.Split( '\n' ),
			line => line.StartsWith( "        ", StringComparison.Ordinal )
		);

		int colorsIndex = canonical.IndexOf( "    colors#256", StringComparison.Ordinal );
		int columnsIndex = canonical.IndexOf( "    cols#132", StringComparison.Ordinal );
		int linesIndex = canonical.IndexOf( "    lines#40", StringComparison.Ordinal );
		Assert.True( colorsIndex >= 0 );
		Assert.True( columnsIndex > colorsIndex );
		Assert.True( linesIndex > columnsIndex );

		AssertSourceAndCompilerRoundTrips( target, parents, canonical );
		AssertSourceAndCompilerRoundTrips( target, parents, singleLine );
		AssertSourceAndCompilerRoundTrips( target, parents, oneCapability );
	}

	[Fact]
	public void MinimalDeltaPreservesSeveralUseReferencesExactly() {
		TerminalDescription left =
			new TerminalDescriptionBuilder( "rs05-left" )
				.SetDescription( "RS05 left parent" )
				.SetNumber( NumericCapability.Columns, 100 )
				.Build();
		TerminalDescription middle =
			new TerminalDescriptionBuilder( "rs05-middle" )
				.SetDescription( "RS05 middle parent" )
				.SetNumber( NumericCapability.Lines, 24 )
				.Build();
		TerminalDescription right =
			new TerminalDescriptionBuilder( "rs05-right" )
				.SetDescription( "RS05 right parent" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetExtendedString( "XTail", "tail" )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs05-minimal" )
				.SetDescription( "RS05 minimal child" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 100 )
				.SetNumber( NumericCapability.Lines, 24 )
				.SetExtendedString( "XTail", "tail" )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( left.Name, left ),
			new( middle.Name, middle ),
			new( right.Name, right ),
		];

		string source =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents
			);

		Assert.Equal(
			"rs05-minimal|RS05 minimal child,\n"
				+ "    use=rs05-left,\n"
				+ "    use=rs05-middle,\n"
				+ "    use=rs05-right,\n",
			source
		);
		AssertLfOnly( source );
		AssertSourceAndCompilerRoundTrips( target, parents, source );
	}

	[Fact]
	public void CancellationHeavyEntryRemainsSemanticallyExact() {
		TerminalDescription parent =
			new TerminalDescriptionBuilder( "rs05-cancel-parent" )
				.SetDescription( "RS05 cancellation parent" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetString( StringCapability.Bell, "bell" )
				.SetExtendedBoolean( "XBool" )
				.SetExtendedNumber( "XNum", 7 )
				.SetExtendedString( "XStr", "value" )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs05-cancel-child" )
				.SetDescription( "RS05 cancellation child" )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( parent.Name, parent ),
		];

		string source =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents
			);

		foreach (
			string cancellation
			in new[] {
				"    am@,\n",
				"    cols@,\n",
				"    bel@,\n",
				"    XBool@,\n",
				"    XNum@,\n",
				"    XStr@,\n",
			}
		) {
			Assert.Contains( cancellation, source );
		}
		AssertLfOnly( source );
		AssertSourceAndCompilerRoundTrips( target, parents, source );
	}

	[Fact]
	public void RenderingIsCultureIndependent() {
		TerminalDescription parent =
			new TerminalDescriptionBuilder( "rs05-culture-parent" )
				.SetDescription( "RS05 culture parent" )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs05-culture-child" )
				.SetDescription( "RS05 culture child" )
				.SetNumber( NumericCapability.Columns, 123456 )
				.SetExtendedString( "IValue", "I" )
				.SetExtendedString( "ivalue", "i" )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( parent.Name, parent ),
		];
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "tr-TR" );
			string first =
				TerminalDescriptionSourceSynthesizer.Synthesize(
					target,
					parents
				);

			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "fr-FR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "fr-FR" );
			string second =
				TerminalDescriptionSourceSynthesizer.Synthesize(
					target,
					parents
				);

			Assert.Equal( first, second );
			AssertLfOnly( first );
			AssertSourceAndCompilerRoundTrips( target, parents, first );
		}
		finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	private static void AssertLfOnly(
		string source
	) {
		ArgumentNullException.ThrowIfNull( source );

		Assert.NotEmpty( source );
		Assert.DoesNotContain( "\r", source );
		Assert.EndsWith( "\n", source );
	}

	private static void AssertSourceAndCompilerRoundTrips(
		TerminalDescription target,
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> parents,
		string relativeSource
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( parents );
		ArgumentNullException.ThrowIfNull( relativeSource );

		string combinedSource =
			BuildCombinedSource(
				parents,
				relativeSource
			);
		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				combinedSource,
				"rs05-source-roundtrip.ti"
			);
		Assert.False( parsed.HasErrors );
		TermInfoSourceResolveResult resolved =
			TermInfoSourceResolver.Resolve(
				parsed.Document,
				target.Name
			);
		Assert.False( resolved.HasErrors );
		Assert.NotNull( resolved.Entry );
		AssertSemanticallyEqual(
			target,
			resolved.Entry!.ToTerminalDescription()
		);

		TermInfoSourceCompilationResult compiled =
			TermInfoSourceCompiler.Compile(
				combinedSource,
				"rs05-compiler-roundtrip.ti"
			);
		Assert.False( compiled.HasErrors );
		CompiledTermInfoSourceEntry compiledTarget = Assert.Single(
			compiled.Entries,
			entry => string.Equals(
				entry.CanonicalName,
				target.Name,
				StringComparison.Ordinal
			)
		);
		TerminalDescription compiledDescription =
			CompiledTermInfoParser.Parse(
				compiledTarget.Data
			);
		AssertSemanticallyEqual(
			target,
			compiledDescription
		);
	}

	private static string BuildCombinedSource(
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> parents,
		string relativeSource
	) {
		ArgumentNullException.ThrowIfNull( parents );
		ArgumentNullException.ThrowIfNull( relativeSource );

		StringBuilder source = new();
		source.Append( relativeSource );
		HashSet<string> rendered = new( StringComparer.Ordinal );
		foreach (
			TerminalDescriptionSourceSynthesisParent parent
			in parents
		) {
			if ( !rendered.Add( parent.Description.Name ) ) {
				continue;
			}
			source.Append(
				TerminalDescriptionSourceRenderer.Render(
					parent.Description
				)
			);
		}
		return source.ToString();
	}

	private static void AssertSemanticallyEqual(
		TerminalDescription expected,
		TerminalDescription actual
	) {
		ArgumentNullException.ThrowIfNull( expected );
		ArgumentNullException.ThrowIfNull( actual );

		TermInfoComparisonResult comparison =
			TerminalDescriptionComparer.Compare(
				expected,
				actual
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

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( AppContext.BaseDirectory );
		while ( current is not null ) {
			if ( File.Exists( Path.Combine( current.FullName, "Icod.TermInfo.sln" ) ) ) {
				return current.FullName;
			}
			current = current.Parent;
		}
		throw new DirectoryNotFoundException(
			"Could not locate the repository root."
		);
	}
}
