using System.Xml.Linq;
using Icod.TermInfo;
using Xunit;

namespace Icod.TermInfo.Tests;

public sealed class T44DocumentationPackageFreezeTests
{
	[Fact]
	public void ReleaseBuildMakesMissingPublicXmlDocumentationFatal()
	{
		string root =
			FindRepositoryRoot();
		XDocument project =
			XDocument.Load(
				Path.Combine(
					root,
					"Icod.TermInfo.csproj"),
				LoadOptions.None);

		XElement release =
			project
				.Descendants()
				.Single(
					element =>
						element.Name.LocalName
							== "PropertyGroup"
						&& (element.Attribute("Condition")?.Value
							.Contains(
								"'$(Configuration)' == 'Release'",
								StringComparison.Ordinal)
							?? false));

		Assert.Equal(
			"true",
			release
				.Elements()
				.Single(
					element =>
						element.Name.LocalName
							== "TreatWarningsAsErrors")
				.Value
				.Trim());

		Assert.DoesNotContain(
			release.Elements(),
			element =>
				element.Name.LocalName
					== "WarningsNotAsErrors"
				&& element.Value.Contains(
					"CS1591",
					StringComparison.Ordinal));

		Assert.Equal(
			"true",
			project
				.Descendants()
				.Single(
					element =>
						element.Name.LocalName
							== "GenerateDocumentationFile")
				.Value
				.Trim());
	}

	[Fact]
	public void OneXVersioningAndCompatibilityPoliciesAreCheckedIn()
	{
		string root =
			FindRepositoryRoot();

		string versioning =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"VERSIONING.md"));
		string compatibility =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"COMPATIBILITY.md"));

		Assert.Contains(
			"Semantic Versioning",
			versioning);
		Assert.Contains(
			"1.0.0.0",
			versioning);
		Assert.Contains(
			"unsigned",
			versioning);

		Assert.Contains(
			"net8.0",
			compatibility);
		Assert.Contains(
			"net10.0",
			compatibility);
		Assert.Contains(
			"Windows",
			compatibility);
		Assert.Contains(
			"Linux",
			compatibility);
		Assert.Contains(
			"macOS",
			compatibility);
	}

	[Fact]
	public void PackageMetadataRetainsFrozenReleaseAssets()
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
			"README.md",
			ReadRequiredProperty(
				project,
				"PackageReadmeFile"));
		Assert.Equal(
			"icon.png",
			ReadRequiredProperty(
				project,
				"PackageIcon"));
		Assert.Equal(
			"LGPL-3.0-or-later",
			ReadRequiredProperty(
				project,
				"PackageLicenseExpression"));
		Assert.Equal(
			"1.0.0.0",
			ReadRequiredProperty(
				project,
				"AssemblyVersion"));
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
