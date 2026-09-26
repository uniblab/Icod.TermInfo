# HW05 Safe Filesystem Commit Implementation Plan

> **For agentic workers:** Use `superpowers:executing-plans` to implement this plan task by task, inline. The user has explicitly prohibited subagents. Checkbox steps track execution.

**Goal:** Publish a fully verified Hash-v9 image under a persistent destination lock, preserving the previous destination on every pre-commit failure.

**Architecture:** Keep the accepted in-memory writer intact and delegate publication to an internal publisher. Separate filesystem operations, lock acquisition, and reopened-image verification so controlled failure tests exercise the real publication lifecycle. Hold the lock through staging, verification, commit, and failure cleanup.

**Tech Stack:** C# 13; net8.0/net9.0/net10.0; System.IO; existing xUnit infrastructure; PowerShell 5.1-compatible scripts and cmd/sh only.

**Spec:** `docs/superpowers/specs/2026-09-24-hw05-safe-filesystem-commit-design.md`

**Baseline:** `e1d87bded9c5d7facb11809a948b09e7fa5ead00` on `1.16.0-berkeley-db-hash-writer`, PR #47.

**Status:** Written for review. Implementation has not started. Execution method is already selected: inline, without subagents.

## Global constraints

- Preserve all accepted Hash-v9 image bytes and the frozen public API.
- Keep BerkeleyDb Runtime-only, pure managed, IL-only, without native assets, P/Invoke, or new production dependencies.
- Preserve net8.0/net9.0/net10.0 API equivalence and reusable assembly version `1.0.0.0`.
- Advance `IcodTermInfoSuiteVersion` from `1.16.0-Alpha-4` to `1.16.0-Alpha-5` with the first implementation test commit; Version and PackageVersion continue consuming it. Do not bump for this planning commit.
- Preserve input validation and complete image construction before filesystem publication.
- Use fresh replacement metadata, persistent per-destination sibling locks, cancellation-aware indefinite contention waits, and no new public options.
- Keep the lock file; never use DeleteOnClose, unlink it, truncate it, or write ownership records into it.
- Honor cancellation until the final pre-move check; never observe cancellation after commit begins.
- Never delete a temporary pathname unless this attempt successfully created it.
- Never fall back to an application-level copy/delete publication sequence.
- Confine guarantees to cooperative Icod writers on filesystems enforcing the required locking and same-directory move semantics.
- Keep `tic` integration in HW06 and migration/catalog automation in 1.17.
- Use C#, PowerShell 5.1, cmd/sh, project XML, workflow YAML, and Markdown; no Python.
- Do not wait for workflows after documentation-only commits.

## Review focus

1. Successful open without enforced Unix locking must fail closed before staging (Task 1).
2. Existing or dangling lock symlinks must be rejected before opening, and failed CreateNew must never delete someone else's file (Tasks 1 and 3).
3. Reader cancellation must reach both stable-observation passes without changing the public reader contract (Task 2).
4. Write failure followed by stream-close or cleanup failure must retain the original exception (Task 3).
5. Cancellation arriving inside successful commit must return success; process death while holding a lock must allow a subsequent process to acquire it (Tasks 3 and 4).

## File map and internal interfaces

Create under `Icod.TermInfo.BerkeleyDb/src/`:

- `BerkeleyDbDatabasePublicationFileSystem.cs`: internal abstract filesystem seam.
- `SystemBerkeleyDbDatabasePublicationFileSystem.cs`: production System.IO implementation and path validation.
- `BerkeleyDbDatabasePublicationLock.cs`: acquisition retry, enforcement check, cancellation, lifetime.
- `BerkeleyDbDatabasePublicationVerifier.cs`: image, physical-record, and logical-catalog equality.
- `BerkeleyDbCancellationReadStream.cs`: internal borrowed-stream adapter for bounded cancellation-aware reads.
- `BerkeleyDbDatabasePublisher.cs`: orchestration and commit boundary.

