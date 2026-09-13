using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class PG01PersistentRasterPlacementContractTests {
	[Fact]
	public void PlacementSubjectMembershipAndNumericsAreFrozen() {
		Assert.Equal(
			new[] {
				PersistentRasterPlacementSubject.SourceRectangle,
				PersistentRasterPlacementSubject.SignedZOrder,
			},
			Enum.GetValues<PersistentRasterPlacementSubject>()
		);
		Assert.Equal( 0, (int)PersistentRasterPlacementSubject.SourceRectangle );
		Assert.Equal( 1, (int)PersistentRasterPlacementSubject.SignedZOrder );
	}

	[Fact]
	public void LifecycleEvidenceSubjectMembershipRemainsFrozenAtOneEleven() {
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
		Assert.Equal( 0, (int)PersistentRasterLifecycleEvidenceSubject.RasterDisplay );
		Assert.Equal( 1, (int)PersistentRasterLifecycleEvidenceSubject.PersistentUpload );
		Assert.Equal( 2, (int)PersistentRasterLifecycleEvidenceSubject.AcknowledgedUpload );
		Assert.Equal( 3, (int)PersistentRasterLifecycleEvidenceSubject.PlacementCreation );
		Assert.Equal( 4, (int)PersistentRasterLifecycleEvidenceSubject.MultiplePlacements );
		Assert.Equal( 5, (int)PersistentRasterLifecycleEvidenceSubject.PlacementUpdate );
		Assert.Equal( 6, (int)PersistentRasterLifecycleEvidenceSubject.PlacementDeletion );
		Assert.Equal( 7, (int)PersistentRasterLifecycleEvidenceSubject.ResourceDeletion );
	}

	[Fact]
	public void PlacementClassificationReusesFrozenLifecycleSupportStates() {
		Assert.Equal(
			new[] {
				PersistentRasterLifecycleSupportStatus.Unknown,
				PersistentRasterLifecycleSupportStatus.Supported,
				PersistentRasterLifecycleSupportStatus.Unsupported,
				PersistentRasterLifecycleSupportStatus.Contradicted,
			},
			Enum.GetValues<PersistentRasterLifecycleSupportStatus>()
		);
		Assert.Equal( 0, (int)PersistentRasterLifecycleSupportStatus.Unknown );
		Assert.Equal( 1, (int)PersistentRasterLifecycleSupportStatus.Supported );
		Assert.Equal( 2, (int)PersistentRasterLifecycleSupportStatus.Unsupported );
		Assert.Equal( 3, (int)PersistentRasterLifecycleSupportStatus.Contradicted );
	}
}
