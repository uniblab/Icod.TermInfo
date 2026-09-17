# HDB08 Packaging and Cross-platform Qualification Implementation Plan

**Execution status:** COMPLETE / ACCEPTED
**Accepted implementation/qualification head:**
`b25733851c963585b56fcf064e9e27c6fdd47ee5`
**Accepted qualification runs:** normal `35171310997` (12/12), HDB00
`35171310971` (3/3)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Freeze the accepted HDB07C behavior as `1.15.0-Alpha-8` with exact C# package verification and package-only net8/net9/net10 consumption on Windows, Linux, and macOS.

**Architecture:** Linux produces and exactly verifies one canonical coordinated package set. Windows, Linux, and macOS consume those same packages through the existing isolated BerkeleyDb consumer, while the established matching-host six-RID archive matrix remains authoritative. A focused net10 C# verifier owns nupkg/snupkg parsing; the PowerShell 5.1-compatible script remains only the stable orchestration entry point.

**Tech Stack:** C# 13; .NET 8/9/10; xUnit 2.9; `System.IO.Compression`, `System.Reflection`, `System.Reflection.PortableExecutable`, and `System.Xml.Linq`; Windows PowerShell 5.1-compatible PowerShell; cmd/sh; GitHub Actions.

**Spec:** `docs/superpowers/specs/2026-09-17-hdb08-packaging-cross-platform-qualification-design.md`

## Global Constraints

- Advance the coordinated version only from `1.15.0-Alpha-7` to exactly `1.15.0-Alpha-8`; HDB09 owns stable promotion.
- Keep reusable assembly versions exactly `1.0.0.0` and public API equivalent across net8/net9/net10.
- Add no public type/member/option/exception, command switch, diagnostic ID, JSON field/schema, or package dependency.
- Preserve dependency direction: `Icod.TermInfo.BerkeleyDb --> Icod.TermInfo`; Runtime and Inspection remain independent of BerkeleyDb.
- Keep production pure managed and read-only: no P/Invoke, native asset, runtime download, database writer, environment, transaction, recovery, or repair path.
- Keep the accepted HDB07C lookup, catalog, acquisition, parsing, exception, and residual-race behavior unchanged.
- Produce one canonical package set and consume it on Windows, Linux, and macOS; do not independently pack three candidate sets.
- Preserve the existing six archive RIDs and their matching-host smoke.
- New work is limited to C#, cmd/sh, Windows PowerShell 5.1-compatible PowerShell, and GitHub Actions YAML; add no Python, C, or C++ source, inline program, or dependency.
- Generated packages, symbols, archives, extracted assemblies, and fixtures remain temporary or workflow artifacts and are not committed.
- Use independently committed and observed RED-to-GREEN checkpoints.
- Run the normal PR workflow for every pushed checkpoint; let HDB00 retain its existing synchronize-delta classifier.
- Require normal 12/12 and HDB00 3/3 on the exact Alpha-8 implementation head and documentation-complete head.
- Keep PR #45 open, draft, and unmerged; do not publish or tag.

---

## File structure and ownership

| Path | Responsibility |
| --- | --- |
| `tools/berkeleydb-package-verifier/Icod.TermInfo.BerkeleyDb.PackageVerifier.csproj` | Non-packable net10 maintenance executable with no package dependency. |
| `tools/berkeleydb-package-verifier/Program.cs` | Exact nupkg/snupkg, dependency, assembly, portable-PDB, Source Link, metadata, and no-native-asset verification. |
| `tools/berkeleydb-package-verifier/README.md` | Maintainer command and verification boundary. |
| `.github/scripts/verify-berkeleydb-package.ps1` | PowerShell 5.1-compatible orchestration of the C# verifier and existing public-API comparisons. |
| `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb08PackagingQualificationTests.cs` | Permanent verifier, workflow-topology, version-authority, and documentation contracts. |
| `.github/workflows/pull-request.yaml` | Canonical Staging artifacts plus three-host package consumption and six-RID archive smoke. |
| `.github/workflows/main.yaml` | Canonical Release artifacts plus three-host package consumption and six-RID archive smoke after merge. |
| `.github/workflows/release.yaml` | Tagged Release artifacts; publication remains gated on three-host package and six-RID archive smoke. |
| `Directory.Build.props` | Single `1.15.0-Alpha-8` version authority. |
| `Icod.TermInfo.BerkeleyDb/Icod.TermInfo.BerkeleyDb.csproj` | Exact Alpha-8 package release notes; no API/dependency change. |
| `docs/1.15.0-HDB08-PACKAGING-AND-CROSS-PLATFORM-QUALIFICATION.md` | Final exact-head acceptance record. |
| `docs/1.15.0-HDB08-QUALIFICATION-CANDIDATE.md` | Pre-qualification scope and evidence checklist without premature acceptance. |

