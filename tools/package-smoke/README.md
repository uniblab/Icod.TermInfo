# Fresh Package Smoke Consumer

This project is intentionally **not** part of `Icod.TermInfo.sln` and has no
project reference to the repository library. It exists to prove that a fresh
consumer can restore and execute only the packed `Icod.TermInfo` NuGet artifact.

Release validation copies this project into a temporary directory, restores it
from the local `artifacts` directory with an isolated NuGet package cache, and
passes the package version through `IcodTermInfoPackageVersion`.

The consumer targets both `net8.0` and `net10.0`, and release validation executes
the same package-reference-only smoke program once for each supported framework.

The source is checked in as ordinary C# so package smoke coverage can grow
without embedding a miniature C# project inside a Bash or CMD script.