Modify `BerkeleyDbTerminalDatabaseWriter.cs` and `BerkeleyDbTerminalDatabaseWriterOptions.cs` for delegation and XML documentation. Add an internal WriteCore overload with the existing public parameters plus the filesystem seam; the public method supplies the production instance. Preserve existing argument checks and preparation order.

The filesystem seam has these internal abstract members:

```csharp
void ValidatePaths(string destinationPath, string lockPath, bool overwriteExisting);
Stream OpenLock(string lockPath, bool createIfMissing);
Stream CreateTemporary(string temporaryPath);
void FlushToDisk(Stream stream);
Stream OpenRead(string temporaryPath);
void Move(string temporaryPath, string destinationPath, bool overwriteExisting);
void DeleteTemporary(string temporaryPath);
```

`ValidatePaths` checks the immediate parent and both named objects without swallowing authorization errors. Absence is permitted only for destination/lock; directories and reparse objects are rejected. It inspects dangling links as well as live links. It runs before any lock open, after acquisition, and before commit. A destination already present with overwrite disabled fails promptly. These preliminary reads do not replace validation under the lock.

Production OpenLock uses OpenOrCreate or Open, ReadWrite, FileShare.None, and no DeleteOnClose. CreateTemporary uses CreateNew, Write, FileShare.None, WriteThrough. OpenRead uses Open, Read, FileShare.Read, SequentialScan. FlushToDisk operates on the production FileStream. Move delegates to File.Move with the selected overwrite policy. A test decorator forwards these operations to the production implementation and may wrap returned streams.

Lock interface:

```csharp
internal static IDisposable Acquire(
    string destinationPath, string lockPath, bool overwriteExisting,
    BerkeleyDbDatabasePublicationFileSystem fileSystem,
    CancellationToken cancellationToken);
internal static bool IsContention(IOException exception);
```

Verifier interface (PreparedPublication is the existing nested writer record):

```csharp
internal static void Verify(
    byte[] reopened, byte[] expected,
    IReadOnlyList<BerkeleyDbHashRecord> plannedRecords,
    IReadOnlyList<BerkeleyDbTerminalDatabaseWriter.PreparedPublication> publications,
    BerkeleyDbTerminalDatabaseWriterOptions options,
    CancellationToken cancellationToken);
```

Publisher interface:

```csharp
internal static void Publish(
    string fullPath, byte[] image,
    IReadOnlyList<BerkeleyDbHashRecord> records,
    IReadOnlyList<BerkeleyDbTerminalDatabaseWriter.PreparedPublication> publications,
    BerkeleyDbTerminalDatabaseWriterOptions options,
    BerkeleyDbDatabasePublicationFileSystem fileSystem,
    CancellationToken cancellationToken);
```

Tests live in `tests/Icod.TermInfo.BerkeleyDb.Tests/src/`. Create `Hw05PublicationTestSupport.cs` for fixture preparation, temporary-directory ownership, and a forwarding fault decorator. Use the existing `Hdb07HashV9FixtureBuilder.CreateCompiledEntry` and existing production preflight/planner/builder to prepare inputs; do not reproduce the codec. Keep helpers internal to tests.

## Verification commands