---

### Task 1: Exact package-verifier RED

**Files:**
- Create: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb08PackagingQualificationTests.cs`

**Interfaces:**
- Consumes: repository-root discovery and xUnit conventions already used by `Hdb01ContractTests`.
- Produces: permanent static contracts for the C# verifier project, program, solution ownership, and PowerShell orchestration entry point.

- [x] **Step 1: Add the verifier-ownership contract test**

Create `Hdb08PackagingQualificationTests` with the repository copyright/license header and this first test/helper shape:

```csharp
using Xunit;

namespace Icod.TermInfo.BerkeleyDb.Tests;

public sealed class Hdb08PackagingQualificationTests {
	[Fact]
	public void ExactPackageVerificationIsOwnedByManagedTool() {
		string root = FindRepositoryRoot();
		string projectPath =
			Path.Combine(
				root,
				"tools",
				"berkeleydb-package-verifier",
				"Icod.TermInfo.BerkeleyDb.PackageVerifier.csproj"
			);
		string programPath =
			Path.Combine(
				root,
				"tools",
				"berkeleydb-package-verifier",
				"Program.cs"
			);

		Assert.True( File.Exists( projectPath ) );
		Assert.True( File.Exists( programPath ) );

		string project = File.ReadAllText( projectPath );
		string program = File.ReadAllText( programPath );
		string solution =
			File.ReadAllText( Path.Combine( root, "Icod.TermInfo.sln" ) );
		string wrapper =
			File.ReadAllText(
				Path.Combine(
					root,
					".github",
					"scripts",
					"verify-berkeleydb-package.ps1"
				)
			);

		Assert.Contains(
			"<TargetFramework>net10.0</TargetFramework>",
			project,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"<IsPackable>false</IsPackable>",
			project,
			StringComparison.Ordinal
		);
		Assert.DoesNotContain(
			"<PackageReference ",
			project,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"tools\\berkeleydb-package-verifier\\Icod.TermInfo.BerkeleyDb.PackageVerifier.csproj",
			solution,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"private const string PackageId = \"Icod.TermInfo.BerkeleyDb\";",
			program,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"private const string RuntimePackageId = \"Icod.TermInfo\";",
			program,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"private const string ExpectedAssemblyVersion = \"1.0.0.0\";",
			program,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"dependency!.Attribute( \"version\" )?.Value == expectedVersion",
			program,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"StartsWith( \"runtimes/\", StringComparison.Ordinal )",
			program,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"pdb.AsSpan().StartsWith( \"BSJB\"u8 )",
			program,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"ContainsAscii( pdb, commit )",
			program,
			StringComparison.Ordinal
		);
		Assert.Contains(
			"tools/berkeleydb-package-verifier/Icod.TermInfo.BerkeleyDb.PackageVerifier.csproj",
			wrapper,
			StringComparison.Ordinal
		);
		Assert.Contains( "--no-build", wrapper, StringComparison.Ordinal );
		Assert.DoesNotContain(
			"System.IO.Compression.ZipFile",
			wrapper,
			StringComparison.Ordinal
		);
	}

