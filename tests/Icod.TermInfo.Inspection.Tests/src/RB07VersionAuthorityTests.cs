using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RB07VersionAuthorityTests {
	[Fact]
	public void Rb07RecordPreservesAcceptedAlphaSevenIdentity() {
		string record = File.ReadAllText(
			Path.Combine(
				FindRepositoryRoot(),
				"docs",
				"1.14.0-RB07-TERMINAL-INTEROPERABILITY-AND-PACKAGE-QUALIFICATION.md"
			)
		);

		Assert.Contains( "1.14.0-Alpha-7", record, StringComparison.Ordinal );
		Assert.Contains(
			"0ce8df7f08064de807db17a2662f38252a814ce5",
			record,
			StringComparison.Ordinal
		);
		Assert.Contains( "34898618294", record, StringComparison.Ordinal );
	}

	[Fact]
	public void Rb07RecordPreservesBaselineRedCorrectionAndBehavioralGreenIdentities() {
		string record = File.ReadAllText(
			Path.Combine(
				FindRepositoryRoot(),
				"docs",
				"1.14.0-RB07-TERMINAL-INTEROPERABILITY-AND-PACKAGE-QUALIFICATION.md"
			)
		);

		Assert.Contains(
			"d94e6ee86c98e27ad283a80243b8c9e9b64d5322",
			record,
			StringComparison.Ordinal
		);
		Assert.Contains( "34896089818", record, StringComparison.Ordinal );
		Assert.Contains(
			"8a96ff878ecdb182634b75a13e86150c5bbee18c",
			record,
			StringComparison.Ordinal
		);
		Assert.Contains( "34896946829", record, StringComparison.Ordinal );
		Assert.Contains(
			"85a198960d1651b697dff9866541e882dc242cf6",
			record,
			StringComparison.Ordinal
		);
		Assert.Contains( "34897424135", record, StringComparison.Ordinal );
		Assert.Contains(
			"fe2f4e57839c70ff158d8cc548b217607b109edc",
			record,
			StringComparison.Ordinal
		);
		Assert.Contains( "34897641986", record, StringComparison.Ordinal );
	}

	[Fact]
	public void Rb07AddsNoInspectionPublicApiLedger() {
		Assert.False(
			File.Exists(
				Path.Combine(
					FindRepositoryRoot(),
					"docs",
					"1.14.0-RB07-INSPECTION-PUBLIC-API-ADDITIVE-MEMBERS.txt"
				)
			)
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
