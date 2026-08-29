using System.Text;
using Icod.CommandFramework.Diagnostics;
using Xunit;

namespace Icod.TermInfo.InfoCmp.Tests;

public sealed class T10CliHardeningTests {
	[Fact]
	public async Task ClusteredAndRepeatedComparisonOptionsAreAccepted() {
		CommandResult result = await RunAsync(
			"-ddqx",
			$"missing-a-{Guid.NewGuid():N}",
			$"missing-b-{Guid.NewGuid():N}"
		);

		Assert.NotEqual( CommandExitCodes.UsageError, result.Status );
		Assert.DoesNotContain(
			"unsupported option",
			result.Stderr,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public async Task AttachedWidthValueIsAccepted() {
		CommandResult result = await RunAsync(
			"-w120",
			$"missing-{Guid.NewGuid():N}"
		);

		Assert.NotEqual( CommandExitCodes.UsageError, result.Status );
	}

	[Fact]
	public async Task EndOfOptionsAllowsHyphenLeadingTerminalName() {
		CommandResult result = await RunAsync(
			"--",
			$"-missing-{Guid.NewGuid():N}"
		);

		Assert.NotEqual( CommandExitCodes.UsageError, result.Status );
		Assert.DoesNotContain(
			"unsupported option",
			result.Stderr,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public async Task ConflictingComparisonModesRemainUsageError() {
		CommandResult result = await RunAsync(
			"-dc",
			"first",
			"second"
		);

		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Contains(
			"mutually exclusive",
			result.Stderr,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public async Task UnsupportedNcursesSwitchIsNeverIgnored() {
		CommandResult result = await RunAsync( "-C" );

		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.StartsWith(
			"infocmp: ",
			result.Stderr,
			StringComparison.Ordinal
		);
	}

	private static async Task<CommandResult> RunAsync(
		params string[] args
	) {
		using var stdin = new MemoryStream();
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
