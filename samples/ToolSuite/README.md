# Icod.TermInfo 1.9 Tool-Suite Sample

This sample exercises the coordinated `tic`, `infocmp`, `toe`, `captoinfo`, and
`infotocap` command suite against controlled terminfo and termcap source. It uses
an explicit local terminfo database root and checked-in source files so results do
not depend on host-installed terminfo or termcap databases.

The commands below use the standalone release-archive launchers. When
`Icod.TermInfo.Tools` is installed as a .NET tool, prefix the same command lines
with `icod-terminfo`, for example `icod-terminfo tic -c -x example.ti`. Run them
from this directory, or adjust the paths as appropriate.

## Terminfo source

`example.ti` defines three entries:

- `icod-demo-base` (`idb`) supplies basic screen geometry and cursor/screen
  capabilities;
- `icod-demo-child` (`idc`) inherits the base through `use=`, overrides `cols`,
  and adds the deliberately unknown extended string capability `IcodDemo`.
- `icod-demo-decoy` (`idd`) is an intentionally inferior planning candidate.

`planning-parent.ti` repeats only the base entry so redirected planning output
can be combined with its selected parent and validated independently.

The unknown extended capability makes `-x` meaningful in the validation,
publication, and rendering examples. It is intentionally not conventional
termcap-representable, so the successful `infotocap` walkthrough below uses the
separate representable `example.termcap` path instead of flattening `example.ti`.

## Validate without publishing

```text
tic -c -x example.ti
```

Validation parses the complete source document, resolves `use=` inheritance,
and checks compiled representability without creating a database. Omitting `-x`
demonstrates the stricter default policy for unknown extended capability names.

## Publish to a controlled database

```text
tic -x -o ./terminfo example.ti
```

The resulting `./terminfo` directory is a conventional compiled terminfo
database containing all three entries and their aliases. To repeat publication over
existing destinations, opt into replacement explicitly:

```text
tic --force -x -o ./terminfo example.ti
```

## Render the inherited child

```text
infocmp -A ./terminfo -1 -x icod-demo-child
```

The effective child description should include the inherited base capabilities,
`cols#120`, and the extended `IcodDemo` value.

## Synthesize the child relative to the base

Version 1.7 adds deterministic relative-source synthesis through `infocmp -u`.
Use the same controlled database for the target and parent:

```text
infocmp -A ./terminfo -B ./terminfo -1 -x -u icod-demo-child icod-demo-base
```

The output preserves the target identity, emits the child-local `cols#120` and
`IcodDemo` values, and ends with:

```text
use=icod-demo-base,
```

Capabilities already supplied by `icod-demo-base` are omitted from the local
delta. The emitted source can therefore be combined with source for the base and
resolved back to the same effective child description. Omitting `-x` fails
rather than silently discarding the child-local extended capability.

## Plan the child from explicit candidates

Version 1.8 adds bounded deterministic parent selection through
`infocmp --plan-use`. Supply the decoy first to demonstrate that candidate order
is a final tie-break rather than a first-match rule:

RP08 freezes this direct and routed walkthrough as release evidence for the
stable 1.8 command and distribution contract.

```text
infocmp -A ./terminfo -B ./terminfo -1 -x --max-parents 1 --require-exhaustive --plan-use icod-demo-child icod-demo-decoy icod-demo-base > planned-child.ti
```

The selected source contains `use=icod-demo-base` and does not contain
`use=icod-demo-decoy`. Standard output contains only that source. The same
operation through the installable router is:

```text
icod-terminfo infocmp -A ./terminfo -B ./terminfo -1 -x --max-parents 1 --require-exhaustive --plan-use icod-demo-child icod-demo-decoy icod-demo-base > planned-child-routed.ti
```

The two output files are byte-for-byte equal. Combine either output with the
checked-in selected-parent source and validate the generated state:

```text
cat planned-child.ti planning-parent.ti > planned-validation.ti
tic -c -x planned-validation.ti
```

This planning form uses only the explicit operands and the explicit `-A` and
`-B` roots. It does not enumerate the catalog or discover additional candidates.

## Compare the base and child

```text
infocmp -A ./terminfo -B ./terminfo -d -x icod-demo-base icod-demo-child
```

This reports semantic differences between the two compiled descriptions. A
semantic difference is normal command output and does not by itself make the
comparison fail.

## Produce machine-readable inspection and planning documents

Version 1.9 projects the same immutable Inspection values through one versioned
JSON envelope. Effective-description and comparison documents are:

```text
infocmp --json -A ./terminfo icod-demo-child > description.json
infocmp --json -d -A ./terminfo -B ./terminfo icod-demo-base icod-demo-child > comparison.json
```

Explicit-candidate and explicit-directory all-candidates planning are:

```text
infocmp --json --plan-use -A ./terminfo -B ./terminfo icod-demo-child icod-demo-decoy icod-demo-base > plan.json
infocmp --json --plan-use --all-candidates --max-parents 1 \
  -A ./terminfo -B ./terminfo icod-demo-child \
  > all-candidates-plan.json
```

The all-candidates form inspects only `./terminfo`, uses canonical catalog order,
and excludes the target. Each successful command writes exactly one compact JSON
document followed by one LF. The routed forms are byte-identical, for example:

```text
icod-terminfo infocmp --json -A ./terminfo icod-demo-child > description-routed.json
```

## Enumerate the database

```text
toe -hs ./terminfo
```

`toe` reads canonical identities and descriptions from the compiled entries
rather than inferring identities from filenames.

The exact explicit catalog is available as a `databaseCatalog` document:

```text
toe --json ./terminfo > catalog.json
icod-terminfo toe --json ./terminfo > catalog-routed.json
```

## Inspect source dependencies

Forward `use=` edges:

```text
toe -u example.ti
```

The sample contains the edge:

```text
icod-demo-child	icod-demo-base
```

Reverse `use=` edges:

```text
toe -U example.ti
```

The same relationship is reported as:

```text
icod-demo-base	icod-demo-child
```

## Convert termcap to effective terminfo

`example.termcap` is deliberately simple and fully representable. Convert it to
resolved terminfo source with:

```text
captoinfo example.termcap > converted-from-termcap.ti
```

The resulting source should contain the `icod-demo-cap` identity together with
`am`, `cols#80`, `lines#24`, and the clear-screen capability. The conversion is
effective state; original termcap formatting and source ancestry are not
reconstructed.

## Convert the effective terminfo back to termcap

Use the just-produced representable terminfo source for the reverse direction:

```text
infotocap converted-from-termcap.ti > roundtrip.termcap
```

The rendered termcap should contain `am`, `co#80`, and `li#24`. A description
which cannot be represented faithfully is rejected rather than silently
approximated.

## Cleanup

Remove the explicit local database and redirected conversion outputs when the
walkthrough is complete:

```text
rm -rf ./terminfo converted-from-termcap.ti roundtrip.termcap \
  planned-child.ti planned-child-routed.ti planned-validation.ti \
  description.json description-routed.json comparison.json plan.json \
  all-candidates-plan.json catalog.json catalog-routed.json
```

On shells without `rm`, remove the same paths with the host's normal file tools.

For the complete command contracts and supported options, see
`../../tic/README.md`, `../../infocmp/README.md`, `../../toe/README.md`,
`../../captoinfo/README.md`, and `../../infotocap/README.md`.
