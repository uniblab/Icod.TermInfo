using System.Text;
using Icod.CommandFramework.Diagnostics;
using Xunit;

namespace Icod.TermInfo.InfoCmp.Tests;

public sealed class CommandTests {
	[Fact]
	public async Task HelpWritesStdoutAndReturnsSuccess() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "--help" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Success, status );
		Assert.Contains(
			"Usage: infocmp",
			ReadText( stdout )
		);
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task VersionReportsCoordinatedDevelopmentVersion() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "--version" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Success, status );
		Assert.Contains( "1.4.0-Alpha-10", ReadText( stdout ) );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task UnsupportedArgumentWritesStderrAndReturnsUsageError() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "--not-an-infocmp-option" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.UsageError, status );
		Assert.Empty( ReadText( stdout ) );
		Assert.Contains( "unsupported option", ReadText( stderr ) );
	}

	[Fact]
	public async Task CancellationReturnsCanceledWithoutOutput() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();

		int status = await Command.RunAsync(
			[],
			stdin,
			stdout,
			stderr,
			cancellation.Token
		);

		Assert.Equal( CommandExitCodes.Canceled, status );
		Assert.Empty( ReadText( stdout ) );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task CommandLeavesCallerOwnedStreamsOpen() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		_ = await Command.RunAsync(
			[ "--help" ],
			stdin,
			stdout,
			stderr
		);

		stdout.WriteByte( 0 );
		stderr.WriteByte( 0 );
		Assert.True( stdin.CanRead );
		Assert.True( stdout.CanWrite );
		Assert.True( stderr.CanWrite );
	}

	[Fact]
	public async Task NullArgumentsAreRejectedBeforeExecution() {
		using var stream = new MemoryStream();

		await Assert.ThrowsAsync<ArgumentNullException>(
			() => Command.RunAsync(
				null!,
				stream,
				stream,
				stream
			)
		);
	}

	private static string ReadText( MemoryStream stream ) {
		ArgumentNullException.ThrowIfNull( stream );

		return Encoding.UTF8.GetString(
			stream.ToArray()
		);
	}
}
