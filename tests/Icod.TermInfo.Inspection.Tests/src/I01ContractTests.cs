using System.Reflection;
using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class I01ContractTests {
	private const string DevelopmentVersion = "1.3.0-Alpha-5";
	private const string StableAssemblyVersion = "1.0.0.0";

	[Fact]
	public void FourPackagesAdvanceTogetherWithoutChangingAssemblyIdentity() {
		string root =
			FindRepositoryRoot();

		foreach (
			string relativePath
			in new[] {
				"Icod.TermInfo.csproj",
				"Icod.TermInfo.Source/Icod.TermInfo.Source.csproj",
				"Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj",
				"Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj",
			}
		) {
			XDocument project =
				LoadProject(
					root,
					relativePath
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

		Assembly inspection =
			Assembly.Load(
				"Icod.TermInfo.Inspection"
			);
		Assert.Equal(
			new Version( 1, 0, 0, 0 ),
			inspection.GetName().Version
		);
		string[] exportedTypes =
			inspection
				.GetExportedTypes()
				.Select(
					type => type.FullName ?? string.Empty
				)
				.OrderBy(
					name => name,
					StringComparer.Ordinal
				)
				.ToArray();
		Assert.Equal(
			new[] {
				"Icod.TermInfo.Inspection.TermInfoComparisonResult",
				"Icod.TermInfo.Inspection.TermInfoDifference",
				"Icod.TermInfo.Inspection.TermInfoDifferenceKind",
				"Icod.TermInfo.Inspection.TermInfoSourceComparer",
				"Icod.TermInfo.Inspection.TermInfoSourceRenderer",
				"Icod.TermInfo.Inspection.TerminalDescriptionComparer",
				"Icod.TermInfo.Inspection.TerminalDescriptionSourceRenderer",
			},
			exportedTypes
		);
	}

	[Fact]
	public void InspectionPackageTargetsThreeFrameworksAndDependsExactlyOnRuntimeAndSource() {
		string root =
			FindRepositoryRoot();
		XDocument project =
			LoadProject(
				root,
				"Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj"
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
		Assert.Equal(
			"Icod.TermInfo.Inspection",
			ReadRequiredProperty(
				project,
				"PackageId"
			)
		);
		Assert.Equal(
			"true",
			ReadRequiredProperty(
				project,
				"GenerateDocumentationFile"
			)
		);
		Assert.Equal(
			"true",
			ReadRequiredProperty(
				project,
				"IncludeSymbols"
			)
		);
		Assert.Equal(
			"snupkg",
			ReadRequiredProperty(
				project,
				"SymbolPackageFormat"
			)
		);

		string[] references =
			ReadProjectReferences(
				project
			);
		Assert.Equal(
			new[] {
				@"..\Icod.TermInfo.Source\Icod.TermInfo.Source.csproj",
				@"..\Icod.TermInfo.csproj",
			},
			references
		);
		Assert.DoesNotContain(
			references,
			reference =>
				reference.Contains(
					"Compiler",
					StringComparison.Ordinal
				)
		);
	}

	[Fact]
	public void ExistingProductionPackagesDoNotAcquireInspectionDependency() {
		string root =
			FindRepositoryRoot();

		foreach (
			string relativePath
			in new[] {
				"Icod.TermInfo.csproj",
				"Icod.TermInfo.Source/Icod.TermInfo.Source.csproj",
				"Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj",
			}
		) {
			string[] references =
				ReadProjectReferences(
					LoadProject(
						root,
						relativePath
					)
				);
			Assert.DoesNotContain(
				references,
				reference =>
					reference.Contains(
						"Inspection",
						StringComparison.Ordinal
					)
			);
		}
	}

	[Fact]
	public void InspectionPublicApiBaselineContainsReviewedI02ThroughI05Surface() {
		string root =
			FindRepositoryRoot();
		string baseline =
			NormalizeLineEndings(
				File.ReadAllText(
					Path.Combine(
						root,
						"docs",
						"1.3.0-INSPECTION-PUBLIC-API-BASELINE.txt"
					)
				)
			);

		Assert.Contains(
			"TYPE class Icod.TermInfo.Inspection.TermInfoComparisonResult [sealed]",
			baseline
		);
		Assert.Contains(
			"TYPE class Icod.TermInfo.Inspection.TermInfoDifference [sealed]",
			baseline
		);
		Assert.Contains(
			"TYPE enum Icod.TermInfo.Inspection.TermInfoDifferenceKind [sealed]",
			baseline
		);
		Assert.Contains(
			"TYPE class Icod.TermInfo.Inspection.TermInfoSourceComparer [static]",
			baseline
		);
		Assert.Contains(
			"METHOD public static Icod.TermInfo.Inspection.TermInfoComparisonResult Compare(Icod.TermInfo.Source.TermInfoSourceDocument left",
			baseline
		);
		Assert.Contains(
			"PROPERTY Icod.TermInfo.Source.TermInfoSourceField LeftSourceField",
			baseline
		);
		Assert.Contains(
			"PROPERTY Icod.TermInfo.Source.TermInfoSourceSpan LeftSourceSpan",
			baseline
		);
		Assert.Contains(
			"TYPE class Icod.TermInfo.Inspection.TermInfoSourceRenderer [static]",
			baseline
		);
		Assert.Contains(
			"METHOD public static System.String Render(Icod.TermInfo.Source.TermInfoSourceDocument document",
			baseline
		);
		Assert.Contains(
			"METHOD public static System.String Render(Icod.TermInfo.Source.TermInfoSourceEntry entry",
			baseline
		);
		Assert.Contains(
			"TYPE class Icod.TermInfo.Inspection.TerminalDescriptionComparer [static]",
			baseline
		);
		Assert.Contains(
			"METHOD public static Icod.TermInfo.Inspection.TermInfoComparisonResult Compare(Icod.TermInfo.TerminalDescription left",
			baseline
		);
		Assert.Contains(
			"TYPE class Icod.TermInfo.Inspection.TerminalDescriptionSourceRenderer [static]",
			baseline
		);
		Assert.Contains(
			"METHOD public static System.String Render(Icod.TermInfo.TerminalDescription description",
			baseline
		);
		Assert.Contains(
			"METHOD public static System.Void Write(System.IO.TextWriter writer",
			baseline
		);
	}

	[Fact]
	public void SolutionAndReleasePipelineAreFourPackageAware() {
		string root =
			FindRepositoryRoot();
		string solution =
			File.ReadAllText(
				Path.Combine(
					root,
					"Icod.TermInfo.sln"
				)
			);

		Assert.Contains(
			"Icod.TermInfo.Inspection",
			solution
		);
		Assert.Contains(
			"Icod.TermInfo.Inspection.Tests",
			solution
		);
		Assert.Contains(
			"Icod.TermInfo.Inspection.PackageVerifier",
			solution
		);

		foreach (
			string relativePath
			in new[] {
				".github/workflows/pr-build-and-test.yaml",
				".github/workflows/push-main.yaml",
				".github/workflows/release.yaml",
			}
		) {
			string workflow =
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
				"Icod.TermInfo.Inspection",
				workflow
			);
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
				"1.3.0-INSPECTION-PUBLIC-API-BASELINE.txt",
				verifier
			);
			Assert.Contains(
				"inspection-package-smoke",
				verifier
			);
			Assert.Contains(
				"inspection-package-verifier",
				verifier
			);
		}

		string release =
			File.ReadAllText(
				Path.Combine(
					root,
					".github",
					"workflows",
					"release.yaml"
				)
			);
		Assert.Contains(
			"if (8 -ne $files.Count)",
			release
		);
		Assert.Contains(
			"if (9 -ne $assets.Count)",
			release
		);
		Assert.Contains(
			"dotnet add package Icod.TermInfo.Inspection --version {0}",
			release
		);
	}

	private static XDocument LoadProject(
		string root,
		string relativePath
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentException.ThrowIfNullOrWhiteSpace( relativePath );

		return XDocument.Load(
			Path.Combine(
				root,
				relativePath.Replace(
					'/',
					Path.DirectorySeparatorChar
				)
			),
			LoadOptions.None
		);
	}

	private static string[] ReadProjectReferences(
		XDocument project
	) {
		ArgumentNullException.ThrowIfNull( project );

		return project
			.Descendants()
			.Where(
				element =>
					element.Name.LocalName == "ProjectReference"
			)
			.Select(
				element =>
					element.Attribute( "Include" )?.Value
					?? string.Empty
			)
			.OrderBy(
				value => value,
				StringComparer.Ordinal
			)
			.ToArray();
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
				element =>
					element.Name.LocalName == propertyName
			)
			.Value
			.Trim();
	}

	private static string NormalizeLineEndings(
		string value
	) {
		ArgumentNullException.ThrowIfNull( value );

		return value
			.Replace(
				"\r\n",
				"\n"
			)
			.Replace(
				'\r',
				'\n'
			);
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new(
				AppContext.BaseDirectory
			);

		while ( current is not null ) {
			if ( File.Exists(
					Path.Combine(
						current.FullName,
						"Icod.TermInfo.sln"
					)
				) ) {
				return current.FullName;
			}

			current =
				current.Parent;
		}

		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root."
		);
	}
}
