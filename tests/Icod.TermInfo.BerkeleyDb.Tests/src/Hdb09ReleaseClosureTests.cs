/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates the HDB09 release-freeze and documentation contract.
	Copyright (C) 2026  Timothy J. Bruce <uniblab@hotmail.com>
*/

/*
	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hdb09ReleaseClosureTests {
	private const string ApiBaselinePath =
		"docs/1.16.0-BERKELEYDB-PUBLIC-API-BASELINE.txt";

	[Fact]
	public void ExactBerkeleyDbPublicApiHasACompleteCheckedInFreeze() {
		string baseline = ReadRequiredRepositoryFile( ApiBaselinePath );
		string freeze = ReadRequiredRepositoryFile(
			"docs/1.16.0-HW01-BERKELEY-DB-WRITER-CONTRACT.md"
		);
		string verifier = ReadRequiredRepositoryFile(
			".github/scripts/verify-berkeleydb-package.ps1"
		);

		Assert.StartsWith(
			"# Icod.TermInfo.BerkeleyDb public API baseline\n",
			NormalizeLf( baseline ),
			StringComparison.Ordinal
		);
		Assert.Contains(
			"# Format: Icod.TermInfo.PublicApiSnapshot/v1",
			baseline,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"# AssemblyVersion: 1.0.0.0",
			baseline,
			StringComparison.Ordinal
		);
		int typeCount = NormalizeLf( baseline ).Split( '\n' ).Count(
			line => line.StartsWith( "TYPE ", StringComparison.Ordinal )
		);
		Assert.Equal( 12, typeCount );
		string sha256 = NormalizedLfSha256( baseline );
		Assert.Contains( sha256, freeze, StringComparison.Ordinal );
		Assert.Contains(
			$"{typeCount} exported public types",
			freeze,
			StringComparison.Ordinal
		);
		foreach ( string token in new[] {
			"1.16.0-Alpha-1",
			"BerkeleyDbTerminalDatabaseEntry",
			"BerkeleyDbTerminalDatabaseWriterOptions",
			"BerkeleyDbTerminalDatabaseWriter",
			"HW02",
		} ) {
			Assert.Contains( token, freeze, StringComparison.Ordinal );
		}

		Assert.Contains( "--check", verifier, StringComparison.Ordinal );
		Assert.Contains( ApiBaselinePath, verifier, StringComparison.Ordinal );
		Assert.Contains(
			"Icod.TermInfo.BerkeleyDb/bin/$Configuration/net10.0/Icod.TermInfo.BerkeleyDb.dll",
			verifier,
			StringComparison.Ordinal
		);
		Assert.Equal( 2, CountOccurrences( verifier, "--compare" ) );
	}

	[Fact]
	public void ProductionDependencyDirectionRemainsFrozen() {
		string root = FindRepositoryRoot();
		XDocument berkeleyDbProject = XDocument.Load(
			Path.Combine(
				root,
				"Icod.TermInfo.BerkeleyDb",
				"Icod.TermInfo.BerkeleyDb.csproj"
			)
		);
		Assert.DoesNotContain(
			berkeleyDbProject.Descendants(),
			element => element.Name.LocalName == "PackageReference"
		);
		XElement runtimeReference = Assert.Single(
			berkeleyDbProject.Descendants(),
			element => element.Name.LocalName == "ProjectReference"
		);
		Assert.Equal(
			"..\\Icod.TermInfo.csproj",
			runtimeReference.Attribute( "Include" )?.Value
		);

		foreach ( string projectPath in new[] {
			"Icod.TermInfo.csproj",
			"Icod.TermInfo.Source/Icod.TermInfo.Source.csproj",
			"Icod.TermInfo.Termcap/Icod.TermInfo.Termcap.csproj",
			"Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj",
			"Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj",
		} ) {
			Assert.DoesNotContain(
				"Icod.TermInfo.BerkeleyDb",
				ReadRequiredRepositoryFile( projectPath ),
				StringComparison.Ordinal
			);
		}

		foreach ( string commandProject in new[] {
			"infocmp/Icod.TermInfo.InfoCmp.csproj",
			"toe/Icod.TermInfo.Toe.csproj",
		} ) {
			Assert.Contains(
				"Icod.TermInfo.BerkeleyDb.csproj",
				ReadRequiredRepositoryFile( commandProject ),
				StringComparison.Ordinal
			);
		}
	}

	[Fact]
	public void RuntimeFrozenApiReconstructionRemainsInTheReleaseGate() {
		foreach ( string verifierPath in new[] {
			".github/scripts/verify-release-package.sh",
			".github/scripts/verify-release-package.cmd",
		} ) {
			string verifier = ReadRequiredRepositoryFile( verifierPath );
			Assert.Contains(
				"-- --check",
				verifier,
				StringComparison.Ordinal
			);
			Assert.Contains(
				"public-api-snapshot",
				verifier,
				StringComparison.OrdinalIgnoreCase
			);
		}

		string baseline = ReadRequiredRepositoryFile(
			"docs/1.0.0-PUBLIC-API-BASELINE.txt"
		);
		Assert.StartsWith(
			"# Icod.TermInfo public API baseline",
			baseline,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void DeterministicSampleUsesOnlyThePublicBerkeleyDbBoundary() {
		string project = ReadRequiredRepositoryFile(
			"samples/Icod.TermInfo.BerkeleyDb.Sample/Icod.TermInfo.BerkeleyDb.Sample.csproj"
		);
		string program = ReadRequiredRepositoryFile(
			"samples/Icod.TermInfo.BerkeleyDb.Sample/Program.cs"
		);
		string readme = ReadRequiredRepositoryFile(
			"samples/Icod.TermInfo.BerkeleyDb.Sample/README.md"
		);
		XDocument projectDocument = XDocument.Parse( project );

		Assert.Equal(
			"net8.0;net9.0;net10.0",
			Assert.Single(
				projectDocument.Descendants(),
				element => element.Name.LocalName == "TargetFrameworks"
			).Value
		);
		Assert.DoesNotContain(
			projectDocument.Descendants(),
			element => element.Name.LocalName == "PackageReference"
		);
		Assert.EndsWith(
			"Icod.TermInfo.BerkeleyDb.csproj",
			Assert.Single(
				projectDocument.Descendants(),
				element => element.Name.LocalName == "ProjectReference"
			).Attribute( "Include" )?.Value,
			StringComparison.Ordinal
		);

		foreach ( string token in new[] {
			"BerkeleyDbTerminalDescriptionProvider",
			"TryLoad",
			"Path.GetTempPath",
			"File.WriteAllBytes",
			"hdb09-sample",
			"NumericCapability.Colors",
		} ) {
			Assert.Contains( token, program, StringComparison.Ordinal );
		}
		Assert.Contains( "controlled", readme, StringComparison.OrdinalIgnoreCase );
		Assert.Contains( "read-only", readme, StringComparison.OrdinalIgnoreCase );
		Assert.Contains( "no native", readme, StringComparison.OrdinalIgnoreCase );

		string solution = ReadRequiredRepositoryFile( "Icod.TermInfo.sln" );
		Assert.Contains(
			"samples\\Icod.TermInfo.BerkeleyDb.Sample\\Icod.TermInfo.BerkeleyDb.Sample.csproj",
			solution,
			StringComparison.Ordinal
		);
		string verifier = ReadRequiredRepositoryFile(
			".github/scripts/verify-berkeleydb-package.ps1"
		);
		Assert.Contains(
			"samples/Icod.TermInfo.BerkeleyDb.Sample/Icod.TermInfo.BerkeleyDb.Sample.csproj",
			verifier,
			StringComparison.Ordinal
		);
		foreach ( string framework in new[] { "net8.0", "net9.0", "net10.0" } ) {
			Assert.Contains( framework, verifier, StringComparison.Ordinal );
		}
	}

	[Fact]
	public void ReleaseGuidesAndAuditsDescribeTheAcceptedBoundaries() {
		AssertContainsAll(
			"docs/1.15.0-BERKELEY-DB-HASHED-ACQUISITION-GUIDE.md",
			"BerkeleyDbTerminalDescriptionProvider",
			"BerkeleyDbTerminalCatalogReader",
			"BerkeleyDbSystemTerminalDescriptionProvider",
			"infocmp",
			"toe",
			"clean miss"
		);
		AssertContainsAll(
			"docs/1.15.0-BERKELEY-DB-HASH-V9-COMPATIBILITY.md",
			"Hash-v9",
			"P_HASHMETA",
			"P_HASH",
			"H_KEYDATA",
			"H_OFFPAGE",
			"marker 0",
			"marker 2",
			"big-endian",
			"Latin-1"
		);
		AssertContainsAll(
			"docs/1.15.0-BERKELEY-DB-SECURITY-AND-RESOURCE-AUDIT.md",
			"MaximumDatabaseSize",
			"MaximumRecordCount",
			"MaximumIndexHops",
			"cancellation",
			"permission",
			"mutation",
			"not an atomic snapshot"
		);
		AssertContainsAll(
			"docs/1.15.0-BERKELEY-DB-ECOSYSTEM-AUDIT.md",
			"Final HDB09 conclusion",
			"no third-party",
			"no native",
			"net8.0",
			"net9.0",
			"net10.0"
		);
		AssertContainsAll(
			"docs/1.15.0-RELEASE-AUDIT.md",
			"HDB09",
			"1.15.0-Alpha-8",
			"public API",
			"dependency",
			"Hash-v9",
			"six reusable-library symbol packages"
		);
		AssertContainsAll(
			"CHANGELOG.md",
			"1.15.0",
			"Icod.TermInfo.BerkeleyDb",
			"Hash-v9",
			"release audit"
		);
	}

	[Fact]
	public void AlphaEightClosureAuthoritiesAreReadyForStablePromotion() {
		AssertContainsAll(
			"README.md",
			"Icod.TermInfo.BerkeleyDb",
			"1.15.0-Alpha-8",
			"Feature Inventory",
			"BERKELEY-DB-HASHED-ACQUISITION-GUIDE"
		);
		AssertContainsAll(
			"samples/README.md",
			"Icod.TermInfo.BerkeleyDb.Sample",
			"1.15",
			"controlled"
		);
		AssertContainsAll(
			"docs/VERSIONING.md",
			"1.15 release line",
			"1.15.0-Alpha-8",
			"1.15.0"
		);
		AssertContainsAll(
			"docs/COMPATIBILITY.md",
			"1.15 compatibility freeze",
			"Hash-v9",
			"pure managed",
			"read only"
		);
		AssertContainsAll(
			"Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md",
			"HDB09",
			"Alpha-8",
			"stable promotion"
		);
	}

	[Fact]
	public void StablePromotionAuthoritiesPreserveTheCoordinated1150ReleaseHistory() {
		AssertContainsAll(
			"README.md",
			"Current release line: `Icod.TermInfo 1.15.0`.",
			"Icod.TermInfo.BerkeleyDb --version 1.15.0"
		);
		Assert.DoesNotContain(
			"HDB09 Alpha-8 closure qualification is in progress",
			ReadRequiredRepositoryFile( "README.md" ),
			StringComparison.Ordinal
		);
		AssertContainsAll(
			"Icod.TermInfo.BerkeleyDb/README.md",
			"`1.15.0` is the stable coordinated release",
			"stable promotion changed no acquisition behavior or public API"
		);

		foreach ( string projectPath in new[] {
			"Icod.TermInfo.csproj",
			"Icod.TermInfo.Source/Icod.TermInfo.Source.csproj",
			"Icod.TermInfo.Termcap/Icod.TermInfo.Termcap.csproj",
			"Icod.TermInfo.BerkeleyDb/Icod.TermInfo.BerkeleyDb.csproj",
			"Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj",
			"Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj",
			"icod-terminfo/Icod.TermInfo.Router.csproj",
		} ) {
			XDocument project = XDocument.Parse(
				ReadRequiredRepositoryFile( projectPath )
			);
			string releaseNotes = Assert.Single(
				project.Descendants(),
				element => element.Name.LocalName == "PackageReleaseNotes"
			).Value;
			Assert.StartsWith(
				"1.15.0 ",
				releaseNotes,
				StringComparison.Ordinal
			);
		}

		AssertContainsAll(
			"docs/VERSIONING.md",
			"Stable `1.15.0` is the current coordinated release",
			"promotion changed release identity and release-facing text only",
			"six reusable libraries",
			"Icod.TermInfo.BerkeleyDb",
			"1.15.0-BERKELEYDB-PUBLIC-API-BASELINE.txt"
		);
		AssertContainsAll(
			"docs/COMPATIBILITY.md",
			"Stable 1.15 promotion preserves",
			"exact nine-type public API freeze",
			"Runtime, Source, Compiler, Inspection, Termcap, and BerkeleyDb"
		);
		AssertContainsAll(
			"docs/superpowers/specs/2026-09-17-hdb09-api-freeze-documentation-stable-promotion-design.md",
			"seven nupkg/six reusable-library snupkg family"
		);
		AssertContainsAll(
			"Icod.TermInfo-Post-1.0-Development-Roadmap.md",
			"**Current coordinated version:** `1.15.0`",
			"**Latest completed line:** `1.15.0`",
			"**Latest completed release audit:** `docs/1.15.0-RELEASE-AUDIT.md`"
		);
		AssertContainsAll(
			"Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md",
			"**Status:** COMPLETE / ACCEPTED (`1.15.0`)",
			"**Current coordinated release:** `1.15.0`"
		);
		AssertContainsAll(
			"docs/1.15.0-RELEASE-AUDIT.md",
			"Stable `1.15.0` is the coordinated version",
			"promotion changed no production behavior or public API",
			"10851c380b2df7eba35ea2151ecb7b982a8a7a2d",
			"35182018598",
			"35182018597",
			"passed 12/12 jobs",
			"passed 3/3 jobs"
		);
		AssertContainsAll(
			"CHANGELOG.md",
			"## 1.15.0",
			"stable coordinated release"
		);
	}

	private static void AssertContainsAll(
		string relativePath,
		params string[] tokens
	) {
		string contents = ReadRequiredRepositoryFile( relativePath );
		foreach ( string token in tokens ) {
			Assert.Contains(
				token,
				contents,
				StringComparison.OrdinalIgnoreCase
			);
		}
	}

	private static int CountOccurrences( string text, string value ) {
		int count = 0;
		int offset = 0;
		while ( true ) {
			int next = text.IndexOf( value, offset, StringComparison.Ordinal );
			if ( next < 0 ) {
				return count;
			}
			count++;
			offset = next + value.Length;
		}
	}

	private static string NormalizedLfSha256( string contents ) {
		return Convert.ToHexString(
			SHA256.HashData( Encoding.UTF8.GetBytes( NormalizeLf( contents ) ) )
		).ToLowerInvariant();
	}

	private static string NormalizeLf( string contents ) {
		return contents
			.Replace( "\r\n", "\n", StringComparison.Ordinal )
			.Replace( "\r", "\n", StringComparison.Ordinal )
			.TrimEnd( '\n' ) + "\n";
	}

	private static string ReadRequiredRepositoryFile( string relativePath ) {
		string path = Path.Combine(
			FindRepositoryRoot(),
			relativePath.Replace( '/', Path.DirectorySeparatorChar )
		);
		Assert.True(
			File.Exists( path ),
			$"Required repository file missing: {relativePath}"
		);
		return File.ReadAllText( path );
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
