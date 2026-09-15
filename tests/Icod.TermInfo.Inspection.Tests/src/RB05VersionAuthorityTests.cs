using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RB05VersionAuthorityTests {
	[Fact]
	public void Rb05RecordPreservesAcceptedAlphaFiveIdentity() {
		string record = File.ReadAllText(
			Path.Combine(
				FindRepositoryRoot(),
				"docs",
				"1.14.0-RB05-RUNTIME-INTEGRATION-COMPOSITION.md"
			)
		);

		Assert.Contains( "1.14.0-Alpha-5", record, StringComparison.Ordinal );
		Assert.Contains(
			"a4aab5e7af4d554cad7c978cef20491744100c80",
			record,
			StringComparison.Ordinal
		);
		Assert.Contains( "34880872852", record, StringComparison.Ordinal );
	}

	[Fact]
	public void Rb05RecordPreservesRedAndBehavioralGreenIdentities() {
		string record = File.ReadAllText(
			Path.Combine(
				FindRepositoryRoot(),
				"docs",
				"1.14.0-RB05-RUNTIME-INTEGRATION-COMPOSITION.md"
			)
		);

		Assert.Contains(
			"f13fa4e487567a94b1edf2849b5b21a10e81b84e",
			record,
			StringComparison.Ordinal
		);
		Assert.Contains( "34878847456", record, StringComparison.Ordinal );
		Assert.Contains(
			"28a0f7037434057cb79387db056639f8b913b6b8",
			record,
			StringComparison.Ordinal
		);
		Assert.Contains( "34879032335", record, StringComparison.Ordinal );
	}

	[Fact]
	public void Rb05AdditiveMemberLedgerContainsExactlyIntegrationConstructor() {
		string[] lines = File.ReadAllLines(
			Path.Combine(
				FindRepositoryRoot(),
				"docs",
				"1.14.0-RB05-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt"
			)
		)
			.Select( line => line.TrimEnd() )
			.Where( line => line.Length != 0 && !line.StartsWith( '#' ) )
			.ToArray();

		string member = Assert.Single( lines );
		Assert.Equal(
			"  CTOR public RasterBackendCandidate(Icod.TermInfo.Inspection.RasterBackendProfile backendProfile null=not-null/not-null, Icod.TermInfo.Inspection.PersistentRasterRuntimeIntegrationResult integration null=not-null/not-null)",
			member
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
