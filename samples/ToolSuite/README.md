# Icod.TermInfo 1.5 Tool-Suite Sample

This sample exercises the coordinated `tic`, `infocmp`, and `toe` command suite
against one controlled terminfo source file. It deliberately uses an explicit
local database root so results do not depend on the host's installed terminfo
database.

The commands below use the standalone release-archive launchers. When
`Icod.TermInfo.Tools` is installed as a .NET tool, prefix the same command lines
with `icod-terminfo`, for example `icod-terminfo tic -c -x example.ti`. Run them
from this directory, or adjust the paths as appropriate.

## Source

`example.ti` defines two entries:

- `icod-demo-base` (`idb`) supplies basic screen geometry and cursor/screen
  capabilities;
- `icod-demo-child` (`idc`) inherits the base through `use=`, overrides
  `cols`, and adds the deliberately unknown extended string capability
  `IcodDemo`.

The unknown extended capability makes `-x` meaningful in the validation,
publication, and rendering examples.

## Validate without publishing

```text
tic -c -x example.ti
```

Validation parses the complete source document, resolves `use=` inheritance,
and checks compiled representability without creating a database.

Omitting `-x` demonstrates the stricter default policy for unknown extended
capability names.

## Publish to a controlled database

```text
tic -x -o ./terminfo example.ti
```

The resulting `./terminfo` directory is a conventional compiled terminfo
database containing both entries and their aliases. To repeat publication over
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

## Compare the base and child

```text
infocmp -A ./terminfo -B ./terminfo -d -x icod-demo-base icod-demo-child
```

This reports semantic differences between the two compiled descriptions. A
semantic difference is normal command output and does not by itself make the
comparison fail.

## Enumerate the database

```text
toe -hs ./terminfo
```

`toe` reads canonical identities and descriptions from the compiled entries
rather than inferring identities from filenames.

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

## Cleanup

The only generated state is the explicit local database root. Remove
`./terminfo` when the walkthrough is complete.

For the complete command contracts and supported options, see
`../../tic/README.md`, `../../infocmp/README.md`, and `../../toe/README.md`.
