# Compiler Package Verifier

`Icod.TermInfo.Compiler.PackageVerifier` performs repository-side structural
validation of the packed `Icod.TermInfo.Compiler` `.nupkg` and `.snupkg`.

It verifies all three target-framework DLL/XML/PDB payloads, assembly identity
`1.0.0.0`, package metadata, the matching runtime-only NuGet dependency,
portable symbols, Source Link, and repository commit metadata.

Run it after packing through the coordinated release verifier; it is a
repository maintenance tool and is not itself packaged.