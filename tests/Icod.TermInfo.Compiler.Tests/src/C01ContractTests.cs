using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo.Compiler;
using Xunit;

namespace Icod.TermInfo.Compiler.Tests;

public sealed class C01ContractTests {
	private const string DevelopmentVersion = "1.4.0-Alpha-7";
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
	public void CompilerPackageTargetsThreeFrameworksAndDependsOnRuntimeAndSource() {
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

		string[] references =
			project
				.Descendants()
				.Where(
					element => element.Name.LocalName == "ProjectReference"
				)
				.Select(
					element => element.Attribute( "Include" )?.Value
						?? string.Empty
				)
				.OrderBy(
					value => value,
					StringComparer.Ordinal
				)
				.ToArray();
		Assert.Equal(
			new[] {
				@"..\Icod.TermInfo.Source\Icod.TermInfo.Source.csproj",
				@"..\Icod.TermInfo.csproj",
			},
			references
		);
	}

	[Fact]
	public void CompilerApiBaselineContainsReviewedC04WriterSurface() {
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
			"TYPE enum Icod.TermInfo.Compiler.CompiledTermInfoFormat [sealed]",
			baseline
		);
		Assert.Contains(
			"ENUM Automatic = 0",
			baseline
		);
		Assert.Contains(
			"ENUM Legacy = 1",
			baseline
		);
		Assert.Contains(
			"ENUM Wide = 2",
			baseline
		);
		Assert.Contains(
			"TYPE class Icod.TermInfo.Compiler.CompiledTermInfoWriter [static]",
			baseline
		);
		Assert.Contains(
			"METHOD public static System.Byte[] Write(Icod.TermInfo.TerminalDescription description null=not-null/not-null) return-null=not-null/not-null<not-null/not-null>",
			baseline
		);
		Assert.Contains(
			"METHOD public static System.Byte[] Write(Icod.TermInfo.TerminalDescription description null=not-null/not-null, Icod.TermInfo.Compiler.CompiledTermInfoWriterOptions options null=not-null/not-null) return-null=not-null/not-null<not-null/not-null>",
			baseline
		);
		Assert.Contains(
			"TYPE class Icod.TermInfo.Compiler.CompiledTermInfoWriterOptions [sealed]",
			baseline
		);
		Assert.Contains(
			"CTOR public CompiledTermInfoWriterOptions()",
			baseline
		);
		Assert.Contains(
			"CTOR public CompiledTermInfoWriterOptions(Icod.TermInfo.Compiler.CompiledTermInfoFormat format null=not-null/not-null, System.Boolean includeExtendedCapabilities null=not-null/not-null default=true)",
			baseline
		);
		Assert.Contains(
			"PROPERTY Icod.TermInfo.Compiler.CompiledTermInfoFormat Format { public get; } null=not-null/unknown",
			baseline
		);
		Assert.Contains(
			"PROPERTY System.Boolean IncludeExtendedCapabilities { public get; } null=not-null/unknown",
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