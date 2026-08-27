using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo.Compiler;
using Xunit;

namespace Icod.TermInfo.Compiler.Tests;

public sealed class C01ContractTests {
	private const string DevelopmentVersion = "1.2.0-Alpha-2";
	private const string StableAssemblyVersion = "1.0.0.0";

	[Fact]
	public void ThreePackagesAdvanceTogetherWithoutChangingAssemblyIdentity() {
		string root = FindRepositoryRoot();

		foreach (
			string relativePath
			in new[] {
				"Icod.TermInfo.csproj",
				"Icod.TermInfo.Source/Icod.TermInfo.Source.csproj",
				"Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj",
			}
		) {
			XDocument project =
				XDocument.Load(
					Path.Combine(
						root,
						relativePath.Replace(
							'/',
							Path.DirectorySeparatorChar
						)
					),
					LoadOptions.None
				);

			Assert.Equal(
				DevelopmentVersion,
				ReadRequiredProperty(
					project,
					"Version"
				)
			);
			Assert.Equal(
				DevelopmentVersion,
				ReadRequiredProperty(
					project,
					"PackageVersion"
				)
			);
			Assert.Equal(
				StableAssemblyVersion,
				ReadRequiredProperty(
					project,
					"AssemblyVersion"
				)
			);
		}

		Assert.Equal(
			new Version( 1, 0, 0, 0 ),
			typeof( CompiledTermInfoWriter )
				.Assembly
				.GetName()
				.Version
		);
	}

	[Fact]
	public void CompilerPackageTargetsThreeFrameworksAndDependsOnlyOnRuntime() {
		string root = FindRepositoryRoot();
		XDocument project =
			XDocument.Load(
				Path.Combine(
					root,
					"Icod.TermInfo.Compiler",
					"Icod.TermInfo.Compiler.csproj"
				),
				LoadOptions.None
			);

		Assert.Equal(
			"net8.0;net9.0;net10.0",
			ReadRequiredProperty(
				project,
				"TargetFrameworks"
			)
		);
		Assert.Equal(
			"13.0",
			ReadRequiredProperty(
				project,
				"LangVersion"
			)
		);

		XElement reference = Assert.Single(
			project.Descendants(),
			element => element.Name.LocalName == "ProjectReference"
		);
		Assert.Equal(
			@"..\Icod.TermInfo.csproj",
			reference.Attribute( "Include" )?.Value
		);
		Assert.DoesNotContain(
			project.Descendants(),
			element =>
				element.Name.LocalName == "ProjectReference"
				&& ( element.Attribute( "Include" )?.Value
					.Contains(
						"Icod.TermInfo.Source",
						StringComparison.OrdinalIgnoreCase
					) ?? false )
		);
	}

	[Fact]
	public void CompilerApiBaselineContainsOnlyTheReviewedC01Writer() {
		string root = FindRepositoryRoot();
		string baseline =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.2.0-COMPILER-PUBLIC-API-BASELINE.txt"
				)
			);

		Assert.Contains(
			"TYPE class Icod.TermInfo.Compiler.CompiledTermInfoWriter [static]",
			baseline
		);
		Assert.Contains(
			"METHOD public static System.Byte[] Write(Icod.TermInfo.TerminalDescription description null=not-null/not-null) return-null=not-null/not-null",
			baseline
		);
	}

	[Fact]
	public void SolutionWorkflowsAndReleaseVerifierIncludeCompilerPackage() {
		string root = FindRepositoryRoot();
		string solution =
			File.ReadAllText(
				Path.Combine(
					root,
					"Icod.TermInfo.sln"
				)
			);
		Assert.Contains( "Icod.TermInfo.Compiler", solution );
		Assert.Contains( "Icod.TermInfo.Compiler.Tests", solution );

		foreach (
			string relativePath
			in new[] {
				".github/workflows/pr-build-and-test.yaml",
				".github/workflows/push-main.yaml",
				".github/scripts/verify-release-package.cmd",
				".github/scripts/verify-release-package.sh",
			}
		) {
			string text =
				File.ReadAllText(
					Path.Combine(
						root,
						relativePath.Replace(
							'/',
							Path.DirectorySeparatorChar
						)
					)
				);
			Assert.Contains( "Icod.TermInfo.Compiler", text );
		}

		foreach (
			string relativePath
			in new[] {
				".github/scripts/verify-release-package.cmd",
				".github/scripts/verify-release-package.sh",
			}
		) {
			string verifier =
				File.ReadAllText(
					Path.Combine(
						root,
						relativePath.Replace(
							'/',
							Path.DirectorySeparatorChar
						)
					)
				);
			Assert.Contains(
				"1.2.0-COMPILER-PUBLIC-API-BASELINE.txt",
				verifier
			);
			Assert.Contains( "compiler-package-smoke", verifier );
			Assert.Contains( "compiler-package-verifier", verifier );
		}
	}

	private static string ReadRequiredProperty(
		XDocument project,
		string propertyName
	) {
		ArgumentNullException.ThrowIfNull( project );
		ArgumentException.ThrowIfNullOrWhiteSpace( propertyName );

		return project
			.Descendants()
			.First(
				element => element.Name.LocalName == propertyName
			)
			.Value
			.Trim();
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new( AppContext.BaseDirectory );

		while ( current is not null ) {
			if ( File.Exists(
				Path.Combine(
					current.FullName,
					"Icod.TermInfo.sln"
				)
			) ) {
				return current.FullName;
			}

			current = current.Parent;
		}

		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root."
		);
	}
}