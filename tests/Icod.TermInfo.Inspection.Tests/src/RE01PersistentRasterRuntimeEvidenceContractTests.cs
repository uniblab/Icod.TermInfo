using System.Xml.Linq;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RE01PersistentRasterRuntimeEvidenceContractTests {
	[Fact]
	public void RuntimeOutcomeMembershipAndNumericsAreFrozen() {
		Assert.Equal(
			new[] {
				PersistentRasterRuntimeObservationOutcome.Supported,
				PersistentRasterRuntimeObservationOutcome.Unsupported,
				PersistentRasterRuntimeObservationOutcome.Inconclusive,
			},
			Enum.GetValues<PersistentRasterRuntimeObservationOutcome>()
		);
		Assert.Equal( 0, (int)PersistentRasterRuntimeObservationOutcome.Supported );
		Assert.Equal( 1, (int)PersistentRasterRuntimeObservationOutcome.Unsupported );
		Assert.Equal( 2, (int)PersistentRasterRuntimeObservationOutcome.Inconclusive );
	}

	[Fact]
	public void RuntimeObservationBoundsAreFrozen() {
		PersistentRasterRuntimeObservationOptions defaults = new();

		Assert.Equal(
			256,
			PersistentRasterRuntimeObservationOptions.DefaultMaximumObservationCount
		);
		Assert.Equal(
			4096,
			PersistentRasterRuntimeObservationOptions.MaximumSupportedObservationCount
		);
		Assert.Equal(
			256,
			PersistentRasterRuntimeObservationOptions.MaximumSourceLabelLength
		);
		Assert.Equal(
			PersistentRasterRuntimeObservationOptions.DefaultMaximumObservationCount,
			defaults.MaximumObservationCount
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterRuntimeObservationOptions( 0 )
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new PersistentRasterRuntimeObservationOptions( 4097 )
		);
	}

	[Fact]
	public void FrozenLifecycleVocabularyRemainsUnchanged() {
		Assert.Equal(
			new[] {
				PersistentRasterLifecycleEvidenceSubject.RasterDisplay,
				PersistentRasterLifecycleEvidenceSubject.PersistentUpload,
				PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload,
				PersistentRasterLifecycleEvidenceSubject.PlacementCreation,
				PersistentRasterLifecycleEvidenceSubject.MultiplePlacements,
				PersistentRasterLifecycleEvidenceSubject.PlacementUpdate,
				PersistentRasterLifecycleEvidenceSubject.PlacementDeletion,
				PersistentRasterLifecycleEvidenceSubject.ResourceDeletion,
			},
			Enum.GetValues<PersistentRasterLifecycleEvidenceSubject>()
		);
		Assert.Equal(
			Enumerable.Range( 0, 8 ).ToArray(),
			Enum.GetValues<PersistentRasterLifecycleEvidenceSubject>()
				.Select( value => (int)value )
				.ToArray()
		);
		Assert.Equal(
			new[] {
				PersistentRasterLifecycleEvidenceKind.CapabilityDerived,
				PersistentRasterLifecycleEvidenceKind.Declared,
				PersistentRasterLifecycleEvidenceKind.Verified,
			},
			Enum.GetValues<PersistentRasterLifecycleEvidenceKind>()
		);
		Assert.Equal(
			new[] { 0, 1, 2 },
			Enum.GetValues<PersistentRasterLifecycleEvidenceKind>()
				.Select( value => (int)value )
				.ToArray()
		);
	}

	[Fact]
	public void FrozenPlacementVocabularyRemainsUnchanged() {
		Assert.Equal(
			new[] {
				PersistentRasterPlacementSubject.SourceRectangle,
				PersistentRasterPlacementSubject.SignedZOrder,
			},
			Enum.GetValues<PersistentRasterPlacementSubject>()
		);
		Assert.Equal( 0, (int)PersistentRasterPlacementSubject.SourceRectangle );
		Assert.Equal( 1, (int)PersistentRasterPlacementSubject.SignedZOrder );
		Assert.Equal(
			new[] {
				PersistentRasterPlacementEvidenceKind.CapabilityDerived,
				PersistentRasterPlacementEvidenceKind.Declared,
				PersistentRasterPlacementEvidenceKind.Verified,
			},
			Enum.GetValues<PersistentRasterPlacementEvidenceKind>()
		);
		Assert.Equal(
			new[] { 0, 1, 2 },
			Enum.GetValues<PersistentRasterPlacementEvidenceKind>()
				.Select( value => (int)value )
				.ToArray()
		);
	}

	[Fact]
	public void JsonVersionFourIdentityRemainsFrozen() {
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:4",
			TermInfoJsonRenderer.PersistentRasterPlacementSchemaIdentifier
		);
		Assert.Equal(
			4,
			TermInfoJsonRenderer.PersistentRasterPlacementSchemaVersion
		);
	}

	[Fact]
	public void RuntimeInterchangeDoesNotIntroduceUnifiedSubjectOrTerminalDependency() {
		Assert.DoesNotContain(
			typeof( PersistentRasterLifecycleProfile ).Assembly.GetExportedTypes(),
			type => string.Equals(
				type.FullName,
				"Icod.TermInfo.Inspection.PersistentRasterRuntimeSubject",
				StringComparison.Ordinal
			)
		);

		string root = FindRepositoryRoot();
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
