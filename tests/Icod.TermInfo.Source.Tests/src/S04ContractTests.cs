using System.Reflection;
using System.Xml.Linq;
using Icod.TermInfo;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Source.Tests;

public sealed class S04ContractTests {
	private const string DevelopmentVersion = "1.3.0-Alpha-6";
	private const string StableAssemblyVersion = "1.0.0.0";

	[Fact]	public void SourceAndRuntimePackagesAdvanceTogetherWithoutChangingAssemblyIdentity() {
		string root = FindRepositoryRoot();

		foreach (
			string relativePath
			in new[]
			{
				"Icod.TermInfo.csproj",
				"Icod.TermInfo.Source/Icod.TermInfo.Source.csproj",
			} ) {
			XDocument project =
				XDocument.Load(
					Path.Combine(
						root,
						relativePath.Replace(
							'/',
							Path.DirectorySeparatorChar ) ),
					LoadOptions.None );

			Assert.Equal(
				DevelopmentVersion,
				ReadRequiredProperty(
					project,
					"Version" ) );
			Assert.Equal(
				DevelopmentVersion,
				ReadRequiredProperty(
					project,
					"PackageVersion" ) );
			Assert.Equal(
				StableAssemblyVersion,
				ReadRequiredProperty(
					project,
					"AssemblyVersion" ) );
		}
	}

	[Fact]
	public void SourcePublicSurfaceIncludesReviewedUnresolvedModelTypes() {
		Assembly assembly =
			typeof( TermInfoSourceParser ).Assembly;
		string[] exportedTypes =
			assembly
				.GetExportedTypes()
				.Select( type => type.FullName! )
				.ToArray();

		Assert.Contains(
			"Icod.TermInfo.Source.TermInfoSourceDocument",
			exportedTypes
		);
		Assert.Contains(
			"Icod.TermInfo.Source.TermInfoSourceEntry",
			exportedTypes
		);
		Assert.Contains(
			"Icod.TermInfo.Source.TermInfoSourceField",
			exportedTypes
		);
		Assert.Contains(
			"Icod.TermInfo.Source.TermInfoSourceFieldKind",
			exportedTypes
		);
		Assert.Contains(
			"Icod.TermInfo.Source.TermInfoSourceParseResult",
			exportedTypes
		);
		Assert.Contains(
			"Icod.TermInfo.Source.TermInfoSourceParser",
			exportedTypes
		);
		Assert.Equal(
			new Version( 1, 0, 0, 0 ),
			assembly.GetName().Version
		);
	}

	[Fact]
	public void UnresolvedModelDoesNotExposeTerminalDescriptionState() {
		Type[] modelTypes =
		[
			typeof(TermInfoSourceDocument),
			typeof(TermInfoSourceEntry),
			typeof(TermInfoSourceField),
			typeof(TermInfoSourceParseResult),
		];

		Assert.DoesNotContain(
			modelTypes
				.SelectMany(
					type =>
						type.GetProperties(
							BindingFlags.Public
							| BindingFlags.Instance ) )
				.SelectMany(
					property =>
						FlattenType( property.PropertyType ) ),
			type => type == typeof( TerminalDescription ) );
	}

	[Fact]
	public void SourcePublicApiBaselineIncludesUnresolvedModelContract() {
		string root = FindRepositoryRoot();
		string baseline =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.1.0-SOURCE-PUBLIC-API-BASELINE.txt" ) );

		Assert.Contains(
			"TYPE class Icod.TermInfo.Source.TermInfoSourceDocument [sealed]",
			baseline );
		Assert.Contains(
			"TYPE class Icod.TermInfo.Source.TermInfoSourceEntry [sealed]",
			baseline );
		Assert.Contains(
			"TYPE class Icod.TermInfo.Source.TermInfoSourceField [sealed]",
			baseline );
		Assert.Contains(
			"TYPE enum Icod.TermInfo.Source.TermInfoSourceFieldKind [sealed]",
			baseline );
		Assert.Contains(
			"TYPE class Icod.TermInfo.Source.TermInfoSourceParseResult [sealed]",
			baseline );
		Assert.Contains(
			"TYPE class Icod.TermInfo.Source.TermInfoSourceParser [static]",
			baseline );
	}

	[Fact]
	public void S04ImplementationRecordAndRoadmapLinkArePresent() {
		string root = FindRepositoryRoot();
		string recordPath =
			Path.Combine(
				root,
				"docs",
				"1.1.0-S04-UNRESOLVED-SOURCE-ENTRY-MODEL.md" );
		string roadmap =
			File.ReadAllText(
				Path.Combine(
					root,
					"Icod.TermInfo-Post-1.0-Development-Roadmap.md" ) );

		Assert.True( File.Exists( recordPath ) );
		string record =
			File.ReadAllText( recordPath );
		Assert.Contains( "1.1.0-Alpha-4", record );
		Assert.Contains( "TermInfoSourceDocument", record );
		Assert.Contains( "TermInfoSourceEntry", record );
		Assert.Contains( "TermInfoSourceField", record );
		Assert.Contains( "S05", record );
		Assert.Contains(
			"1.1.0-S04-UNRESOLVED-SOURCE-ENTRY-MODEL.md",
			roadmap );
	}

	private static IEnumerable<Type> FlattenType(
		Type type ) {
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
