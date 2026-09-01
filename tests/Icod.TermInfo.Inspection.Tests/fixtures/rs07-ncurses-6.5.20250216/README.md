# RS07 pinned ncurses differential corpus

This directory captures semantic reference material for RS07 from a controlled
Linux environment. The capture is intentionally pinned rather than refreshed
implicitly by CI.

Reference environment:

- ncurses: `ncurses 6.5.20250216`
- host: Debian GNU/Linux 13 (trixie), x86_64
- system terminfo database: `/usr/share/terminfo`
- full effective entries: `infocmp -x -1 TERMINAL`
- relative cases: `infocmp -x -u TARGET PARENT [PARENT ...]`

`effective.ti` contains the effective source forms needed by every case. Each
other `.ti` file is the exact relative source emitted by the pinned ncurses
`infocmp -u` invocation named in `cases.tsv`.

Normal Icod CI does not execute host ncurses. Tests parse this checked-in corpus,
resolve the target and parents through `Icod.TermInfo.Source`, and require both
the pinned ncurses relative source and Icod's synthesized relative source to
resolve semantically to the same target `TerminalDescription`.

The corpus covers xterm, screen, tmux, Linux console, VT-family inheritance,
256-color extensions, cancellation-heavy output, and a multi-parent rewrite.