	private static string FindRepositoryRoot() {
		DirectoryInfo? current =
			new DirectoryInfo( AppContext.BaseDirectory );

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
			"Repository root not found."
		);
	}
}
```

- [x] **Step 2: Verify the focused RED**

Run:

```bash
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter FullyQualifiedName~Hdb08PackagingQualificationTests.ExactPackageVerificationIsOwnedByManagedTool
```

Expected: FAIL at `Assert.True( File.Exists( projectPath ) )`; the new C# verifier does not exist. With no local .NET SDK, commit and push this test-only checkpoint and require the normal workflow to show the same focused failure on Windows, Linux, and macOS across net8/net9/net10.

- [x] **Step 3: Commit the RED**

```bash
git add tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb08PackagingQualificationTests.cs
git commit -m "test: require exact HDB08 package verification"
```

Record the exact commit, workflow IDs, failing host jobs, and assertion. Do not add verifier code until the RED is observed.

---

### Task 2: Exact package-verifier GREEN

**Files:**
- Create: `tools/berkeleydb-package-verifier/Icod.TermInfo.BerkeleyDb.PackageVerifier.csproj`
- Create: `tools/berkeleydb-package-verifier/Program.cs`
- Create: `tools/berkeleydb-package-verifier/README.md`
- Modify: `Icod.TermInfo.sln`
- Modify: `.github/scripts/verify-berkeleydb-package.ps1`

**Interfaces:**
- Consumes: `Directory.Build.props`, `Icod.TermInfo.BerkeleyDb.csproj`, canonical package artifacts, and `Icod.TermInfo.PublicApiSnapshot`.
- Produces: `Icod.TermInfo.BerkeleyDb.PackageVerifier.Main(string[]) -> int`; stable wrapper command `.github/scripts/verify-berkeleydb-package.ps1 -ArtifactDirectory <path> -Configuration <configuration>`.

- [x] **Step 1: Add the non-packable verifier project**

Create the project exactly as:

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<OutputType>Exe</OutputType>
		<TargetFramework>net10.0</TargetFramework>
		<AssemblyName>Icod.TermInfo.BerkeleyDb.PackageVerifier</AssemblyName>
		<RootNamespace>Icod.TermInfo.BerkeleyDb.PackageVerifier</RootNamespace>
		<IsPackable>false</IsPackable>
	</PropertyGroup>
</Project>
```

Add it to `Icod.TermInfo.sln` as a normal C# project with Debug, Release, and Staging Any CPU configuration entries. Add no project or package reference.

- [x] **Step 2: Implement CLI/version discovery and exact package paths**

`Program.Main` accepts zero or one artifact-directory argument. Use these constants and target list:

```csharp
private const string PackageId = "Icod.TermInfo.BerkeleyDb";
private const string RuntimePackageId = "Icod.TermInfo";
private const string RepositoryUrl =
	"https://github.com/uniblab/Icod.TermInfo";
private const string ExpectedAssemblyVersion = "1.0.0.0";
private const string ExpectedDescription =
	"Managed read-only acquisition support for ncurses-compatible "
		+ "Berkeley DB Hash-v9 terminfo stores.";
private const string ExpectedTags =
	"terminfo libtinfo terminal berkeleydb hash ncurses database dotnet csharp";

private static readonly string[] TargetFrameworks = [
	"net8.0",
	"net9.0",
	"net10.0",
];
```

Read the suite version from `Directory.Build.props`. Require both `<Version>` and `<PackageVersion>` in the BerkeleyDb project to equal `$(IcodTermInfoSuiteVersion)`. Require the project `<PackageTags>` authority to equal the semicolon-separated tag string from the spec. Resolve these exact artifact names:

```csharp
string nupkg =
	Path.Combine(
		artifactDirectory,
		$"{PackageId}.{packageVersion}.nupkg"
	);
string snupkg =
	Path.Combine(
		artifactDirectory,
		$"{PackageId}.{packageVersion}.snupkg"
	);
```

More than one argument writes the documented usage and returns 2. Catch `IOException`, `UnauthorizedAccessException`, `InvalidDataException`, `InvalidOperationException`, and `XmlException`; write the exact exception message and return 1.

- [x] **Step 3: Verify exact nupkg structure, identity, and dependency graph**

Open the nupkg with `ZipFile.OpenRead`. Require these exact entries:

```csharp
List<string> required = [
	"LICENSE",
	"README.md",
	"icon.png",
];
foreach ( string targetFramework in TargetFrameworks ) {
	required.Add(
		$"lib/{targetFramework}/{PackageId}.dll"
	);
	required.Add(
		$"lib/{targetFramework}/{PackageId}.xml"
	);
}
```

Require the complete `.dll` set to equal the three expected managed assemblies. Reject every `runtimes/` entry and every entry ending in `.so`, `.dylib`, `.a`, `.lib`, `.o`, `.obj`, or `.exe`, case-insensitively.

For each assembly entry, copy to one temporary `.dll`, use `AssemblyName.GetAssemblyName`, and require:

```csharp
assemblyName.Name == PackageId
assemblyName.Version?.ToString() == ExpectedAssemblyVersion
assemblyName.GetPublicKeyToken() is null or { Length: 0 }
```

