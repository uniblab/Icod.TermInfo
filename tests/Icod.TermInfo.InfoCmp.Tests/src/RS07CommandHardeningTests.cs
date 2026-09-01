using Icod.CommandFramework.Diagnostics;
using Xunit;

namespace Icod.TermInfo.InfoCmp.Tests;

public sealed class RS07CommandHardeningTests {
	[Fact]
	public async Task RelativeSynthesisHonorsPreCanceledTokenWithoutOutput() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();

		int status = await Command.RunAsync(
			[ "-u", "target", "parent" ],
			stdin,
			stdout,
			stderr,
			cancellation.Token
		);

		Assert.Equal( CommandExitCodes.Canceled, status );
		Assert.Empty( stdout.ToArray() );
		Assert.Empty( stderr.ToArray() );
	}
}
