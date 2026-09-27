# tic

`tic` is part of the `Icod.TermInfo` managed terminfo tool suite.

## 1.16 hashed publication

Version `1.16.0` adds explicit Hash-v9 publication through the managed BerkeleyDb
writer. Directory output remains the default. Both standalone `tic` and routed
`icod-terminfo tic` support:

```sh
tic --database-format hashed -o ./terminfo.db source.ti
icod-terminfo tic --database-format hashed -e demo-alias -s -o ./selected.db source.ti
tic --database-format hashed --force -o ./terminfo.db replacement.ti
```

Hashed output requires an explicit `-o` naming the exact destination file; no
suffix is appended and no format is inferred from its name. The parent directory
must already exist. Ambient `TERMINFO`, home, and system locations are never
hashed write destinations. `--force` replaces the whole database with the selected
entries; it does not merge an existing catalog. Alias selection and `use=`
inheritance use the same source-resolution path as directory publication.

The writer stages, flushes, and verifies a complete database before one
same-directory move. A persistent sibling `.name.icod-terminfo.lock` coordinates
Icod writers. Cancellation before commit preserves the old destination; after
commit, the command reports success even if cancellation arrives during its
summary. Filesystem atomicity and open-reader replacement restrictions still
apply; native writers are not coordinated. `-s` reports format, exact normalized
path, canonical-entry count, alias-key count, and warnings on standard error.

`--database-format directory` explicitly selects the existing directory path.
Neither format selection nor publication options may be combined with `-c`.
Use `tic -c source.ti` for validation without filesystem publication artifacts.

## Compatibility with 1.15

Version `1.15.0` carries the frozen `tic` compiler, source-language,
conventional database, and command contracts forward unchanged. Read-only hashed
acquisition is isolated in the optional BerkeleyDb package; `tic` acquires no
hashed writing, new option, dependency, target-framework change, or archive
topology. Both standalone `tic` and routed `icod-terminfo tic` retain their
existing command behavior while reporting the coordinated suite version.

## 1.9 status

Version `1.9.0` carries the frozen `tic` compiler, source-language, database,
and command contracts forward unchanged. Machine-readable inspection and
planning automation are isolated in Inspection, `infocmp`, and `toe`; `tic`
acquires no new semantics or dependencies.

## 1.6.x history

Version `1.6.0` retains the frozen T04/T05 semantic engine, T10 CLI/distribution
contract, and T11 differential, hostile-input, and artifact validation gates.
The 1.6 release adds the separate Termcap package and conversion commands without
changing `tic` compiler, source-language, or command semantics.

Version `1.6.1` is a release-infrastructure hotfix over that frozen 1.6.0
contract. It does not change `tic` compiler, source-language, or command
semantics.

Supported as either `tic ...` from a release archive or
`icod-terminfo tic ...` from the .NET tool:

```text
tic [options] file
tic -c [options] file
tic -D
tic -V
tic --version
tic --help
```

`file` may be `-` to read strict UTF-8 source from standard input. The complete
source document is parsed through `Icod.TermInfo.Source`; selected entries are
resolved with their `use=` inheritance and checked for compiled representability
before publication begins.

Use `-c` for the non-mutating T04 validation path:

```text
tic -c file
tic -c -e name,alias file
tic -c -x file
```

Without `-c`, the default directory mode follows successful validation with publication through
`CompiledTermInfoDatabaseWriter`:

```text
tic -o ./terminfo file
tic -e xterm,xterm-256color -o ./terminfo file
tic --force -o ./terminfo file
tic -s -o ./terminfo file
```

In directory mode, `-o` chooses an explicit conventional database root. When `-o` is absent, the
command selects only these safe writable candidates, in this order:

```text
1. directory-valued TERMINFO
2. the Runtime-defined user database
3. otherwise fail and require -o
```

Encoded `TERMINFO`, `TERMINFO_DIRS`, and platform system/default roots are never
selected implicitly for writes.

Existing destinations are rejected by default. `--force` opts into the
Compiler writer's existing overwrite policy. The writer preflights the complete
publication plan, stages temporary files with write-through, and then commits the
canonical and alias destinations. The command does not duplicate that path or
transaction logic.

`-s` writes a successful publication summary to standard error containing the
normalized destination root, number of compiled source entries, and warning
count. Ordinary successful publication remains quiet.

Known extended capabilities are accepted normally. Syntactically valid unknown
extended capability names require `-x`.

`-D` prints the ordered Runtime database-location discovery model supplied by
`Icod.TermInfo.Inspection`; encoded `TERMINFO` values are identified without
printing their encoded payload.

Exit status follows the command-suite contract:

```text
0    validation/publication succeeded, including warnings-only source
1    source/input/destination/publication failure
2    command usage error
130  cancellation before the publication commit begins
```

In **directory mode**, publication through the synchronous Compiler writer is
one non-interruptible commit boundary. Cancellation is checked before that
boundary; once publication begins, the writer finishes so the command does not
report cancellation after files have actually been committed.

In **hashed mode**, cancellation remains active during image preparation, lock
waiting, staging, and verification, through the final pre-move check. Cancellation
before that check completes leaves the old destination (or its absence) intact.
Once the move begins and succeeds, later cancellation cannot turn the successful
publication into exit status 130. The post-commit summary is also non-cancellable.

The command targets .NET 10. The reusable `Icod.TermInfo` libraries remain
available for `net8.0`, `net9.0`, and `net10.0`.

## Synopsis

```text
tic [options] file
tic -c [options] file
tic -D
tic -V
tic --version
tic --help
```

## Options

```text
-c              validate only; never publish
-e name,...     select canonical names or aliases
-x              permit unknown extended capability names
-o path         directory root, or exact file for hashed output
--database-format directory|hashed   output format; directory is the default
-s              write the successful publication summary to stderr
--force         replace existing compiled destinations safely
-D              report Runtime database discovery locations
-V, --version   print the coordinated tool-suite version
--help          display help
--              end option parsing
```

Unambiguous Boolean short options may be clustered. `-e` and `-o` accept either
a separated or attached value, for example `-edemo` and `-o./terminfo`.

## Operands

Exactly one source operand is accepted. `-` means standard input. Use `--`
before a source filename beginning with `-`.

## Environment

When publishing directory output without `-o`, `tic` considers a directory-valued `TERMINFO`,
then the Runtime-defined user database. Encoded `TERMINFO`, `TERMINFO_DIRS`, and
platform system roots are not implicit write destinations. `-D` reports the
Runtime discovery model without mutating environment variables.

## Exit statuses

```text
0    success, including warnings-only validation
1    source/input/destination/publication failure
2    usage error
130  cancellation before the publication commit boundary
```

## Examples

```text
tic -c -- source.ti
tic -cx -edemo source.ti
tic -o./terminfo source.ti
tic --force -s -o ./terminfo source.ti
```

## Compatibility

The command adopts mainstream ncurses option names only where the existing Icod
engines implement the semantics honestly. Unsupported ncurses switches are
usage errors and are never silently ignored. Unlike native `tic` variants, Icod
does not implicitly write platform system databases.

## Non-goals

T10 does not add termcap conversion, historical vendor subsets, translation
presentation modes, C initializer generation, trace internals, or candidate
`-Q1/-Q2/-Q3` / `-v[n]` features.