Focused cycle (substitute the task's test class):

```sh
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --filter FullyQualifiedName~Hw05PublicationLockTests --logger trx
```

Omitting `-f` runs all three target frameworks. A valid RED is a compiling test that fails the intended behavioral assertion; a compiler failure is not RED. If a new internal interface is needed, introduce its minimal throwing boundary with the tests, then implement the behavior after observing failure.

Full managed gate:

```sh
dotnet test tests/Icod.TermInfo.BerkeleyDb.Tests/Icod.TermInfo.BerkeleyDb.Tests.csproj -c Release --logger trx -p:ContinuousIntegrationBuild=true
dotnet test Icod.TermInfo.sln -c Staging --logger trx -p:ContinuousIntegrationBuild=true
git diff --check
```

The present workspace has no dotnet executable. Use the existing GitHub Actions host matrix for executable RED/GREEN evidence when that remains true; record the actual commit SHA, failing assertion, and subsequent passing run. A clearly labeled `test: HW05 ... RED` checkpoint may be committed and pushed to obtain that evidence; the GREEN commit instructions below identify accepted task checkpoints, not a ban on intermediate CI commits. Never claim a local test ran. Final acceptance requires the normal and HDB00 workflows green on the same code commit. Intermediate focused filesystem changes do not require new native fixtures.

## Task 1: Verified persistent lock and path validation

**Files:** filesystem seam, System.IO implementation, publication lock, test support, `Hw05PublicationLockTests.cs`, `Directory.Build.props`.

**Interfaces:** Produces the filesystem and lock interfaces above; acquires an IDisposable owning the exclusive stream.

- [ ] Add compiling failing tests `SecondHolderWaitsUntilRelease`, `CancelledWaitReleasesResources`, `NonContentionFailureIsNotRetried`, `UnenforcedLockFailsBeforeStaging`, and `ExistingLockIsNeverTruncatedOrDeleted`. Use ManualResetEventSlim/barriers in the forwarding decorator to prove the second acquisition attempt occurred; bounded test deadlines only prevent hangs. Assert second completion only after release, OperationCanceledException on cancellation, same injected exception on unrelated I/O, NotSupportedException when both exclusive opens succeed, and unchanged bytes for a preexisting lock file.
- [ ] Add path tests for missing parent, directory at destination/lock, live and dangling symlinks at destination/lock, linked immediate parent, and permission denial. Assert no temporary creation or destination mutation. Test Windows junctions where symbolic-link privileges are unavailable; report genuinely unavailable host capabilities explicitly.
- [ ] Advance the suite version to Alpha-5 and run the focused command with `Hw05PublicationLockTests`; record the intended failures.
- [ ] Implement path inspection before opening the lock. Acquisition retries only Windows HRESULT 0x80070020/0x80070021, Linux errno 11, or macOS errno 35, with host-specific classification. Use a 50 ms cancellation-aware wait between attempts; this is polling cadence, not a timeout. Propagate every other error.
- [ ] After obtaining the first stream, attempt a second Open of that lock path while holding it. Recognized contention confirms exclusion. If the second open succeeds, close both and throw NotSupportedException before staging. Propagate unrelated probe errors after releasing ownership. Validate paths again after successful acquisition and retain the first stream until disposal.
- [ ] Run the focused tests across all TFMs and qualify actual contention error codes on the three CI hosts. Commit as `feat: add verified cancellable publication locks` only after GREEN.

Source evidence for the enforcement check and error mapping: Microsoft .NET source, [HeldFileLease.cs](https://source.dot.net/Aspire.Hosting.Dotnet/src/Shared/HeldFileLease.cs.html), inspected 2026-09-26. Use it as behavior evidence; do not import its implementation or its different lock-file deletion policy.

## Task 2: Cancellable reopened-image verification

**Files:** verifier, cancellation read adapter, `Hw05PublicationVerificationTests.cs`, test support.

**Interfaces:** Produces Verify above and `BerkeleyDbCancellationReadStream(Stream inner, CancellationToken token)`. The wrapper borrows inner and does not own it.

- [ ] Add failing tests `ExactImageAndCatalogVerify`, `ChangedImageIsRejected`, `MissingPhysicalRecordIsRejected`, `WrongPreparedAliasIsRejected`, and `CancellationInterruptsBothStableReadPasses`. Vary each expected input independently so byte equality cannot conceal missing record/catalog checks. Assert InvalidDataException for equality discrepancies, preserve the reader's own format exceptions, and assert OperationCanceledException on both read passes.
- [ ] Run the focused command with `Hw05PublicationVerificationTests` and record RED.
- [ ] Implement the stream adapter: delegate seeking/length/position, reject writes, cap every Read request at 81,920 bytes, and check cancellation before and after Read/ReadByte. This lets existing `BerkeleyDbHashReader.ReadStableDatabase(Stream, int)` perform both observations without changing the reader's public API or inventing a second acquisition algorithm.
- [ ] Implement Verify. Compare image bytes in bounded chunks with cancellation checks. Call ReadRecords with checked `options.ParserOptions.MaximumEntrySize + 1` and `options.MaximumRecordCount`; compare exact key/value bytes against planned records using the existing exact-byte comparer. Reject missing/extra records. Call NcursesCatalogReader.Read with the parser snapshot and existing DefaultMaximumIndexHops; compare exact names, canonical/alias kind, resolved Terminal.Name, and ordered Terminal.Aliases against prepared publications. Exact payload equality is proved by the storage-record value comparison; the public catalog has no raw payload property. Reject extra logical names. Use ordinal dictionaries/sorted collections, not repeated full-file lookups.
- [ ] Run the focused tests and existing HW01-HW04 tests. Commit as `feat: verify staged Hash-v9 publications` after GREEN.

## Task 3: Staged publisher and public writer integration

**Files:** publisher, writer, `Hw05AtomicPublicationTests.cs`, test support.

**Interfaces:** Consumes lock and Verify; produces Publish and internal WriteCore with the filesystem seam as its final parameter.

- [ ] Add fault-matrix tests `PreCommitFailurePreservesExistingDestination` and `PreCommitFailureLeavesAbsentDestinationAbsent` for temporary create, partial write, flush, close, reopen, read, byte/record/catalog mismatch, path recheck, and move refusal. Use distinctive prior bytes and forward successful operations to real temporary-directory files. Assert prior bytes or destination absence, no owned temporary artifact when cleanup succeeds, and persistent lock availability.
- [ ] Add `CreateNewCollisionNeverDeletesForeignArtifact`, `WriteFailureSurvivesCloseAndCleanupFailures`, `CancellationDuringStagingPreservesDestination`, `CancellationInsideCommitDoesNotTurnSuccessIntoFailure`, and `PublicWriterUsesVerifiedPublication`. For the last case corrupt the staged bytes through WriteCore's decorator and assert rejection with the old destination intact. Distinguish a move failure before rename from a successful rename; do not inject an after-rename exception and falsely require rollback.
- [ ] Run the focused command with `Hw05AtomicPublicationTests` and record RED.
- [ ] Implement Publish: derive sibling names from the final filename, acquire lock, create the unique temporary, and mark ownership only after CreateTemporary succeeds. Write in 81,920-byte chunks with cancellation checks; flush to disk and close before reopen. Read through the cancellation adapter and production stable reader; close read handles before verification/commit. Verify, revalidate paths, make the final cancellation check, then invoke Move once with the requested policy.
- [ ] Implement explicit exception ownership around write/flush/close and outer cleanup. A close error is primary only when no earlier failure exists. Keep the lock held while best-effort deleting the owned temporary. Preserve the primary exception using normal rethrow/ExceptionDispatchInfo where necessary. After successful move clear temporary ownership; never check cancellation or delete a recreated old temporary pathname afterward. Release the zero-length lock handle without writing it.
- [ ] Replace only the writer's final direct FileStream block with publication delegation through WriteCore. Preserve snapshots, validation, ordering, and existing public signature. Existing HW04 byte-oracle and round-trip tests must still pass.
- [ ] Run focused and full BerkeleyDb Release tests. Commit as `feat: atomically publish verified Hash-v9 databases` after GREEN.

## Task 4: Cross-process, host, and lifecycle qualification

**Files:** `Hw05PublicationConcurrencyTests.cs`; test-only `tools/hw05-publication-probe/Program.cs` and `Hw05.PublicationProbe.csproj`; `Hw05PublicationProcessTests.cs`; test project build/copy wiring; `.github/workflows/pull-request.yaml` only if the existing test project invocation cannot build the helper.

**Interfaces:** Helper is an unpacked executable targeting all three TFMs, referencing BerkeleyDb. It accepts `hold-lock <lockPath>` or `write <destination> <compiledPayloadPath> <canonical> <alias> <overwrite>`. It writes a readiness marker to stdout and reads release/control from stdin. Stdout/stdin are intentional test IPC. The writer command calls the public API. Tests launch the helper with the dotnet host for their TFM; do not invoke recursive builds from inside tests.

- [ ] Add failing tests `OverwritePublicationIsSerialized`, `NonOverwriteHasExactlyOneWinner`, `DifferentDestinationsDoNotBlockEachOther`, and `CaseEquivalentPathsShareLockOnInsensitiveFileSystem`. Use the forwarding seam's barriers to hold the first writer after acquisition and observe the second writer's actual contended attempt; avoid sleep-based correctness assertions. Determine directory case behavior with a disposable ordinary file, not an OS-name assumption.
- [ ] Add process tests `IndependentProcessHonorsHeldLock`, `KilledHolderReleasesLock`, and `PersistentLockIsReusable`. Read the helper's acquired marker, start the second writer, establish contention with a direct exclusive-open probe, release or terminate only the spawned holder, and require a complete provider-readable result. Drain child stderr, impose bounded harness timeouts, and always kill/reap owned children in finally. Run on every TFM and OS.
- [ ] Run the focused tests and record failures before any needed corrections. Repair only gaps exposed in Tasks 1-3 while preserving their interfaces.
- [ ] Test concurrent readers accepting only complete old/new bytes when a read succeeds. On Windows use FileShare.ReadWrite | FileShare.Delete for the observational raw reader; ordinary readers that deny deletion may cause the writer to fail safely. Do not promise that every historical open handle allows replacement.
- [ ] Test fresh metadata by contrasting an explicitly restricted old Unix file with a normally created sibling control file; new destination mode should match the control. Restore permissions in finally. Test Windows read-only/permission failures without requiring Unix-equivalent outcomes. Test long derived sidecar names failing before destination mutation. Ensure unrelated temp files and persistent lock contents are unchanged.
- [ ] Run full BerkeleyDb tests and existing host qualification. Commit as `test: qualify HW05 publication concurrency across hosts` with observed results.

## Task 5: Documentation, packaging, and acceptance

**Files:** writer/options XML documentation; `Icod.TermInfo.BerkeleyDb/README.md` if present, otherwise the existing BerkeleyDb section in root `README.md`; `docs/1.16.0-HW05-SAFE-FILESYSTEM-COMMIT.md`; both root development roadmaps. Preserve existing package API/version verification tooling.

- [ ] Document persistent sidecars, fresh replacement metadata, cooperative serialization, cancellation-aware waits without a timeout, and the final commit boundary. Explain filesystem-dependent atomic visibility, no directory power-loss durability claim, unsupported locking rejection, and no native writer coordination. Ensure examples do not imply HW06 command support exists.
- [ ] Run the full managed gate, existing exact API/package verifiers through normal CI, and HDB00 on the same code SHA. Inspect actual test and job results; resolve specific failures and requalify the changed code. Preserve all existing byte fixtures and production dependency constraints.
- [ ] Self-review the full diff against spec sections 1-14 and the five review-focus conditions. Verify no test helper ships in packages or tool archives. Do not use subagents.
- [ ] Record the actual code SHA, workflow IDs/URLs, per-host/TFM counts, and any explicitly unsupported host fixture. Mark HW05 complete only when acceptance evidence exists. Set HW06 as next; do not implement it in this plan.
- [ ] Run `git diff --check`, inspect changed paths, commit acceptance documentation as `docs: accept HW05 safe filesystem publication`, and push to PR #47. Update the PR description to the accepted checkpoint without merging or publishing. Do not wait on documentation-only workflow runs.

## Coverage and handoff

Spec sections 1-5 map to Tasks 1 and 3; sections 6-7 to Tasks 1/3/4; section 8 to Task 2; sections 9-10 to Tasks 1/3/4; sections 11-14 to Tasks 4/5. Every review-focus item has a named behavioral test above.

This plan is ready for user review. Its existence does not mark HW05 implemented or accepted. After review, execute inline using the existing isolated branch and task-by-task test evidence.
