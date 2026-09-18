/*
	Icod.TermInfo.BerkeleyDb.Tests
	Validates the HDB01 optional-package foundation contract.
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

using System.Reflection;
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hdb01ContractTests {
	[Fact]
	public void Hdb05ExportsOnlyTheReviewedPublicContracts() {
		string assemblyPath =
			Path.Combine(
				AppContext.BaseDirectory,
				"Icod.TermInfo.BerkeleyDb.dll"
			);
		Assert.True( File.Exists( assemblyPath ) );

		Assembly assembly =
			Assembly.LoadFrom( assemblyPath );

		Assert.Equal(
			new[] {
				"Icod.TermInfo.BerkeleyDb.BerkeleyDbDatabaseFormatException",
				"Icod.TermInfo.BerkeleyDb.BerkeleyDbSystemTerminalDescriptionProvider",
				"Icod.TermInfo.BerkeleyDb.BerkeleyDbSystemTerminalDescriptionProviderOptions",
				"Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalCatalogEntry",
				"Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalCatalogEntryKind",
				"Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalCatalogReader",
				"Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalCatalogReaderOptions",
				"Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalDatabaseEntry",
				"Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalDatabaseWriter",
				"Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalDatabaseWriterOptions",
				"Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalDescriptionProvider",
				"Icod.TermInfo.BerkeleyDb.BerkeleyDbTerminalDescriptionProviderOptions",
			},
			assembly.GetExportedTypes()
				.Select( type => type.FullName )
				.OrderBy( name => name, StringComparer.Ordinal )
				.ToArray()
		);
	}

	[Fact]
	public void PackageProjectPreservesReviewedFoundationBoundary() {
		string root = FindRepositoryRoot();
		string project = File.ReadAllText(
			Path.Combine(
				root,
				"Icod.TermInfo.BerkeleyDb",
				"Icod.TermInfo.BerkeleyDb.csproj"
			)
		);

		Assert.Contains(
			"<TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>",
			project,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"<AssemblyVersion>1.0.0.0</AssemblyVersion>",
			project,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"<PackageLicenseExpression>LGPL-3.0-or-later</PackageLicenseExpression>",
			project,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"<ProjectReference Include=\"..\\Icod.TermInfo.csproj\" />",
			project,
			StringComparison.Ordinal
		);
		Assert.DoesNotContain(
			"<PackageReference ",
			project,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void CoordinatedDevelopmentLineAndPackagePipelineIncludeBerkeleyDb() {
		string root = FindRepositoryRoot();
		string props =
			File.ReadAllText(
				Path.Combine(
					root,
					"Directory.Build.props"
				)
			);
		string pack =
			File.ReadAllText(
				Path.Combine(
					root,
					"packaging",
					"PackPackages.ps1"
				)
			);
		string artifactVerifier =
			File.ReadAllText(
				Path.Combine(
					root,
					"packaging",
					"VerifyPackageArtifact.ps1"
				)
			);
		string pullRequestWorkflow =
			File.ReadAllText(
				Path.Combine(
					root,
					".github",
					"workflows",
					"pull-request.yaml"
				)
			);
		string mainWorkflow =
			File.ReadAllText(
				Path.Combine(
					root,
					".github",
					"workflows",
					"main.yaml"
				)
			);
		string releaseWorkflow =
			File.ReadAllText(
				Path.Combine(
					root,
					".github",
					"workflows",
					"release.yaml"
				)
			);

		Assert.Contains(
			"<IcodTermInfoSuiteVersion>1.16.0-Alpha-2</IcodTermInfoSuiteVersion>",
			props,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Icod.TermInfo.BerkeleyDb/Icod.TermInfo.BerkeleyDb.csproj",
			pack,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"verify-berkeleydb-package.ps1",
			artifactVerifier,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Icod.TermInfo.BerkeleyDb.Tests.csproj",
			pullRequestWorkflow,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Icod.TermInfo.BerkeleyDb.Tests.csproj",
			mainWorkflow,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Icod.TermInfo.BerkeleyDb.Tests.csproj",
			releaseWorkflow,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"if (7 -ne $packages.Count)",
			releaseWorkflow,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"if (19 -ne $files.Count)",
			releaseWorkflow,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"if (20 -ne $assets.Count)",
			releaseWorkflow,
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
