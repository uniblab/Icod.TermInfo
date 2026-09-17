# BerkeleyDb Package Verifier

`Icod.TermInfo.BerkeleyDb.PackageVerifier` performs repository-side exact
validation of the packed `Icod.TermInfo.BerkeleyDb` `.nupkg` and `.snupkg`.

It verifies the coordinated version and project tag authority, exact primary
package metadata and dependency groups, the three managed DLL/XML target
framework payloads, IL-only unsigned assembly identity `1.0.0.0`, the exact
three-PDB symbol payload, portable symbols, Source Link repository/commit
identity, and the absence of runtime-specific or native assets.

Run it after building and packing:

```text
dotnet run --project tools/berkeleydb-package-verifier/Icod.TermInfo.BerkeleyDb.PackageVerifier.csproj -c Release --no-build -- artifacts
```

The artifact-directory argument is optional and defaults to `artifacts`. More
than one argument returns usage status 2. Validation failures return status 1;
success returns status 0.

Normal package validation invokes this tool through
`.github/scripts/verify-berkeleydb-package.ps1`. That PowerShell wrapper also
owns the net8/net9 and net8/net10 public-API equivalence comparisons. This
maintenance tool has no package dependency and is not itself packaged.
