using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RS04ContractTests {
	private const string HistoricalDevelopmentVersion = "1.7.0-Alpha-4";

	[Fact]
	public void ImplementationRecordPreservesRs04History() {
		string root = FindRepositoryRoot();
		string implementation =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.7.0-RS04-ORDERED-MULTI-PARENT-SEMANTICS.md"
				)
			);

		Assert.Contains( HistoricalDevelopmentVersion, implementation );
		Assert.Contains( "leftmost", implementation, StringComparison.OrdinalIgnoreCase );
		Assert.Contains( "UseName", implementation );
		Assert.Contains( "repeated", implementation, StringComparison.OrdinalIgnoreCase );
		Assert.Contains( "RS05", implementation );
	}

	[Fact]
	public void ThreeParentAggregateMatchesSourceResolverAcrossCapabilityKinds() {
		TerminalDescription left =
			new TerminalDescriptionBuilder( "rs04-left" )
				.SetDescription( "RS04 left parent" )
				.SetNumber( NumericCapability.Columns, 100 )
				.SetString( StringCapability.Bell, "left" )
				.SetExtendedString( "Collision", "left" )
				.Build();
		TerminalDescription middle =
			new TerminalDescriptionBuilder( "rs04-middle" )
				.SetDescription( "RS04 middle parent" )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetString( StringCapability.Bell, "middle" )
				.SetExtendedNumber( "Collision", 7 )
				.Build();
		TerminalDescription right =
			new TerminalDescriptionBuilder( "rs04-right" )
				.SetDescription( "RS04 right parent" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 60 )
				.SetNumber( NumericCapability.Lines, 24 )
				.SetString( StringCapability.Bell, "right" )
				.SetString( StringCapability.ClearScreen, "\u001b[H\u001b[2J" )
				.SetExtendedBoolean( "Collision" )
				.SetExtendedString( "Fallback", "right" )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs04-child" )
				.SetDescription( "RS04 child" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 100 )
				.SetNumber( NumericCapability.Lines, 24 )
				.SetString( StringCapability.Bell, "left" )
				.SetString( StringCapability.ClearScreen, "\u001b[H\u001b[2J" )
				.SetExtendedString( "Collision", "left" )
				.SetExtendedString( "Fallback", "right" )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( left.Name, left ),
			new( middle.Name, middle ),
			new( right.Name, right ),
		];

		string independentSource =
			"rs04-child|RS04 child,\n"
				+ "    use=rs04-left,\n"
				+ "    use=rs04-middle,\n"
				+ "    use=rs04-right,\n";
		TerminalDescription independentlyResolved =
			ResolveRelativeSource(
				target.Name,
				parents,
				independentSource
			);
		TermInfoComparisonResult independentComparison =
			TerminalDescriptionComparer.Compare(
				target,
				independentlyResolved
			);
		Assert.True(
			independentComparison.AreEqual,
			FormatDifferences( independentComparison )
		);

		string synthesized =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents
			);

		Assert.Equal( independentSource, synthesized );
		AssertRoundTrips(
			target,
			parents,
			synthesized
		);
	}

	[Fact]
	public void AliasReferenceSpellingIsPreservedInsteadOfCanonicalized() {
		TerminalDescription parent =
			new TerminalDescriptionBuilder( "rs04-parent" )
				.AddAlias( "rs04-parent-alias" )
				.SetDescription( "RS04 aliased parent" )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetExtendedString( "Vendor", "parent" )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs04-child" )
				.SetDescription( "RS04 alias child" )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetExtendedString( "Vendor", "parent" )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( "rs04-parent-alias", parent ),
		];

		string source =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents
			);

		Assert.EndsWith(
			"    use=rs04-parent-alias,\n",
			source
		);
		Assert.DoesNotContain(
			"    use=rs04-parent,\n",
			source
		);
		AssertRoundTrips(
			target,
			parents,
			source
		);
	}

	[Fact]
	public void SameEffectiveParentMayBeRepeatedUnderDistinctReferences() {
		TerminalDescription parent =
			new TerminalDescriptionBuilder( "rs04-repeat-parent" )
				.AddAlias( "rs04-repeat-alias" )
				.SetDescription( "RS04 repeated parent" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetExtendedNumber( "RGB", 16_777_216 )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs04-repeat-child" )
				.SetDescription( "RS04 repeated child" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetExtendedNumber( "RGB", 16_777_216 )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( parent.Name, parent ),
			new( "rs04-repeat-alias", parent ),
		];

		string source =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents
			);

		Assert.Equal(
			"rs04-repeat-child|RS04 repeated child,\n"
				+ "    use=rs04-repeat-parent,\n"
				+ "    use=rs04-repeat-alias,\n",
			source
		);
		AssertRoundTrips(
			target,
			parents,
			source
		);
	}

	[Fact]
	public void CaseDistinctUseNamesRemainDistinctUnderOrdinalPolicy() {
		TerminalDescription upper =
			new TerminalDescriptionBuilder( "RS04-Base" )
				.SetDescription( "RS04 upper base" )
				.SetNumber( NumericCapability.Columns, 100 )
				.Build();
		TerminalDescription lower =
			new TerminalDescriptionBuilder( "rs04-base" )
				.SetDescription( "RS04 lower base" )
				.SetNumber( NumericCapability.Lines, 24 )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs04-case-child" )
				.SetDescription( "RS04 case child" )
				.SetNumber( NumericCapability.Columns, 100 )
				.SetNumber( NumericCapability.Lines, 24 )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( upper.Name, upper ),
			new( lower.Name, lower ),
		];

		string source =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents
			);

		int upperIndex =
			source.IndexOf(
				"use=RS04-Base",
				StringComparison.Ordinal
			);
		int lowerIndex =
			source.IndexOf(
				"use=rs04-base",
				StringComparison.Ordinal
			);
		Assert.True( upperIndex >= 0 );
		Assert.True( lowerIndex > upperIndex );
		AssertRoundTrips(
			target,
			parents,
			source
		);
	}

	[Fact]
	public void MultiParentOutputIgnoresCapabilityInsertionOrder() {
		TerminalDescription firstLeft =
			new TerminalDescriptionBuilder( "rs04-order-left" )
				.SetNumber( NumericCapability.Columns, 100 )
				.SetString( StringCapability.Bell, "left" )
				.SetExtendedString( "Zeta", "z" )
				.SetExtendedNumber( "Alpha", 1 )
				.SetDescription( "RS04 order left" )
				.Build();
		TerminalDescription secondLeft =
			new TerminalDescriptionBuilder( "rs04-order-left" )
				.SetExtendedNumber( "Alpha", 1 )
				.SetExtendedString( "Zeta", "z" )
				.SetString( StringCapability.Bell, "left" )
				.SetNumber( NumericCapability.Columns, 100 )
				.SetDescription( "RS04 order left" )
				.Build();
		TerminalDescription firstRight =
			new TerminalDescriptionBuilder( "rs04-order-right" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Lines, 24 )
				.SetExtendedBoolean( "Beta" )
				.SetDescription( "RS04 order right" )
				.Build();
		TerminalDescription secondRight =
			new TerminalDescriptionBuilder( "rs04-order-right" )
				.SetExtendedBoolean( "Beta" )
				.SetNumber( NumericCapability.Lines, 24 )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetDescription( "RS04 order right" )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs04-order-child" )
				.SetDescription( "RS04 order child" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 100 )
				.SetNumber( NumericCapability.Lines, 24 )
				.SetString( StringCapability.Bell, "left" )
				.SetExtendedNumber( "Alpha", 1 )
				.SetExtendedBoolean( "Beta" )
				.SetExtendedString( "Zeta", "z" )
				.Build();

		string first =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				new[] {
					new TerminalDescriptionSourceSynthesisParent(
						firstLeft.Name,
						firstLeft
					),
					new TerminalDescriptionSourceSynthesisParent(
						firstRight.Name,
						firstRight
					),
				}
			);
		string second =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				new[] {
					new TerminalDescriptionSourceSynthesisParent(
						secondLeft.Name,
						secondLeft
					),
					new TerminalDescriptionSourceSynthesisParent(
						secondRight.Name,
						secondRight
					),
				}
			);

		Assert.Equal( first, second );
	}

	private static void AssertRoundTrips(
		TerminalDescription target,
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> parents,
		string relativeSource
	) {
		TerminalDescription actual =
			ResolveRelativeSource(
				target.Name,
				parents,
				relativeSource
			);
		TermInfoComparisonResult comparison =
			TerminalDescriptionComparer.Compare(
				target,
				actual
			);
		Assert.True(
			comparison.AreEqual,
			FormatDifferences( comparison )
		);
	}

	private static TerminalDescription ResolveRelativeSource(
		string targetName,
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> parents,
		string relativeSource
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( targetName );
		ArgumentNullException.ThrowIfNull( parents );
		ArgumentNullException.ThrowIfNull( relativeSource );

		StringBuilder source =
			new();
		source.Append( relativeSource );

		HashSet<string> renderedParents =
			new(
				StringComparer.Ordinal
			);
		foreach (
			TerminalDescriptionSourceSynthesisParent parent
			in parents
		) {
			if ( !renderedParents.Add( parent.Description.Name ) ) {
				continue;
			}

			source.Append(
				TerminalDescriptionSourceRenderer.Render(
					parent.Description
				)
			);
		}

		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				source.ToString(),
				"rs04-roundtrip.ti"
			);
		Assert.False( parsed.HasErrors );

		TermInfoSourceResolveResult resolved =
			TermInfoSourceResolver.Resolve(
				parsed.Document,
				targetName
			);
		Assert.False( resolved.HasErrors );
		Assert.NotNull( resolved.Entry );

		return resolved.Entry!.ToTerminalDescription();
	}

	private static string FormatDifferences(
		TermInfoComparisonResult comparison
	) {
		ArgumentNullException.ThrowIfNull( comparison );

		return string.Join(
			Environment.NewLine,
			comparison.Differences.Select(
				difference => difference.ToString()
			)
		);
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new(
				AppContext.BaseDirectory
			);
		while ( current is not null ) {
			if (
				File.Exists(
					Path.Combine(
						current.FullName,
						"Icod.TermInfo.sln"
					)
				)
			) {
				return current.FullName;
			}
			current = current.Parent;
		}

		throw new DirectoryNotFoundException(
			"Could not locate the repository root."
		);
	}
}
