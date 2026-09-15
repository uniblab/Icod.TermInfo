#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 1 ]]; then
    echo "Usage: $0 WORK_ROOT" >&2
    exit 64
fi

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
work_root="$1"
database="$work_root/hashed-db.db"
managed_primary="$work_root/hdb00-managed-primary.bin"
managed_alias="$work_root/hdb00-managed-alias.bin"
overflow_database="$work_root/overflow-hashed-db.db"
overflow_native="$work_root/hdb00-overflow.bin"
overflow_managed="$work_root/hdb00-managed-overflow.bin"
project="$repo_root/tools/hdb00/managed/Hdb00.ManagedProbe.csproj"

printf '%s\n' "== HDB00 managed: exact canonical-name lookup =="
dotnet run \
    --project "$project" \
    -c Release \
    -- \
    "$database" \
    hdb00-primary \
    "$managed_primary" \
    | tee "$work_root/managed-primary-probe.txt"

printf '%s\n' "== HDB00 managed: exact alias lookup =="
dotnet run \
    --project "$project" \
    -c Release \
    -- \
    "$database" \
    hdb00-alias \
    "$managed_alias" \
    | tee "$work_root/managed-alias-probe.txt"

printf '%s\n' "== HDB00 managed: prove managed and native extraction are identical =="
cmp "$managed_primary" "$work_root/hdb00-primary.bin"
cmp "$managed_alias" "$work_root/hdb00-alias.bin"
cmp "$managed_primary" "$managed_alias"

grep -F "Berkeley DB Hash version: 9" "$work_root/managed-primary-probe.txt"
grep -F "Hops: 2" "$work_root/managed-primary-probe.txt"
grep -F "Data records: 1" "$work_root/managed-primary-probe.txt"
grep -F "Index records: 2" "$work_root/managed-primary-probe.txt"

printf '%s\n' "== HDB00 managed: clean miss is distinct =="
set +e
dotnet run \
    --project "$project" \
    -c Release \
    -- \
    "$database" \
    hdb00-missing \
    "$work_root/hdb00-managed-missing.bin" \
    >"$work_root/managed-missing.out" \
    2>"$work_root/managed-missing.err"
missing_status=$?
set -e
if [[ $missing_status -ne 3 ]]; then
    echo "Expected managed clean-miss status 3, got $missing_status" >&2
    cat "$work_root/managed-missing.err" >&2
    exit 1
fi
grep -F "Clean miss for 'hdb00-missing'." "$work_root/managed-missing.err"

printf '%s\n' "== HDB00 managed: wrong access method is rejected =="
set +e
dotnet run \
    --project "$project" \
    -c Release \
    -- \
    "$work_root/not-hash.db" \
    key \
    "$work_root/hdb00-managed-not-hash.bin" \
    >"$work_root/managed-not-hash.out" \
    2>"$work_root/managed-not-hash.err"
wrong_type_status=$?
set -e
if [[ $wrong_type_status -eq 0 ]]; then
    echo "The managed reader incorrectly accepted a Btree database." >&2
    exit 1
fi
cat "$work_root/managed-not-hash.err"

printf '%s\n' "== HDB00 managed: arbitrary bytes are rejected =="
set +e
dotnet run \
    --project "$project" \
    -c Release \
    -- \
    "$work_root/random.db" \
    key \
    "$work_root/hdb00-managed-random.bin" \
    >"$work_root/managed-random.out" \
    2>"$work_root/managed-random.err"
random_status=$?
set -e
if [[ $random_status -eq 0 ]]; then
    echo "The managed reader incorrectly accepted arbitrary bytes." >&2
    exit 1
fi
cat "$work_root/managed-random.err"

printf '%s\n' "== HDB00 managed: reconstruct forced overflow record =="
test -f "$overflow_database"
test -f "$overflow_native"
dotnet run \
    --project "$project" \
    -c Release \
    -- \
    "$overflow_database" \
    hdb00-overflow \
    "$overflow_managed" \
    | tee "$work_root/managed-overflow-probe.txt"
cmp "$overflow_managed" "$overflow_native"
grep -F "Berkeley DB Hash version: 9" "$work_root/managed-overflow-probe.txt"
python3 \
    "$repo_root/tools/hdb00/assert-overflow-pages.py" \
    "$overflow_database" \
    | tee "$work_root/managed-overflow-pages.txt"

printf '%s\n' "HDB00 managed Hash v9 probe passed."
