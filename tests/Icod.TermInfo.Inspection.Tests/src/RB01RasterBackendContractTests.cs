using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RB01RasterBackendContractTests {
	[Fact]
	public void BackendAndStatusNumericsAreFrozen() {
		Assert.Equal(
			new[] {
				RasterBackendKind.Sixel,
				RasterBackendKind.KittyGraphics,
			},
			Enum.GetValues<RasterBackendKind>()
		);
		Assert.Equal( 0, (int)RasterBackendKind.Sixel );
		Assert.Equal( 1, (int)RasterBackendKind.KittyGraphics );

		Assert.Equal(
			new[] {
				RasterBackendEvidenceKind.CapabilityDerived,
				RasterBackendEvidenceKind.Declared,
				RasterBackendEvidenceKind.Verified,
			},
			Enum.GetValues<RasterBackendEvidenceKind>()
		);
		Assert.Equal( 0, (int)RasterBackendEvidenceKind.CapabilityDerived );
		Assert.Equal( 1, (int)RasterBackendEvidenceKind.Declared );
		Assert.Equal( 2, (int)RasterBackendEvidenceKind.Verified );

		Assert.Equal(
			new[] {
				RasterBackendSupportStatus.Unknown,
				RasterBackendSupportStatus.Supported,
				RasterBackendSupportStatus.Unsupported,
				RasterBackendSupportStatus.Contradicted,
			},
			Enum.GetValues<RasterBackendSupportStatus>()
		);
		Assert.Equal( 0, (int)RasterBackendSupportStatus.Unknown );
		Assert.Equal( 1, (int)RasterBackendSupportStatus.Supported );
		Assert.Equal( 2, (int)RasterBackendSupportStatus.Unsupported );
		Assert.Equal( 3, (int)RasterBackendSupportStatus.Contradicted );

		Assert.Equal(
			new[] {
				RasterBackendCandidateStatus.Satisfied,
				RasterBackendCandidateStatus.RequiresRuntimeVerification,
				RasterBackendCandidateStatus.Impossible,
			},
			Enum.GetValues<RasterBackendCandidateStatus>()
		);
		Assert.Equal( 0, (int)RasterBackendCandidateStatus.Satisfied );
		Assert.Equal( 1, (int)RasterBackendCandidateStatus.RequiresRuntimeVerification );
		Assert.Equal( 2, (int)RasterBackendCandidateStatus.Impossible );

		Assert.Equal(
			new[] {
				RasterBackendSelectionStatus.Selected,
				RasterBackendSelectionStatus.RequiresRuntimeVerification,
				RasterBackendSelectionStatus.RequiresPreference,
				RasterBackendSelectionStatus.Impossible,
			},
			Enum.GetValues<RasterBackendSelectionStatus>()
		);
		Assert.Equal( 0, (int)RasterBackendSelectionStatus.Selected );
		Assert.Equal( 1, (int)RasterBackendSelectionStatus.RequiresRuntimeVerification );
		Assert.Equal( 2, (int)RasterBackendSelectionStatus.RequiresPreference );
		Assert.Equal( 3, (int)RasterBackendSelectionStatus.Impossible );
	}

	[Fact]
	public void BackendEvidenceBoundsAndValidationAreFrozen() {
		RasterBackendEvidenceOptions defaults = new();

		Assert.Equal( 256, RasterBackendEvidenceOptions.DefaultMaximumEvidenceCount );
		Assert.Equal( 4096, RasterBackendEvidenceOptions.MaximumSupportedEvidenceCount );
		Assert.Equal( 256, RasterBackendEvidenceOptions.MaximumSourceLabelLength );
		Assert.Equal( 256, defaults.MaximumEvidenceCount );
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new RasterBackendEvidenceOptions( 0 )
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new RasterBackendEvidenceOptions( 4097 )
		);

		string maximumLabel = new( 'x', 256 );
		RasterBackendEvidence accepted = new(
			RasterBackendKind.Sixel,
			isPositive: true,
			RasterBackendEvidenceKind.Verified,
			maximumLabel,
			0
		);
		Assert.Equal( maximumLabel, accepted.SourceLabel );

		Assert.Throws<ArgumentOutOfRangeException>(
			() => new RasterBackendEvidence(
				(RasterBackendKind)99,
				true,
				RasterBackendEvidenceKind.Verified,
				"source",
				0
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new RasterBackendEvidence(
				RasterBackendKind.Sixel,
				true,
				(RasterBackendEvidenceKind)99,
				"source",
				0
			)
		);
		Assert.Throws<ArgumentException>(
			() => new RasterBackendEvidence(
				RasterBackendKind.Sixel,
				true,
				RasterBackendEvidenceKind.Verified,
				" ",
				0
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new RasterBackendEvidence(
				RasterBackendKind.Sixel,
				true,
				RasterBackendEvidenceKind.Verified,
				new string( 'x', 257 ),
				0
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new RasterBackendEvidence(
				RasterBackendKind.Sixel,
				true,
				RasterBackendEvidenceKind.Verified,
				"source",
				-1
			)
		);
	}

	[Fact]
	public void BackendEvidenceSnapshotIsBoundedCanonicalAndImmutable() {
		List<RasterBackendEvidence> source = [
			new RasterBackendEvidence(
				RasterBackendKind.KittyGraphics,
				true,
				RasterBackendEvidenceKind.Declared,
				"z",
				1
			),
			new RasterBackendEvidence(
				RasterBackendKind.Sixel,
				false,
				RasterBackendEvidenceKind.Verified,
				"b",
				2
			),
			new RasterBackendEvidence(
				RasterBackendKind.Sixel,
				true,
				RasterBackendEvidenceKind.CapabilityDerived,
				"a",
				0
			),
		];

		IReadOnlyList<RasterBackendEvidence> snapshot =
			RasterBackendEvidence.Snapshot( source );
		source.Clear();

		Assert.Equal( 3, snapshot.Count );
		Assert.Equal( RasterBackendKind.Sixel, snapshot[ 0 ].Backend );
		Assert.Equal( 0, snapshot[ 0 ].SourceOrdinal );
		Assert.Equal( RasterBackendKind.Sixel, snapshot[ 1 ].Backend );
		Assert.Equal( 2, snapshot[ 1 ].SourceOrdinal );
		Assert.Equal( RasterBackendKind.KittyGraphics, snapshot[ 2 ].Backend );

		Assert.Throws<ArgumentNullException>(
			() => RasterBackendEvidence.Snapshot( null! )
		);
		Assert.Throws<ArgumentException>(
			() => RasterBackendEvidence.Snapshot(
				new RasterBackendEvidence[] { null! }
			)
		);

		RasterBackendEvidence repeated = new(
			RasterBackendKind.Sixel,
			true,
			RasterBackendEvidenceKind.Declared,
			"bounded",
			0
		);
		RasterBackendEvidence[] maximum =
			Enumerable.Repeat( repeated, 4096 ).ToArray();
		Assert.Equal(
			4096,
			RasterBackendEvidence.Snapshot(
				maximum,
				new RasterBackendEvidenceOptions( 4096 )
			).Count
		);
		Assert.Throws<ArgumentException>(
			() => RasterBackendEvidence.Snapshot(
				Enumerable.Repeat( repeated, 4097 ),
				new RasterBackendEvidenceOptions( 4096 )
			)
		);
	}

	[Fact]
	public void SelectionOptionsAreExplicitUniqueBoundedAndSnapshotted() {
		RasterBackendSelectionOptions defaults = new();
		Assert.Equal( 2, RasterBackendSelectionOptions.MaximumSupportedPreferenceCount );
		Assert.Empty( defaults.PreferenceOrder );

		List<RasterBackendKind> preference = [
			RasterBackendKind.KittyGraphics,
			RasterBackendKind.Sixel,
		];
		RasterBackendSelectionOptions options = new( preference );
		preference.Reverse();

		Assert.Equal(
			new[] {
				RasterBackendKind.KittyGraphics,
				RasterBackendKind.Sixel,
			},
			options.PreferenceOrder
		);
		Assert.Throws<ArgumentException>(
			() => new RasterBackendSelectionOptions(
				new[] {
					RasterBackendKind.Sixel,
					RasterBackendKind.Sixel,
				}
			)
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new RasterBackendSelectionOptions(
				new[] { (RasterBackendKind)99 }
			)
		);
		Assert.Throws<ArgumentException>(
			() => new RasterBackendSelectionOptions(
				new[] {
					RasterBackendKind.Sixel,
					RasterBackendKind.KittyGraphics,
					RasterBackendKind.Sixel,
				}
			)
		);
	}

	[Fact]
	public void SelectionRequestSupportsLifecycleOnlyOrAdvancedPlacement() {
		PersistentRasterLifecycleRequest lifecycle = new( displayEphemeral: true );
		RasterBackendSelectionRequest lifecycleOnly = new( lifecycle );

		Assert.Same( lifecycle, lifecycleOnly.LifecycleRequest );
		Assert.Null( lifecycleOnly.PlacementRequest );

		PersistentRasterLifecycleRequest persistent = new( placementCount: 1 );
		PersistentRasterPlacementRequest placement = new(
			requireSourceRectangle: true
		);
		RasterBackendSelectionRequest advanced = new(
			persistent,
			placement
		);
		Assert.Same( persistent, advanced.LifecycleRequest );
		Assert.Same( placement, advanced.PlacementRequest );
	}

	[Fact]
	public void DerivedProfileAndPlanConstructionRemainLibraryOwned() {
		Assert.Empty( typeof( RasterBackendProfile ).GetConstructors() );
		Assert.Empty( typeof( RasterBackendCandidateEvaluation ).GetConstructors() );
		Assert.Empty( typeof( RasterBackendSelectionPlan ).GetConstructors() );

		ConstructorInfo? candidateConstructor =
			typeof( RasterBackendCandidate ).GetConstructor(
				new[] {
					typeof( RasterBackendProfile ),
					typeof( PersistentRasterLifecycleProfile ),
					typeof( PersistentRasterPlacementProfile ),
				}
			);
		Assert.NotNull( candidateConstructor );

		Assert.Equal(
			typeof( RasterBackendProfile ),
			typeof( RasterBackendCandidate )
				.GetProperty( nameof( RasterBackendCandidate.BackendProfile ) )!
				.PropertyType
		);
		Assert.Equal(
			typeof( PersistentRasterLifecyclePlan ),
			typeof( RasterBackendCandidateEvaluation )
				.GetProperty( nameof( RasterBackendCandidateEvaluation.LifecyclePlan ) )!
				.PropertyType
		);
		Assert.Equal(
			typeof( PersistentRasterPlacementPlan ),
			Nullable.GetUnderlyingType(
				typeof( RasterBackendCandidateEvaluation )
					.GetProperty( nameof( RasterBackendCandidateEvaluation.PlacementPlan ) )!
					.PropertyType
			) ?? typeof( PersistentRasterPlacementPlan )
		);
		Assert.Equal(
			typeof( RasterBackendKind? ),
			typeof( RasterBackendSelectionPlan )
				.GetProperty( nameof( RasterBackendSelectionPlan.SelectedBackend ) )!
				.PropertyType
		);
	}

	[Fact]
	public void OneThirteenAndDependencyBoundariesRemainFrozen() {
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:5",
			TermInfoJsonRenderer.PersistentRasterRuntimeSchemaIdentifier
		);
		Assert.Equal( 5, TermInfoJsonRenderer.PersistentRasterRuntimeSchemaVersion );

		string root = FindRepositoryRoot();
		string freeze = File.ReadAllText(
			Path.Combine(
				root,
				"docs",
				"1.13.0-INSPECTION-PUBLIC-API-FREEZE.md"
			)
		);
		Assert.Contains(
			"fd827a25abafb8e9ff3915567f45f9f2ec51b332bfd8a82dd4e4bca20290e764",
			freeze,
			StringComparison.Ordinal
		);

		XDocument inspectionProject = XDocument.Load(
			Path.Combine(
				root,
				"Icod.TermInfo.Inspection",
				"Icod.TermInfo.Inspection.csproj"
			)
		);
		Assert.DoesNotContain(
			inspectionProject.Descendants(),
			element =>
				(element.Name.LocalName == "PackageReference"
					|| element.Name.LocalName == "ProjectReference")
				&& string.Equals(
					element.Attribute( "Include" )?.Value,
					"Icod.Terminal",
					StringComparison.Ordinal
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
