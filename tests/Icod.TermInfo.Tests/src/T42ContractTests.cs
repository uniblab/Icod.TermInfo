using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T42ContractTests
{
	[Fact]
	public void StableOneXAssemblyIdentityAndUnsignedPolicyAreFrozen()
	{
		AssemblyName assemblyName =
			typeof(TerminalDescription)
				.Assembly
				.GetName();

		Assert.Equal(
			new Version(1, 0, 0, 0),
			assemblyName.Version);

		byte[]? publicKeyToken =
			assemblyName.GetPublicKeyToken();
		Assert.True(
			publicKeyToken is null
				|| publicKeyToken.Length == 0,
			"Icod.TermInfo 1.x is intentionally unsigned.");
	}

	[Fact]
	public void SupportedTargetFrameworkMatrixIsFrozen()
	{
		string root =
			FindRepositoryRoot();

		string[] multiTargetProjects =
		[
			"Icod.TermInfo.csproj",
			"tests/Icod.TermInfo.Tests/Icod.TermInfo.Tests.csproj",
			"samples/Icod.TermInfo.Sample/Icod.TermInfo.Sample.csproj",
			"samples/Icod.TermInfo.Acquisition.Sample/Icod.TermInfo.Acquisition.Sample.csproj",
			"tools/package-smoke/Icod.TermInfo.PackageSmoke.csproj",
		];

		foreach (string relativePath in multiTargetProjects)
		{
			Assert.Equal(
				"net8.0;net9.0;net10.0",
				ReadProjectProperty(
					root,
					relativePath,
					"TargetFrameworks"));
		}

		string[] maintenanceProjects =
		[
			"tools/terminfo-metadata/Icod.TermInfo.MetadataGenerator.csproj",
			"tools/compiled-terminfo-fixtures/Icod.TermInfo.FixtureGenerator.csproj",
			"tools/package-verifier/Icod.TermInfo.PackageVerifier.csproj",
			"tools/public-api-snapshot/Icod.TermInfo.PublicApiSnapshot.csproj",
		];

		foreach (string relativePath in maintenanceProjects)
		{
			Assert.Equal(
				"net10.0",
				ReadProjectProperty(
					root,
					relativePath,
					"TargetFramework"));
			Assert.Null(
				ReadOptionalProjectProperty(
					root,
					relativePath,
					"TargetFrameworks"));
		}
	}

	[Fact]
	public void AssemblyAndSigningPolicyIsExplicitInProject()
	{
		string root =
			FindRepositoryRoot();
		XDocument project =
			XDocument.Load(
				Path.Combine(
					root,
					"Icod.TermInfo.csproj"),
				LoadOptions.None);

		Assert.Equal(
			"1.0.0.0",
			project
				.Descendants()
				.Single(
					element =>
						element.Name.LocalName
							== "AssemblyVersion")
				.Value
				.Trim());

		string[] signingValues =
			project
				.Descendants()
				.Where(
					element =>
						element.Name.LocalName
							== "SignAssembly")
				.Select(
					element =>
						element.Value.Trim())
				.ToArray();

		Assert.NotEmpty(
			signingValues);
		Assert.All(
			signingValues,
			value =>
				Assert.Equal(
					"false",
					value));
	}

	[Fact]
	public void BuildAndPackageValidationInstallAllSupportedSdks()
	{
		string root =
			FindRepositoryRoot();
		string pullRequest =
			NormalizeLineEndings(
				File.ReadAllText(
					Path.Combine(
						root,
						".github",
						"workflows",
						"pull-request.yaml")));
		string main =
			NormalizeLineEndings(
				File.ReadAllText(
					Path.Combine(
						root,
						".github",
						"workflows",
						"main.yaml")));
		string release =
			NormalizeLineEndings(
				File.ReadAllText(
					Path.Combine(
						root,
						".github",
						"workflows",
						"release.yaml")));

		foreach (string workflow in new[] { pullRequest, main, release })
		{
			Assert.Contains(
				"DOTNET_VERSIONS: |\n"
				+ "    8.0.x\n"
				+ "    9.0.x\n"
				+ "    10.0.x\n",
				workflow);
		}

		Assert.StartsWith(
			"name: main\n"
			+ "\n"
			+ "on:\n"
			+ "  push:\n"
			+ "    branches:\n"
			+ "      - main\n",
			main);
		Assert.DoesNotContain(
			"pull_request:",
			main);
		Assert.Contains(
			"CONFIGURATION: Staging",
			pullRequest);
		Assert.Contains(
			"CONFIGURATION: Release",
			main);
		Assert.Contains(
			"CONFIGURATION: Release",
			release);

		foreach (string workflow in new[] { pullRequest, main, release })
		{
			Assert.Contains(
				"dotnet build ${{ env.SOLUTION_PATH }}",
				workflow);
			Assert.Contains(
				"dotnet test ${{ env.SOLUTION_PATH }}",
				workflow);
		}

		Assert.Contains(
			"./packaging/PackPackages.ps1",
			pullRequest);
		Assert.Contains(
			"./packaging/VerifyPackageArtifact.ps1",
			pullRequest);
		Assert.Contains(
			"./packaging/PackPackages.ps1",
			main);
		Assert.Contains(
			"./packaging/VerifyPackageArtifact.ps1",
			main);
		Assert.Contains(
			"./packaging/PackPackages.ps1",
			release);
		Assert.Contains(
			"./packaging/VerifyPackageArtifact.ps1",
			release);

		Assert.Contains(
			"actions/upload-artifact@v4",
			pullRequest);
		Assert.Contains(
			"name: terminfo-pr-packages",
			pullRequest);
		Assert.DoesNotContain(
			"dotnet nuget push",
			pullRequest);
		Assert.DoesNotContain(
			"dotnet nuget push",
			main);
		Assert.Contains(
			"dotnet nuget push",
			release);
	}

	private static string ReadProjectProperty(
		string root,
		string relativePath,
		string propertyName)
	{
		ArgumentNullException.ThrowIfNull(root);
		ArgumentNullException.ThrowIfNull(relativePath);
		ArgumentNullException.ThrowIfNull(propertyName);

		return ReadOptionalProjectProperty(
				root,
				relativePath,
				propertyName)
			?? throw new InvalidOperationException(
				$"Project '{relativePath}' does not define '{propertyName}'.");
	}

	private static string? ReadOptionalProjectProperty(
		string root,
		string relativePath,
		string propertyName)
	{
		ArgumentNullException.ThrowIfNull(root);
		ArgumentNullException.ThrowIfNull(relativePath);
		ArgumentNullException.ThrowIfNull(propertyName);

		XDocument project =
			XDocument.Load(
				Path.Combine(
					root,
					relativePath.Replace(
						'/',
						Path.DirectorySeparatorChar)),
				LoadOptions.None);

		return project
			.Descendants()
			.FirstOrDefault(
				element =>
					element.Name.LocalName
						== propertyName)
			?.Value
			.Trim();
	}

	private static string FindRepositoryRoot()
	{
		DirectoryInfo? current =
			new(
				AppContext.BaseDirectory);

		while (current is not null)
		{
			if (File.Exists(
					Path.Combine(
						current.FullName,
						"Icod.TermInfo.csproj")))
			{
				return current.FullName;
			}

			current =
				current.Parent;
		}

		throw new InvalidOperationException(
			"Unable to locate the Icod.TermInfo repository root.");
	}

	private static string NormalizeLineEndings(
		string value)
	{
		ArgumentNullException.ThrowIfNull(value);

		return value
			.Replace(
				"\r\n",
				"\n")
			.Replace(
				'\r',
				'\n');
	}
}
