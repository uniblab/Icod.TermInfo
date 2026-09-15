using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RB03VersionAuthorityTests {
	[Fact]
	public void Rb03RecordPreservesAcceptedAlphaThreeIdentity() {
		string record = File.ReadAllText(
			Path.Combine(
				FindRepositoryRoot(),
				"docs",
				"1.14.0-RB03-CANDIDATE-EVALUATION.md"
			)
		);

		Assert.Contains( "1.14.0-Alpha-3", record, StringComparison.Ordinal );
		Assert.Contains(
			"22f38e9c6516bec9d5f6b8662f00e9b5bace7c2b",
			record,
			StringComparison.Ordinal
		);
		Assert.Contains( "34871582203", record, StringComparison.Ordinal );
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
