# infocmp

`infocmp` is part of the `Icod.TermInfo` managed terminfo tool suite.

## T06 status

Version `1.4.0-Alpha-6` implements one-terminal acquisition and deterministic
effective-source rendering over the reusable Runtime and Inspection APIs.

Supported through T06:

```text
infocmp [options] [terminal]
infocmp -D
infocmp -V
infocmp --version
infocmp --help
```

With no terminal operand, `TERM` supplies the requested name. One operand is
inspected directly. Two or more operands remain a usage error until T07 adds
semantic comparison.

Presentation options are:

```text
-A <directory>    use one explicit conventional terminfo database
-0                emit one logical source line
-1                emit one capability per line
-w <width>        request canonical wrapping width
-s d|i|l|c        order standard capabilities by database, short name,
                  long name, or termcap code
-x                include effective extended capabilities
-D                report Runtime database discovery locations
```

Default output contains standard capabilities only. `-x` includes extended
capabilities. The rendered text represents effective `TerminalDescription` state;
it does not reconstruct original comments, whitespace, `use=` history,
cancellations, disabled fields, or source provenance.

`-A` creates an explicit `DirectoryTerminalDescriptionProvider`; it does not
mutate `TERMINFO` or other process environment variables. Without `-A`, the
normal Runtime `SystemTerminalDescriptionProvider` search policy is used.

The command targets .NET 10. The reusable `Icod.TermInfo` libraries remain
available for `net8.0`, `net9.0`, and `net10.0`.
