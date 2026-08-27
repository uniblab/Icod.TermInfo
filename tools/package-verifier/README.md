# Package Verifier

`Icod.TermInfo.PackageVerifier` performs repository-side structural validation of
an already packed `Icod.TermInfo` `.nupkg` and `.snupkg`.

It verifies:

- `<Version>` and `<PackageVersion>` remain present and identical;
- required `net8.0`, `net9.0`, and `net10.0` package payloads are present;
- all packaged assemblies retain assembly version `1.0.0.0` and remain
  unsigned;
- all framework XML documentation files are non-empty, parseable, identify
  `Icod.TermInfo`, and contain documented members;
- `README.md` and `icon.png` are present and identified by NuGet metadata;
- title, authors, project URL, license expression, license-acceptance flag,
  description, tags, repository URL, and repository commit remain valid;
- no runtime/native dependency payload was introduced;
- repository-only tests, tools, fixtures, `.ti`, and `.bin` assets are absent;
- the package has no runtime NuGet dependencies;
- the symbol package contains portable PDBs for all supported frameworks with
  the expected Source Link data;
- the generic parameterization source layer contains no terminal-profile-specific
  reference.

Run it after packing:

```text
dotnet run --project tools/package-verifier/Icod.TermInfo.PackageVerifier.csproj -- artifacts
```

Normal release validation invokes this tool through either
`.github/scripts/verify-release-package.sh` or
`.github/scripts/verify-release-package.cmd`.
