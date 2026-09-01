#!/usr/bin/env bash
set -euo pipefail

expected='ncurses 6.5.20250216'
actual="$(infocmp -V)"
if [[ "$actual" != "$expected" ]]; then
    printf 'expected %s but found %s\n' "$expected" "$actual" >&2
    exit 1
fi

terms=(
    xterm
    xterm-256color
    screen
    screen-256color
    tmux-256color
    linux
    vt100
    vt220
)

: > effective.ti
for index in "${!terms[@]}"; do
    infocmp -x -1 "${terms[ index ]}" >> effective.ti
    if (( index + 1 < ${#terms[@]} )); then
        printf '\n' >> effective.ti
    fi
done

infocmp -x -u xterm-256color xterm \
    > xterm-256color-from-xterm.ti
infocmp -x -u screen-256color screen \
    > screen-256color-from-screen.ti
infocmp -x -u tmux-256color screen-256color \
    > tmux-256color-from-screen-256color.ti
infocmp -x -u linux vt100 \
    > linux-from-vt100.ti
infocmp -x -u vt220 vt100 \
    > vt220-from-vt100.ti
infocmp -x -u xterm-256color xterm screen-256color \
    > xterm-256color-from-xterm-and-screen.ti
