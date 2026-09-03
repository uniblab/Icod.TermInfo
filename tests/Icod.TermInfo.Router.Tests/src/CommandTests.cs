using System.Text;
using Xunit;

namespace Icod.TermInfo.Router.Tests;

public sealed class CommandTests {
	[Theory]
	[InlineData( "-h" )]
	[InlineData( "--help" )]
	public async Task HelpListsRoutedCommands( string option ) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			new string[] { option },
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
		Assert.Contains( "captoinfo", output );
		Assert.Contains( "infotocap", output );
		Assert.Empty( ReadText( stderr ) );
	}

	[Theory]
	[InlineData( "-V" )]
	[InlineData( "--version" )]
	public async Task VersionReportsCentralSuiteVersion(
		string option
	) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			new string[] { option },
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( 0, status );
		Assert.Contains( "1.9.0", ReadText( stdout ) );
		Assert.Empty( ReadText( stderr ) );
	}

	[Theory]
	[InlineData( "tic" )]
	[InlineData( "infocmp" )]
	[InlineData( "toe" )]
	[InlineData( "captoinfo" )]
	[InlineData( "infotocap" )]
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
		Assert.Contains( "1.9.0", ReadText( stdout ) );
		Assert.Empty( ReadText( stderr ) );
	}

	[Theory]
	[InlineData( "tic", "Usage: tic" )]
	[InlineData( "infocmp", "Usage: infocmp" )]
	[InlineData( "toe", "Usage: toe" )]
	[InlineData( "captoinfo", "Usage: captoinfo" )]
	[InlineData( "infotocap", "Usage: infotocap" )]
	public async Task RoutesHelpRequestToSelectedCommand(
		string commandName,
		string expectedUsage
	) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			new string[] { commandName, "--help" },
			stdin,
			stdout,
			stderr
		);

		Assert.Equal( 0, status );
		Assert.Contains( expectedUsage, ReadText( stdout ) );
		Assert.Empty( ReadText( stderr ) );
	}

	[Theory]
	[InlineData( "tic", "--not-a-t05-option" )]
	[InlineData( "infocmp", "--not-an-infocmp-option" )]
	[InlineData( "toe", "--not-a-toe-option" )]
	[InlineData( "captoinfo", "--not-a-captoinfo-option" )]
	[InlineData( "infotocap", "--not-an-infotocap-option" )]
	public async Task RoutedFailureMatchesSelectedCommand(
		string commandName,
		string argument
	) {
		using var directStdin = new MemoryStream();
		using var directStdout = new MemoryStream();
		using var directStderr = new MemoryStream();
		using var routedStdin = new MemoryStream();
		using var routedStdout = new MemoryStream();
		using var routedStderr = new MemoryStream();

		int directStatus = await RunDirectAsync(
			commandName,
			new string[] { argument },
			directStdin,
			directStdout,
			directStderr
		);
		int routedStatus = await Command.RunAsync(
			new string[] { commandName, argument },
			routedStdin,
			routedStdout,
			routedStderr
		);

		Assert.Equal( directStatus, routedStatus );
		Assert.Equal(
			ReadText( directStdout ),
			ReadText( routedStdout )
		);
		Assert.Equal(
			ReadText( directStderr ),
			ReadText( routedStderr )
		);
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

	private static Task<int> RunDirectAsync(
		string commandName,
		string[] arguments,
		Stream stdin,
		Stream stdout,
		Stream stderr
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( commandName );
		ArgumentNullException.ThrowIfNull( arguments );
		ArgumentNullException.ThrowIfNull( stdin );
		ArgumentNullException.ThrowIfNull( stdout );
		ArgumentNullException.ThrowIfNull( stderr );

		switch ( commandName ) {
			case "tic":
				return Icod.TermInfo.Tic.Command.RunAsync(
					arguments,
					stdin,
					stdout,
					stderr
				);
			case "infocmp":
				return Icod.TermInfo.InfoCmp.Command.RunAsync(
					arguments,
					stdin,
					stdout,
					stderr
				);
			case "toe":
				return Icod.TermInfo.Toe.Command.RunAsync(
					arguments,
					stdin,
					stdout,
					stderr
				);
			case "captoinfo":
				return Icod.TermInfo.CapToInfo.Command.RunAsync(
					arguments,
					stdin,
					stdout,
					stderr
				);
			case "infotocap":
				return Icod.TermInfo.InfoToCap.Command.RunAsync(
					arguments,
					stdin,
					stdout,
					stderr
				);
			default:
				throw new ArgumentOutOfRangeException( nameof( commandName ) );
		}
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
