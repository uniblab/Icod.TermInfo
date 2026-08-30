# Icod.TermInfo.Termcap

`Icod.TermInfo.Termcap` is the optional termcap interoperability layer for the
Icod.TermInfo package family.

The `1.6.0-Alpha-1` TC01 tranche establishes bounded parsing of conventional
termcap source into an unresolved, source-aware model. It intentionally does not
yet resolve `tc=` inheritance, map two-character termcap capabilities into the
canonical terminfo capability catalog, construct `TerminalDescription` values,
read `TERMCAP` or `TERMPATH`, or provide conversion commands.

The package targets `net8.0`, `net9.0`, and `net10.0` and depends only on
`Icod.TermInfo`. Existing Runtime, Source, Compiler, and Inspection package APIs
remain unchanged.

## TC01 parser surface

The initial source surface is centered on:

```csharp
TermcapSourceParseResult result = TermcapSourceParser.Parse(
    "vt|vt100|DEC VT100:am:co#80:cl=\\E[H\\E[2J:"
);

if (!result.HasErrors)
{
    TermcapSourceEntry entry = result.Document.Entries[0];
    Console.WriteLine(entry.Names[1]);
}
```

The parser preserves source spans and field order. Capability fields remain in
termcap form until later 1.6 tranches define mapping, inheritance, and conversion
semantics.

## Resource limits

`TermcapSourceParserOptions` bounds caller-supplied source length. The default is
4 MiB and the supported upper bound is 64 MiB. Inputs beyond the configured
limit fail deterministically with a source diagnostic rather than being parsed
partially.
