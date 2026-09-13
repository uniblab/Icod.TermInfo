using System.Globalization;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class PG05PersistentRasterPlacementCompositionTests {
	[Fact]
	public void SemanticExtendedCapabilityNamesAreFrozen() {
		Assert.Equal(
			"IcodPersistentRasterSourceRectangle",
			PersistentRasterPlacementInspector.SourceRectangleCapabilityName
		);
		Assert.Equal(
			"IcodPersistentRasterSignedZOrder",
			PersistentRasterPlacementInspector.SignedZOrderCapabilityName
		);
	}

	[Fact]
	public void ExplicitSemanticBooleansProduceCapabilityDerivedEvidence() {
		TerminalDescription description = CreateTerminal(
			"explicit",
			semanticCapabilities: [
				PersistentRasterPlacementInspector.SourceRectangleCapabilityName,
				PersistentRasterPlacementInspector.SignedZOrderCapabilityName,
			]
		);

		PersistentRasterPlacementProfile profile =
			PersistentRasterPlacementInspector.Inspect( description );

		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			profile.SourceRectangle
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			profile.SignedZOrder
		);
		Assert.Equal( 2, profile.Evidence.Count );
		Assert.All(
			profile.Evidence,
			evidence => Assert.Equal(
				PersistentRasterPlacementEvidenceKind.CapabilityDerived,
				evidence.Kind
			)
		);
	}

	[Fact]
	public void UnrecognizedGraphicsCapabilityDoesNotImplyPlacementSupport() {
		TerminalDescriptionBuilder builder = new( "unrecognized" );
		builder.SetExtendedBoolean( "Sixel" );

		PersistentRasterPlacementProfile profile =
			PersistentRasterPlacementInspector.Inspect( builder.Build() );

		Assert.Empty( profile.Evidence );
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unknown,
			profile.SourceRectangle
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unknown,
			profile.SignedZOrder
		);
	}

	[Fact]
	public void SemanticCapabilityWithWrongValueKindIsRejected() {
		TerminalDescriptionBuilder builder = new( "wrong-kind" );
		builder.SetExtendedString(
			PersistentRasterPlacementInspector.SourceRectangleCapabilityName,
			"yes"
		);

		Assert.Throws<InvalidOperationException>(
			() => PersistentRasterPlacementInspector.Inspect( builder.Build() )
		);
	}

	[Fact]
	public void CallerEvidenceLayersAfterDescriptionEvidence() {
		TerminalDescription description = CreateTerminal(
			"caller",
			semanticCapabilities: [
				PersistentRasterPlacementInspector.SourceRectangleCapabilityName,
			]
		);
		PersistentRasterPlacementEvidence[] callerEvidence = [
			new(
				PersistentRasterPlacementSubject.SourceRectangle,
				false,
				PersistentRasterPlacementEvidenceKind.Verified,
				"live-probe",
				100
			),
		];

		PersistentRasterPlacementProfile profile =
			PersistentRasterPlacementInspector.Inspect(
				description,
				callerEvidence
			);

		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unsupported,
			profile.SourceRectangle
		);
		Assert.Equal( 2, profile.Evidence.Count );
	}

	[Fact]
	public void KnownDatabaseWinnerSuppliesEffectiveProfileAndRetainsShadowProfiles() {
		TermInfoDatabaseSet set = TermInfoDatabaseInspector.CreateSet(
			[
				CreateCatalog(
					"winner",
					CreateTerminal(
						"target",
						semanticCapabilities: [
							PersistentRasterPlacementInspector.SourceRectangleCapabilityName,
						]
					)
				),
				CreateCatalog(
					"shadow",
					CreateTerminal(
						"target",
						semanticCapabilities: [
							PersistentRasterPlacementInspector.SignedZOrderCapabilityName,
						]
					)
				),
			]
		);

		PersistentRasterPlacementDatabaseSetInspection inspection =
			PersistentRasterPlacementInspector.Inspect( set, "target" );

		Assert.Same( set, inspection.DatabaseSet );
		Assert.Equal(
			TermInfoDatabaseSetLookupStatus.WinnerKnown,
			inspection.Lookup.Status
		);
		Assert.Equal( 2, inspection.OccurrenceProfiles.Count );
		Assert.NotNull( inspection.EffectiveProfile );
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			inspection.EffectiveProfile!.SourceRectangle
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			inspection.OccurrenceProfiles[ 1 ].SignedZOrder
		);
	}

	[Fact]
	public void CallerEvidenceAppliesOnlyAfterEffectiveWinnerBoundary() {
		TermInfoDatabaseSet set = TermInfoDatabaseInspector.CreateSet(
			[
				CreateCatalog(
					"winner",
					CreateTerminal(
						"target",
						semanticCapabilities: [
							PersistentRasterPlacementInspector.SourceRectangleCapabilityName,
						]
					)
				),
				CreateCatalog(
					"shadow",
					CreateTerminal( "target" )
				),
			]
		);
		PersistentRasterPlacementEvidence[] callerEvidence = [
			new(
				PersistentRasterPlacementSubject.SignedZOrder,
				true,
				PersistentRasterPlacementEvidenceKind.Verified,
				"runtime",
				100
			),
		];

		PersistentRasterPlacementDatabaseSetInspection inspection =
			PersistentRasterPlacementInspector.Inspect(
				set,
				"target",
				callerEvidence
			);

		Assert.NotNull( inspection.EffectiveProfile );
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			inspection.EffectiveProfile!.SignedZOrder
		);
		Assert.DoesNotContain(
			inspection.OccurrenceProfiles.SelectMany(
				profile => profile.Evidence
			),
			evidence => evidence.SourceLabel == "runtime"
		);
	}

	[Fact]
	public void EarlierIncompleteDatabasePreventsCallerEvidenceFromManufacturingEffectiveProfile() {
		TermInfoDatabaseSet set = TermInfoDatabaseInspector.CreateSet(
			[
				CreateIncompleteCatalog(
					"blocking",
					Array.Empty<TerminalDescription>()
				),
				CreateCatalog(
					"later",
					CreateTerminal(
						"target",
						semanticCapabilities: [
							PersistentRasterPlacementInspector.SourceRectangleCapabilityName,
						]
					)
				),
			]
		);
		PersistentRasterPlacementEvidence[] callerEvidence = [
			new(
				PersistentRasterPlacementSubject.SignedZOrder,
				true,
				PersistentRasterPlacementEvidenceKind.Verified,
				"runtime",
				100
			),
		];

		PersistentRasterPlacementDatabaseSetInspection inspection =
			PersistentRasterPlacementInspector.Inspect(
				set,
				"target",
				callerEvidence
			);

		Assert.Equal(
			TermInfoDatabaseSetLookupStatus.Indeterminate,
			inspection.Lookup.Status
		);
		Assert.Null( inspection.EffectiveProfile );
		Assert.Single( inspection.OccurrenceProfiles );
		Assert.DoesNotContain(
			inspection.OccurrenceProfiles[ 0 ].Evidence,
			evidence => evidence.SourceLabel == "runtime"
		);
	}

	[Fact]
	public void AliasDoesNotBecomeCanonicalPlacementLookupKey() {
		TermInfoDatabaseSet set = TermInfoDatabaseInspector.CreateSet(
			[
				CreateCatalog(
					"alias",
					CreateTerminal(
						"canonical",
						aliases: [ "alias-name" ],
						semanticCapabilities: [
							PersistentRasterPlacementInspector.SourceRectangleCapabilityName,
						]
					)
				),
			]
		);

		PersistentRasterPlacementDatabaseSetInspection inspection =
			PersistentRasterPlacementInspector.Inspect( set, "alias-name" );

		Assert.Equal(
			TermInfoDatabaseSetLookupStatus.NotObserved,
			inspection.Lookup.Status
		);
		Assert.Null( inspection.EffectiveProfile );
		Assert.Empty( inspection.OccurrenceProfiles );
	}

	[Fact]
	public void CompositionPreservesRepeatedOccurrenceOrderAndIsCultureIndependent() {
		TermInfoDatabaseSet set = TermInfoDatabaseInspector.CreateSet(
			[
				CreateCatalog(
					"first",
					CreateTerminal(
						"I-target",
						semanticCapabilities: [
							PersistentRasterPlacementInspector.SourceRectangleCapabilityName,
						]
					),
					CreateTerminal(
						"I-target",
						semanticCapabilities: [
							PersistentRasterPlacementInspector.SignedZOrderCapabilityName,
						]
					)
				),
			]
		);
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );

			PersistentRasterPlacementDatabaseSetInspection inspection =
				PersistentRasterPlacementInspector.Inspect( set, "I-target" );

			Assert.Equal( 2, inspection.OccurrenceProfiles.Count );
			Assert.Equal(
				PersistentRasterLifecycleSupportStatus.Supported,
				inspection.OccurrenceProfiles[ 0 ].SourceRectangle
			);
			Assert.Equal(
				PersistentRasterLifecycleSupportStatus.Supported,
				inspection.OccurrenceProfiles[ 1 ].SignedZOrder
			);
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
		}
	}

	private static TerminalDescription CreateTerminal(
		string name,
		IReadOnlyList<string>? aliases = null,
		IReadOnlyList<string>? semanticCapabilities = null
	) {
		TerminalDescriptionBuilder builder = new( name );
		if ( aliases is not null ) {
			foreach ( string alias in aliases ) {
				builder.AddAlias( alias );
			}
		}
		if ( semanticCapabilities is not null ) {
			foreach ( string capabilityName in semanticCapabilities ) {
				builder.SetExtendedBoolean( capabilityName );
			}
		}
		return builder.Build();
	}

	private static TermInfoDatabaseCatalog CreateCatalog(
		string rootName,
		params TerminalDescription[] terminals
	) =>
		CreateCatalogCore(
			rootName,
			terminals,
			Array.Empty<TermInfoDatabaseCatalogIssue>()
		);

	private static TermInfoDatabaseCatalog CreateIncompleteCatalog(
		string rootName,
		IReadOnlyList<TerminalDescription> terminals
	) {
		string root = AbsolutePath( rootName );
		TermInfoDatabaseCatalogIssue issue = new(
			TermInfoDatabaseCatalogIssueKind.MalformedEntry,
			Path.Combine( root, "entries", "malformed" ),
			"PG05 incomplete fixture."
		);
		return CreateCatalogCore( rootName, terminals, [ issue ] );
	}

	private static TermInfoDatabaseCatalog CreateCatalogCore(
		string rootName,
		IEnumerable<TerminalDescription> terminals,
		IEnumerable<TermInfoDatabaseCatalogIssue> issues
	) {
		string root = AbsolutePath( rootName );
		TermInfoDatabaseCatalogEntry[] entries = terminals
			.Select(
				( terminal, index ) => new TermInfoDatabaseCatalogEntry(
					Path.Combine(
						root,
						"entries",
						index.ToString( CultureInfo.InvariantCulture )
					),
					terminal
				)
			)
			.OrderBy( entry => entry.Name, StringComparer.Ordinal )
			.ThenBy( entry => entry.Path, StringComparer.Ordinal )
			.ToArray();
		string[] duplicates = entries
			.GroupBy( entry => entry.Name, StringComparer.Ordinal )
			.Where( group => group.Count() > 1 )
			.Select( group => group.Key )
			.OrderBy( name => name, StringComparer.Ordinal )
			.ToArray();
		return new TermInfoDatabaseCatalog(
			root,
			TermInfoDatabaseCatalogKind.ConventionalDirectory,
			entries,
			issues,
			duplicates
		);
	}

	private static string AbsolutePath(
		string suffix
	) =>
		Path.Combine(
			Path.GetTempPath(),
			$"icod-terminfo-pg05-{suffix}-{Guid.NewGuid():N}"
		);
}
