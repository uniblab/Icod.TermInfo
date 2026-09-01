using System.Text;
using Icod.CommandFramework.Diagnostics;
using Xunit;

namespace Icod.TermInfo.Toe.Tests;

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
			"Usage: toe",
			ReadText( stdout )
		);
		Assert.Contains( "-a", ReadText( stdout ) );
		Assert.Contains( "-h", ReadText( stdout ) );
		Assert.Contains( "-s", ReadText( stdout ) );
		Assert.Contains( "-u", ReadText( stdout ) );
		Assert.Contains( "-U", ReadText( stdout ) );
		Assert.Contains( "-D", ReadText( stdout ) );
		Assert.Empty( ReadText( stderr ) );
	}

	[Theory]
	[InlineData( "-V" )]
	[InlineData( "--version" )]
	public async Task VersionReportsCoordinatedStableVersion( string option ) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ option ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.Success, status );
		Assert.Contains( "1.6.1", ReadText( stdout ) );
		Assert.DoesNotContain( "Alpha", ReadText( stdout ) );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task UnsupportedArgumentWritesStderrAndReturnsUsageError() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "--not-a-toe-option" ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.UsageError, status );
		Assert.Empty( ReadText( stdout ) );
		Assert.Contains( "unsupported option", ReadText( stderr ) );
	}

	[Theory]
	[InlineData( "-u" )]
	[InlineData( "-U" )]
	public async Task SourceDependencyModeRequiresExactlyOneOperand( string option ) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ option ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.UsageError, status );
		Assert.Empty( ReadText( stdout ) );
		Assert.Contains( "exactly one source file operand", ReadText( stderr ) );
	}

	[Fact]
	public async Task SpecialModeCannotBeCombinedWithListingArguments() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			[ "-D", "." ],
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( CommandExitCodes.UsageError, status );
		Assert.Empty( ReadText( stdout ) );
		Assert.Contains( "must be used alone", ReadText( stderr ) );
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