Also open the entry through `PEReader` and require `HasMetadata`, a non-null COR header, `CorFlags.ILOnly`, and zero native entry point. Delete every temporary file in `finally`.

Load each XML documentation entry, require it nonempty, and require `<assembly><name>` to equal `Icod.TermInfo.BerkeleyDb`. HDB09 owns the final member baseline.

Load the single nuspec and require exact id, version, title, authors, project URL, README, icon, description, copyright, normalized-space tags, `requireLicenseAcceptance == "true"`, and the LGPL expression from the spec. Require repository type `git`, exact URL, and a 40-hex-character commit.

Require exactly three dependency groups, one per target framework. Each group contains exactly one dependency, verified with:

```csharp
Require(
	dependency!.Attribute( "id" )?.Value == RuntimePackageId,
	$"{targetFramework} does not depend only on {RuntimePackageId}."
);
Require(
	dependency!.Attribute( "version" )?.Value == expectedVersion,
	$"{targetFramework} does not depend on the matching {RuntimePackageId} version."
);
Require(
	dependency!.Attribute( "exclude" )?.Value == "Build,Analyzers",
	$"{targetFramework} has an unexpected dependency exclusion."
);
```

- [x] **Step 4: Verify exact snupkg and Source Link**

Require the symbol package to contain exactly these PDB paths and no DLL, native extension, or `runtimes/` path:

```csharp
string[] expectedPdbs =
	TargetFrameworks
		.Select(
			targetFramework =>
				$"lib/{targetFramework}/{PackageId}.pdb"
		)
		.OrderBy( path => path, StringComparer.Ordinal )
		.ToArray();
```

For each PDB, require portable-PDB signature and Source Link evidence:

```csharp
Require(
	pdb.AsSpan().StartsWith( "BSJB"u8 ),
	$"{path} is not a portable PDB."
);
Require(
	ContainsAscii(
		pdb,
		"raw.githubusercontent.com/uniblab/Icod.TermInfo/"
	),
	$"{path} does not contain the expected GitHub Source Link mapping."
);
Require(
	ContainsAscii( pdb, commit ),
	$"{path} Source Link data does not contain the package repository commit."
);
```

Require the symbol nuspec id/version to match and its package type name to equal `SymbolsPackage`. Require its repository URL/commit and dependency groups to match the primary package.

- [x] **Step 5: Replace PowerShell archive parsing with orchestration**

Keep the current parameter block, root/path normalization, strict mode, and `Push-Location`/`finally`. Replace the PowerShell ZIP/nuspec parsing with:

```powershell
& dotnet run `
	--project tools/berkeleydb-package-verifier/Icod.TermInfo.BerkeleyDb.PackageVerifier.csproj `
	-c $Configuration `
	--no-build `
	-- `
	$ArtifactDirectory
if (0 -ne $LASTEXITCODE) {
	throw "HDB08 BerkeleyDb package verification exited with status $LASTEXITCODE."
}
```

Retain the two existing `Icod.TermInfo.PublicApiSnapshot --compare` invocations for net8/net9 and net8/net10. Do not use `ConvertFrom-Json`, `System.IO.Compression.ZipFile`, PowerShell classes, or PowerShell 7-only syntax.

- [x] **Step 6: Document the maintainer command**

Create the verifier README with the exact command, zero/one-argument behavior, primary/symbol package checks, API-comparison ownership, no-native-asset rule, and statement that this tool is not packaged.

- [x] **Step 7: Run focused and artifact verification**

Run:

```bash
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter FullyQualifiedName~Hdb08PackagingQualificationTests.ExactPackageVerificationIsOwnedByManagedTool
dotnet build Icod.TermInfo.sln -c Release
pwsh ./packaging/PackPackages.ps1 -Configuration Release -OutputDirectory artifacts/hdb08
pwsh ./.github/scripts/verify-berkeleydb-package.ps1 -ArtifactDirectory artifacts/hdb08 -Configuration Release
```

Expected: PASS and exact verifier success. In this workspace, use CI for compilation/execution because no local .NET SDK is installed. Run `git diff --check` locally before commit.

- [x] **Step 8: Commit and qualify verifier GREEN**

```bash
git add tools/berkeleydb-package-verifier Icod.TermInfo.sln .github/scripts/verify-berkeleydb-package.ps1
git commit -m "build: verify exact HDB08 package artifacts"
```

Push and require normal 12/12. Require HDB00 3/3 if triggered. Capture exact logs proving seven nupkg/six snupkg verification and the new BerkeleyDb verifier summary.

---

### Task 3: Three-host package-consumer RED

**Files:**
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb08PackagingQualificationTests.cs`

