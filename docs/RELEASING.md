# Releasing Icod.TermInfo

This document describes the publication path for Icod.TermInfo 0.6.0 and later releases using the same repository workflows.

## Release principles

- `<Version />` and `<PackageVersion />` in `Icod.TermInfo.csproj` must always be identical.
- A release tag must be exactly `v<PackageVersion>`.
- Release validation must pass on Windows, Linux, and macOS.
- The package is packed once after validation; that same `.nupkg` is published to GitHub Packages and NuGet.org.
- Release builds use the repository's deterministic/continuous-integration build settings and .NET SDK Source Link support.
- The `.snupkg` generated beside the primary package is retained as a release artifact and published to the NuGet.org symbol server through `dotnet nuget push`.
- Publication never occurs merely because `main` changed.

## One-time GitHub repository setup

The release workflow uses the repository `GITHUB_TOKEN` for GitHub Packages and GitHub Releases. Its job-level permissions request only the scopes needed for those operations.

No long-lived GitHub Packages token is required for the repository's own package.

## One-time NuGet.org trusted-publishing setup

The release workflow uses NuGet.org trusted publishing so no long-lived NuGet API key is stored in GitHub.

On NuGet.org, create a trusted-publishing policy for:

- repository owner: `uniblab`;
- repository: `Icod.TermInfo`;
- workflow file: `release.yml`;
- environment: leave blank unless the workflow is deliberately changed to use a GitHub environment.

In the GitHub repository, create a secret named `NUGET_USER` containing the NuGet.org profile/user name that owns or is permitted to publish `Icod.TermInfo`. It is a user name, not an email address.

The `NuGet/login@v1` action exchanges the GitHub OIDC identity for a short-lived NuGet.org API key during the publish job.

## Publishing to GitHub Packages manually

The `publish-github-packages.yml` workflow is manually invoked with `workflow_dispatch`.

It:

1. restores the solution;
2. builds and tests Release;
3. packs `Icod.TermInfo.csproj`;
4. runs the T10 release-package verifier, including the fresh-consumer local-package smoke test;
5. uploads the `.nupkg` and `.snupkg` as workflow artifacts;
6. publishes the primary `.nupkg` to the repository owner's GitHub Packages NuGet feed using `GITHUB_TOKEN`.

The workflow uses `--skip-duplicate`, so re-running it for an already-published version does not replace an immutable package version.

## Publishing a tagged release

Before tagging:

1. confirm the intended version in both `<Version />` and `<PackageVersion />`;
2. run Debug and Release builds/tests locally;
3. pack locally if desired and run `.github/scripts/verify-release-package.sh artifacts`;
4. merge the release-ready commit to the desired branch;
5. confirm the normal GitHub Actions build, test, package-validation, and fresh-consumer checks are green;
6. create and push an annotated or lightweight tag named exactly `v<PackageVersion>`.

For the 0.6.0 contract release:

```text
git tag v0.6.0
git push origin v0.6.0
```

The `release.yml` workflow then:

1. validates Release builds/tests on Windows, Linux, and macOS;
2. rejects the tag if it does not match `<PackageVersion />`;
3. packs once on Ubuntu after all validation jobs succeed;
4. runs the T10 release-package verifier against the exact artifacts that will be published;
5. publishes the same `.nupkg` to GitHub Packages;
6. obtains a temporary NuGet.org API key through OIDC trusted publishing;
7. publishes the `.nupkg` and associated `.snupkg` to NuGet.org;
8. creates a GitHub Release for the existing tag and attaches both package files.

If any publication step fails, correct the configuration or transient problem and re-run the failed job. Do not change package contents for a version that has already been successfully published; increment the prerelease/final version instead.

## Final 0.6.0 release

The T10 release-ready commit sets both version elements to `0.6.0`. Before tagging, require the normal `main` build workflow to be green; its package job performs package validation, artifact inspection, Source Link metadata checks, and a fresh-consumer restore/build/run using only the local `.nupkg`. Then tag exactly:

```text
v0.6.0
```

Do not create the tag if the T10 checks are not green. See `docs/0.6.0-CONTRACT-AUDIT.md` for the complete gate-to-evidence mapping. After `v0.6.0` is published, public API changes should be treated as deliberate contract changes and reviewed against the T8 API baseline.
