using System.Security.Cryptography;
using System.Text;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RP08ReleaseClosureTests {
	private const string DevelopmentVersion = "1.10.0-Alpha-7";
	private const string HistoricalDevelopmentVersion = "1.8.0-Alpha-8";
	private const string Rp07Head =
		"a88237d0d2f0ecdf74a7d96f6ff1cb9a2e8e647d";
	private const string HistoricalOneSevenBaselineSha256 =
		"ba87cb17abe4d2c2a89851b3f9205f95bfd1116022e8b46d2883941c378f5811";

	[Fact]
	public void FrozenOneEightPlanningTypesRemainAvailableWithinAdditiveSurface() {
		Type[] exportedTypes =
			typeof( TerminalDescriptionSourcePlanner )
				.Assembly
				.GetExportedTypes();

		Assert.True( exportedTypes.Length >= 29 );
		Assert.Contains(
			typeof( TerminalDescriptionSourcePlan ),
			exportedTypes );
		Assert.Contains(
			typeof( TerminalDescriptionSourcePlanner ),
			exportedTypes );
		Assert.Contains(
			typeof( TerminalDescriptionSourcePlanningOptions ),
			exportedTypes );
		Assert.Contains(
			typeof( TerminalDescriptionSourcePlanningScore ),
			exportedTypes );

		string baseline =
			File.ReadAllText(
				Path.Combine(
					FindRepositoryRoot(),
					"docs",
					"1.8.0-INSPECTION-PUBLIC-API-BASELINE.txt" ) );
		Assert.Equal(
			29,
			baseline
				.Split( '\n' )
				.Count(
					line => line.StartsWith(
						"TYPE ",
						StringComparison.Ordinal ) ) );
		Assert.Contains(
			"TerminalDescriptionSourcePlanner [static]",
			baseline,
			StringComparison.Ordinal );
		Assert.Contains(
			"DefaultMaximumEvaluatedPlanCount",
			baseline,
			StringComparison.Ordinal );
		Assert.Contains(
			"PlanFromDirectory",
			baseline,
			StringComparison.Ordinal );
		Assert.Contains(
			"# AssemblyVersion: 1.0.0.0",
			baseline,
			StringComparison.Ordinal );
	}

	[Fact]
	public void OneSevenInspectionBaselineRemainsImmutableHistoricalEvidence() {
		string baseline =
			File.ReadAllText(
				Path.Combine(
					FindRepositoryRoot(),
					"docs",
					"1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt" ) )
				.Replace( "\r\n", "\n", StringComparison.Ordinal )
				.Replace( '\r', '\n' );
		string sha256 =
			Convert.ToHexString(
				SHA256.HashData(
					Encoding.UTF8.GetBytes( baseline ) )
			).ToLowerInvariant();

		Assert.Equal(
			HistoricalOneSevenBaselineSha256,
			sha256 );
	}

	[Fact]
	public void ReleaseVerifiersRetainHistoricalOneEightEvidenceDuringDevelopment() {
		string root = FindRepositoryRoot();
		foreach (
			string relativePath
			in new[] {
				Path.Combine(
					".github",
					"scripts",
					"verify-release-package.sh" ),
				Path.Combine(
					".github",
					"scripts",
					"verify-release-package.cmd" ),
			}
		) {
			string verifier =
				File.ReadAllText(
					Path.Combine(
						root,
						relativePath ) );

			Assert.Contains(
				"1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt",
				verifier,
				StringComparison.OrdinalIgnoreCase );
			Assert.Contains(
				"1.8.0-INSPECTION-PUBLIC-API-BASELINE.txt",
				verifier,
				StringComparison.OrdinalIgnoreCase );
			Assert.Contains(
				"MI07",
				verifier,
				StringComparison.Ordinal );
			Assert.Contains(
				"--compare",
				verifier,
				StringComparison.Ordinal );
			Assert.Contains(
				"Icod.TermInfo.Inspection",
				verifier,
				StringComparison.Ordinal );
		}
	}

	[Fact]
	public void ImplementationRecordFreezesScoreBoundsCompletenessAndPublicationAuthority() {
		string record =
			File.ReadAllText(
				Path.Combine(
					FindRepositoryRoot(),
					"docs",
					"1.8.0-RP08-API-PACKAGING-AND-RELEASE-CLOSURE.md" ) );

		Assert.Contains(
			HistoricalDevelopmentVersion,
			record,
			StringComparison.Ordinal );
		Assert.Contains( Rp07Head, record, StringComparison.Ordinal );
		Assert.Contains( "LocalDirectiveCount", record, StringComparison.Ordinal );
		Assert.Contains( "SelectedCandidateIndices", record, StringComparison.Ordinal );
		Assert.Contains( "4,097", record, StringComparison.Ordinal );
		Assert.Contains( "1,000,000", record, StringComparison.Ordinal );
		Assert.Contains( "IsExhaustive == false", record, StringComparison.Ordinal );
		Assert.Contains( "version-only", record, StringComparison.Ordinal );
		Assert.Contains( "repository owner", record, StringComparison.OrdinalIgnoreCase );
	}

	[Fact]
	public void ReleaseAuditClosesApiPackagesCommandsArchivesAndSamples() {
		string audit =
			File.ReadAllText(
				Path.Combine(
					FindRepositoryRoot(),
					"docs",
					"1.8.0-RELEASE-AUDIT.md" ) );

		foreach (
			string marker
			in new[] {
				"1.8.0-INSPECTION-PUBLIC-API-BASELINE.txt",
				"AssemblyVersion 1.0.0.0",
				"infocmp --plan-use",
				"Icod.TermInfo.Tools",
				"win-x64",
				"win-arm64",
				"linux-x64",
				"linux-arm64",
				"osx-x64",
				"osx-arm64",
				"Toolchain",
				"v1.8.0",
				"pending owner publication",
				"fc75edf470eefc1b3f367d268dd0618f5f03e38e",
				"33603732871",
			}
		) {
			Assert.Contains(
				marker,
				audit,
				StringComparison.OrdinalIgnoreCase );
		}
	}

	[Fact]
	public void CoordinatedMetadataPreservesStableClosureAndIdentifiesMi01() {
		string root = FindRepositoryRoot();
		string buildProperties =
			File.ReadAllText(
				Path.Combine(
					root,
					"Directory.Build.props" ) );
		string roadmap =
			File.ReadAllText(
				Path.Combine(
					root,
					"Icod.TermInfo-1.8.0-Relative-Source-Planning-and-Parent-Selection-Roadmap.md" ) );
		string activeRoadmap =
			File.ReadAllText(
				Path.Combine(
					root,
					"Icod.TermInfo-Post-1.0-Development-Roadmap.md" ) );

		Assert.Contains(
			DevelopmentVersion,
			buildProperties,
			StringComparison.Ordinal );
		Assert.Contains(
			"Stable 1.8.0 release contract frozen",
			roadmap,
			StringComparison.Ordinal );
		Assert.Contains(
			"DA06",
			activeRoadmap,
			StringComparison.Ordinal );
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? directory =
			new( AppContext.BaseDirectory );

		while ( directory is not null ) {
			if (
				File.Exists(
					Path.Combine(
						directory.FullName,
						"Icod.TermInfo.sln" ) )
			) {
				return directory.FullName;
			}

			directory = directory.Parent;
		}

		throw new InvalidOperationException(
			"Repository root not found." );
	}
}