**Interfaces:**
- Consumes: the three workflow YAML files and the existing isolated consumer script.
- Produces: permanent job-local assertions that package smoke installs all supported SDKs and invokes BerkeleyDb package consumption exactly once per workflow.

- [x] **Step 1: Add workflow-job extraction helpers**

Add:

```csharp
private static string ReadRepositoryFile(
	string root,
	params string[] segments
) {
	string[] pathSegments = [ root, .. segments ];
	return File.ReadAllText( Path.Combine( pathSegments ) );
}

private static string ReadWorkflowJob(
	string workflow,
	string jobName,
	string nextJobName
) {
	string startMarker = $"  {jobName}:";
	string endMarker = $"  {nextJobName}:";
	int start = workflow.IndexOf( startMarker, StringComparison.Ordinal );
	Assert.True( start >= 0, $"Missing workflow job '{jobName}'." );
	int end = workflow.IndexOf( endMarker, start, StringComparison.Ordinal );
	Assert.True( end > start, $"Missing workflow job '{nextJobName}'." );
	return workflow[start..end];
}

private static int CountOccurrences(
	string value,
	string fragment
) {
	int count = 0;
	int index = 0;
	while (
		(index = value.IndexOf(
			fragment,
			index,
			StringComparison.Ordinal
		)) >= 0
	) {
		count++;
		index += fragment.Length;
	}
	return count;
}
```

- [x] **Step 2: Add the failing workflow theory**

```csharp
[Theory]
[InlineData( "pull-request.yaml" )]
[InlineData( "main.yaml" )]
[InlineData( "release.yaml" )]
public void PackageSmokeConsumesBerkeleyDbOnEveryOperatingSystem(
	string workflowName
) {
	string root = FindRepositoryRoot();
	string workflow =
		ReadRepositoryFile(
			root,
			".github",
			"workflows",
			workflowName
		);
	string job =
		ReadWorkflowJob(
			workflow,
			"smoke-tool-package",
			"smoke-tool-archives"
		);

	Assert.Contains(
		"os: [windows-latest, ubuntu-latest, macos-latest]",
		job,
		StringComparison.Ordinal
	);
	Assert.Contains(
		"dotnet-version: ${{ env.DOTNET_VERSIONS }}",
		job,
		StringComparison.Ordinal
	);
	Assert.Contains(
		"smoke-hdb03-package-consumer.ps1 artifacts",
		job,
		StringComparison.Ordinal
	);
	Assert.Equal(
		1,
		CountOccurrences(
			workflow,
			"smoke-hdb03-package-consumer.ps1"
		)
	);
}
```

- [x] **Step 3: Add the already-green six-RID characterization**

Add a theory over all three workflows that asserts `windows-11-arm`, `ubuntu-24.04-arm`, `macos-15-intel`, both macOS entries, both x64 entries, `name: Archive ${{ matrix.name }}`, and `smoke-tool-archive.ps1`. This test must pass before workflow edits and permanently prevents HDB08 from narrowing archive coverage.

- [x] **Step 4: Observe and commit RED**

Run:

```bash
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter FullyQualifiedName~Hdb08PackagingQualificationTests
```

Expected: the three package-smoke theory cases fail per TFM. PR currently invokes the consumer only in the Linux packaging job; `main` and release omit it, and all package-smoke jobs install only .NET 10. The six-RID characterization remains green.

```bash
git add tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb08PackagingQualificationTests.cs
git commit -m "test: require cross-platform HDB08 package smoke"
```

Push and record the exact failing assertions before workflow changes.

---

### Task 4: Three-host package-consumer GREEN

**Files:**
- Modify: `.github/workflows/pull-request.yaml`
- Modify: `.github/workflows/main.yaml`
- Modify: `.github/workflows/release.yaml`

**Interfaces:**
- Consumes: one uploaded package set and `.github/scripts/smoke-hdb03-package-consumer.ps1`.
- Produces: net8/net9/net10 package-only execution on Windows, Linux, and macOS in all workflow families.

- [x] **Step 1: Move PR ownership to the cross-host smoke job**

Remove only this Linux packaging-host step from `pull-request.yaml`:

