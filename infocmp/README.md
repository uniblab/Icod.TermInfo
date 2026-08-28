# infocmp

`infocmp` is part of the `Icod.TermInfo` managed terminfo tool suite.

## T02 status

Version `1.4.0-Alpha-2` retains the executable, command-host, stream,
cancellation, version, and help contracts established by T01. Operational `infocmp` behavior is intentionally
introduced by later 1.4 tranches.

Supported through T02:

```text
infocmp --help
infocmp --version
```

Other arguments return the command-framework usage status (`2`) with a
diagnostic on standard error.

The command targets .NET 10. The reusable `Icod.TermInfo` libraries remain
available for `net8.0`, `net9.0`, and `net10.0`.
