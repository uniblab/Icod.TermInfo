using System.Text;
using Icod.CommandFramework.Diagnostics;
using Xunit;

namespace Icod.TermInfo.Toe.Tests;

public sealed class T10CliHardeningTests {
	[Fact]
	public async Task ClusteredListingOptionsAndEndMarkerAreAccepted() {
		string missing = $"-missing-t10-{Guid.NewGuid():N}";
		CommandResult result = await RunAsync(
			"-ahs",
			"--",
			missing
		);

		Assert.Equal( CommandExitCodes.Failure, result.Status );
		Assert.DoesNotContain(
			"unsupported option",
			result.Stderr,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public async Task RepeatedListingFlagsAreIdempotent() {
		string missing = $"-missing-t10-{Guid.NewGuid():N}";
		CommandResult result = await RunAsync(
			"-aa",
			"--",
			missing
		);

		Assert.Equal( CommandExitCodes.Failure, result.Status );
		Assert.DoesNotContain(
			"unsupported option",
			result.Stderr,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public async Task AttachedSourceValueIsAccepted() {
		string root = CreateTemporaryDirectory();
		try {
			string sourcePath = System.IO.Path.Combine(
				root,
				"dependencies.ti"
			);
			await File.WriteAllTextAsync(
				sourcePath,
				"parent|Parent terminal,\n"
					+ "    am,\n"
					+ "child|Child terminal,\n"
					+ "    use=parent,\n",
				new UTF8Encoding( false )
			);

			CommandResult result = await RunAsync( "-u" + sourcePath );

			Assert.Equal( CommandExitCodes.Success, result.Status );
			Assert.Contains(
				$"child\tparent{Environment.NewLine}",
				result.Stdout,
				StringComparison.Ordinal
			);
		} finally {
			DeleteTemporaryDirectory( root );
		}
	}

	[Fact]
	public async Task SourceEndMarkerAllowsHyphenLeadingSourcePath() {
		string sourcePath = $"-missing-t10-{Guid.NewGuid():N}.ti";
		CommandResult result = await RunAsync(
			"-u",
			"--",
			sourcePath
		);

		Assert.Equal( CommandExitCodes.Failure, result.Status );
		Assert.DoesNotContain(
			"usage",
			result.Stderr,
			StringComparison.OrdinalIgnoreCase
		);
		Assert.Contains(
			": TOE0006 error: ",
			result.Stderr,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public async Task ListingOptionsCannotBeMixedWithSourceMode() {
		CommandResult result = await RunAsync(
			"-a",
			"-u",
			"source.ti"
		);

		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.Contains(
			"standalone mode",
			result.Stderr,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public async Task UnsupportedNcursesSwitchIsNeverIgnored() {
		CommandResult result = await RunAsync( "-C" );

		Assert.Equal( CommandExitCodes.UsageError, result.Status );
		Assert.StartsWith(
			"toe: ",
			result.Stderr,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public async Task OperationalDiagnosticsUseSuiteFormat() {
		string missing = System.IO.Path.Combine(
			System.IO.Path.GetTempPath(),
			"Icod.TermInfo.Toe.Tests",
			Guid.NewGuid().ToString( "N" )
		);
		CommandResult result = await RunAsync( missing );

		Assert.Equal( CommandExitCodes.Failure, result.Status );
		Assert.Contains(
			": TOE0002 error: requested database root does not exist",
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

	private static string CreateTemporaryDirectory() {
		string path = System.IO.Path.Combine(
			System.IO.Path.GetTempPath(),
			"Icod.TermInfo.Toe.Tests",
			Guid.NewGuid().ToString( "N" )
		);
		Directory.CreateDirectory( path );
		return path;
	}

	private static void DeleteTemporaryDirectory( string path ) {
		if ( Directory.Exists( path ) ) {
			Directory.Delete(
				path,
				recursive: true
			);
		}
	}

	private sealed record CommandResult(
		int Status,
		string Stdout,
		string Stderr
	);
}