```yaml
      - name: Smoke isolated HDB03 BerkeleyDb package consumer
        if: matrix.package_host
        shell: pwsh
        run: ./.github/scripts/smoke-hdb03-package-consumer.ps1 artifacts -Configuration '${{ env.CONFIGURATION }}'
```

In `smoke-tool-package`, change setup-dotnet to:

```yaml
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSIONS }}
```

After artifact download and before installed-tool smoke, add:

```yaml
      - name: Smoke isolated HDB08 BerkeleyDb package consumer
        shell: pwsh
        run: ./.github/scripts/smoke-hdb03-package-consumer.ps1 artifacts -Configuration '${{ env.CONFIGURATION }}'
```

- [x] **Step 2: Add the same consumer to `main`**

Change `main.yaml` package-smoke SDK setup to `${{ env.DOTNET_VERSIONS }}` and add the exact named step above after artifact download. Use the workflow's `Release` configuration through `${{ env.CONFIGURATION }}`.

- [x] **Step 3: Add the same consumer to tagged release**

Change `release.yaml` package-smoke SDK setup to `${{ env.DOTNET_VERSIONS }}` and add the exact named step above after artifact download. Retain `needs: [metadata, package]` and retain publication dependencies on both `smoke-tool-package` and `smoke-tool-archives`.

- [x] **Step 4: Run static and focused verification**

Run:

```bash
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter FullyQualifiedName~Hdb08PackagingQualificationTests
git diff --check
```

Expected: all HDB08 contract tests pass.

- [x] **Step 5: Commit and qualify workflow GREEN**

```bash
git add .github/workflows/pull-request.yaml .github/workflows/main.yaml .github/workflows/release.yaml
git commit -m "ci: consume HDB08 packages on every host"
```

Push and require normal 12/12. Verify each Windows/Linux/macOS package-smoke log reports the isolated BerkeleyDb consumer passing net8, net9, and net10, followed by installed-tool smoke. Verify all six archive jobs remain green. Require HDB00 3/3 if triggered.

---

### Task 5: Alpha-8 authority RED

**Files:**
- Modify: `tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb08PackagingQualificationTests.cs`

**Interfaces:**
- Consumes: version authority, BerkeleyDb package project, roadmap, candidate record, and release-facing READMEs.
- Produces: exact Alpha-8/version/documentation synchronization contract without claiming HDB08 acceptance prematurely.

- [x] **Step 1: Add exact Alpha-8 authority test**

```csharp
[Fact]
public void Alpha8AuthorityAndQualificationCandidateAreSynchronized() {
	string root = FindRepositoryRoot();
	string props =
		ReadRepositoryFile( root, "Directory.Build.props" );
	string project =
		ReadRepositoryFile(
			root,
			"Icod.TermInfo.BerkeleyDb",
			"Icod.TermInfo.BerkeleyDb.csproj"
		);
	string roadmap =
		ReadRepositoryFile(
			root,
			"Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md"
		);
	string candidatePath =
		Path.Combine(
			root,
			"docs",
			"1.15.0-HDB08-QUALIFICATION-CANDIDATE.md"
		);

	Assert.Contains(
		"<IcodTermInfoSuiteVersion>1.15.0-Alpha-8</IcodTermInfoSuiteVersion>",
		props,
		StringComparison.Ordinal
	);
	Assert.Contains(
		"<PackageReleaseNotes>1.15.0-Alpha-8 completes exact package and cross-platform qualification",
		project,
		StringComparison.Ordinal
	);
	Assert.Contains(
		"**Current coordinated prerelease:** `1.15.0-Alpha-8`",
		roadmap,
		StringComparison.Ordinal
	);
	Assert.Contains(
		"HDB08 — Packaging and Cross-platform Qualification — COMPLETE / ACCEPTED",
		roadmap,
		StringComparison.Ordinal
	);
	Assert.True( File.Exists( candidatePath ) );
	string candidate = File.ReadAllText( candidatePath );
	Assert.Contains( "1.15.0-Alpha-8", candidate, StringComparison.Ordinal );
	Assert.Contains( "normal PR workflow: 12/12", candidate, StringComparison.Ordinal );
	Assert.Contains( "HDB00 workflow: 3/3", candidate, StringComparison.Ordinal );
}
```

- [x] **Step 2: Observe and commit RED**

Run the focused test. Expected: FAIL because the version is still Alpha-7, the roadmap still says HDB08 is next, and the candidate record is absent.

