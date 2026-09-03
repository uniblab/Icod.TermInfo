using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class MI01JsonContractTests {
	private const string DevelopmentVersion = "1.9.0-Alpha-1";
	private const string PublishedBaseCommit =
		"d07d923aeec758f00c4e2025fe79d6d2f97fbe83";
	private const string OneEightBaselineSha256 =
		"12e31674f63ed9483a7261fc7f7214c390df9c6025a78dc8ae83aa3b01ea2bcc";

	[Fact]
	public void JsonOptionsFreezeBoundedDefaults() {
		TermInfoJsonRendererOptions options =
			new();

		Assert.Equal( 4_194_304, options.MaximumOutputByteCount );
		Assert.False( options.WriteIndented );
		Assert.Equal(
			4_194_304,
			TermInfoJsonRendererOptions.DefaultMaximumOutputByteCount
		);
		Assert.Equal(
			67_108_864,
			TermInfoJsonRendererOptions.MaximumSupportedOutputByteCount
		);
	}

	[Fact]
	public void JsonOptionsRetainExplicitImmutablePolicy() {
		TermInfoJsonRendererOptions options =
			new(
				8_192,
				writeIndented: true
			);

		Assert.Equal( 8_192, options.MaximumOutputByteCount );
		Assert.True( options.WriteIndented );
		Assert.DoesNotContain(
			typeof( TermInfoJsonRendererOptions ).GetProperties(),
			property => property.SetMethod is not null
		);
		Assert.DoesNotContain(
			typeof( TermInfoJsonRendererOptions ).GetProperties(),
			property =>
				typeof( Delegate ).IsAssignableFrom( property.PropertyType )
		);
	}

	[Fact]
	public void JsonOptionsRejectInvalidOutputBounds() {
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new TermInfoJsonRendererOptions( 0 )
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() => new TermInfoJsonRendererOptions( -1 )
		);
		Assert.Throws<ArgumentOutOfRangeException>(
			() =>
				new TermInfoJsonRendererOptions(
					TermInfoJsonRendererOptions.MaximumSupportedOutputByteCount + 1
				)
		);

		TermInfoJsonRendererOptions minimum =
			new( 1 );
		TermInfoJsonRendererOptions maximum =
			new(
				TermInfoJsonRendererOptions.MaximumSupportedOutputByteCount
			);
		Assert.Equal( 1, minimum.MaximumOutputByteCount );
		Assert.Equal(
			67_108_864,
			maximum.MaximumOutputByteCount
		);
	}

	[Fact]
	public void JsonRendererFreezesSchemaIdentityAndTypedSurface() {
		Assert.Equal(
			"urn:icod:terminfo:inspection:json:1",
			TermInfoJsonRenderer.SchemaIdentifier
		);
		Assert.Equal( 1, TermInfoJsonRenderer.SchemaVersion );

		MethodInfo[] renderMethods =
			typeof( TermInfoJsonRenderer )
				.GetMethods(
					BindingFlags.Public
						| BindingFlags.Static
						| BindingFlags.DeclaredOnly
				)
				.Where( method => method.Name == "Render" )
				.ToArray();
		Assert.Equal( 8, renderMethods.Length );
		Assert.All(
			renderMethods,
			method => Assert.Equal( typeof( string ), method.ReturnType )
		);
		Type[] oneParameterTypes =
			renderMethods
				.Where( method => method.GetParameters().Length == 1 )
				.Select( method => method.GetParameters()[ 0 ].ParameterType )
				.ToArray();
		Assert.Equal( 4, oneParameterTypes.Length );
		Assert.Contains( typeof( TerminalDescription ), oneParameterTypes );
		Assert.Contains( typeof( TermInfoComparisonResult ), oneParameterTypes );
		Assert.Contains( typeof( TerminalDescriptionSourcePlan ), oneParameterTypes );
		Assert.Contains( typeof( TermInfoDatabaseCatalog ), oneParameterTypes );
		Assert.DoesNotContain(
			renderMethods,
			method => method.GetParameters()[ 0 ].ParameterType == typeof( object )
		);
	}

	[Fact]
	public void JsonRendererRejectsNullValuesAndExplicitOptions() {
		TerminalDescription description =
			CreateDescription();
		TermInfoComparisonResult comparison =
			TerminalDescriptionComparer.Compare(
				description,
				description
			);
		TerminalDescriptionSourcePlan plan =
			CreatePlan( description );
		TermInfoDatabaseCatalog catalog =
			CreateMissingCatalog();

		Assert.Throws<ArgumentNullException>(
			() => TermInfoJsonRenderer.Render( (TerminalDescription)null! )
		);
		Assert.Throws<ArgumentNullException>(
			() => TermInfoJsonRenderer.Render( (TermInfoComparisonResult)null! )
		);
		Assert.Throws<ArgumentNullException>(
			() => TermInfoJsonRenderer.Render( (TerminalDescriptionSourcePlan)null! )
		);
		Assert.Throws<ArgumentNullException>(
			() => TermInfoJsonRenderer.Render( (TermInfoDatabaseCatalog)null! )
		);
		Assert.Throws<ArgumentNullException>(
			() => TermInfoJsonRenderer.Render( description, null! )
		);
		Assert.Throws<ArgumentNullException>(
			() => TermInfoJsonRenderer.Render( comparison, null! )
		);
		Assert.Throws<ArgumentNullException>(
			() => TermInfoJsonRenderer.Render( plan, null! )
		);
		Assert.Throws<ArgumentNullException>(
			() => TermInfoJsonRenderer.Render( catalog, null! )
		);
	}

	[Fact]
	public void JsonRendererObservesCancellationBeforeOperationalBoundary() {
		TerminalDescription description =
			CreateDescription();
		TermInfoComparisonResult comparison =
			TerminalDescriptionComparer.Compare(
				description,
				description
			);
		TerminalDescriptionSourcePlan plan =
			CreatePlan( description );
		TermInfoDatabaseCatalog catalog =
			CreateMissingCatalog();
		TermInfoJsonRendererOptions options =
			new();
		using var source =
			new CancellationTokenSource();
		source.Cancel();

		Assert.Throws<OperationCanceledException>(
			() => TermInfoJsonRenderer.Render( description, options, source.Token )
		);
		Assert.Throws<OperationCanceledException>(
			() => TermInfoJsonRenderer.Render( comparison, options, source.Token )
		);
		Assert.Throws<OperationCanceledException>(
			() => TermInfoJsonRenderer.Render( plan, options, source.Token )
		);
		Assert.Throws<OperationCanceledException>(
			() => TermInfoJsonRenderer.Render( catalog, options, source.Token )
		);
	}

	[Fact]
	public void JsonRendererMethodsRespectCurrentOperationalTranches() {
		TerminalDescription description =
			CreateDescription();
		TermInfoComparisonResult comparison =
			TerminalDescriptionComparer.Compare(
				description,
				description
			);
		TerminalDescriptionSourcePlan plan =
			CreatePlan( description );
		TermInfoDatabaseCatalog catalog =
			CreateMissingCatalog();

		Assert.Contains(
			"terminalDescription",
			TermInfoJsonRenderer.Render( description ),
			StringComparison.Ordinal
		);
		Assert.Contains(
			"comparison",
			TermInfoJsonRenderer.Render( comparison ),
			StringComparison.Ordinal
		);
		Assert.Contains(
			"sourcePlan",
			TermInfoJsonRenderer.Render( plan ),
			StringComparison.Ordinal
		);
		Assert.Contains(
			"databaseCatalog",
			TermInfoJsonRenderer.Render( catalog ),
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void Mi01AddsOnlyReviewedInspectionTypes() {
		Type[] exportedTypes =
			typeof( TermInfoJsonRenderer )
				.Assembly
				.GetExportedTypes();

		Assert.InRange( exportedTypes.Length, 31, int.MaxValue );
		Assert.Contains( typeof( TermInfoJsonRenderer ), exportedTypes );
		Assert.Contains( typeof( TermInfoJsonRendererOptions ), exportedTypes );
		Assert.Contains( typeof( TerminalDescriptionSourcePlanner ), exportedTypes );
		Assert.Contains( typeof( TerminalDescriptionSourceSynthesizer ), exportedTypes );
	}

	[Fact]
	public void OneEightInspectionBaselineRemainsImmutableHistoricalEvidence() {
		string baseline =
			File.ReadAllText(
				Path.Combine(
					FindRepositoryRoot(),
					"docs",
					"1.8.0-INSPECTION-PUBLIC-API-BASELINE.txt"
				)
			)
				.Replace( "\r\n", "\n", StringComparison.Ordinal )
				.Replace( '\r', '\n' );
		string sha256 =
			Convert.ToHexString(
				SHA256.HashData(
					Encoding.UTF8.GetBytes( baseline )
				)
			).ToLowerInvariant();

		Assert.Equal( OneEightBaselineSha256, sha256 );
		Assert.Equal(
			29,
			baseline
				.Split( '\n' )
				.Count(
					line => line.StartsWith(
						"TYPE ",
						StringComparison.Ordinal
					)
				)
		);
	}

	[Fact]
	public void Mi01PreservesInspectionDependencyDirection() {
		string projectPath =
			Path.Combine(
				FindRepositoryRoot(),
				"Icod.TermInfo.Inspection",
				"Icod.TermInfo.Inspection.csproj"
			);
		XDocument project =
			XDocument.Load(
				projectPath,
				LoadOptions.None
			);
		string[] references =
			project
				.Descendants()
				.Where( element => element.Name.LocalName == "ProjectReference" )
				.Select( element => (string)element.Attribute( "Include" )! )
				.ToArray();

		Assert.Equal( 2, references.Length );
		Assert.Contains(
			references,
			reference => reference.EndsWith(
				"Icod.TermInfo.csproj",
				StringComparison.OrdinalIgnoreCase
			)
		);
		Assert.Contains(
			references,
			reference => reference.EndsWith(
				"Icod.TermInfo.Source.csproj",
				StringComparison.OrdinalIgnoreCase
			)
		);
		Assert.DoesNotContain(
			project.Descendants(),
			element => element.Name.LocalName == "PackageReference"
		);
	}

	[Fact]
	public void RoadmapAndImplementationRecordFreezeSchemaBoundsAndSequence() {
		string root =
			FindRepositoryRoot();
		string roadmap =
			File.ReadAllText(
				Path.Combine(
					root,
					"Icod.TermInfo-1.9.0-Machine-Readable-Inspection-and-Planning-Automation-Roadmap.md"
				)
			);
		string implementation =
			File.ReadAllText(
				Path.Combine(
					root,
					"docs",
					"1.9.0-MI01-JSON-CONTRACT-AND-RENDERER-FOUNDATION.md"
				)
			);

		foreach (
			string marker
			in new[] {
				"urn:icod:terminfo:inspection:json:1",
				"schemaVersion",
				"documentKind",
				"terminalDescription",
				"comparison",
				"sourcePlan",
				"databaseCatalog",
				"4,194,304",
				"67,108,864",
				"MI01",
				"MI02",
				"MI03",
				"MI04",
				"MI05",
				"MI06",
				"MI07",
			}
		) {
			Assert.Contains( marker, roadmap, StringComparison.Ordinal );
		}

		Assert.Contains( DevelopmentVersion, implementation, StringComparison.Ordinal );
		Assert.Contains( PublishedBaseCommit, implementation, StringComparison.Ordinal );
		Assert.Contains( "v1.8.0", implementation, StringComparison.Ordinal );
		Assert.Contains(
			"does not publish a JSON Schema file containing empty placeholder payloads",
			implementation,
			StringComparison.Ordinal
		);
	}

	private static TerminalDescription CreateDescription() =>
		new TerminalDescriptionBuilder( "mi01-description" )
			.SetDescription( "MI01 JSON contract fixture" )
			.SetBoolean( BooleanCapability.AutoRightMargin )
			.Build();

	private static TerminalDescriptionSourcePlan CreatePlan(
		TerminalDescription description
	) =>
		TerminalDescriptionSourcePlanner.Plan(
			description,
			Array.Empty<TerminalDescriptionSourceSynthesisParent>()
		);

	private static TermInfoDatabaseCatalog CreateMissingCatalog() =>
		TermInfoDatabaseInspector.InspectDirectory(
			Path.Combine(
				System.IO.Path.GetTempPath(),
				$"icod-terminfo-mi01-missing-{Guid.NewGuid():N}"
			)
		);

	private static string FindRepositoryRoot() {
		DirectoryInfo? directory =
			new( AppContext.BaseDirectory );

		while ( directory is not null ) {
			if (
				File.Exists(
					Path.Combine(
						directory.FullName,
						"Icod.TermInfo.sln"
					)
				)
			) {
				return directory.FullName;
			}

			directory = directory.Parent;
		}

		throw new InvalidOperationException(
			"Repository root not found."
		);
	}
}
