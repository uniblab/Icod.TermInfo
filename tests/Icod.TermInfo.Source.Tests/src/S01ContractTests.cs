using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Source.Tests;

public sealed class S01ContractTests {
	private const string DevelopmentVersion = "1.4.0";
	private const string StableAssemblyVersion = "1.0.0.0";

	[Fact]
	public void SourcePackageFoundationIsVersionedAndLayered() {
		string root = FindRepositoryRoot();
		XDocument sourceProject =
			XDocument.Load(
				Path.Combine(
					root,
					"Icod.TermInfo.Source",
					"Icod.TermInfo.Source.csproj" ),
				LoadOptions.None );
		XDocument runtimeProject =
			XDocument.Load(
				Path.Combine(
					root,
					"Icod.TermInfo.csproj" ),
				LoadOptions.None );

		Assert.Equal(
			"net8.0;net9.0;net10.0",
			ReadRequiredProperty(
				sourceProject,
				"TargetFrameworks" ) );
		Assert.Equal(
			"13.0",
			ReadRequiredProperty(
				sourceProject,
				"LangVersion" ) );
		Assert.Equal(
			DevelopmentVersion,
			ReadRequiredProperty(
				sourceProject,
				"Version" ) );
		Assert.Equal(
			DevelopmentVersion,
			ReadRequiredProperty(
				sourceProject,
				"PackageVersion" ) );
		Assert.Equal(
			StableAssemblyVersion,
			ReadRequiredProperty(
				sourceProject,
				"AssemblyVersion" ) );
		Assert.Equal(
			"Icod.TermInfo.Source",
			ReadRequiredProperty(
				sourceProject,
				"PackageId" ) );

		XElement sourceReference =
			sourceProject
				.Descendants()
				.Single(
					element =>
						element.Name.LocalName
							== "ProjectReference" );
		Assert.Equal(
			@"..\Icod.TermInfo.csproj",
			sourceReference
				.Attribute( "Include" )
				?.Value );

		Assert.DoesNotContain(
			runtimeProject.Descendants(),
			element =>
				element.Name.LocalName
					== "ProjectReference"
				&& ( element.Attribute( "Include" )?.Value
					.Contains(
						"Icod.TermInfo.Source",
						StringComparison.OrdinalIgnoreCase )
					?? false ) );
	}

	[Fact]
	public void RuntimePackageStartsOneOneDevelopmentWithoutChangingAssemblyIdentity() {
		string root = FindRepositoryRoot();
		XDocument runtimeProject =
			XDocument.Load(
				Path.Combine(
					root,
					"Icod.TermInfo.csproj" ),
				LoadOptions.None );

		Assert.Equal(
			DevelopmentVersion,
			ReadRequiredProperty(
				runtimeProject,
				"Version" ) );
		Assert.Equal(
			DevelopmentVersion,
			ReadRequiredProperty(
				runtimeProject,
				"PackageVersion" ) );
		Assert.Equal(
			StableAssemblyVersion,
			ReadRequiredProperty(
				runtimeProject,
				"AssemblyVersion" ) );
	}

	[Fact]
	public void SourceTestsAndFreshConsumerMatchTheSupportedFrameworkMatrix() {
		string root = FindRepositoryRoot();

		Assert.Equal(
			"net8.0;net9.0;net10.0",
			ReadProjectProperty(
				root,
				"tests/Icod.TermInfo.Source.Tests/Icod.TermInfo.Source.Tests.csproj",
				"TargetFrameworks" ) );
		Assert.Equal(
			"net8.0;net9.0;net10.0",
			ReadProjectProperty(
				root,
				"tools/source-package-smoke/Icod.TermInfo.Source.PackageSmoke.csproj",
				"TargetFrameworks" ) );
	}

	[Fact]
	public void SolutionAndContinuousIntegrationIncludeTheSourcePackage() {
		string root = FindRepositoryRoot();
		string solution =
			File.ReadAllText(
				Path.Combine(
					root,
					"Icod.TermInfo.sln" ) );
		string pullRequest =
			File.ReadAllText(
				Path.Combine(
					root,
					".github",
					"workflows",
					"pr-build-and-test.yaml" ) );
		string pushMain =
			File.ReadAllText(
				Path.Combine(
					root,
					".github",
					"workflows",
					"push-main.yaml" ) );

		Assert.Contains(
			"Icod.TermInfo.Source",
			solution );
		Assert.Contains(
			"Icod.TermInfo.Source.Tests",
			solution );
		Assert.Contains(
			"dotnet pack Icod.TermInfo.Source/Icod.TermInfo.Source.csproj -c Staging",
			pullRequest );
		Assert.Contains(
			"dotnet pack Icod.TermInfo.Source/Icod.TermInfo.Source.csproj -c Release",
			pushMain );
	}

	[Fact]
	public void PackageValidationCoversSourceApiParityAndFreshConsumption() {
		string root = FindRepositoryRoot();

		foreach (
			string relativePath
			in new[]
			{
				Path.Combine(
					".github",
					"scripts",
					"verify-release-package.cmd"),
				Path.Combine(
					".github",
					"scripts",
					"verify-release-package.sh"),
			} ) {
			string verifier =
				File.ReadAllText(
					Path.Combine(
						root,
						relativePath ) );

			Assert.Contains(
				"Icod.TermInfo.Source",
				verifier );
			Assert.Contains(
				"source-package-smoke",
				verifier );
			Assert.Contains(
				"--compare",
				verifier );
			Assert.Contains(
				"net8.0",
				verifier );
			Assert.Contains(
				"net9.0",
				verifier );
			Assert.Contains(
				"net10.0",
				verifier );
		}
	}

	[Fact]
	public void SourceDiagnosticConventionIsRecordedBeforeParserApi() {
		string root = FindRepositoryRoot();
		string record =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.1.0-S01-SOURCE-PACKAGE-FOUNDATION.md" ) );

		Assert.Contains(
			"TIS",
			record );
		Assert.True(
			record.Contains(
				"severity",
				StringComparison.OrdinalIgnoreCase ) );
		Assert.True(
			record.Contains(
				"source location",
				StringComparison.OrdinalIgnoreCase ) );
		Assert.True(
			record.Contains(
				"deterministic",
				StringComparison.OrdinalIgnoreCase ) );
		Assert.Contains(
			"S02",
			record );
	}

	private static string ReadProjectProperty(
		string root,
		string relativePath,
		string propertyName ) {
		ArgumentNullException.ThrowIfNull( root );
		ArgumentNullException.ThrowIfNull( relativePath );
		ArgumentNullException.ThrowIfNull( propertyName );

		XDocument project =
			XDocument.Load(
				Path.Combine(
					root,
					relativePath.Replace(
						'/',
						Path.DirectorySeparatorChar ) ),
				LoadOptions.None );

		return ReadRequiredProperty(
			project,
			propertyName );
	}

	private static string ReadRequiredProperty(
		XDocument project,
		string propertyName ) {
		ArgumentNullException.ThrowIfNull( project );
		ArgumentNullException.ThrowIfNull( propertyName );

		return project
			.Descendants()
			.First(
				element =>
					element.Name.LocalName
						== propertyName )
			.Value
			.Trim();
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new(
				AppContext.BaseDirectory );

		while ( current is not null ) {
			if ( File.Exists(
					Path.Combine(
						current.FullName,
						"Icod.TermInfo.sln" ) ) ) {
				return current.FullName;
			}

			current =
				current.Parent;
		}

		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root." );
	}
}
