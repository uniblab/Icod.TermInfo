using System.Xml.Linq;
using Icod.TermInfo;
using Icod.TermInfo.Termcap;
using Xunit;

namespace Icod.TermInfo.Termcap.Tests;

public sealed class TC02ContractTests
{
	private const string Tc02DevelopmentVersion = "1.6.0-Alpha-2";

	[Fact]
	public void Tc02VersionAndCentralVersionWiringRemainRecorded() {
		string root = FindRepositoryRoot();
		XDocument termcapProject =
			XDocument.Load(
				Path.Combine(
					root,
					"Icod.TermInfo.Termcap",
					"Icod.TermInfo.Termcap.csproj"
				),
				LoadOptions.None
			);

		string implementation =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.6.0-TC02-CAPABILITY-METADATA-AND-CLASSIFICATION.md"
				)
			);

		Assert.Contains( Tc02DevelopmentVersion, implementation );
		Assert.Equal(
			"$(IcodTermInfoSuiteVersion)",
			ReadRequiredProperty(
				termcapProject,
				"Version"
			)
		);
		Assert.Equal(
			"$(IcodTermInfoSuiteVersion)",
			ReadRequiredProperty(
				termcapProject,
				"PackageVersion"
			)
		);
	}

	[Fact]
	public void CanonicalMappingsAreDerivedFromEveryRuntimeStandardCapability() {
		int expectedCanonicalCount =
			StandardCapabilityCatalog.BooleanCapabilities.Count
			+ StandardCapabilityCatalog.NumericCapabilities.Count
			+ StandardCapabilityCatalog.StringCapabilities.Count;
		Assert.Equal(
			expectedCanonicalCount,
			TermcapCapabilityCatalog.Mappings.Count(
				mapping => !mapping.IsObsoleteAlias
			)
		);

		foreach (
			StandardCapabilityMetadata<BooleanCapability> metadata
			in StandardCapabilityCatalog.BooleanCapabilities
		) {
			Assert.Contains(
				TermcapCapabilityCatalog.GetMappings( metadata.TermcapCode ),
				mapping =>
					!mapping.IsObsoleteAlias
					&& mapping.ValueKind == metadata.Kind
					&& mapping.BinaryIndex == metadata.BinaryIndex
					&& mapping.TermInfoShortName == metadata.ShortName
					&& mapping.TermInfoLongName == metadata.LongName
					&& mapping.BooleanCapability == metadata.Capability
			);
		}
		foreach (
			StandardCapabilityMetadata<NumericCapability> metadata
			in StandardCapabilityCatalog.NumericCapabilities
		) {
			Assert.Contains(
				TermcapCapabilityCatalog.GetMappings( metadata.TermcapCode ),
				mapping =>
					!mapping.IsObsoleteAlias
					&& mapping.ValueKind == metadata.Kind
					&& mapping.BinaryIndex == metadata.BinaryIndex
					&& mapping.TermInfoShortName == metadata.ShortName
					&& mapping.TermInfoLongName == metadata.LongName
					&& mapping.NumericCapability == metadata.Capability
			);
		}
		foreach (
			StandardCapabilityMetadata<StringCapability> metadata
			in StandardCapabilityCatalog.StringCapabilities
		) {
			Assert.Contains(
				TermcapCapabilityCatalog.GetMappings( metadata.TermcapCode ),
				mapping =>
					!mapping.IsObsoleteAlias
					&& mapping.ValueKind == metadata.Kind
					&& mapping.BinaryIndex == metadata.BinaryIndex
					&& mapping.TermInfoShortName == metadata.ShortName
					&& mapping.TermInfoLongName == metadata.LongName
					&& mapping.StringCapability == metadata.Capability
			);
		}
	}

	[Fact]
	public void AdoptedObsoleteAliasBaselineIsExplicitAndResolvesThroughCanonicalCodes() {
		(string Alias, string Canonical, string Origin)[] expected =
		[
			( "BO", "mr", "AT&T" ),
			( "CI", "vi", "AT&T" ),
			( "CV", "ve", "AT&T" ),
			( "DS", "mh", "AT&T" ),
			( "EE", "me", "AT&T" ),
			( "FE", "LF", "AT&T" ),
			( "FL", "LO", "AT&T" ),
			( "XS", "mk", "AT&T" ),
			( "EN", "@7", "XENIX" ),
			( "GE", "ae", "XENIX" ),
			( "GS", "as", "XENIX" ),
			( "HM", "kh", "XENIX" ),
			( "LD", "kL", "XENIX" ),
			( "PD", "kN", "XENIX" ),
			( "PN", "po", "XENIX" ),
			( "PS", "pf", "XENIX" ),
			( "PU", "kP", "XENIX" ),
			( "RT", "@8", "XENIX" ),
			( "UP", "ku", "XENIX" ),
			( "KA", "k;", "Tektronix" ),
			( "KB", "F1", "Tektronix" ),
			( "KC", "F2", "Tektronix" ),
			( "KD", "F3", "Tektronix" ),
			( "KE", "F4", "Tektronix" ),
			( "KF", "F5", "Tektronix" ),
			( "BC", "Sb", "Tektronix" ),
			( "FC", "Sf", "Tektronix" ),
			( "HS", "mh", "IRIX" ),
		];

		string[] actualAliasCodes =
			TermcapCapabilityCatalog.Mappings
				.Where(
					mapping => mapping.IsObsoleteAlias
				)
				.Select(
					mapping => mapping.TermcapCode
				)
				.Distinct(
					StringComparer.Ordinal
				)
				.OrderBy(
					code => code,
					StringComparer.Ordinal
				)
				.ToArray();
		Assert.Equal(
			expected
				.Select( item => item.Alias )
				.OrderBy(
					code => code,
					StringComparer.Ordinal
				)
				.ToArray(),
			actualAliasCodes
		);

		foreach (
			(string alias, string canonical, string origin)
			in expected
		) {
			TermcapStandardCapabilityMapping[] mappings =
				TermcapCapabilityCatalog.GetMappings( alias )
					.Where(
						mapping => mapping.IsObsoleteAlias
					)
					.ToArray();
			Assert.NotEmpty( mappings );
			Assert.All(
				mappings,
				mapping =>
				{
					Assert.Equal( canonical, mapping.CanonicalTermcapCode );
					Assert.Equal( origin, mapping.AliasOrigin );
				}
			);
		}
	}

	[Fact]
	public void MappingOrderAndAmbiguityAreDeterministic() {
		string[] orderedCodes =
			TermcapCapabilityCatalog.Mappings
				.Select(
					mapping => mapping.TermcapCode
				)
				.ToArray();
		Assert.Equal(
			orderedCodes
				.OrderBy(
					code => code,
					StringComparer.Ordinal
				)
				.ToArray(),
			orderedCodes
		);

		foreach (
			IGrouping<string, TermcapStandardCapabilityMapping> group
			in TermcapCapabilityCatalog.Mappings.GroupBy(
				mapping => mapping.TermcapCode,
				StringComparer.Ordinal
			)
		) {
			Assert.Equal(
				group.ToArray(),
				TermcapCapabilityCatalog.GetMappings( group.Key ).ToArray()
			);
		}
	}

	[Fact]
	public void TC02DocumentationFreezesTheClassificationBoundary() {
		string root = FindRepositoryRoot();
		string roadmap =
			File.ReadAllText(
				Path.Combine(
					root,
					"Icod.TermInfo-1.6.0-Termcap-Interoperability-Roadmap.md"
				)
			);
		string implementationPath =
			Path.Combine(
				root,
				"docs",
				"1.6.0-TC02-CAPABILITY-METADATA-AND-CLASSIFICATION.md"
			);

		Assert.True( File.Exists( implementationPath ) );
		Assert.Contains( Tc02DevelopmentVersion, File.ReadAllText( implementationPath ) );
		Assert.Contains( "TermcapCapabilityCatalog", roadmap );
		Assert.Contains( "TermcapCapabilityClassifier", roadmap );
		Assert.Contains( "No conversion occurs merely because a field is classified.", roadmap );
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
