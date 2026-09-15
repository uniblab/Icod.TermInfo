using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RB01VersionAuthorityTests {
	[Fact]
	public void Rb01RecordPreservesAcceptedAlphaOneIdentity() {
		string record = File.ReadAllText(
			Path.Combine(
				FindRepositoryRoot(),
				"docs",
				"1.14.0-RB01-CONTRACT-AND-PUBLIC-API-REGRET-GATE.md"
			)
		);

		Assert.Contains( "1.14.0-Alpha-1", record, StringComparison.Ordinal );
		Assert.Contains(
			"7f43c4ad27f1858f8648237cbcb26e6808142458",
			record,
			StringComparison.Ordinal
		);
		Assert.Contains( "34862203053", record, StringComparison.Ordinal );
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
