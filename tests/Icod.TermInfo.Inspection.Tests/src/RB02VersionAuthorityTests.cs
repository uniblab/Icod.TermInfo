using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RB02VersionAuthorityTests {
	[Fact]
	public void Rb02RecordPreservesAcceptedAlphaTwoIdentity() {
		string record = File.ReadAllText(
			Path.Combine(
				FindRepositoryRoot(),
				"docs",
				"1.14.0-RB02-BACKEND-EVIDENCE-CLASSIFICATION-AND-STATIC-INSPECTION.md"
			)
		);

		Assert.Contains( "1.14.0-Alpha-2", record, StringComparison.Ordinal );
		Assert.Contains(
			"6321de472a6e54a8382dd214996c22a4cf62e816",
			record,
			StringComparison.Ordinal
		);
		Assert.Contains( "34865731077", record, StringComparison.Ordinal );
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
