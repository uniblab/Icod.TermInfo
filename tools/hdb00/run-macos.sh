#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
ncurses_commit="87c2c84cbd2332d6d94b12a1dcaf12ad1a51a938"
work_root="${RUNNER_TEMP:-/tmp}/icod-terminfo-hdb00-macos"
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
overflow_directory_db="$work_root/overflow-directory-db"
overflow_hashed_base="$work_root/overflow-hashed-db"
overflow_hashed_db="$overflow_hashed_base.db"
overflow_fixture="$work_root/hdb00-overflow.src"
overflow_payload="$work_root/hdb00-overflow.bin"
multi_hashed_base="$work_root/multi-hashed-db"
multi_hashed_db="$multi_hashed_base.db"
multi_fixture="$work_root/hdb07-multi.src"
multi_dump="$work_root/multi-hashed-db.dump"
repacker="$work_root/hdb07c-repack"
big_endian_db="$work_root/big-endian-hashed-db.db"
big_endian_dump="$work_root/big-endian-hashed-db.dump"
source_dump="$work_root/hashed-db.dump"
big_endian_primary="$work_root/hdb07c-big-endian-primary.bin"
big_endian_alias="$work_root/hdb07c-big-endian-alias.bin"
native_verifier_project="$repo_root/tools/hdb00/native-verifier/Hdb07c.NativeVerifier.csproj"
db_prefix="$(brew --prefix berkeley-db@5)"

rm -rf "$work_root"
mkdir -p \
    "$work_root" \
    "$source_root" \
    "$directory_source" \
    "$hashed_source" \
    "$directory_db" \
    "$overflow_directory_db"

printf '%s\n' "== HDB00 macOS: Berkeley DB prefix =="
printf '%s\n' "$db_prefix"

printf '%s\n' "== HDB00 macOS: fetch pinned ncurses source =="
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

printf '%s\n' "== HDB00 macOS: build conventional ncurses tic =="
(
    cd "$directory_source"
    ./configure "${configure_common[@]}" --without-hashed-db >"$work_root/configure-directory.log"
    make -j2 >"$work_root/build-directory.log"
)