```bash
git add tests/Icod.TermInfo.BerkeleyDb.Tests/src/Hdb08PackagingQualificationTests.cs
git commit -m "test: require HDB08 Alpha-8 authority"
```

Push and record the exact RED before version/documentation changes.

---

### Task 6: Alpha-8 candidate GREEN and exact implementation qualification

**Files:**
- Modify: `Directory.Build.props`
- Modify: `Icod.TermInfo.BerkeleyDb/Icod.TermInfo.BerkeleyDb.csproj`
- Create: `docs/1.15.0-HDB08-QUALIFICATION-CANDIDATE.md`
- Modify: `Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md`
- Modify: `README.md`
- Modify: `Icod.TermInfo.BerkeleyDb/README.md`
- Modify: `packaging/README.md`
- Modify: `.github/scripts/README.md`

**Interfaces:**
- Consumes: qualified verifier/workflow checkpoints.
- Produces: one exact Alpha-8 implementation/qualification candidate with no production behavior change.

- [x] **Step 1: Advance only the coordinated version authority**

Change:

```xml
<IcodTermInfoSuiteVersion>1.15.0-Alpha-7</IcodTermInfoSuiteVersion>
```

to:

```xml
<IcodTermInfoSuiteVersion>1.15.0-Alpha-8</IcodTermInfoSuiteVersion>
```

Do not change any assembly version or individual `<Version>`/`<PackageVersion>` property reference.

- [x] **Step 2: Replace BerkeleyDb package release notes exactly**

Use one XML element containing:

```text
1.15.0-Alpha-8 completes exact package and cross-platform qualification for managed read-only Berkeley DB Hash-v9 acquisition. Public API, dependencies, acquisition behavior, pure-managed deployment, and the read-only boundary remain unchanged.
```

- [x] **Step 3: Create the qualification-candidate record**

The record states `STATUS: QUALIFICATION CANDIDATE`, Alpha-8, the accepted HDB07C behavior head, verifier/workflow RED and GREEN heads/runs, exact expected normal 12/12 and HDB00 3/3 evidence, seven nupkg/six snupkg accounting, three-host net8/net9/net10 consumption, installed-tool smoke, six archive RIDs, unchanged contracts, and PR draft/unmerged requirement. It must explicitly state that the document does not claim acceptance until the exact containing head is green.

- [x] **Step 4: Synchronize release-facing descriptions without claiming acceptance**

Update the roadmap status to `HDB00–HDB07C accepted; HDB08 in qualification`, current prerelease Alpha-8, and the HDB08 heading/status required by the test. Update root and package READMEs to describe Alpha-8 as a qualification candidate. Update packaging/script documentation to describe the C# verifier and cross-host package-only consumer topology.

Do not edit HDB07C's accepted head or reinterpret its claims. Do not mark HDB08 complete in this commit.

- [x] **Step 5: Run complete static/focused checks**

Run:

```bash
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter FullyQualifiedName~Hdb08PackagingQualificationTests
git diff --check
rg -n "1.15.0-Alpha-7" Directory.Build.props Icod.TermInfo.BerkeleyDb/Icod.TermInfo.BerkeleyDb.csproj README.md Icod.TermInfo.BerkeleyDb/README.md Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md docs/1.15.0-HDB08-QUALIFICATION-CANDIDATE.md
```

Expected: focused tests pass, no whitespace errors, and the final `rg` finds Alpha-7 only where it identifies the prior accepted HDB07/HDB07C state rather than current authority.

- [x] **Step 6: Commit candidate GREEN**

```bash
git add Directory.Build.props Icod.TermInfo.BerkeleyDb/Icod.TermInfo.BerkeleyDb.csproj docs/1.15.0-HDB08-QUALIFICATION-CANDIDATE.md Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md README.md Icod.TermInfo.BerkeleyDb/README.md packaging/README.md .github/scripts/README.md
git commit -m "build: qualify HDB08 Alpha-8 packages"
```

- [x] **Step 7: Require exact implementation qualification**

Push the exact candidate head and require:

```text
normal pull-request workflow: 12/12 jobs
hdb00-interoperability: 3/3 jobs
```

Capture exact head SHA, workflow IDs/URLs, BerkeleyDb count per TFM/host, package verifier summary, three-host consumer lines, installed-tool results, all six archive results, artifact counts, public-API equivalence, dependency identity, and no-native-asset evidence. Do not begin closure edits until both workflows are green on the same exact head.

