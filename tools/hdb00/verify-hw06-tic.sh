#!/usr/bin/env bash
set -euo pipefail

# Uses the pinned native ncurses build and Berkeley DB tools from HDB00.
fixture_root="$1"
db_verify="$2"
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
work_root="$fixture_root/hw06-tic"
mkdir -p "$work_root"
cat > "$work_root/source.ti" <<'SOURCE'
hw06-parent|HW06 inherited terminal,
    am,
    lines#24,
hw06-child|hw06-alias|HW06 published terminal,
    cols#80,
    clear=\E[H\E[2J,
    use=hw06-parent,
SOURCE

dotnet build "$repo_root/icod-terminfo/Icod.TermInfo.Router.csproj" -c Release -f net10.0 -m:1
router="$repo_root/icod-terminfo/bin/Release/net10.0/icod-terminfo.dll"
tic="$repo_root/tic/bin/Release/net10.0/tic.dll"
dotnet "$tic" -e hw06-child -o "$work_root/directory" "$work_root/source.ti"
entry="$(find "$work_root/directory" -type f -name hw06-child -print -quit)"
test -n "$entry"

for mode in direct routed; do
    candidate="$work_root/$mode.db"
    if test "$mode" = direct; then
        command=(dotnet "$tic")
    else
        command=(dotnet "$router" tic)
    fi
    "${command[@]}" --database-format hashed -e hw06-alias -s -o "$candidate" "$work_root/source.ti" 2> "$work_root/$mode.summary"
    test -f "$candidate"
    test ! -e "$candidate.db"
    grep -F 'tic: format: hashed' "$work_root/$mode.summary"
    grep -F 'tic: compiled entries: 1' "$work_root/$mode.summary"
    grep -F 'tic: alias keys: 1' "$work_root/$mode.summary"
    "$db_verify" "$candidate"
    for name in hw06-child hw06-alias; do
        "$fixture_root/hdb00-probe" "$candidate" "$name" "$work_root/$mode-$name.bin"
        cmp "$entry" "$work_root/$mode-$name.bin"
        # Pinned native infocmp's -A bypasses hashed lookup and reads a directory.
        # TERMINFO selects the native database path, including Hash-v9 files.
        TERMINFO="$candidate" TERMINFO_DIRS="$candidate" \
            "$fixture_root/ncurses-hashed/progs/infocmp" "$name" > "$work_root/$mode-$name.native.txt"
        grep -F "$candidate" "$work_root/$mode-$name.native.txt"
        grep -F 'cols#80' "$work_root/$mode-$name.native.txt"
        grep -F 'lines#24' "$work_root/$mode-$name.native.txt"
        dotnet "$router" infocmp -A "$candidate" "$name" > "$work_root/$mode-$name.managed.txt"
        grep -F 'hw06-child|hw06-alias|HW06 published terminal' "$work_root/$mode-$name.managed.txt"
    done
done
cmp "$work_root/direct.db" "$work_root/routed.db"
printf '%s\n' 'HW06 direct/routed tic publication passed native and managed consumers.'
