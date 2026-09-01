# infotocap

`infotocap` is the managed Icod.TermInfo terminfo-to-termcap conversion command,
introduced in version 1.6.0.

TC07 composes the existing Source parser/resolver with the TC05 termcap
representability and rendering engine. It does not add another termcap semantic
model or conversion table. TC08 freezes that composition for the stable 1.6.0
release.

Version `1.6.1` carries that frozen conversion behavior unchanged. It is a
release-infrastructure hotfix and does not alter `infotocap` command semantics.

```text
Usage: infotocap [OPTION]... FILE...

  -w WIDTH        request deterministic output wrapping width
  -h, --help     display help
  -V, --version  display the coordinated suite version
      --          end option processing
```

Use `-` as a file operand to read standard input. Every source entry is resolved
through `use=` before rendering, so output represents effective terminal state.
Comments, cancellations, original formatting, and `use=` ancestry are not
reconstructed.

Traditional termcap cannot represent every terminfo description. When TC05
preflight rejects a capability or parameter program, `infotocap` reports the
diagnostic and returns failure instead of emitting a silently lossy substitute.