---

### Task 7: HDB08 closure and handoff

**Files:**
- Create: `docs/1.15.0-HDB08-PACKAGING-AND-CROSS-PLATFORM-QUALIFICATION.md`
- Modify: `docs/1.15.0-HDB08-QUALIFICATION-CANDIDATE.md`
- Modify: `docs/superpowers/specs/2026-09-17-hdb08-packaging-cross-platform-qualification-design.md`
- Modify: `docs/superpowers/plans/2026-09-17-hdb08-packaging-cross-platform-qualification.md`
- Modify: `Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md`
- Modify: `README.md`
- Modify: `Icod.TermInfo.BerkeleyDb/README.md`
- Modify: PR #45 body

**Interfaces:**
- Consumes: exact RED/GREEN heads and exact Alpha-8 workflow evidence.
- Produces: auditable HDB08 acceptance with HDB09 as the next tranche.

- [x] **Step 1: Write the closure record**

Record:

- purpose and strict qualification-only scope;
- every RED/GREEN commit and workflow;
- accepted implementation head;
- final normal/HDB00 workflow links and job counts;
- exact seven nupkg/six snupkg evidence;
- exact verifier guarantees and no-native-asset result;
- net8/net9/net10 package-only results on Windows/Linux/macOS;
- installed-tool and six archive-RID results;
- unchanged public API, assembly, dependency, JSON, command, native, and write boundaries;
- explicit absence of tag, publication, merge, or ready-for-review transition; and
- HDB09 stable-freeze handoff.

- [x] **Step 2: Mark spec, plan, candidate, roadmap, and READMEs accepted**

Set spec and plan status to `COMPLETE / ACCEPTED`, mark completed plan checkboxes, convert the candidate record to point at the accepted closure record, and mark HDB08 complete in the roadmap without changing HDB07C evidence. State that Alpha-8 remains the current coordinated prerelease and HDB09 is next.

- [x] **Step 3: Commit documentation closure**

```bash
git add docs/1.15.0-HDB08-PACKAGING-AND-CROSS-PLATFORM-QUALIFICATION.md docs/1.15.0-HDB08-QUALIFICATION-CANDIDATE.md docs/superpowers/specs/2026-09-17-hdb08-packaging-cross-platform-qualification-design.md docs/superpowers/plans/2026-09-17-hdb08-packaging-cross-platform-qualification.md Icod.TermInfo-1.15.0-Berkeley-DB-Hashed-Terminfo-Acquisition-Roadmap.md README.md Icod.TermInfo.BerkeleyDb/README.md
git commit -m "docs: accept HDB08 packaging qualification"
```

- [x] **Step 4: Verify documentation-complete head**

Require normal 12/12 and HDB00 3/3 on the exact documentation head. Run:

```bash
git diff --check HEAD^
git status --short --branch
```

Expected: no whitespace errors and a clean branch synchronized with origin.

- [x] **Step 5: Update and verify PR #45**

Update the PR body with HDB08 implementation/documentation heads, RED/GREEN evidence, exact workflows and counts, verifier and three-host package evidence, preserved boundaries, Alpha-8 identity, and HDB09 next. Re-fetch and require:

```text
state: open
draft: true
merged: false
merged_at: null
head_sha: exact documentation head
```

Do not merge, publish, tag, or mark ready for review.

---

## Plan self-review

- **Spec coverage:** Sections 5 and 8A map to Tasks 1–2; Sections 6–7 and 8B map to Tasks 3–4; Sections 4, 8C, and 9 map to Tasks 5–6; Sections 10–15 map to Tasks 6–7.
- **Scope:** The verifier and cross-host consumption are independent RED/GREEN checkpoints but one coherent HDB08 plan. Stable API-manifest freeze, audits, stable promotion, tagging, and publication remain HDB09.
- **Placeholder scan:** No implementation placeholder remains; paths, test names, helper signatures, commands, expected failures, exact metadata, version text, workflow fragments, and commit boundaries are specified.
- **Type consistency:** `Hdb08PackagingQualificationTests`, `ReadRepositoryFile`, `ReadWorkflowJob`, `CountOccurrences`, verifier constants, target-framework arrays, artifact names, and script/project paths are identical across producers and consumers.
- **Execution mode:** The user previously selected inline execution and explicitly asked to implement this approved design. Use `superpowers:executing-plans`; do not dispatch subagents.
