# toe

`toe` is part of the `Icod.TermInfo` managed terminfo tool suite.

## T01 status

Version `1.4.0-Alpha-1` establishes only the executable, command-host, stream,
cancellation, version, and help contracts. Operational `toe` behavior is intentionally
introduced by later 1.4 tranches.

Supported in T01:

```text
toe --help
toe --version
```

Other arguments return the command-framework usage status (`2`) with a
diagnostic on standard error.

The command targets .NET 10. The reusable `Icod.TermInfo` libraries remain
available for `net8.0`, `net9.0`, and `net10.0`.
