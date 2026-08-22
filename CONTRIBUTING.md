# Contributing to Icod.TermInfo

Contributions are welcome when they preserve the small, capability-driven design described by the 0.6.0 roadmap.

## Development requirements

Use the .NET 10 SDK and C# 13. Before submitting a change, run Debug and Release builds and tests for `Icod.TermInfo.sln`.

Repository text files use UTF-8 and LF line endings. Use braces for all control-flow bodies. Public, protected, and internal API entry points should validate their parameters before performing work.

## Terminal profiles

Terminal profiles should be expressed through `TerminalDescriptionBuilder` and supplied through an `ITerminalDescriptionProvider`. Generic capability lookup, parameter expansion, and output code must not grow terminal-specific branches.

Do not map unsupported terminal identities such as `xterm`, `screen`, `tmux`, or `linux` to ANSI or VT100 merely because they understand similar escape sequences.

Every advertised capability in a built-in profile requires a golden test, and capabilities which must remain absent should also be tested explicitly.

## Parameter-expansion changes

Changes to the terminfo parameter engine require direct tests for the affected operator or formatting rule. Parse failures and evaluation failures must remain deterministic managed errors rather than producing partial output.

Persistent state must be caller-owned. Do not add process-global equivalents of `cur_term` or hidden persistent `%P/%g` variable storage.

## Scope discipline

The 0.6.0 contract deliberately excludes curses windows, terminal emulation, PTY management, system terminfo database loading, xterm extensions, 256-color/true-color behavior, mouse decoding, and modern graphics protocols. New work outside that scope should first be discussed as a post-0.6.0 extension.