printf '%s\n' "== HDB00 macOS: build Berkeley DB-backed ncurses tic =="
(
    cd "$hashed_source"
    CPPFLAGS="-I$db_prefix/include" \
    LDFLAGS="-L$db_prefix/lib" \
        ./configure "${configure_common[@]}" --with-hashed-db="$db_prefix" >"$work_root/configure-hashed.log"
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

python3 - "$overflow_fixture" <<'PY'
from pathlib import Path
import sys

blob = "A" * 2800
Path(sys.argv[1]).write_text(
    "hdb00-overflow|Icod HDB00 overflow terminfo fixture,\n"
    "    am,\n"
    "    cols#80,\n"
    "    lines#24,\n"
    "    colors#8,\n"
    "    clear=\\E[H\\E[2J,\n"
    "    cup=\\E[%i%p1%d;%p2%dH,\n"
    f"    hdb00blob={blob},\n",
    encoding="utf-8",
)
PY

python3 - "$multi_fixture" <<'PY'
from pathlib import Path
import sys

entries = []
for index in range(64):
    number = f"{index:03d}"
    canonical = f"hdb07-multi-{number}"
    alias = f"{canonical}-alias"
    entries.append(
        f"{canonical}|{alias}|Icod HDB07 multi {number} fixture,\n"
        "    am,\n"
        f"    cols#{80 + index},\n"
        "    lines#24,\n"
        "    clear=\\E[H\\E[2J,\n"
    )

Path(sys.argv[1]).write_text("".join(entries), encoding="ascii")
PY

printf '%s\n' "== HDB00 macOS: compile identical source into directory and hashed stores =="
"$directory_source/progs/tic" -x -o "$directory_db" "$fixture"
DYLD_LIBRARY_PATH="$db_prefix/lib${DYLD_LIBRARY_PATH:+:$DYLD_LIBRARY_PATH}" \
    "$hashed_source/progs/tic" -x -o "$hashed_base" "$fixture"

test -f "$hashed_db"
directory_entry="$(find "$directory_db" -type f -name hdb00-primary -print -quit)"
test -n "$directory_entry"

printf '%s\n' "== HDB00 macOS: build direct Berkeley DB read-only probe =="
cc \
    -std=c11 \
    -Wall \
    -Wextra \
    -Werror \
    -I"$db_prefix/include" \
    "$repo_root/tools/hdb00/hdb00_probe.c" \
    -L"$db_prefix/lib" \
    -ldb \
    -o "$probe"

printf '%s\n' "== HDB07C macOS: build CI-only Berkeley DB repacker =="
cc \
    -std=c11 \
    -Wall \
    -Wextra \
    -Werror \
    -I"$db_prefix/include" \
    "$repo_root/tools/hdb00/hdb07c_repack.c" \
    -L"$db_prefix/lib" \
    -ldb \
    -o "$repacker"

run_probe() {
    DYLD_LIBRARY_PATH="$db_prefix/lib${DYLD_LIBRARY_PATH:+:$DYLD_LIBRARY_PATH}" \
        "$probe" "$@"
}

run_hashed_tic() {
    DYLD_LIBRARY_PATH="$db_prefix/lib${DYLD_LIBRARY_PATH:+:$DYLD_LIBRARY_PATH}" \
        "$hashed_source/progs/tic" "$@"
}

run_repacker() {
    DYLD_LIBRARY_PATH="$db_prefix/lib${DYLD_LIBRARY_PATH:+:$DYLD_LIBRARY_PATH}" \
        "$repacker" "$@"
}

printf '%s\n' "== HDB00 macOS: exact canonical-name lookup =="
run_probe "$hashed_db" hdb00-primary "$primary_payload" | tee "$work_root/primary-probe.txt"

printf '%s\n' "== HDB00 macOS: exact alias lookup =="
run_probe "$hashed_db" hdb00-alias "$alias_payload" | tee "$work_root/alias-probe.txt"

printf '%s\n' "== HDB00 macOS: prove alias and canonical requests reach the same compiled bytes =="
cmp "$primary_payload" "$alias_payload"

printf '%s\n' "== HDB00 macOS: prove hashed payload equals same-source conventional compiled entry =="
cmp "$primary_payload" "$directory_entry"

printf '%s\n' "== HDB07C macOS: produce and verify native big-endian Hash container =="
run_repacker "$hashed_db" "$big_endian_db" 4321
DYLD_LIBRARY_PATH="$db_prefix/lib${DYLD_LIBRARY_PATH:+:$DYLD_LIBRARY_PATH}" \
    "$db_prefix/bin/db_dump" -k -f "$source_dump" "$hashed_db"
DYLD_LIBRARY_PATH="$db_prefix/lib${DYLD_LIBRARY_PATH:+:$DYLD_LIBRARY_PATH}" \
    "$db_prefix/bin/db_dump" -k -f "$big_endian_dump" "$big_endian_db"
dotnet run \
    --project "$native_verifier_project" \
    -c Release \
    -- \
    "$big_endian_db" \
    "$source_dump" \
    "$big_endian_dump"
run_probe \
    "$big_endian_db" \
    hdb00-primary \
    "$big_endian_primary"
run_probe \
    "$big_endian_db" \
    hdb00-alias \
    "$big_endian_alias"
cmp "$big_endian_primary" "$primary_payload"
cmp "$big_endian_alias" "$primary_payload"

printf '%s\n' "== HDB00 macOS: prove existing managed parser accepts extracted bytes unchanged =="
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

printf '%s\n' "== HDB00 macOS: clean miss is distinct from corruption =="
set +e
run_probe "$hashed_db" hdb00-missing "$work_root/missing.bin" >"$work_root/missing.out" 2>"$work_root/missing.err"
missing_status=$?
set -e
if [[ $missing_status -ne 3 ]]; then
    echo "Expected clean-miss status 3, got $missing_status" >&2
    cat "$work_root/missing.err" >&2
    exit 1
fi
grep -F "Clean miss for 'hdb00-missing'." "$work_root/missing.err"

printf '%s\n' "== HDB00 macOS: wrong Berkeley DB access method is rejected =="
printf 'key\tvalue\n' | "$db_prefix/bin/db_load" -T -t btree "$work_root/not-hash.db"
set +e
run_probe "$work_root/not-hash.db" key "$work_root/not-hash.bin" >"$work_root/not-hash.out" 2>"$work_root/not-hash.err"
wrong_type_status=$?
set -e
if [[ $wrong_type_status -eq 0 ]]; then
    echo "A Btree database was incorrectly accepted as DB_HASH." >&2
    exit 1
fi
cat "$work_root/not-hash.err"

printf '%s\n' "== HDB00 macOS: arbitrary bytes are rejected as a database =="
printf 'not a Berkeley DB\n' >"$work_root/random.db"
set +e
run_probe "$work_root/random.db" key "$work_root/random.bin" >"$work_root/random.out" 2>"$work_root/random.err"
random_status=$?
set -e
if [[ $random_status -eq 0 ]]; then
    echo "Arbitrary bytes were incorrectly accepted as DB_HASH." >&2
    exit 1
fi
cat "$work_root/random.err"

printf '%s\n' "== HDB00 macOS: force and verify Berkeley DB overflow pages =="
"$directory_source/progs/tic" -x -o "$overflow_directory_db" "$overflow_fixture"
run_hashed_tic -x -o "$overflow_hashed_base" "$overflow_fixture"
test -f "$overflow_hashed_db"
overflow_directory_entry="$(find "$overflow_directory_db" -type f -name hdb00-overflow -print -quit)"
test -n "$overflow_directory_entry"
run_probe \
    "$overflow_hashed_db" \
    hdb00-overflow \
    "$overflow_payload" \
    | tee "$work_root/overflow-native-probe.txt"
cmp "$overflow_payload" "$overflow_directory_entry"
python3 \
    "$repo_root/tools/hdb00/assert-overflow-pages.py" \
    "$overflow_hashed_db" \
    | tee "$work_root/overflow-pages.txt"
dotnet run \
    --project "$repo_root/samples/Icod.TermInfo.Acquisition.Sample/Icod.TermInfo.Acquisition.Sample.csproj" \
    -c Release \
    -f net10.0 \
    -- \
    parse "$overflow_payload" \
    | tee "$work_root/overflow-managed-parse.txt"
grep -F "Name: hdb00-overflow" "$work_root/overflow-managed-parse.txt"

printf '%s\n' "== HDB07 macOS: generate native 64-entry Hash-v9 matrix =="
run_hashed_tic -x -o "$multi_hashed_base" "$multi_fixture"
test -f "$multi_hashed_db"
DYLD_LIBRARY_PATH="$db_prefix/lib${DYLD_LIBRARY_PATH:+:$DYLD_LIBRARY_PATH}" \
    "$db_prefix/bin/db_dump" -k -f "$multi_dump" "$multi_hashed_db"

multi_record_count="$(python3 - "$multi_dump" <<'PY'
from pathlib import Path
import sys

lines = Path(sys.argv[1]).read_text(encoding="ascii").splitlines()
header_end = lines.index("HEADER=END")
if lines[-1] != "DATA=END":
    raise SystemExit("The native multi-record dump is missing DATA=END.")
data_lines = lines[header_end + 1:-1]
if not data_lines or len(data_lines) % 2:
    raise SystemExit("The native multi-record dump has malformed key/value lines.")
print(len(data_lines) // 2)
PY
)"
if [[ "$multi_record_count" -ne 192 ]]; then
    echo "Expected 192 native multi-record entries, got $multi_record_count." >&2
    exit 1
fi
printf 'HDB07 multi native records: %s\n' "$multi_record_count"

for index in 000 032 063; do
    canonical="hdb07-multi-$index"
    alias="$canonical-alias"
    primary_output="$work_root/$canonical.bin"
    alias_output="$work_root/$alias.bin"
    run_probe "$multi_hashed_db" "$canonical" "$primary_output"
    run_probe "$multi_hashed_db" "$alias" "$alias_output"
    cmp "$primary_output" "$alias_output"
done

printf '%s\n' "== HDB00 macOS: evidence summary =="
printf 'ncurses commit: %s\n' "$ncurses_commit"
printf 'Berkeley DB prefix: %s\n' "$db_prefix"
printf 'hashed store: %s\n' "$hashed_db"
printf 'conventional entry: %s\n' "$directory_entry"
printf 'overflow hashed store: %s\n' "$overflow_hashed_db"
printf 'overflow conventional entry: %s\n' "$overflow_directory_entry"
printf 'multi-record hashed store: %s\n' "$multi_hashed_db"
printf 'big-endian hashed store: %s\n' "$big_endian_db"
shasum -a 256 \
    "$hashed_db" \
    "$big_endian_db" \
    "$big_endian_dump" \
    "$directory_entry" \
    "$primary_payload" \
    "$alias_payload" \
    "$overflow_hashed_db" \
    "$overflow_directory_entry" \
    "$overflow_payload" \
    "$multi_hashed_db" \
    "$multi_dump"
printf '%s\n' "HDB00 macOS interoperability probe passed."
