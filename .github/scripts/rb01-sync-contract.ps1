$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Replace-Exact {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Old,
        [Parameter(Mandatory = $true)]
        [string]$New
    )

    $text = [System.IO.File]::ReadAllText($Path)
    if ($text.IndexOf($Old, [System.StringComparison]::Ordinal) -lt 0) {
        throw "Expected text not found in $Path"
    }
    $updated = $text.Replace($Old, $New)
    [System.IO.File]::WriteAllText(
        $Path,
        $updated,
        [System.Text.UTF8Encoding]::new($false)
    )
}

$roadmapPath = 'Icod.TermInfo-1.14.0-Raster-Backend-Capability-Evidence-Selection-and-Planning-Roadmap.md'
$specPath = 'docs/superpowers/specs/2026-09-14-1.14.0-raster-backend-evidence-selection-design.md'
$re08Path = 'tests/Icod.TermInfo.Inspection.Tests/src/RE08ReleaseClosureTests.cs'

$roadmapOld = @'
The 1.14 request SHALL compose existing request types:

```text
PersistentRasterLifecycleRequest
PersistentRasterPlacementRequest
```

1.14 SHALL NOT duplicate their semantic fields.
'@
$roadmapNew = @'
The 1.14 request SHALL compose existing request types:

```text
PersistentRasterLifecycleRequest
PersistentRasterPlacementRequest?
```

`PlacementRequest = null` means no advanced 1.12 placement semantics are requested. This is required because the frozen `PersistentRasterPlacementRequest` intentionally rejects an empty request.

1.14 SHALL NOT duplicate their semantic fields.
'@
Replace-Exact -Path $roadmapPath -Old $roadmapOld -New $roadmapNew

$roadmapAlgorithmOld = @'
6. lifecycle `Indeterminate` -> candidate `RequiresRuntimeVerification`;
7. lifecycle `Success` -> call the existing placement planner;
8. placement `Impossible` -> candidate `Impossible`;
9. placement `RequiresRuntimeVerification` or `Indeterminate` -> candidate `RequiresRuntimeVerification`; and
10. placement `Satisfied` -> candidate `Satisfied`.
'@
$roadmapAlgorithmNew = @'
6. lifecycle `Indeterminate` -> candidate `RequiresRuntimeVerification`;
7. lifecycle `Success` with no placement request -> candidate `Satisfied`;
8. lifecycle `Success` with a placement request -> call the existing placement planner;
9. placement `Impossible` -> candidate `Impossible`;
10. placement `RequiresRuntimeVerification` or `Indeterminate` -> candidate `RequiresRuntimeVerification`; and
11. placement `Satisfied` -> candidate `Satisfied`.
'@
Replace-Exact -Path $roadmapPath -Old $roadmapAlgorithmOld -New $roadmapAlgorithmNew

$specOld = @'
RasterBackendSelectionRequest
    LifecycleRequest : PersistentRasterLifecycleRequest
    PlacementRequest : PersistentRasterPlacementRequest
```

Selecting a raster backend always requires the backend itself to be supported. Lifecycle and placement requirements are evaluated by the existing planners.
'@
$specNew = @'
RasterBackendSelectionRequest
    LifecycleRequest : PersistentRasterLifecycleRequest
    PlacementRequest : PersistentRasterPlacementRequest?
```

`PlacementRequest = null` means the caller requests no advanced 1.12 placement semantics. This preserves valid lifecycle-only selection requests because the frozen `PersistentRasterPlacementRequest` intentionally rejects an empty request.

Selecting a raster backend always requires the backend itself to be supported. Lifecycle requirements are always evaluated by the existing planner; placement requirements are evaluated by the existing placement planner only when `PlacementRequest` is non-null.
'@
Replace-Exact -Path $specPath -Old $specOld -New $specNew

$specMappingOld = @'
- lifecycle `Indeterminate` -> `RequiresRuntimeVerification`;
- lifecycle `Success` continues into the frozen placement planner;
- placement `Impossible` -> `Impossible`;
- placement `RequiresRuntimeVerification` or `Indeterminate` -> `RequiresRuntimeVerification`;
- placement `Satisfied` -> `Satisfied`.
'@
$specMappingNew = @'
- lifecycle `Indeterminate` -> `RequiresRuntimeVerification`;
- lifecycle `Success` with `PlacementRequest = null` -> `Satisfied`;
- lifecycle `Success` with a non-null placement request continues into the frozen placement planner;
- placement `Impossible` -> `Impossible`;
- placement `RequiresRuntimeVerification` or `Indeterminate` -> `RequiresRuntimeVerification`;
- placement `Satisfied` -> `Satisfied`.
'@
Replace-Exact -Path $specPath -Old $specMappingOld -New $specMappingNew

$re08 = [System.IO.File]::ReadAllText($re08Path)
$loadPattern = '(?m)^\t\tstring buildProperties = ReadRequiredRepositoryFile\( "Directory\.Build\.props" \);\r?\n'
$assertPattern = '(?ms)^\t\tAssert\.Contains\(\r?\n\t\t\t"<IcodTermInfoSuiteVersion>1\.13\.0</IcodTermInfoSuiteVersion>",\r?\n\t\t\tbuildProperties,\r?\n\t\t\tStringComparison\.Ordinal\r?\n\t\t\);\r?\n'
$withoutLoad = [regex]::Replace($re08, $loadPattern, '', 1)
if ([string]::Equals($withoutLoad, $re08, [System.StringComparison]::Ordinal)) {
    throw 'RE08 current-version load ownership line was not found.'
}
$withoutAssertion = [regex]::Replace($withoutLoad, $assertPattern, '', 1)
if ([string]::Equals($withoutAssertion, $withoutLoad, [System.StringComparison]::Ordinal)) {
    throw 'RE08 current-version assertion ownership block was not found.'
}
[System.IO.File]::WriteAllText(
    $re08Path,
    $withoutAssertion,
    [System.Text.UTF8Encoding]::new($false)
)
