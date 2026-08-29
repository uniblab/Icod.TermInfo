using System.Text;
using Icod.CommandFramework.Diagnostics;
using Xunit;

namespace Icod.TermInfo.Tic.Tests;

public sealed class T10CliHardeningTests {
	private const string SimpleSource =
		"demo|demo-alias|Demo terminal,\n"
		+ "    am,\n"
		+ "    cols#80,\n";

	[Fact]
	public async Task ClusteredAndRepeatedBooleanOptionsAreAccepted() {
		CommandResult result = await RunAsync(
			SimpleSource,
			"-ccx",
			"-"
		);

		Assert.Equal( CommandExitCodes.Success, result.Status );
		Assert.Equal( string.Empty, result.Stdout );
		Assert.Equal( string.Empty, result.Stderr );
	}

	[Fact]
	public async Task AttachedSelectionValueIsAccepted() {
		CommandResult result = await RunAsync(
			SimpleSource,
			"-c",
			"-edemo",
			"-"
		);

		Assert.Equal( CommandExitCodes.Success, result.Status );
		Assert.Equal( string.Empty, result.Stderr );
	}

	[Fact]
	public async Task EndOfOptionsAllowsHyphenLeadingSourceOperand() {
		string sourceName = $"-missing-t10-{Guid.NewGuid():N}.ti";
		CommandResult result = await RunAsync(
			string.Empty,
			"-c",
			"--",
			sourceName
		);

		Assert.Equal( CommandExitCodes.Failure, result.Status );
		Assert.DoesNotContain(
			"unsupported option",
			result.Stderr,
			StringComparison.Ordinal
		);
		Assert.Contains(
			sourceName,
			result.Stderr,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public async Task ConflictingCheckOnlyPublicationModeIsUsageError() {
		CommandResult result = await RunAsync(
			SimpleSource,
			"-cs",
			"-"
		);

		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.StartsWith(
			"tic: ",
			result.Stderr,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public async Task UnsupportedNcursesSwitchIsNeverIgnored() {
		CommandResult result = await RunAsync(
			SimpleSource,
			"-C",
			"-"
		);

		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Contains(
			"unsupported option",
			result.Stderr,
			StringComparison.Ordinal
		);
	}

	private static async Task<CommandResult> RunAsync(
		string stdinText,
		params string[] args
	) {
		using MemoryStream stdin = new(
			new UTF8Encoding( false ).GetBytes( stdinText )
		);
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();

		int status = await Command.RunAsync(
			args,
			stdin,
			stdout,
			stderr
		);

		return new CommandResult(
			status,
			ReadText( stdout ),
			ReadText( stderr )
		);
	}

	private static string ReadText( MemoryStream stream ) {
		return new UTF8Encoding( false ).GetString( stream.ToArray() );
	}

	private sealed record CommandResult(
		int Status,
		string Stdout,
		string Stderr
	);
}
