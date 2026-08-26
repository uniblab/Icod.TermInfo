using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T45CompletionGateTests
{
	[Fact]
	public void AssemblyRetainsStableIdentityDuringOneOneDevelopment()
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
		Assert.NotNull(
			informationalVersion);

		string semanticVersion =
			informationalVersion!
				.Split(
					'+',
					2)[0];

		Assert.Equal(
			"1.1.0-Alpha-3",
			semanticVersion);
	}

	[Fact]
	public void ProjectMetadataIdentifiesOneOneDevelopmentAndStableAssembly()
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
			"1.1.0-Alpha-3",
			ReadRequiredProperty(
				project,
				"Version"));
		Assert.Equal(
			"1.1.0-Alpha-3",
			ReadRequiredProperty(
				project,
				"PackageVersion"));
		Assert.Equal(
			"1.0.0.0",
			ReadRequiredProperty(
				project,
				"AssemblyVersion"));
		Assert.Equal(
			"net8.0;net10.0",
			ReadRequiredProperty(
				project,
				"TargetFrameworks"));
	}

	[Fact]
	public void FinalReadmeUsesStablePackageVersionAndPolicies()
	{
		string root =
			FindRepositoryRoot();
		string readme =
			File.ReadAllText(
				Path.Combine(
					root,
					"README.md"));

		Assert.Contains(
			"dotnet add package Icod.TermInfo --version 1.0.0",
			readme);
		Assert.DoesNotContain(
			"1.0.0-rc.1",
			readme);
		Assert.Contains(
			"docs/VERSIONING.md",
			readme);
		Assert.Contains(
			"docs/COMPATIBILITY.md",
			readme);
		Assert.Contains(
			"docs/1.0.0-CONTRACT-AUDIT.md",
			readme);
	}

	[Fact]
	public void ReleaseVerifierRetainsAllFinalCompatibilityGates()
	{
		string root =
			FindRepositoryRoot();

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
			})
		{
			string verifier =
				File.ReadAllText(
					Path.Combine(
						root,
						relativePath));

			Assert.Contains(
				"--check",
				verifier);
			Assert.Contains(
				"--compare",
				verifier);
			Assert.Contains(
				"net8.0",
				verifier);
			Assert.Contains(
				"net10.0",
				verifier);
			Assert.True(
				verifier.Contains(
					"package-smoke",
					StringComparison.OrdinalIgnoreCase));
		}
	}

	[Fact]
	public void FinalContractAuditDefinesReleaseSignOffWithoutClaimingIt()
	{
		string root =
			FindRepositoryRoot();
		string audit =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.0.0-CONTRACT-AUDIT.md"));

		Assert.Contains(
			"Release sign-off pending",
			audit);
		Assert.Contains(
			"docs/1.0.0-PUBLIC-API-BASELINE.txt",
			audit);
		Assert.Contains(
			"public-api-snapshot --check",
			audit);
		Assert.Contains(
			"ncurses 6.5.20250216",
			audit);
		Assert.Contains(
			"verify-release-package",
			audit);
		Assert.Contains(
			"v1.0.0",
			audit);
	}

	private static string ReadRequiredProperty(
		XDocument project,
		string name)
	{
		ArgumentNullException.ThrowIfNull(
			project);
		ArgumentNullException.ThrowIfNull(
			name);

		return project
			.Descendants()
			.Single(
				element =>
					element.Name.LocalName
						== name)
			.Value
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
}
