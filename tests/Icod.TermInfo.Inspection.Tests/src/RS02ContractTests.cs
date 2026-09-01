using System.Text;
using System.Xml.Linq;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RS02ContractTests {
	private const string DevelopmentVersion = "1.7.0-Alpha-2";

	[Fact]
	public void CoordinatedVersionAndImplementationRecordIdentifyRs02() {
		string root =
			FindRepositoryRoot();
		XDocument buildProperties =
			XDocument.Load(
				Path.Combine(
					root,
					"Directory.Build.props"
				),
				LoadOptions.None
			);
		string version =
			buildProperties
				.Descendants()
				.Single(
					element =>
						element.Name.LocalName == "IcodTermInfoSuiteVersion"
				)
				.Value
				.Trim();
		string implementation =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.7.0-RS02-STANDARD-DELTA-AND-CANCELLATION.md"
				)
			);

		Assert.Equal( DevelopmentVersion, version );
		Assert.Contains( DevelopmentVersion, implementation );
		Assert.Contains( "leftmost", implementation, StringComparison.OrdinalIgnoreCase );
		Assert.Contains( "cancellation", implementation, StringComparison.OrdinalIgnoreCase );
		Assert.Contains( "RS03", implementation );
	}

	[Fact]
	public void IdenticalParentEmitsOnlyUseReferenceAndRoundTrips() {
		TerminalDescription parent =
			new TerminalDescriptionBuilder( "rs02-parent" )
				.SetDescription( "RS02 parent" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetString( StringCapability.ClearScreen, "\u001b[H\u001b[2J" )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs02-child" )
				.SetDescription( "RS02 child" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetString( StringCapability.ClearScreen, "\u001b[H\u001b[2J" )
				.Build();
		TerminalDescriptionSourceSynthesisParent synthesisParent =
			new(
				parent.Name,
				parent
			);

		string source =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				new[] {
					synthesisParent,
				}
			);

		Assert.Equal(
			"rs02-child|RS02 child,\n"
				+ "    use=rs02-parent,\n",
			source
		);
		AssertRoundTrips(
			target,
			new[] {
				synthesisParent,
			},
			source
		);
	}

	[Fact]
	public void TargetAddsOverridesAndCancelsStandardCapabilities() {
		TerminalDescription parent =
			new TerminalDescriptionBuilder( "rs02-parent" )
				.SetDescription( "RS02 parent" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetNumber( NumericCapability.Lines, 24 )
				.SetString( StringCapability.Bell, "\a" )
				.SetString( StringCapability.ClearScreen, "\u001b[H\u001b[2J" )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs02-child" )
				.SetDescription( "RS02 child" )
				.SetNumber( NumericCapability.Columns, 132 )
				.SetNumber( NumericCapability.Lines, 24 )
				.SetString( StringCapability.Bell, "\b" )
				.SetString( StringCapability.CarriageReturn, "\r" )
				.Build();
		TerminalDescriptionSourceSynthesisParent synthesisParent =
			new(
				parent.Name,
				parent
			);

		string source =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				new[] {
					synthesisParent,
				}
			);

		Assert.Contains( "    am@,\n", source );
		Assert.Contains( "    cols#132,\n", source );
		Assert.DoesNotContain( "lines#", source );
		Assert.Contains( "    bel=", source );
		Assert.Contains( "    cr=", source );
		Assert.Contains( "    clear@,\n", source );
		Assert.EndsWith( "    use=rs02-parent,\n", source );
		AssertRoundTrips(
			target,
			new[] {
				synthesisParent,
			},
			source
		);
	}

	[Fact]
	public void EmptyParentAllowsCompleteLocalStandardDelta() {
		TerminalDescription parent =
			new TerminalDescriptionBuilder( "rs02-empty" )
				.SetDescription( "RS02 empty parent" )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs02-child" )
				.SetDescription( "RS02 child" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 100 )
				.SetString( StringCapability.Bell, "\a" )
				.Build();
		TerminalDescriptionSourceSynthesisParent synthesisParent =
			new(
				parent.Name,
				parent
			);

		string source =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				new[] {
					synthesisParent,
				}
			);

		Assert.Contains( "    am,\n", source );
		Assert.Contains( "    cols#100,\n", source );
		Assert.Contains( "    bel=", source );
		AssertRoundTrips(
			target,
			new[] {
				synthesisParent,
			},
			source
		);
	}

	[Fact]
	public void LeftmostParentWinsWithoutRedundantLocalOverrides() {
		TerminalDescription left =
			new TerminalDescriptionBuilder( "rs02-left" )
				.SetDescription( "RS02 left parent" )
				.SetNumber( NumericCapability.Columns, 100 )
				.Build();
		TerminalDescription right =
			new TerminalDescriptionBuilder( "rs02-right" )
				.SetDescription( "RS02 right parent" )
				.SetNumber( NumericCapability.Columns, 80 )
				.SetNumber( NumericCapability.Lines, 24 )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs02-child" )
				.SetDescription( "RS02 child" )
				.SetNumber( NumericCapability.Columns, 100 )
				.SetNumber( NumericCapability.Lines, 24 )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( left.Name, left ),
			new( right.Name, right ),
		];

		string source =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents
			);

		Assert.DoesNotContain( "cols#", source );
		Assert.DoesNotContain( "lines#", source );
		Assert.True(
			source.IndexOf(
				"use=rs02-left",
				StringComparison.Ordinal
			) < source.IndexOf(
				"use=rs02-right",
				StringComparison.Ordinal
			)
		);
		AssertRoundTrips(
			target,
			parents,
			source
		);
	}

	[Fact]
	public void ThreeParentAggregateStillAllowsLocalTargetOverride() {
		TerminalDescription first =
			CreateColumnsTerminal(
				"rs02-first",
				100
			);
		TerminalDescription second =
			CreateColumnsTerminal(
				"rs02-second",
				80
			);
		TerminalDescription third =
			CreateColumnsTerminal(
				"rs02-third",
				60
			);
		TerminalDescription target =
			CreateColumnsTerminal(
				"rs02-child",
				132
			);
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( first.Name, first ),
			new( second.Name, second ),
			new( third.Name, third ),
		];

		string source =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents
			);

		Assert.Contains( "    cols#132,\n", source );
		AssertRoundTrips(
			target,
			parents,
			source
		);
	}

	[Fact]
	public void TargetIdentityAndConfiguredLayoutArePreservedDeterministically() {
		TerminalDescription parent =
			new TerminalDescriptionBuilder( "rs02-parent" )
				.SetDescription( "RS02 parent" )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs02-child" )
				.AddAlias( "rs02-child-alias" )
				.SetDescription( "RS02 target terminal" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 132 )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( parent.Name, parent ),
		];
		TerminalDescriptionSourceSynthesisOptions options =
			new(
				120,
				TerminalDescriptionSourceLayout.SingleLine,
				TerminalDescriptionSourceCapabilityOrder.TermInfoName
			);

		string first =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents,
				options
			);
		string second =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents,
				options
			);
		using var writer =
			new StringWriter();

		TerminalDescriptionSourceSynthesizer.Write(
			writer,
			target,
			parents,
			options
		);

		Assert.Equal( first, second );
		Assert.Equal( first, writer.ToString() );
		Assert.StartsWith(
			"rs02-child|rs02-child-alias|RS02 target terminal, ",
			first
		);
		Assert.EndsWith( " use=rs02-parent,\n", first );
		AssertRoundTrips(
			target,
			parents,
			first
		);
	}

	[Fact]
	public void ParentedExtendedCapabilitiesRemainReservedForRs03() {
		TerminalDescription targetWithExtension =
			new TerminalDescriptionBuilder( "rs02-child" )
				.SetDescription( "RS02 child" )
				.SetExtendedString( "Vendor", "value" )
				.Build();
		TerminalDescription plainParent =
			new TerminalDescriptionBuilder( "rs02-parent" )
				.SetDescription( "RS02 parent" )
				.Build();
		TerminalDescription extendedParent =
			new TerminalDescriptionBuilder( "rs02-extended-parent" )
				.SetDescription( "RS02 extended parent" )
				.SetExtendedNumber( "RGB", 16_777_216 )
				.Build();

		NotSupportedException targetException =
			Assert.Throws<NotSupportedException>(
				() =>
					TerminalDescriptionSourceSynthesizer.Synthesize(
						targetWithExtension,
						new[] {
							new TerminalDescriptionSourceSynthesisParent(
								plainParent.Name,
								plainParent
							),
						}
					)
			);
		NotSupportedException parentException =
			Assert.Throws<NotSupportedException>(
				() =>
					TerminalDescriptionSourceSynthesizer.Synthesize(
						plainParent,
						new[] {
							new TerminalDescriptionSourceSynthesisParent(
								extendedParent.Name,
								extendedParent
							),
						}
					)
			);

		Assert.Contains( "RS03", targetException.Message );
		Assert.Contains( "RS03", parentException.Message );
	}

	private static TerminalDescription CreateColumnsTerminal(
		string name,
		int columns
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		return new TerminalDescriptionBuilder( name )
			.SetDescription( "RS02 columns terminal" )
			.SetNumber( NumericCapability.Columns, columns )
			.Build();
	}

	private static void AssertRoundTrips(
		TerminalDescription target,
		IReadOnlyList<TerminalDescriptionSourceSynthesisParent> parents,
		string relativeSource
	) {
		ArgumentNullException.ThrowIfNull( target );
		ArgumentNullException.ThrowIfNull( parents );
		ArgumentNullException.ThrowIfNull( relativeSource );

		StringBuilder source =
			new();
		source.Append( relativeSource );
		foreach (
			TerminalDescriptionSourceSynthesisParent parent
			in parents
		) {
			source.Append(
				TerminalDescriptionSourceRenderer.Render(
					parent.Description
				)
			);
		}

		TermInfoSourceParseResult parsed =
			TermInfoSourceParser.Parse(
				source.ToString(),
				"rs02-roundtrip.ti"
			);
		Assert.False( parsed.HasErrors );

		TermInfoSourceResolveResult resolved =
			TermInfoSourceResolver.Resolve(
				parsed.Document,
				target.Name
			);
		Assert.False( resolved.HasErrors );
		Assert.NotNull( resolved.Entry );

		TerminalDescription actual =
			resolved.Entry!.ToTerminalDescription();
		TermInfoComparisonResult comparison =
			TerminalDescriptionComparer.Compare(
				target,
				actual
			);
		Assert.True( comparison.AreEqual );
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
