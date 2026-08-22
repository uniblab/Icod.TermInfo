# Terminfo Metadata Tooling

This directory is reserved for deterministic tooling and checked-in reference
data used to build or validate the complete standard capability metadata
introduced by T22.

Runtime code remains under `src/Capabilities/`. Nothing in this directory is a
runtime package dependency.

## Rules

- No network access is required by normal build or test.
- Upstream provenance must be recorded for imported capability tables.
- Existing 0.7 enum numeric values are immutable.
- New capability enum members are append-only.
- Managed enum numeric values are never compiled terminfo binary indices.
- The canonical metadata record owns the future binary index.
- Generated output used by the runtime is checked in and reviewable.
- Regeneration must be deterministic.

T21 establishes this convention. T22 chooses and implements the concrete
canonical metadata file/tool format.
