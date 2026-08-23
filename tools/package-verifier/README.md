# Package Verifier

`Icod.TermInfo.PackageVerifier` performs repository-side structural validation of
an already packed `Icod.TermInfo` `.nupkg` and `.snupkg`.

It verifies:

- `<Version>` and `<PackageVersion>` remain present and identical;
- required `net8.0` and `net10.0` package payloads are present;
- no runtime/native dependency payload was introduced;
- repository-only tests, tools, fixtures, `.ti`, and `.bin` assets are absent;
- NuGet metadata identifies the expected package and repository commit;
- the package has no runtime NuGet dependencies;
- the symbol package contains portable PDBs for both supported frameworks with the expected Source Link data;
- the generic parameterization source layer contains no terminal-profile-specific
  reference.

Run it after packing:

```text
dotnet run --project tools/package-verifier/Icod.TermInfo.PackageVerifier.csproj -- artifacts
```

Normal release validation invokes this tool through either
`.github/scripts/verify-release-package.sh` or
`.github/scripts/verify-release-package.cmd`.
