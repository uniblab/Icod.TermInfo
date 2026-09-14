using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RB06VersionAuthorityTests {
	[Fact]
	public void Rb06RecordPreservesAcceptedAlphaSixIdentity() {
		string record = File.ReadAllText(
			Path.Combine(
				FindRepositoryRoot(),
				"docs",
				"1.14.0-RB06-JSON-V6-BACKEND-AUTOMATION.md"
			)
		);

		Assert.Contains( "1.14.0-Alpha-6", record, StringComparison.Ordinal );
		Assert.Contains(
			"d20d1c7b9a727b20dbcb931f1ac980a0e961b372",
			record,
			StringComparison.Ordinal
		);
		Assert.Contains( "34893643336", record, StringComparison.Ordinal );
	}

	[Fact]
	public void Rb06RecordPreservesRedImplementationAndBehavioralGreenIdentities() {
		string record = File.ReadAllText(
			Path.Combine(
				FindRepositoryRoot(),
				"docs",
				"1.14.0-RB06-JSON-V6-BACKEND-AUTOMATION.md"
			)
		);

		Assert.Contains(
			"b270388c1754d9ad446bc36d375783e7b74bf9d6",
			record,
			StringComparison.Ordinal
		);
		Assert.Contains( "34891761005", record, StringComparison.Ordinal );
		Assert.Contains(
			"0a2966b863f6ea4545e27f9d13fde3a7c05e2b94",
			record,
			StringComparison.Ordinal
		);
		Assert.Contains( "34892281561", record, StringComparison.Ordinal );
		Assert.Contains(
			"2c76dab854f30542122d455f2a63e0c768aec8b3",
			record,
			StringComparison.Ordinal
		);
		Assert.Contains( "34892727360", record, StringComparison.Ordinal );
	}

	[Fact]
	public void Rb06AdditiveMemberLedgerContainsExactlySixRendererMembers() {
		string[] lines = File.ReadAllLines(
			Path.Combine(
				FindRepositoryRoot(),
				"docs",
				"1.14.0-RB06-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt"
			)
		)
			.Select( line => line.TrimEnd() )
			.Where( line => line.Length != 0 && !line.StartsWith( '#' ) )
			.ToArray();

		Assert.Equal( 6, lines.Length );
		Assert.Equal(
			new[] {
				"  FIELD public static const System.Int32 RasterBackendSchemaVersion null=not-null/not-null value=6",
				"  FIELD public static const System.String RasterBackendSchemaIdentifier null=not-null/not-null value=\"urn:icod:terminfo:inspection:json:6\"",
				"  METHOD public static System.String Render(Icod.TermInfo.Inspection.RasterBackendProfile profile null=not-null/not-null) return-null=not-null/not-null",
				"  METHOD public static System.String Render(Icod.TermInfo.Inspection.RasterBackendProfile profile null=not-null/not-null, Icod.TermInfo.Inspection.TermInfoJsonRendererOptions options null=not-null/not-null, System.Threading.CancellationToken cancellationToken null=not-null/not-null default=null) return-null=not-null/not-null",
				"  METHOD public static System.String Render(Icod.TermInfo.Inspection.RasterBackendSelectionPlan plan null=not-null/not-null) return-null=not-null/not-null",
				"  METHOD public static System.String Render(Icod.TermInfo.Inspection.RasterBackendSelectionPlan plan null=not-null/not-null, Icod.TermInfo.Inspection.TermInfoJsonRendererOptions options null=not-null/not-null, System.Threading.CancellationToken cancellationToken null=not-null/not-null default=null) return-null=not-null/not-null",
			},
			lines
		);
	}

	[Fact]
	public void Rb06CompatibilityVerifierConsumesExactMemberLedger() {
		string verifier = File.ReadAllText(
			Path.Combine(
				FindRepositoryRoot(),
				".github",
				"scripts",
				"verify-inspection-compatibility.ps1"
			)
		);

		Assert.Contains(
			"1.14.0-RB06-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt",
			verifier,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Remove-ApprovedRendererMembers",
			verifier,
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
