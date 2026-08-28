using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Icod.TermInfo.Source;
using Xunit;

namespace Icod.TermInfo.Source.Tests;

public sealed class S06ContractTests {
	private const string DevelopmentVersion = "1.4.0-Alpha-6";
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
	public void CancellationStateRemainsInternalSourceResolutionMachinery() {
		Assembly assembly =
			typeof( TermInfoSourceParser ).Assembly;

		Assert.DoesNotContain(
			assembly.GetExportedTypes(),
			type =>
				type.FullName
					== "Icod.TermInfo.Source.TermInfoSourceCapabilityState" );
		Assert.Contains(
			assembly.GetCustomAttributes<InternalsVisibleToAttribute>(),
			attribute =>
				attribute.AssemblyName
					== "Icod.TermInfo.Source.Tests" );

		string root = FindRepositoryRoot();
		string baseline =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.1.0-SOURCE-PUBLIC-API-BASELINE.txt" ) );
		Assert.DoesNotContain(
			"TermInfoSourceCapabilityState",
			baseline );
	}

	[Fact]
	public void S06ImplementationRecordAndRoadmapLinkArePresent() {
		string root = FindRepositoryRoot();
		string recordPath =
			Path.Combine(
				root,
				"docs",
				"1.1.0-S06-CANCELLATION-SEMANTICS.md" );
		string roadmap =
			File.ReadAllText(
				Path.Combine(
					root,
					"Icod.TermInfo-Post-1.0-Development-Roadmap.md" ) );

		Assert.True( File.Exists( recordPath ) );
		string record =
			File.ReadAllText( recordPath );
		Assert.Contains( "1.1.0-Alpha-6", record );
		Assert.Contains( "capability@", record );
		Assert.True(
			record.Contains(
				"tombstone",
				StringComparison.OrdinalIgnoreCase ) );
		Assert.True(
			record.Contains(
				"rightmost",
				StringComparison.OrdinalIgnoreCase ) );
		Assert.Contains( "S07", record );
		Assert.Contains(
			"1.1.0-S06-CANCELLATION-SEMANTICS.md",
			roadmap );
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
