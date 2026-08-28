# toe

`toe` is part of the `Icod.TermInfo` managed terminfo tool suite.

## T07 status

Version `1.4.0-Alpha-7` retains the executable, command-host, stream,
cancellation, version, and help contracts established by T01. T07 remains
focused on `infocmp`; operational `toe` behavior begins in T08.

Supported through T07:

```text
toe --help
toe --version
```

Other arguments return the command-framework usage status (`2`) with a
diagnostic on standard error.

The command targets .NET 10. The reusable `Icod.TermInfo` libraries remain
available for `net8.0`, `net9.0`, and `net10.0`.
