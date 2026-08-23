using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T42ContractTests
{
	[Fact]
	public void AssemblyIdentifiesT42AndStableOneXIdentity()
	{
		Assembly assembly =
			typeof(TerminalDescription).Assembly;
		AssemblyName assemblyName =
			assembly.GetName();
		string? informationalVersion =
			assembly
				.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
				?.InformationalVersion;

		Assert.Equal(
			new Version(1, 0, 0, 0),
			assemblyName.Version);
		Assert.NotNull(informationalVersion);

		string semanticVersion =
			informationalVersion!
				.Split(
					'+',
					2)[0];

		Assert.Equal(
			"1.0.0-alpha.1",
			semanticVersion);

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

		string[] dualTargetProjects =
		[
			"Icod.TermInfo.csproj",
			"tests/Icod.TermInfo.Tests/Icod.TermInfo.Tests.csproj",
			"samples/Icod.TermInfo.Sample/Icod.TermInfo.Sample.csproj",
			"samples/Icod.TermInfo.Acquisition.Sample/Icod.TermInfo.Acquisition.Sample.csproj",
			"tools/package-smoke/Icod.TermInfo.PackageSmoke.csproj",
		];

		foreach (string relativePath in dualTargetProjects)
		{
			Assert.Equal(
				"net8.0;net10.0",
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
	public void BuildAndPackageValidationInstallBothSupportedSdks()
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
						"pr-build-and-test.yaml")));
		string pushMain =
			NormalizeLineEndings(
				File.ReadAllText(
					Path.Combine(
						root,
						".github",
						"workflows",
						"push-main.yaml")));

		Assert.Contains(
			"dotnet-version: |\n"
			+ "            8.0.x\n"
			+ "            10.0.x\n",
			pullRequest);
		Assert.True(
			CountOccurrences(
				pushMain,
				"dotnet-version: |\n"
				+ "            8.0.x\n"
				+ "            10.0.x\n")
				>= 2,
			"The main build/test and package-validation jobs must install "
				+ "both supported SDK/runtime lines.");

		Assert.StartsWith(
			"name: build and publish\n"
			+ "\n"
			+ "on:\n"
			+ "  push:\n"
			+ "    branches:\n"
			+ "      - main\n",
			pushMain);
		Assert.DoesNotContain(
			"pull_request:",
			pushMain);
		Assert.DoesNotContain(
			"dotnet pack",
			pullRequest);
		Assert.DoesNotContain(
			"dotnet nuget push",
			pullRequest);
	}

	private static int CountOccurrences(
		string value,
		string fragment)
	{
		ArgumentNullException.ThrowIfNull(value);
		ArgumentNullException.ThrowIfNull(fragment);

		int count = 0;
		int startIndex = 0;

		while (true)
		{
			int index =
				value.IndexOf(
					fragment,
					startIndex,
					StringComparison.Ordinal);
			if (index < 0)
			{
				return count;
			}

			count++;
			startIndex =
				index + fragment.Length;
		}
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
