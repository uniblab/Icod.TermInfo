using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo;
using Icod.TermInfo.Termcap;
using Xunit;

namespace Icod.TermInfo.Termcap.Tests;

public sealed class TC01ContractTests
{
	private const string DevelopmentVersion = "1.6.0-Alpha-1";
	private const string VersionReference = "$(IcodTermInfoSuiteVersion)";

	[Fact]
	public void TermcapPackageIsSeparateAndDependsOnlyOnRuntime() {
		string root = FindRepositoryRoot();
		XDocument project =
			XDocument.Load(
				Path.Combine(
					root,
					"Icod.TermInfo.Termcap",
					"Icod.TermInfo.Termcap.csproj"
				),
				LoadOptions.None
			);

		Assert.Equal(
			VersionReference,
			ReadRequiredProperty(
				project,
				"Version"
			)
		);
		Assert.Equal(
			VersionReference,
			ReadRequiredProperty(
				project,
				"PackageVersion"
			)
		);
		Assert.Equal(
			"1.0.0.0",
			ReadRequiredProperty(
				project,
				"AssemblyVersion"
			)
		);

		string[] references =
			project
				.Descendants()
				.Where(
					element => element.Name.LocalName == "ProjectReference"
				)
				.Select(
					element => element.Attribute( "Include" )?.Value ?? string.Empty
				)
				.ToArray();
		Assert.Equal(
			new[] { @"..\Icod.TermInfo.csproj" },
			references
		);
	}

	[Fact]
	public void CoordinatedVersionAndTc01WorkflowBoundaryAreExplicit() {
		string root = FindRepositoryRoot();
		XDocument buildProperties =
			XDocument.Load(
				Path.Combine(
					root,
					"Directory.Build.props"
				),
				LoadOptions.None
			);

		Assert.Equal(
			DevelopmentVersion,
			ReadRequiredProperty(
				buildProperties,
				"IcodTermInfoSuiteVersion"
			)
		);

		string pullRequest =
			File.ReadAllText(
				Path.Combine(
					root,
					".github",
					"workflows",
					"pr-build-and-test.yaml"
				)
			);
		string pushMain =
			File.ReadAllText(
				Path.Combine(
					root,
					".github",
					"workflows",
					"push-main.yaml"
				)
			);
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
			"dotnet pack Icod.TermInfo.Termcap/Icod.TermInfo.Termcap.csproj -c Staging",
			pullRequest
		);
		Assert.Contains(
			"dotnet pack Icod.TermInfo.Termcap/Icod.TermInfo.Termcap.csproj -c Release",
			pushMain
		);
		Assert.DoesNotContain(
			"Icod.TermInfo.Termcap/Icod.TermInfo.Termcap.csproj",
			release
		);
	}

	[Fact]
	public void TermcapAssemblyAdvancesPackageVersionWithoutChangingAssemblyIdentity() {
		Assembly assembly =
			typeof( TermcapSourceParser ).Assembly;
		Assert.Equal(
			new Version( 1, 0, 0, 0 ),
			assembly.GetName().Version
		);

		string? informationalVersion =
			assembly
				.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
				?.InformationalVersion;
		Assert.NotNull( informationalVersion );
		Assert.Equal(
			DevelopmentVersion,
			informationalVersion!
				.Split(
					'+',
					2
				)[0]
		);
	}

	[Fact]
	public void TC01ModelDoesNotExposeResolvedTerminalDescription() {
		Type[] modelTypes =
		[
			typeof( TermcapSourceDocument ),
			typeof( TermcapSourceEntry ),
			typeof( TermcapSourceField ),
			typeof( TermcapSourceParseResult ),
		];

		Assert.DoesNotContain(
			modelTypes
				.SelectMany(
					type =>
						type.GetProperties(
							BindingFlags.Public
							| BindingFlags.Instance
						)
				)
				.SelectMany(
					property => FlattenType( property.PropertyType )
				),
			type => type == typeof( TerminalDescription )
		);
	}

	[Fact]
	public void DetailedRoadmapAndTC01ImplementationRecordArePresent() {
		string root = FindRepositoryRoot();
		string roadmapPath =
			Path.Combine(
				root,
				"Icod.TermInfo-1.6.0-Termcap-Interoperability-Roadmap.md"
			);
		string implementationPath =
			Path.Combine(
				root,
				"docs",
				"1.6.0-TC01-TERMCAP-PACKAGE-AND-PARSER-FOUNDATION.md"
			);

		Assert.True( File.Exists( roadmapPath ) );
		Assert.True( File.Exists( implementationPath ) );
		Assert.Contains(
			"Icod.TermInfo.Termcap",
			File.ReadAllText( roadmapPath )
		);
		Assert.Contains(
			"1.6.0-Alpha-1",
			File.ReadAllText( implementationPath )
		);
	}

	private static IEnumerable<Type> FlattenType(
		Type type
	) {
		ArgumentNullException.ThrowIfNull( type );

		yield return type;
		if ( type.IsGenericType ) {
			foreach ( Type argument in type.GetGenericArguments() ) {
				foreach ( Type nested in FlattenType( argument ) ) {
					yield return nested;
				}
			}
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
			new(
				AppContext.BaseDirectory
			);

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
			"Unable to locate the Icod.TermInfo repository root."
		);
	}
}
