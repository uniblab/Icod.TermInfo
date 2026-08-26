# Icod.TermInfo.Source Package Smoke Consumer

This project is a package-reference-only consumer used by repository validation.

It intentionally references only `Icod.TermInfo.Source`. Its program also uses
`Icod.TermInfo.TerminalDescription`, proving that the source package exposes the
intended transitive dependency on the stable runtime package.

The validation scripts copy this project and `Program.cs` to a temporary
directory before restore so repository-local project references cannot satisfy
the test accidentally.
