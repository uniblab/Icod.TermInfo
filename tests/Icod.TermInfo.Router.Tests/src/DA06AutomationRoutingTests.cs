using System.Text;
using Xunit;

namespace Icod.TermInfo.Router.Tests;

public sealed class DA06AutomationRoutingTests {
	[Theory]
	[InlineData( "toe", "--compare-set" )]
	[InlineData( "infocmp", "--candidate-root" )]
	public async Task RouterForwardsDa06HelpSurface(
		string commandName,
		string expectedOption
	) {
		using var stdin = new MemoryStream();
		using var stdout = new MemoryStream();
		using var stderr = new MemoryStream();
		int status = await Command.RunAsync(
			[ commandName, "--help" ],
			stdin,
			stdout,
			stderr
		);
		Assert.Equal( 0, status );
		Assert.Contains(
			expectedOption,
			Encoding.UTF8.GetString( stdout.ToArray() ),
			StringComparison.Ordinal
		);
		Assert.Empty( stderr.ToArray() );
	}
}
