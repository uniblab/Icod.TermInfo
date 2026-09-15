#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
ncurses_commit="87c2c84cbd2332d6d94b12a1dcaf12ad1a51a938"
work_root="${RUNNER_TEMP:-/tmp}/icod-terminfo-hdb00"
source_root="$work_root/ncurses-source"
directory_source="$work_root/ncurses-directory"
hashed_source="$work_root/ncurses-hashed"
directory_db="$work_root/directory-db"
hashed_base="$work_root/hashed-db"
hashed_db="$hashed_base.db"
fixture="$work_root/hdb00.src"
probe="$work_root/hdb00-probe"
primary_payload="$work_root/hdb00-primary.bin"
alias_payload="$work_root/hdb00-alias.bin"

rm -rf "$work_root"
mkdir -p "$work_root" "$source_root" "$directory_source" "$hashed_source" "$directory_db"

printf '%s\n' "== HDB00: fetch pinned ncurses source =="
git -C "$source_root" init -q
git -C "$source_root" remote add origin https://github.com/mirror/ncurses.git
git -C "$source_root" fetch -q --depth=1 origin "$ncurses_commit"
git -C "$source_root" checkout -q --detach FETCH_HEAD
git -C "$source_root" archive HEAD | tar -x -C "$directory_source"
git -C "$source_root" archive HEAD | tar -x -C "$hashed_source"

configure_common=(
    --without-debug
    --without-ada
    --without-cxx
    --without-cxx-binding
    --without-tests
    --disable-db-install
)

printf '%s\n' "== HDB00: build conventional ncurses tic =="
(
    cd "$directory_source"
    ./configure "${configure_common[@]}" --without-hashed-db >"$work_root/configure-directory.log"
    make -j2 >"$work_root/build-directory.log"
)

printf '%s\n' "== HDB00: build Berkeley DB-backed ncurses tic =="
(
    cd "$hashed_source"
    ./configure "${configure_common[@]}" --with-hashed-db=/usr >"$work_root/configure-hashed.log"
    make -j2 >"$work_root/build-hashed.log"
)

cat >"$fixture" <<'EOF'
hdb00-primary|hdb00-alias|Icod HDB00 hashed terminfo fixture,
    am,
    cols#80,
    lines#24,
    colors#8,
    clear=\E[H\E[2J,
    cup=\E[%i%p1%d;%p2%dH,
    setaf=\E[3%p1%dm,
    setab=\E[4%p1%dm,
EOF

printf '%s\n' "== HDB00: compile identical source into directory and hashed stores =="
"$directory_source/progs/tic" -x -o "$directory_db" "$fixture"
"$hashed_source/progs/tic" -x -o "$hashed_base" "$fixture"

test -f "$hashed_db"
directory_entry="$(find "$directory_db" -type f -name hdb00-primary -print -quit)"
test -n "$directory_entry"

printf '%s\n' "== HDB00: build direct Berkeley DB read-only probe =="
cc \
    -std=c11 \
    -Wall \
    -Wextra \
    -Werror \
    "$repo_root/tools/hdb00/hdb00_probe.c" \
    -ldb \
    -o "$probe"

printf '%s\n' "== HDB00: exact canonical-name lookup =="
"$probe" "$hashed_db" hdb00-primary "$primary_payload" | tee "$work_root/primary-probe.txt"

printf '%s\n' "== HDB00: exact alias lookup =="
"$probe" "$hashed_db" hdb00-alias "$alias_payload" | tee "$work_root/alias-probe.txt"

printf '%s\n' "== HDB00: prove alias and canonical requests reach the same compiled bytes =="
cmp "$primary_payload" "$alias_payload"

printf '%s\n' "== HDB00: prove hashed payload equals same-source conventional compiled entry =="
cmp "$primary_payload" "$directory_entry"

printf '%s\n' "== HDB00: prove existing managed parser accepts extracted bytes unchanged =="
dotnet run \
    --project "$repo_root/samples/Icod.TermInfo.Acquisition.Sample/Icod.TermInfo.Acquisition.Sample.csproj" \
    -c Release \
    -f net10.0 \
    -- \
    parse "$primary_payload" \
    | tee "$work_root/managed-parse.txt"
grep -F "Name: hdb00-primary" "$work_root/managed-parse.txt"
grep -F "Aliases: hdb00-alias" "$work_root/managed-parse.txt"
grep -F "Columns: 80" "$work_root/managed-parse.txt"
grep -F "Lines: 24" "$work_root/managed-parse.txt"

printf '%s\n' "== HDB00: clean miss is distinct from corruption =="
set +e
"$probe" "$hashed_db" hdb00-missing "$work_root/missing.bin" >"$work_root/missing.out" 2>"$work_root/missing.err"
missing_status=$?
set -e
if [[ $missing_status -ne 3 ]]; then
    echo "Expected clean-miss status 3, got $missing_status" >&2
    cat "$work_root/missing.err" >&2
    exit 1
fi
grep -F "Clean miss for 'hdb00-missing'." "$work_root/missing.err"

printf '%s\n' "== HDB00: wrong Berkeley DB access method is rejected =="
printf 'key\tvalue\n' | db5.3_load -T -t btree "$work_root/not-hash.db"
set +e
"$probe" "$work_root/not-hash.db" key "$work_root/not-hash.bin" >"$work_root/not-hash.out" 2>"$work_root/not-hash.err"
wrong_type_status=$?
set -e
if [[ $wrong_type_status -eq 0 ]]; then
    echo "A Btree database was incorrectly accepted as DB_HASH." >&2
    exit 1
fi
cat "$work_root/not-hash.err"

printf '%s\n' "== HDB00: arbitrary bytes are rejected as a database =="
printf 'not a Berkeley DB\n' >"$work_root/random.db"
set +e
"$probe" "$work_root/random.db" key "$work_root/random.bin" >"$work_root/random.out" 2>"$work_root/random.err"
random_status=$?
set -e
if [[ $random_status -eq 0 ]]; then
    echo "Arbitrary bytes were incorrectly accepted as DB_HASH." >&2
    exit 1
fi
cat "$work_root/random.err"

printf '%s\n' "== HDB00: evidence summary =="
printf 'ncurses commit: %s\n' "$ncurses_commit"
printf 'hashed store: %s\n' "$hashed_db"
printf 'conventional entry: %s\n' "$directory_entry"
sha256sum "$hashed_db" "$directory_entry" "$primary_payload" "$alias_payload"
printf '%s\n' "HDB00 Linux interoperability probe passed."
