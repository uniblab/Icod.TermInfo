using System.Reflection;
using System.Xml.Linq;
using Xunit;

namespace Icod.TermInfo.Inspection.Tests;

public sealed class RS08ContractTests {
	private const string ReleaseVersion = "1.7.0";
	private const string CurrentDevelopmentVersion = "1.9.0-Alpha-6";

	[Fact]
	public void CurrentDevelopmentRetainsRs08ReleaseRecords() {
		string root = FindRepositoryRoot();
		XDocument buildProperties = XDocument.Load(
			Path.Combine( root, "Directory.Build.props" ),
			LoadOptions.None
		);
		string version = buildProperties
			.Descendants()
			.Single(
				element => element.Name.LocalName == "IcodTermInfoSuiteVersion"
			)
			.Value
			.Trim();
		string roadmap = File.ReadAllText(
			Path.Combine(
				root,
				"Icod.TermInfo 1.7.0 - Relative Terminfo Source Synthesis Roadmap.md"
			)
		);
		string audit = File.ReadAllText(
			Path.Combine(
				root,
				"docs",
				"1.7.0-RELEASE-AUDIT.md"
			)
		);

		Assert.Equal( CurrentDevelopmentVersion, version );
		Assert.Contains( "RS08", roadmap, StringComparison.Ordinal );
		Assert.Contains( ReleaseVersion, audit, StringComparison.Ordinal );
		Assert.Contains(
			"Stable release commit: pending stable publication",
			audit,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"Stable release tag:    v1.7.0 (pending)",
			audit,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void FrozenOneSevenTypesRemainAvailableWithinAdditiveSurface() {
		Type[] exportedTypes = typeof( TerminalDescriptionComparer )
			.Assembly
			.GetExportedTypes();

		Assert.True( exportedTypes.Length >= 25 );
		Assert.Contains(
			exportedTypes,
			type => type == typeof( TerminalDescriptionSourceSynthesisParent )
		);
		Assert.Contains(
			exportedTypes,
			type => type == typeof( TerminalDescriptionSourceSynthesisOptions )
		);
		Assert.Contains(
			exportedTypes,
			type => type == typeof( TerminalDescriptionSourceSynthesizer )
		);
		Assert.Equal(
			new Version( 1, 0, 0, 0 ),
			typeof( TerminalDescriptionComparer ).Assembly.GetName().Version
		);
	}

	[Fact]
	public void FrozenInspectionBaselineRecordsCompleteSynthesisSurface() {
		string root = FindRepositoryRoot();
		string baseline = File.ReadAllText(
			Path.Combine(
				root,
				"docs",
				"1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt"
			)
		);

		Assert.Contains(
			"TYPE class Icod.TermInfo.Inspection.TerminalDescriptionSourceSynthesisParent [sealed]",
			baseline,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"FIELD public static const System.Int32 DefaultMaximumParentCount null=not-null/not-null value=64",
			baseline,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"FIELD public static const System.Int32 MaximumSupportedParentCount null=not-null/not-null value=256",
			baseline,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"System.Boolean includeExtendedCapabilities null=not-null/not-null",
			baseline,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"METHOD public static System.String Synthesize(",
			baseline,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"METHOD public static System.Void Write(",
			baseline,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void ReusablePackageDependencyDirectionRemainsFrozen() {
		string root = FindRepositoryRoot();
		string[] inspection = ReadProjectReferences(
			root,
			"Icod.TermInfo.Inspection/Icod.TermInfo.Inspection.csproj"
		);
		string[] source = ReadProjectReferences(
			root,
			"Icod.TermInfo.Source/Icod.TermInfo.Source.csproj"
		);
		string[] compiler = ReadProjectReferences(
			root,
			"Icod.TermInfo.Compiler/Icod.TermInfo.Compiler.csproj"
		);
		string[] termcap = ReadProjectReferences(
			root,
			"Icod.TermInfo.Termcap/Icod.TermInfo.Termcap.csproj"
		);

		Assert.Contains( @"..\Icod.TermInfo.csproj", inspection );
		Assert.Contains(
			@"..\Icod.TermInfo.Source\Icod.TermInfo.Source.csproj",
			inspection
		);
		Assert.DoesNotContain(
			inspection,
			reference => reference.Contains( "Compiler", StringComparison.Ordinal )
		);
		Assert.DoesNotContain(
			inspection,
			reference => reference.Contains( "Termcap", StringComparison.Ordinal )
		);

		Assert.Contains( @"..\Icod.TermInfo.csproj", source );
		Assert.DoesNotContain(
			source,
			reference => reference.Contains( "Compiler", StringComparison.Ordinal )
		);
		Assert.DoesNotContain(
			source,
			reference => reference.Contains( "Inspection", StringComparison.Ordinal )
		);

		Assert.Contains( @"..\Icod.TermInfo.csproj", compiler );
		Assert.Contains(
			@"..\Icod.TermInfo.Source\Icod.TermInfo.Source.csproj",
			compiler
		);

		Assert.Contains( @"..\Icod.TermInfo.csproj", termcap );
		Assert.DoesNotContain(
			termcap,
			reference => reference.Contains( "Source", StringComparison.Ordinal )
		);
		Assert.DoesNotContain(
			termcap,
			reference => reference.Contains( "Compiler", StringComparison.Ordinal )
		);
		Assert.DoesNotContain(
			termcap,
			reference => reference.Contains( "Inspection", StringComparison.Ordinal )
		);
	}

	[Fact]
	public void ReleaseVerifiersEnforceFrozenInspectionBaseline() {
		string root = FindRepositoryRoot();
		string shell = File.ReadAllText(
			Path.Combine(
				root,
				".github",
				"scripts",
				"verify-release-package.sh"
			)
		);
		string command = File.ReadAllText(
			Path.Combine(
				root,
				".github",
				"scripts",
				"verify-release-package.cmd"
			)
		);

		Assert.Contains(
			"docs/1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt",
			shell,
			StringComparison.Ordinal
		);
		Assert.Contains(
			@"docs\1.7.0-INSPECTION-PUBLIC-API-BASELINE.txt",
			command,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void ClosureGatesExerciseFrozenSynthesisComposition() {
		string root = FindRepositoryRoot();
		string packageSmoke = File.ReadAllText(
			Path.Combine(
				root,
				"tools",
				"inspection-package-smoke",
				"Program.cs"
			)
		);
		string sample = File.ReadAllText(
			Path.Combine(
				root,
				"samples",
				"Icod.TermInfo.Toolchain.Sample",
				"Program.cs"
			)
		);
		string archiveSmoke = File.ReadAllText(
			Path.Combine(
				root,
				".github",
				"scripts",
				"smoke-tool-archive.ps1"
			)
		);

		Assert.Contains(
			"TerminalDescriptionSourceSynthesisOptions.DefaultMaximumParentCount",
			packageSmoke,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"includeExtendedCapabilities: true",
			packageSmoke,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"icod-toolchain-child|Toolchain sample child",
			sample,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"includeExtendedCapabilities: true",
			sample,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"TerminalDescriptionSourcePlanner.Plan",
			sample,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"planningCandidates",
			sample,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"'infocmp'",
			archiveSmoke,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"'release-child'",
			archiveSmoke,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"'release-base'",
			archiveSmoke,
			StringComparison.Ordinal
		);
	}

	[Fact]
	public void ReleaseAuditRecordsDifferentialAndDistributionEvidence() {
		string root = FindRepositoryRoot();
		string audit = File.ReadAllText(
			Path.Combine(
				root,
				"docs",
				"1.7.0-RELEASE-AUDIT.md"
			)
		);

		Assert.Contains( "ncurses      6.5.20250216", audit, StringComparison.Ordinal );
		Assert.Contains( "TermInfoSourceParser", audit, StringComparison.Ordinal );
		Assert.Contains( "TermInfoSourceCompiler", audit, StringComparison.Ordinal );
		Assert.Contains( "Icod.TermInfo.Tools", audit, StringComparison.Ordinal );
		Assert.Contains( "win-arm64", audit, StringComparison.Ordinal );
		Assert.Contains( "linux-arm64", audit, StringComparison.Ordinal );
		Assert.Contains( "osx-arm64", audit, StringComparison.Ordinal );
	}

	private static string[] ReadProjectReferences(
		string root,
		string relativePath
	) {
		ArgumentException.ThrowIfNullOrWhiteSpace( root );
		ArgumentException.ThrowIfNullOrWhiteSpace( relativePath );

		XDocument project = XDocument.Load(
			Path.Combine(
				root,
				relativePath.Replace(
					'/',
					Path.DirectorySeparatorChar
				)
			),
			LoadOptions.None
		);
		return project
			.Descendants()
			.Where(
				element => element.Name.LocalName == "ProjectReference"
			)
			.Select(
				element => element.Attribute( "Include" )?.Value ?? string.Empty
			)
			.OrderBy(
				reference => reference,
				StringComparer.Ordinal
			)
			.ToArray();
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current = new( AppContext.BaseDirectory );
		while ( current is not null ) {
			if ( File.Exists( Path.Combine( current.FullName, "Icod.TermInfo.sln" ) ) ) {
				return current.FullName;
			}
			current = current.Parent;
		}

		throw new DirectoryNotFoundException(
			"Could not locate the repository root."
		);
	}
}
