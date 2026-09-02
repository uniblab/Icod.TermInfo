# captoinfo

`captoinfo` is the managed Icod.TermInfo termcap-to-terminfo conversion command,
introduced in version 1.6.0.

TC07 deliberately keeps the executable thin. Conventional termcap text is
parsed by `Icod.TermInfo.Termcap`, `tc=` inheritance is resolved by the TC03
resolver, semantic conversion uses TC04, and the resulting immutable
`TerminalDescription` is rendered as deterministic terminfo source by
`Icod.TermInfo.Inspection`. TC08 freezes that composition for the stable 1.6.0
release.

Version `1.8.0` carries that frozen conversion behavior unchanged. Relative
terminfo source planning is isolated in Inspection and `infocmp --plan-use`; it
does not alter `captoinfo` command semantics or dependencies.

```text
Usage: captoinfo [OPTION]... [FILE]...

  -w WIDTH        request deterministic output wrapping width
  -h, --help     display help
  -V, --version  display the coordinated suite version
      --          end option processing
```

Use `-` as a file operand to read standard input. With no file operand,
`captoinfo` uses the historical command-level environment convention: `TERM`
selects the requested name and TC06 explicitly snapshots `TERMCAP`, `TERMPATH`,
and `HOME`, with the ncurses conventional default path policy enabled. This does
not modify Runtime `TERMINFO` discovery.

Output is effective resolved state. Source comments, disabled fields, exact
physical formatting, and the original `tc=` ancestry are not reconstructed.
Conversion warnings and loss decisions are written to standard error rather than
being silently hidden.
