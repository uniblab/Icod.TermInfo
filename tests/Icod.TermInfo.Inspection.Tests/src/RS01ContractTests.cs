using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RS01ContractTests {
	private const string DevelopmentVersion = "1.7.0-Alpha-1";

	[Fact]
	public void SynthesisParentRetainsExplicitReferenceAndEffectiveDescription() {
		TerminalDescription description =
			CreateTerminal(
				"parent-terminal"
			);
		TerminalDescriptionSourceSynthesisParent parent =
			new(
				"parent-alias",
				description
			);

		Assert.Equal( "parent-alias", parent.UseName );
		Assert.Same( description, parent.Description );
	}

	[Theory]
	[InlineData( " " )]
	[InlineData( "bad name" )]
	[InlineData( "bad|name" )]
	[InlineData( "bad,name" )]
	[InlineData( "bad\\" )]
	public void SynthesisParentRejectsUnrepresentableReferenceNames(
		string useName
	) {
		TerminalDescription description =
			CreateTerminal(
				"parent-terminal"
			);

		Assert.Throws<ArgumentException>(
			() =>
				new TerminalDescriptionSourceSynthesisParent(
					useName,
					description
				)
		);
	}

	[Fact]
	public void SynthesizerRejectsNullRequiredInputs() {
		TerminalDescription target =
			CreateTerminal(
				"target-terminal"
			);

		Assert.Throws<ArgumentNullException>(
			() =>
				TerminalDescriptionSourceSynthesizer.Synthesize(
					null!,
					Array.Empty<TerminalDescriptionSourceSynthesisParent>()
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				TerminalDescriptionSourceSynthesizer.Synthesize(
					target,
					null!
				)
		);
		Assert.Throws<ArgumentNullException>(
			() =>
				TerminalDescriptionSourceSynthesizer.Write(
					null!,
					target,
					Array.Empty<TerminalDescriptionSourceSynthesisParent>()
				)
		);
	}

	[Fact]
	public void SynthesisOptionsFreezeCanonicalDefaultsAndParentBounds() {
		TerminalDescriptionSourceSynthesisOptions options =
			new();

		Assert.Equal( 80, options.LineWidth );
		Assert.Equal(
			TerminalDescriptionSourceLayout.Canonical,
			options.Layout
		);
		Assert.Equal(
			TerminalDescriptionSourceCapabilityOrder.Database,
			options.CapabilityOrder
		);
		Assert.Equal(
			TerminalDescriptionSourceSynthesisOptions.DefaultMaximumParentCount,
			options.MaximumParentCount
		);
		Assert.Equal(
			64,
			TerminalDescriptionSourceSynthesisOptions.DefaultMaximumParentCount
		);
		Assert.Equal(
			256,
			TerminalDescriptionSourceSynthesisOptions.MaximumSupportedParentCount
		);
		Assert.Equal(
			0,
			new TerminalDescriptionSourceSynthesisOptions(
				80,
				maximumParentCount: 0
			).MaximumParentCount
		);
	}

	[Fact]
	public void SynthesisOptionsRejectInvalidPolicyValues() {
		Assert.Throws<ArgumentOutOfRangeException>(
			() =>
				new TerminalDescriptionSourceSynthesisOptions(
					0
				)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() =>
				new TerminalDescriptionSourceSynthesisOptions(
					80,
					(TerminalDescriptionSourceLayout)( -1 )
				)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() =>
				new TerminalDescriptionSourceSynthesisOptions(
					80,
					TerminalDescriptionSourceLayout.Canonical,
					(TerminalDescriptionSourceCapabilityOrder)( -1 )
				)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() =>
				new TerminalDescriptionSourceSynthesisOptions(
					80,
					maximumParentCount: -1
				)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() =>
				new TerminalDescriptionSourceSynthesisOptions(
					80,
					maximumParentCount:
						TerminalDescriptionSourceSynthesisOptions.MaximumSupportedParentCount + 1
				)
		);
	}

	[Fact]
	public void PlanPreservesParentOrderAndRejectsDuplicateReferenceNames() {
		TerminalDescription target =
			CreateTerminal(
				"target-terminal"
			);
		TerminalDescriptionSourceSynthesisParent first =
			new(
				"first-parent",
				CreateTerminal( "first-parent" )
			);
		TerminalDescriptionSourceSynthesisParent second =
			new(
				"second-parent",
				CreateTerminal( "second-parent" )
			);

		TerminalDescriptionSourceSynthesisPlan plan =
			TerminalDescriptionSourceSynthesizer.CreatePlan(
				target,
				new[] {
					first,
					second,
				}
			);

		Assert.Same( target, plan.Target );
		Assert.Equal( 2, plan.Parents.Count );
		Assert.Same( first, plan.Parents[ 0 ] );
		Assert.Same( second, plan.Parents[ 1 ] );
		Assert.Throws<ArgumentException>(
			() =>
				TerminalDescriptionSourceSynthesizer.CreatePlan(
					target,
					new[] {
						first,
						new TerminalDescriptionSourceSynthesisParent(
							first.UseName,
							second.Description
						),
					}
				)
		);
	}

	[Fact]
	public void PlanTreatsReferenceNamesAsOrdinalAndAllowsDistinctAliasesForOneDescription() {
		TerminalDescription target =
			CreateTerminal(
				"target-terminal"
			);
		TerminalDescription sharedParent =
			CreateTerminal(
				"shared-parent"
			);
		TerminalDescriptionSourceSynthesisPlan plan =
			TerminalDescriptionSourceSynthesizer.CreatePlan(
				target,
				new[] {
					new TerminalDescriptionSourceSynthesisParent(
						"Base",
						sharedParent
					),
					new TerminalDescriptionSourceSynthesisParent(
						"base",
						sharedParent
					),
				}
			);

		Assert.Equal( 2, plan.Parents.Count );
		Assert.Equal( "Base", plan.Parents[ 0 ].UseName );
		Assert.Equal( "base", plan.Parents[ 1 ].UseName );
		Assert.Same( sharedParent, plan.Parents[ 0 ].Description );
		Assert.Same( sharedParent, plan.Parents[ 1 ].Description );
	}

	[Fact]
	public void PlanRejectsNullParentItemsBeforeExecution() {
		TerminalDescription target =
			CreateTerminal(
				"target-terminal"
			);
		TerminalDescriptionSourceSynthesisParent[] parents = [
			null!,
		];

		Assert.Throws<ArgumentException>(
			() =>
				TerminalDescriptionSourceSynthesizer.CreatePlan(
					target,
					parents
				)
		);
	}

	[Fact]
	public void PlanEnforcesConfiguredParentLimitWithoutReordering() {
		TerminalDescription target =
			CreateTerminal(
				"target-terminal"
			);
		TerminalDescriptionSourceSynthesisOptions options =
			new(
				80,
				maximumParentCount: 1
			);

		Assert.Throws<ArgumentException>(
			() =>
				TerminalDescriptionSourceSynthesizer.CreatePlan(
					target,
					new[] {
						new TerminalDescriptionSourceSynthesisParent(
							"first-parent",
							CreateTerminal( "first-parent" )
						),
						new TerminalDescriptionSourceSynthesisParent(
							"second-parent",
							CreateTerminal( "second-parent" )
						),
					},
					options
				)
		);
	}

	[Fact]
	public void ZeroParentSynthesisReusesEffectiveRendererSemantics() {
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs01-target" )
				.SetDescription( "RS01 target terminal" )
				.SetBoolean( BooleanCapability.AutoRightMargin )
				.SetNumber( NumericCapability.Columns, 80 )
				.Build();
		TerminalDescriptionSourceSynthesisOptions options =
			new(
				100,
				TerminalDescriptionSourceLayout.Canonical,
				TerminalDescriptionSourceCapabilityOrder.TermInfoName
			);

		string expected =
			TerminalDescriptionSourceRenderer.Render(
				target,
				new TerminalDescriptionSourceRendererOptions(
					100,
					TerminalDescriptionSourceLayout.Canonical,
					TerminalDescriptionSourceCapabilityOrder.TermInfoName,
					includeExtendedCapabilities: true
				)
			);
		string actual =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				Array.Empty<TerminalDescriptionSourceSynthesisParent>(),
				options
			);

		Assert.Equal( expected, actual );
	}

	[Fact]
	public void ZeroParentWriteMatchesStringSynthesis() {
		TerminalDescription target =
			CreateTerminal(
				"writer-target"
			);
		using var writer =
			new StringWriter();

		TerminalDescriptionSourceSynthesizer.Write(
			writer,
			target,
			Array.Empty<TerminalDescriptionSourceSynthesisParent>()
		);

		Assert.Equal(
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				Array.Empty<TerminalDescriptionSourceSynthesisParent>()
			),
			writer.ToString()
		);
	}

	[Fact]
	public void Rs01RecordRetainsOriginalRelativeExecutionBoundary() {
		string root =
			FindRepositoryRoot();
		string implementation =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.7.0-RS01-SYNTHESIS-CONTRACT-AND-MODEL.md"
				)
			);

		Assert.Contains(
			"reserved for RS02",
			implementation,
			StringComparison.OrdinalIgnoreCase
		);
	}

	[Fact]
	public void Rs01ImplementationRecordFreezesDevelopmentVersionAndDecisions() {
		string root =
			FindRepositoryRoot();
		string implementation =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.7.0-RS01-SYNTHESIS-CONTRACT-AND-MODEL.md"
				)
			);

		Assert.Contains( DevelopmentVersion, implementation );
		Assert.Contains( "64", implementation );
		Assert.Contains( "256", implementation );
		Assert.Contains( "unique", implementation, StringComparison.OrdinalIgnoreCase );
		Assert.Contains( "Inspection", implementation );
		Assert.Contains( "RS02", implementation );
	}

	private static TerminalDescription CreateTerminal(
		string name
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		return new TerminalDescriptionBuilder( name )
			.SetDescription( "RS01 test terminal" )
			.Build();
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
