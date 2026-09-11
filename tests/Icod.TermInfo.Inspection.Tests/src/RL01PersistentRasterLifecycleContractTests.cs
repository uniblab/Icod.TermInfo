using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RL01PersistentRasterLifecycleContractTests {
	[Fact]
	public void SupportStatusMembershipAndNumericsAreFrozen() {
		Assert.Equal(
			new[] {
				PersistentRasterLifecycleSupportStatus.Unknown,
				PersistentRasterLifecycleSupportStatus.Supported,
				PersistentRasterLifecycleSupportStatus.Unsupported,
				PersistentRasterLifecycleSupportStatus.Contradicted,
			},
			Enum.GetValues<PersistentRasterLifecycleSupportStatus>()
		);
		Assert.Equal(0, (int)PersistentRasterLifecycleSupportStatus.Unknown);
		Assert.Equal(1, (int)PersistentRasterLifecycleSupportStatus.Supported);
		Assert.Equal(2, (int)PersistentRasterLifecycleSupportStatus.Unsupported);
		Assert.Equal(3, (int)PersistentRasterLifecycleSupportStatus.Contradicted);
	}

	[Fact]
	public void OperationMembershipAndNumericsAreFrozen() {
		Assert.Equal(
			new[] {
				PersistentRasterLifecycleOperation.DisplayEphemeral,
				PersistentRasterLifecycleOperation.UploadResource,
				PersistentRasterLifecycleOperation.CreatePlacement,
				PersistentRasterLifecycleOperation.UpdatePlacement,
				PersistentRasterLifecycleOperation.DeletePlacement,
				PersistentRasterLifecycleOperation.DeleteResource,
			},
			Enum.GetValues<PersistentRasterLifecycleOperation>()
		);
		Assert.Equal(0, (int)PersistentRasterLifecycleOperation.DisplayEphemeral);
		Assert.Equal(1, (int)PersistentRasterLifecycleOperation.UploadResource);
		Assert.Equal(2, (int)PersistentRasterLifecycleOperation.CreatePlacement);
		Assert.Equal(3, (int)PersistentRasterLifecycleOperation.UpdatePlacement);
		Assert.Equal(4, (int)PersistentRasterLifecycleOperation.DeletePlacement);
		Assert.Equal(5, (int)PersistentRasterLifecycleOperation.DeleteResource);
	}
}
