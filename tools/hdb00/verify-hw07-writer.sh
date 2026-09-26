#!/usr/bin/env bash
set -euo pipefail

root="$1"
db_verify="$2"
db_dump="$3"
db_load="$4"
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
project="$repo_root/tools/hdb00/hw03-writer-probe/Hw03.ManagedWriterProbe.csproj"
native_directory="$root/hw07-native-directory"
mkdir -p "$native_directory"
"$root/ncurses-directory/progs/tic" -x \
    -e vt100,linux,xterm,xterm-256color,screen,screen-256color,tmux,tmux-256color \
    -o "$native_directory" "$root/ncurses-source/misc/terminfo.src"
dotnet run --project "$project" -c Release -f net10.0 -- hw07-generate "$root" "$native_directory"

for seed in 7 23 131 733; do
    stem="$root/hw07-matrix-$seed"
    "$db_verify" "$stem.db"
    "$db_dump" -k -f "$stem.dump" "$stem.db"
    "$db_load" -f "$stem.dump" "$stem-repacked.db"
    "$db_verify" "$stem-repacked.db"
    "$db_dump" -k -f "$stem-repacked.dump" "$stem-repacked.db"
done
dotnet run --project "$project" -c Release -f net10.0 --no-build -- hw07-verify "$root"

for stem in hw07-native hw07-utf8; do
    candidate="$root/$stem.db"
    "$db_verify" "$candidate"
    "$db_dump" -k -f "$root/$stem.dump" "$candidate"
    while IFS=$'\t' read -r name payload; do
        "$root/hdb00-probe" "$candidate" "$name" "$root/hw07-lookup.bin"
        cmp "$root/$payload" "$root/hw07-lookup.bin"
        TERMINFO="$candidate" TERMINFO_DIRS="$candidate" \
            "$root/ncurses-hashed/progs/infocmp" -x "$name" > "$root/hw07-native-infocmp.txt"
        grep -aF "$candidate" "$root/hw07-native-infocmp.txt"
    done < "$root/$stem.names"
done
printf '%s\n' 'HW07 representative and exact UTF-8 publications passed native consumers.'
