using System.Globalization;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RL05PersistentRasterLifecycleCompositionTests {
	[Fact]
	public void SemanticExtendedCapabilityNamesAreFrozen() {
		Assert.Equal(
			"IcodRasterDisplay",
			PersistentRasterLifecycleInspector.RasterDisplayCapabilityName
		);
		Assert.Equal(
			"IcodPersistentRasterUpload",
			PersistentRasterLifecycleInspector.PersistentUploadCapabilityName
		);
		Assert.Equal(
			"IcodPersistentRasterAcknowledgedUpload",
			PersistentRasterLifecycleInspector.AcknowledgedUploadCapabilityName
		);
		Assert.Equal(
			"IcodPersistentRasterPlacement",
			PersistentRasterLifecycleInspector.PlacementCreationCapabilityName
		);
		Assert.Equal(
			"IcodPersistentRasterMultiplePlacements",
			PersistentRasterLifecycleInspector.MultiplePlacementsCapabilityName
		);
		Assert.Equal(
			"IcodPersistentRasterPlacementUpdate",
			PersistentRasterLifecycleInspector.PlacementUpdateCapabilityName
		);
		Assert.Equal(
			"IcodPersistentRasterPlacementDeletion",
			PersistentRasterLifecycleInspector.PlacementDeletionCapabilityName
		);
		Assert.Equal(
			"IcodPersistentRasterResourceDeletion",
			PersistentRasterLifecycleInspector.ResourceDeletionCapabilityName
		);
	}

	[Fact]
	public void ExplicitSemanticBooleansProduceCapabilityDerivedPositiveEvidence() {
		TerminalDescription description = CreateTerminal(
			"explicit",
			PersistentRasterLifecycleInspector.RasterDisplayCapabilityName,
			PersistentRasterLifecycleInspector.PersistentUploadCapabilityName,
			PersistentRasterLifecycleInspector.ResourceDeletionCapabilityName
		);

		PersistentRasterLifecycleProfile profile =
			PersistentRasterLifecycleInspector.Inspect( description );

		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			profile.RasterDisplay
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			profile.PersistentUpload
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			profile.ResourceDeletion
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unknown,
			profile.AcknowledgedUpload
		);
		Assert.Equal( 3, profile.Evidence.Count );
		Assert.All(
			profile.Evidence,
			evidence => {
				Assert.True( evidence.IsPositive );
				Assert.Equal(
					PersistentRasterLifecycleEvidenceKind.CapabilityDerived,
					evidence.Kind
				);
			}
		);
		Assert.Equal(
			new[] {
				PersistentRasterLifecycleInspector.RasterDisplayCapabilityName,
				PersistentRasterLifecycleInspector.PersistentUploadCapabilityName,
				PersistentRasterLifecycleInspector.ResourceDeletionCapabilityName,
			},
			profile.Evidence.Select( evidence => evidence.SourceLabel )
		);
	}

	[Fact]
	public void UnrecognizedGraphicsCapabilityDoesNotImplyLifecycleSupport() {
		TerminalDescriptionBuilder builder = new( "unrecognized" );
		builder.SetExtendedBoolean( "Sixel" );
		TerminalDescription description = builder.Build();

		PersistentRasterLifecycleProfile profile =
			PersistentRasterLifecycleInspector.Inspect( description );

		Assert.Empty( profile.Evidence );
		foreach (
			PersistentRasterLifecycleEvidenceSubject subject
			in Enum.GetValues<PersistentRasterLifecycleEvidenceSubject>()
		) {
			Assert.Equal(
				PersistentRasterLifecycleSupportStatus.Unknown,
				profile.GetStatus( subject )
			);
		}
	}

	[Fact]
	public void SemanticCapabilityWithWrongValueKindIsRejected() {
		TerminalDescriptionBuilder builder = new( "wrong-kind" );
		builder.SetExtendedString(
			PersistentRasterLifecycleInspector.PersistentUploadCapabilityName,
			"yes"
		);
		TerminalDescription description = builder.Build();

		Assert.Throws<InvalidOperationException>(
			() => PersistentRasterLifecycleInspector.Inspect( description )
		);
	}

	[Fact]
	public void KnownDatabaseWinnerSuppliesEffectiveProfileAndRetainsShadowProfiles() {
		TerminalDescription winner = CreateTerminal(
			"target",
			PersistentRasterLifecycleInspector.PersistentUploadCapabilityName
		);
		TerminalDescription shadow = CreateTerminal(
			"target",
			PersistentRasterLifecycleInspector.ResourceDeletionCapabilityName
		);
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog( "winner", winner ),
					CreateCatalog( "shadow", shadow ),
				]
			);

		PersistentRasterLifecycleDatabaseSetInspection inspection =
			PersistentRasterLifecycleInspector.Inspect(
				set,
				"target"
			);

		Assert.Same( set, inspection.DatabaseSet );
		Assert.Equal(
			TermInfoDatabaseSetLookupStatus.WinnerKnown,
			inspection.Lookup.Status
		);
		Assert.Equal( 2, inspection.Lookup.Occurrences.Count );
		Assert.Equal( 2, inspection.OccurrenceProfiles.Count );
		PersistentRasterLifecycleProfile effective =
			Assert.IsType<PersistentRasterLifecycleProfile>(
				inspection.EffectiveProfile
			);
		Assert.Same( inspection.OccurrenceProfiles[ 0 ], effective );
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			effective.PersistentUpload
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Unknown,
			effective.ResourceDeletion
		);
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			inspection.OccurrenceProfiles[ 1 ].ResourceDeletion
		);
	}

	[Fact]
	public void EarlierIncompleteDatabasePreventsLaterObservedProfileFromBecomingEffective() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateIncompleteCatalog(
						"blocking",
						Array.Empty<TerminalDescription>()
					),
					CreateCatalog(
						"later",
						CreateTerminal(
							"target",
							PersistentRasterLifecycleInspector.PersistentUploadCapabilityName
						)
					),
				]
			);

		PersistentRasterLifecycleDatabaseSetInspection inspection =
			PersistentRasterLifecycleInspector.Inspect(
				set,
				"target"
			);

		Assert.Equal(
			TermInfoDatabaseSetLookupStatus.Indeterminate,
			inspection.Lookup.Status
		);
		Assert.Null( inspection.EffectiveProfile );
		Assert.Equal( new[] { 0 }, inspection.Lookup.BlockingDatabaseIndices );
		Assert.Single( inspection.OccurrenceProfiles );
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			inspection.OccurrenceProfiles[ 0 ].PersistentUpload
		);
	}

	[Fact]
	public void LaterIncompleteDatabaseDoesNotInvalidateEarlierKnownWinner() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog(
						"winner",
						CreateTerminal(
							"target",
							PersistentRasterLifecycleInspector.PlacementDeletionCapabilityName
						)
					),
					CreateIncompleteCatalog(
						"later-incomplete",
						Array.Empty<TerminalDescription>()
					),
				]
			);

		PersistentRasterLifecycleDatabaseSetInspection inspection =
			PersistentRasterLifecycleInspector.Inspect(
				set,
				"target"
			);

		Assert.Equal(
			TermInfoDatabaseSetLookupStatus.WinnerKnown,
			inspection.Lookup.Status
		);
		Assert.Equal( new[] { 1 }, inspection.Lookup.IncompleteDatabaseIndices );
		Assert.Empty( inspection.Lookup.BlockingDatabaseIndices );
		Assert.NotNull( inspection.EffectiveProfile );
		Assert.Equal(
			PersistentRasterLifecycleSupportStatus.Supported,
			inspection.EffectiveProfile!.PlacementDeletion
		);
	}

	[Fact]
	public void CompleteDatabaseAbsenceHasNoEffectiveOrOccurrenceProfile() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog(
						"absence",
						CreateTerminal( "other" )
					),
				]
			);

		PersistentRasterLifecycleDatabaseSetInspection inspection =
			PersistentRasterLifecycleInspector.Inspect(
				set,
				"missing"
			);

		Assert.Equal(
			TermInfoDatabaseSetLookupStatus.NotObserved,
			inspection.Lookup.Status
		);
		Assert.Null( inspection.EffectiveProfile );
		Assert.Empty( inspection.OccurrenceProfiles );
	}

	[Fact]
	public void DatabaseCompositionIsCultureAndRepetitionIndependent() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog(
						"culture",
						CreateTerminal(
							"I-target",
							PersistentRasterLifecycleInspector.MultiplePlacementsCapabilityName,
							PersistentRasterLifecycleInspector.PersistentUploadCapabilityName
						)
					),
				]
			);
		PersistentRasterLifecycleDatabaseSetInspection baseline =
			PersistentRasterLifecycleInspector.Inspect(
				set,
				"I-target"
			);

		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "tr-TR" );
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "tr-TR" );
			PersistentRasterLifecycleDatabaseSetInspection repeated =
				PersistentRasterLifecycleInspector.Inspect(
					set,
					"I-target"
				);

			Assert.Equal( baseline.Lookup.Status, repeated.Lookup.Status );
			Assert.Equal(
				baseline.EffectiveProfile!.Evidence.Select(
					evidence => evidence.SourceLabel
				),
				repeated.EffectiveProfile!.Evidence.Select(
					evidence => evidence.SourceLabel
				)
			);
			Assert.Equal(
				baseline.EffectiveProfile.MultiplePlacements,
				repeated.EffectiveProfile.MultiplePlacements
			);
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}
	}

	[Fact]
	public void DatabaseInspectionSnapshotsOccurrenceProfiles() {
		TermInfoDatabaseSet set =
			TermInfoDatabaseInspector.CreateSet(
				[
					CreateCatalog(
						"immutable",
						CreateTerminal(
							"target",
							PersistentRasterLifecycleInspector.RasterDisplayCapabilityName
						)
					),
				]
			);

		PersistentRasterLifecycleDatabaseSetInspection inspection =
			PersistentRasterLifecycleInspector.Inspect(
				set,
				"target"
			);

		Assert.False(
			inspection.OccurrenceProfiles is PersistentRasterLifecycleProfile[]
		);
		if (
			inspection.OccurrenceProfiles
				is IList<PersistentRasterLifecycleProfile> profiles
		) {
			Assert.True( profiles.IsReadOnly );
		}
		Assert.All(
			typeof( PersistentRasterLifecycleDatabaseSetInspection ).GetProperties(),
			property => Assert.Null( property.SetMethod )
		);
	}

	private static TerminalDescription CreateTerminal(
		string name,
		params string[] semanticCapabilities
	) {
		TerminalDescriptionBuilder builder = new( name );
		foreach ( string capabilityName in semanticCapabilities ) {
			builder.SetExtendedBoolean( capabilityName );
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
		TermInfoDatabaseCatalogIssue issue =
			new(
				TermInfoDatabaseCatalogIssueKind.MalformedEntry,
				Path.Combine( root, "entries", "malformed" ),
				"RL05 incomplete fixture."
			);
		return CreateCatalogCore(
			rootName,
			terminals,
			[ issue ]
		);
	}

	private static TermInfoDatabaseCatalog CreateCatalogCore(
		string rootName,
		IEnumerable<TerminalDescription> terminals,
		IEnumerable<TermInfoDatabaseCatalogIssue> issues
	) {
		string root = AbsolutePath( rootName );
		TermInfoDatabaseCatalogEntry[] entries =
			terminals
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
		string[] duplicates =
			entries
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
			$"icod-terminfo-rl05-{suffix}-{Guid.NewGuid():N}"
		);
}
