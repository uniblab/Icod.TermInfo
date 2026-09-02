using System.Globalization;
using System.Text;
using System.Text.Json;
using Icod.TermInfo;
using Icod.TermInfo.Inspection;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class MI06CrossHostHardeningTests {
	private const string DevelopmentVersion = "1.9.0-Alpha-7";

	[Fact]
	public void LargePathologicalJsonIsCultureInvariantAndExactlyBounded() {
		TerminalDescription description =
			new TerminalDescriptionBuilder( "mi06-large" )
				.AddAlias( "mi06-I\u0130" )
				.SetDescription( "MI06 \u2028 \u2029 \u001b culture" )
				.SetExtendedString(
					"XMI06",
					string.Concat(
						Enumerable.Repeat(
							"I\u0130\u001b\n\\\"",
							16_384
						)
					)
				)
				.Build();
		string expected = TermInfoJsonRenderer.Render( description );

		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
		try {
			foreach ( string cultureName in new[] { "ar-SA", "tr-TR" } ) {
				CultureInfo.CurrentCulture =
					CultureInfo.GetCultureInfo( cultureName );
				CultureInfo.CurrentUICulture =
					CultureInfo.GetCultureInfo( cultureName );
				Assert.Equal(
					expected,
					TermInfoJsonRenderer.Render( description )
				);
			}
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUiCulture;
		}

		using JsonDocument document = JsonDocument.Parse( expected );
		Assert.Equal(
			"terminalDescription",
			document.RootElement.GetProperty( "documentKind" ).GetString()
		);
		int byteCount = Encoding.UTF8.GetByteCount( expected );
		Assert.Equal(
			expected,
			TermInfoJsonRenderer.Render(
				description,
				new TermInfoJsonRendererOptions( byteCount )
			)
		);
		Assert.Throws<InvalidOperationException>(
			() => TermInfoJsonRenderer.Render(
				description,
				new TermInfoJsonRendererOptions( byteCount - 1 )
			)
		);
	}

	[Fact]
	public void SamplesPackageConsumerAndReleaseGatesFreezeMi06Evidence() {
		string root = FindRepositoryRoot();
		string sampleRoot = Path.Combine(
			root,
			"samples",
			"Icod.TermInfo.Toolchain.Sample"
		);
		string fixture = ReadNormalized(
			Path.Combine( sampleRoot, "expected-source-plan.json" )
		);
		using JsonDocument document = JsonDocument.Parse( fixture );
		Assert.Equal(
			"sourcePlan",
			document.RootElement.GetProperty( "documentKind" ).GetString()
		);
		Assert.Equal(
			"icod-toolchain-base",
			document.RootElement
				.GetProperty( "data" )
				.GetProperty( "selectedParentUseNames" )
				.EnumerateArray()
				.Single()
				.GetString()
		);
		Assert.EndsWith( "\n", fixture, StringComparison.Ordinal );
		Assert.DoesNotContain( "\r", fixture, StringComparison.Ordinal );

		string sample = File.ReadAllText(
			Path.Combine( sampleRoot, "Program.cs" )
		);
		Assert.Contains( "TermInfoJsonRenderer.Render( plan )", sample, StringComparison.Ordinal );
		Assert.Contains( "--verify-fixture", sample, StringComparison.Ordinal );

		string packageProject = File.ReadAllText(
			Path.Combine(
				root,
				"tools",
				"inspection-package-smoke",
				"Icod.TermInfo.Inspection.PackageSmoke.csproj"
			)
		);
		string packageConsumer = File.ReadAllText(
			Path.Combine(
				root,
				"tools",
				"inspection-package-smoke",
				"Program.cs"
			)
		);
		Assert.Contains( "PackageReference", packageProject, StringComparison.Ordinal );
		Assert.DoesNotContain( "ProjectReference", packageProject, StringComparison.Ordinal );
		Assert.Contains( "pathologicalTerminal", packageConsumer, StringComparison.Ordinal );
		Assert.Contains( "ar-SA", packageConsumer, StringComparison.Ordinal );
		Assert.Contains( "tr-TR", packageConsumer, StringComparison.Ordinal );

		foreach ( string relativePath in new[] {
			".github/scripts/verify-release-package.sh",
			".github/scripts/verify-release-package.cmd",
		} ) {
			string gate = File.ReadAllText(
				Path.Combine(
					root,
					relativePath.Replace(
						'/',
						Path.DirectorySeparatorChar
					)
				)
			);
			Assert.Contains( "expected-source-plan.json", gate, StringComparison.Ordinal );
			Assert.Contains( "--verify-fixture", gate, StringComparison.Ordinal );
		}
	}

	[Fact]
	public void ImplementationRecordAndDocumentationFreezeMi06Gate() {
		string root = FindRepositoryRoot();
		string implementation = File.ReadAllText(
			Path.Combine(
				root,
				"docs",
				"1.9.0-MI06-SAMPLES-PACKAGE-CONSUMERS-AND-CROSS-HOST-HARDENING.md"
			)
		);
		string roadmap = File.ReadAllText(
			Path.Combine(
				root,
				"Icod.TermInfo-1.9.0-Machine-Readable-Inspection-and-Planning-Automation-Roadmap.md"
			)
		);

		foreach ( string marker in new[] {
			DevelopmentVersion,
			"package-reference-only",
			"culture",
			"separate processes",
			"large and pathological",
			"MI07",
		} ) {
			Assert.Contains( marker, implementation, StringComparison.OrdinalIgnoreCase );
		}
		Assert.Contains( "**Status:** MI07 complete", roadmap, StringComparison.Ordinal );
	}

	private static string ReadNormalized(
		string path
	) =>
		File.ReadAllText( path )
			.Replace( "\r\n", "\n", StringComparison.Ordinal )
			.Replace( '\r', '\n' );

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( AppContext.BaseDirectory );
		while ( current is not null ) {
			if ( File.Exists( Path.Combine( current.FullName, "Icod.TermInfo.sln" ) ) ) {
				return current.FullName;
			}
			current = current.Parent;
		}

		throw new InvalidOperationException( "Could not locate the repository root." );
	}
}
