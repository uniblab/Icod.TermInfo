/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates the HDB08 packaging and cross-platform qualification contract.
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

using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hdb08PackagingQualificationTests {
	[Fact]
	public void ExactPackageVerificationIsOwnedByManagedTool() {
		string root = FindRepositoryRoot();
		string projectPath =
			Path.Combine(
				root,
				"tools",
				"berkeleydb-package-verifier",
				"Icod.TermInfo.BerkeleyDb.PackageVerifier.csproj"
			);
		string programPath =
			Path.Combine(
				root,
				"tools",
				"berkeleydb-package-verifier",
				"Program.cs"
			);

		Assert.True( File.Exists( projectPath ) );
		Assert.True( File.Exists( programPath ) );

		string project = File.ReadAllText( projectPath );
		string program = File.ReadAllText( programPath );
		string solution =
			File.ReadAllText( Path.Combine( root, "Icod.TermInfo.sln" ) );
		string wrapper =
			File.ReadAllText(
				Path.Combine(
					root,
					".github",
					"scripts",
					"verify-berkeleydb-package.ps1"
				)
			);

		Assert.Contains(
			"<TargetFramework>net10.0</TargetFramework>",
			project,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"<IsPackable>false</IsPackable>",
			project,
			StringComparison.Ordinal
		);
		Assert.DoesNotContain(
			"<PackageReference ",
			project,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"tools\\berkeleydb-package-verifier\\Icod.TermInfo.BerkeleyDb.PackageVerifier.csproj",
			solution,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"private const string PackageId = \"Icod.TermInfo.BerkeleyDb\";",
			program,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"private const string RuntimePackageId = \"Icod.TermInfo\";",
			program,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"private const string ExpectedAssemblyVersion = \"1.0.0.0\";",
			program,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"dependency!.Attribute( \"version\" )?.Value == expectedVersion",
			program,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"StartsWith( \"runtimes/\", StringComparison.Ordinal )",
			program,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"pdb.AsSpan().StartsWith( \"BSJB\"u8 )",
			program,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"ContainsAscii( pdb, commit )",
			program,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"tools/berkeleydb-package-verifier/Icod.TermInfo.BerkeleyDb.PackageVerifier.csproj",
			wrapper,
			StringComparison.Ordinal
		);
		Assert.Contains( "--no-build", wrapper, StringComparison.Ordinal );
		Assert.DoesNotContain(
			"System.IO.Compression.ZipFile",
			wrapper,
			StringComparison.Ordinal
		);
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new DirectoryInfo( AppContext.BaseDirectory );

		while ( current is not null ) {
			if (
				File.Exists(
					Path.Combine(
						current.FullName,
						"Icod.TermInfo.sln"
					)
				)
			) {
				return current.FullName;
			}
			current = current.Parent;
		}

		throw new InvalidOperationException(
			"Repository root not found."
		);
	}
}
