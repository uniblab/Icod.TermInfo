using System.Text;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RS03ContractTests {
	private const string HistoricalDevelopmentVersion = "1.7.0-Alpha-3";

	[Fact]
	public void ImplementationRecordPreservesRs03History() {
		string root = FindRepositoryRoot();
		string implementation =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.7.0-RS03-EXTENDED-CAPABILITY-SYNTHESIS.md"
				)
			);

		Assert.Contains( HistoricalDevelopmentVersion, implementation );
		Assert.Contains( "ordinal", implementation, StringComparison.OrdinalIgnoreCase );
		Assert.Contains( "case-sensitive", implementation, StringComparison.OrdinalIgnoreCase );
		Assert.Contains( "IncludeExtendedCapabilities", implementation );
		Assert.Contains( "RS04", implementation );
	}

	[Fact]
	public void SynthesisOptionsIncludeExtendedCapabilitiesByDefault() {
		TerminalDescriptionSourceSynthesisOptions defaults =
			new();
		TerminalDescriptionSourceSynthesisOptions explicitDisabled =
			new(
				80,
				TerminalDescriptionSourceLayout.Canonical,
				TerminalDescriptionSourceCapabilityOrder.Database,
				TerminalDescriptionSourceSynthesisOptions.DefaultMaximumParentCount,
				includeExtendedCapabilities: false
			);

		Assert.True( defaults.IncludeExtendedCapabilities );
		Assert.False( explicitDisabled.IncludeExtendedCapabilities );
	}

	[Fact]
	public void TargetOnlyExtendedCapabilitiesAreDeclaredAndRoundTrip() {
		TerminalDescription parent =
			CreateTerminal( "rs03-parent" );
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs03-child" )
				.SetDescription( "RS03 child" )
				.SetExtendedBoolean( "XBool" )
				.SetExtendedNumber( "XNum", 42 )
				.SetExtendedString( "XStr", "value" )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( parent.Name, parent ),
		];

		string source =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents
			);

		Assert.Contains( "    XBool,\n", source );
		Assert.Contains( "    XNum#42,\n", source );
		Assert.Contains( "    XStr=value,\n", source );
		AssertRoundTrips(
			target,
			parents,
			source
		);
	}

	[Fact]
	public void ParentOnlyExtendedCapabilitiesAreCancelled() {
		TerminalDescription parent =
			new TerminalDescriptionBuilder( "rs03-parent" )
				.SetDescription( "RS03 parent" )
				.SetExtendedBoolean( "XBool" )
				.SetExtendedNumber( "XNum", 42 )
				.SetExtendedString( "XStr", "value" )
				.Build();
		TerminalDescription target =
			CreateTerminal( "rs03-child" );
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( parent.Name, parent ),
		];

		string source =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents
			);

		Assert.Contains( "    XBool@,\n", source );
		Assert.Contains( "    XNum@,\n", source );
		Assert.Contains( "    XStr@,\n", source );
		AssertRoundTrips(
			target,
			parents,
			source
		);
	}

	[Fact]
	public void EqualInheritedExtendedValuesAreOmittedAndMayDisableExtendedOutput() {
		TerminalDescription parent =
			new TerminalDescriptionBuilder( "rs03-parent" )
				.SetDescription( "RS03 parent" )
				.SetExtendedBoolean( "XBool" )
				.SetExtendedNumber( "XNum", 42 )
				.SetExtendedString( "XStr", "value" )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs03-child" )
				.SetDescription( "RS03 child" )
				.SetExtendedBoolean( "XBool" )
				.SetExtendedNumber( "XNum", 42 )
				.SetExtendedString( "XStr", "value" )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( parent.Name, parent ),
		];
		TerminalDescriptionSourceSynthesisOptions options =
			new(
				80,
				TerminalDescriptionSourceLayout.Canonical,
				TerminalDescriptionSourceCapabilityOrder.Database,
				TerminalDescriptionSourceSynthesisOptions.DefaultMaximumParentCount,
				includeExtendedCapabilities: false
			);

		string source =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents,
				options
			);

		Assert.DoesNotContain( "XBool", source );
		Assert.DoesNotContain( "XNum", source );
		Assert.DoesNotContain( "XStr", source );
		AssertRoundTrips(
			target,
			parents,
			source
		);
	}

	[Fact]
	public void ExtendedOverridesAndKindChangesUseTargetValueKind() {
		TerminalDescription parent =
			new TerminalDescriptionBuilder( "rs03-parent" )
				.SetDescription( "RS03 parent" )
				.SetExtendedNumber( "SameKind", 1 )
				.SetExtendedNumber( "KindChange", 2 )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs03-child" )
				.SetDescription( "RS03 child" )
				.SetExtendedNumber( "SameKind", 9 )
				.SetExtendedString( "KindChange", "text" )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( parent.Name, parent ),
		];

		string source =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				target,
				parents
			);

		Assert.Contains( "    SameKind#9,\n", source );
		Assert.Contains( "    KindChange=text,\n", source );
		Assert.DoesNotContain( "KindChange@", source );
		AssertRoundTrips(
			target,
			parents,
			source
		);
	}

	[Fact]
	public void LeftmostParentWinsExtendedKindCollisions() {
		TerminalDescription left =
			new TerminalDescriptionBuilder( "rs03-left" )
				.SetDescription( "RS03 left" )
				.SetExtendedString( "Collision", "left" )
				.Build();
		TerminalDescription middle =
			new TerminalDescriptionBuilder( "rs03-middle" )
				.SetDescription( "RS03 middle" )
				.SetExtendedNumber( "Collision", 7 )
				.Build();
		TerminalDescription right =
			new TerminalDescriptionBuilder( "rs03-right" )
				.SetDescription( "RS03 right" )
				.SetExtendedBoolean( "Collision" )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs03-child" )
				.SetDescription( "RS03 child" )
				.SetExtendedString( "Collision", "left" )
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

		Assert.DoesNotContain( "Collision=", source );
		Assert.DoesNotContain( "Collision#", source );
		Assert.DoesNotContain( "Collision@", source );
		AssertRoundTrips(
			target,
			parents,
			source
		);
	}

	[Fact]
	public void ExtendedNamesAreOrdinalCaseSensitiveAndInsertionOrderIndependent() {
		TerminalDescription parent =
			CreateTerminal( "rs03-parent" );
		TerminalDescription first =
			new TerminalDescriptionBuilder( "rs03-child" )
				.SetDescription( "RS03 child" )
				.SetExtendedString( "vendor", "lower" )
				.SetExtendedNumber( "Alpha", 1 )
				.SetExtendedString( "Vendor", "upper" )
				.SetExtendedBoolean( "Beta" )
				.Build();
		TerminalDescription second =
			new TerminalDescriptionBuilder( "rs03-child" )
				.SetDescription( "RS03 child" )
				.SetExtendedBoolean( "Beta" )
				.SetExtendedString( "Vendor", "upper" )
				.SetExtendedNumber( "Alpha", 1 )
				.SetExtendedString( "vendor", "lower" )
				.Build();
		TerminalDescriptionSourceSynthesisParent[] parents = [
			new( parent.Name, parent ),
		];

		string firstSource =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				first,
				parents
			);
		string secondSource =
			TerminalDescriptionSourceSynthesizer.Synthesize(
				second,
				parents
			);

		Assert.Equal( firstSource, secondSource );
		Assert.Contains( "    Vendor=upper,\n", firstSource );
		Assert.Contains( "    vendor=lower,\n", firstSource );
		AssertRoundTrips(
			first,
			parents,
			firstSource
		);
	}

	[Fact]
	public void DisablingExtendedOutputRejectsRequiredLocalDirectives() {
		TerminalDescription parent =
			new TerminalDescriptionBuilder( "rs03-parent" )
				.SetDescription( "RS03 parent" )
				.SetExtendedNumber( "XNum", 1 )
				.Build();
		TerminalDescription target =
			new TerminalDescriptionBuilder( "rs03-child" )
				.SetDescription( "RS03 child" )
				.SetExtendedNumber( "XNum", 2 )
				.Build();
		TerminalDescriptionSourceSynthesisOptions options =
			new(
				80,
				TerminalDescriptionSourceLayout.Canonical,
				TerminalDescriptionSourceCapabilityOrder.Database,
				TerminalDescriptionSourceSynthesisOptions.DefaultMaximumParentCount,
				includeExtendedCapabilities: false
			);

		InvalidOperationException parentedException =
			Assert.Throws<InvalidOperationException>(
				() =>
					TerminalDescriptionSourceSynthesizer.Synthesize(
						target,
						new[] {
							new TerminalDescriptionSourceSynthesisParent(
								parent.Name,
								parent
							),
						},
						options
					)
			);
		InvalidOperationException zeroParentException =
			Assert.Throws<InvalidOperationException>(
				() =>
					TerminalDescriptionSourceSynthesizer.Synthesize(
						target,
						Array.Empty<TerminalDescriptionSourceSynthesisParent>(),
						options
					)
			);

		Assert.Contains( "disabled", parentedException.Message, StringComparison.OrdinalIgnoreCase );
		Assert.Contains( "disabled", zeroParentException.Message, StringComparison.OrdinalIgnoreCase );
	}

	[Fact]
	public void RuntimeStillRejectsExtendedStandardNameShadowing() {
		TerminalDescriptionBuilder builder =
			new( "rs03-shadow" );

		Assert.Throws<ArgumentException>(
			() =>
				builder.SetExtendedString(
					"am",
					"invalid"
				)
		);
	}

	private static TerminalDescription CreateTerminal(
		string name
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( name );

		return new TerminalDescriptionBuilder( name )
			.SetDescription( "RS03 terminal" )
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
				"rs03-roundtrip.ti"
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
