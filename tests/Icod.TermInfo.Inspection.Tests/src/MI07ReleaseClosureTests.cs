using System.Security.Cryptography;
using System.Text;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class MI07ReleaseClosureTests {
	private const string StableReleaseVersion = "1.9.0";
	private const string HistoricalDevelopmentVersion = "1.9.0-Alpha-7";
	private const string Mi06Head =
		"1f6560dedc45495132fbf50ef333e3ec5ac2b384";
	private const string Mi06Run = "33685454683";
	private const string HistoricalOneSevenBaselineSha256 =
		"ba87cb17abe4d2c2a89851b3f9205f95bfd1116022e8b46d2883941c378f5811";
	private const string HistoricalOneEightBaselineSha256 =
		"12e31674f63ed9483a7261fc7f7214c390df9c6025a78dc8ae83aa3b01ea2bcc";
	private const string VersionOneSchemaSha256 =
		"76578f421b254802d24453af6868edaf8c23c4b78a87c7e8ef86b233ff0e8500";
	private const string OneNineBaselineSha256 =
		"759e0e256d04c0da53774a80ce178fa3048853bf1c1da778daee547d7883881e";

	[Fact]
	public void FrozenOneNineInspectionSurfaceContainsExactlyReviewedTypes() {
		Type[] exportedTypes =
			typeof( TermInfoJsonRenderer )
				.Assembly
				.GetExportedTypes();

		Assert.Equal( 31, exportedTypes.Length );
		Assert.Contains( typeof( TermInfoJsonRenderer ), exportedTypes );
		Assert.Contains( typeof( TermInfoJsonRendererOptions ), exportedTypes );

		string baseline =
			File.ReadAllText(
				Path.Combine(
					FindRepositoryRoot(),
					"docs",
					"1.9.0-INSPECTION-PUBLIC-API-BASELINE.txt" ) );
		Assert.Equal(
			31,
			baseline
				.Split( '\n' )
				.Count(
					line => line.StartsWith(
						"TYPE ",
						StringComparison.Ordinal ) ) );
		Assert.Contains(
			"TYPE class Icod.TermInfo.Inspection.TermInfoJsonRenderer [static]",
			baseline,
			StringComparison.Ordinal );
		Assert.Contains(
			"SchemaIdentifier",
			baseline,
			StringComparison.Ordinal );
		Assert.Contains(
			"MaximumSupportedOutputByteCount",
			baseline,
			StringComparison.Ordinal );
		Assert.Contains(
			"# AssemblyVersion: 1.0.0.0",
			baseline,
			StringComparison.Ordinal );
		Assert.Equal(
			OneNineBaselineSha256,
			ComputeSha256( baseline ) );
	}

	[Theory]
	[InlineData(
		"1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt",
		HistoricalOneSevenBaselineSha256 )]
	[InlineData(
		"1.8.0-INSPECTION-PUBLIC-API-BASELINE.txt",
		HistoricalOneEightBaselineSha256 )]
	[InlineData(
		"Icod.TermInfo.Inspection.schema.json",
		VersionOneSchemaSha256 )]
	public void FrozenHistoricalManifestsAndVersionOneSchemaRemainExact(
		string fileName,
		string expectedSha256
	) {
		string contents =
			File.ReadAllText(
				Path.Combine(
					FindRepositoryRoot(),
					"docs",
					fileName ) )
				.Replace( "\r\n", "\n", StringComparison.Ordinal )
				.Replace( '\r', '\n' );
		string sha256 = ComputeSha256( contents );

		Assert.Equal( expectedSha256, sha256 );
	}

	[Fact]
	public void ReleaseVerifiersRequireExactOneNineInspectionBaseline() {
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
				"1.9.0-INSPECTION-PUBLIC-API-BASELINE.txt",
				verifier,
				StringComparison.OrdinalIgnoreCase );
			Assert.Contains(
				"--check",
				verifier,
				StringComparison.Ordinal );
			Assert.Contains(
				"Icod.TermInfo.Inspection",
				verifier,
				StringComparison.Ordinal );
		}
	}

	[Fact]
	public void ClosureRecordsFreezeSchemaCommandsPackagesArchivesAndAuthority() {
		string root = FindRepositoryRoot();
		string record =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.9.0-MI07-API-SCHEMA-PACKAGING-AND-RELEASE-CLOSURE.md" ) );
		string audit =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.9.0-RELEASE-AUDIT.md" ) );

		foreach (
			string marker
			in new[] {
				HistoricalDevelopmentVersion,
				Mi06Head,
				Mi06Run,
				"31 exported types",
				"urn:icod:terminfo:inspection:json:1",
				VersionOneSchemaSha256,
				"infocmp --json",
				"toe --json",
				"Icod.TermInfo.Tools",
				"win-x64",
				"win-arm64",
				"linux-x64",
				"linux-arm64",
				"osx-x64",
				"osx-arm64",
				"version-only",
				"repository owner",
			}
		) {
			Assert.Contains(
				marker,
				record,
				StringComparison.OrdinalIgnoreCase );
		}

		foreach (
			string marker
			in new[] {
				"1.9.0-INSPECTION-PUBLIC-API-BASELINE.txt",
				"assembly version `1.0.0.0`",
				"sourcePlan",
				"databaseCatalog",
				"Toolchain",
				"v1.9.0",
				"pending owner publication",
			}
		) {
			Assert.Contains(
				marker,
				audit,
				StringComparison.OrdinalIgnoreCase );
		}
	}

	[Fact]
	public void CoordinatedMetadataIdentifiesStableReleaseAndCompletedTranche() {
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
					"Icod.TermInfo-1.9.0-Machine-Readable-Inspection-and-Planning-Automation-Roadmap.md" ) );
		string activeRoadmap =
			File.ReadAllText(
				Path.Combine(
					root,
					"Icod.TermInfo-Post-1.0-Development-Roadmap.md" ) );

		Assert.Contains(
			StableReleaseVersion,
			buildProperties,
			StringComparison.Ordinal );
		Assert.Contains(
			"Stable 1.9.0 release contract frozen",
			roadmap,
			StringComparison.Ordinal );
		Assert.Contains(
			"Release closure - exact-main validation and publication",
			activeRoadmap,
			StringComparison.OrdinalIgnoreCase );
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

	private static string ComputeSha256(
		string contents
	) =>
		Convert.ToHexString(
			SHA256.HashData(
				Encoding.UTF8.GetBytes(
					contents
						.Replace( "\r\n", "\n", StringComparison.Ordinal )
						.Replace( '\r', '\n' ) ) )
		).ToLowerInvariant();
}
