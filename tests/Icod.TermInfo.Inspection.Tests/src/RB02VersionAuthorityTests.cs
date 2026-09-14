using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RB02VersionAuthorityTests {
	[Fact]
	public void CoordinatedVersionIsAlphaTwo() {
		string properties = File.ReadAllText(
			Path.Combine(
				FindRepositoryRoot(),
				"Directory.Build.props"
			)
		);

		Assert.Contains(
			"<IcodTermInfoSuiteVersion>1.14.0-Alpha-2</IcodTermInfoSuiteVersion>",
			properties,
			StringComparison.Ordinal
		);
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( AppContext.BaseDirectory );
		while ( current is not null ) {
			if ( File.Exists( Path.Combine( current.FullName, "Icod.TermInfo.sln" ) ) ) {
				return current.FullName;
			}
			current = current.Parent;
		}
		throw new DirectoryNotFoundException(
			"Could not locate the repository root."
		);
	}
}
