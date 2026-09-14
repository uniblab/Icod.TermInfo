using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RB04VersionAuthorityTests {
	[Fact]
	public void CoordinatedVersionIsAlphaFour() {
		string properties = File.ReadAllText(
			Path.Combine(
				FindRepositoryRoot(),
				"Directory.Build.props"
			)
		);

		Assert.Contains(
			"<IcodTermInfoSuiteVersion>1.14.0-Alpha-4</IcodTermInfoSuiteVersion>",
			properties,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void Rb04RecordPreservesRedAndBehavioralGreenIdentities() {
		string record = File.ReadAllText(
			Path.Combine(
				FindRepositoryRoot(),
				"docs",
				"1.14.0-RB04-DETERMINISTIC-BACKEND-SELECTION.md"
			)
		);

		Assert.Contains(
			"4410808a1626dbdb6a21ca2be3bf78b344f10255",
			record,
			StringComparison.Ordinal
		);
		Assert.Contains( "34872792326", record, StringComparison.Ordinal );
		Assert.Contains(
			"e6e276b6d4b86e495733228571e13f77385e00f7",
			record,
			StringComparison.Ordinal
		);
		Assert.Contains( "34874959660", record, StringComparison.Ordinal );
	}

	[Fact]
	public void Rb04AdditiveMemberLedgerContainsExactlyPlan() {
		string[] lines = File.ReadAllLines(
			Path.Combine(
				FindRepositoryRoot(),
				"docs",
				"1.14.0-RB04-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt"
			)
		)
			.Select( line => line.TrimEnd() )
			.Where( line => line.Length != 0 && !line.StartsWith( '#') )
			.ToArray();

		string member = Assert.Single( lines );
		Assert.Equal(
			"  METHOD public static Icod.TermInfo.Inspection.RasterBackendSelectionPlan Plan(System.Collections.Generic.IEnumerable<Icod.TermInfo.Inspection.RasterBackendCandidate> candidates null=not-null/not-null<not-null/not-null>, Icod.TermInfo.Inspection.RasterBackendSelectionRequest request null=not-null/not-null, Icod.TermInfo.Inspection.RasterBackendSelectionOptions options null=nullable/nullable default=null) return-null=not-null/not-null",
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
