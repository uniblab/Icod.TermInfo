using System.Text;
using Xunit;

namespace Icod.TermInfo.Router.Tests;

public sealed class CommandTests {
	[Fact]
	public async Task HelpListsRoutedCommands() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			new string[] { "--help" },
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( 0, status );
		string output =
			ReadText( stdout );
		Assert.Contains( "tic", output );
		Assert.Contains( "infocmp", output );
		Assert.Contains( "toe", output );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task VersionReportsCentralSuiteVersion() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			new string[] { "-V" },
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( 0, status );
		Assert.Contains( "1.5.0", ReadText( stdout ) );
		Assert.Empty( ReadText( stderr ) );
	}

	[Theory]
	[InlineData( "tic" )]
	[InlineData( "infocmp" )]
	[InlineData( "toe" )]
	public async Task RoutesVersionRequestToSelectedCommand(
		string commandName
	) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			new string[] { commandName, "-V" },
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( 0, status );
		Assert.Contains( "1.5.0", ReadText( stdout ) );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task MissingCommandReturnsUsageError() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			Array.Empty<string>(),
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( 2, status );
		Assert.Empty( ReadText( stdout ) );
		Assert.Contains( "missing command", ReadText( stderr ) );
	}

	[Fact]
	public async Task UnknownCommandReturnsUsageError() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			new string[] { "not-a-command" },
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( 2, status );
		Assert.Empty( ReadText( stdout ) );
		Assert.Contains( "unknown command", ReadText( stderr ) );
	}

	[Fact]
	public async Task CancellationReturnsCanceledWithoutOutput() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();

		int status = await Command.RunAsync(
			new string[] { "tic", "-V" },
			stdin,
			stdout,
			stderr,
			cancellation.Token
		);

		Assert.Equal( 130, status );
		Assert.Empty( ReadText( stdout ) );
		Assert.Empty( ReadText( stderr ) );
	}

	[Fact]
	public async Task RouterLeavesCallerOwnedStreamsOpen() {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		_ = await Command.RunAsync(
			new string[] { "toe", "-V" },
			stdin,
			stdout,
			stderr
		);

		stdout.WriteByte( 0 );
		stderr.WriteByte( 0 );
	}

	private static string ReadText(
		MemoryStream stream
	) {
		ArgumentNullException.ThrowIfNull( stream );

		return Encoding.UTF8.GetString(
			stream.ToArray()
		);
	}
}
