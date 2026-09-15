# Icod.TermInfo 1.14.0 — Raster Backend Capability Evidence, Selection, and Planning

**Status:** COMPLETE  
**Stable coordinated version:** `1.14.0`  
**Feature freeze:** `1.14.0-Alpha-8`  
**Alpha-8 accepted exact head:** `911e8d44428a26b588193a27e07696c2489bcb93`  
**Alpha-8 qualification:** workflow #924 / run `34903967732`, all 12 jobs green  
**Stable publication:** gated by the normal merge-to-`main` and immutable-tag workflow

## 1. Purpose

`Icod.TermInfo 1.14.0` adds an advisory raster-backend selection layer above the
frozen persistent-raster semantic stack:

```text
1.11  lifecycle evidence / classification / planning
1.12  advanced placement semantics / planning
1.13  backend-neutral runtime observations / integration
1.14  backend availability evidence / candidate evaluation / selection
```

The layer lives in `Icod.TermInfo.Inspection`. TermInfo does not probe terminals,
execute graphics protocols, allocate terminal-side resource identities, or depend
on `Icod.Terminal` in production.

## 2. Frozen release-wide rules

1. The initial concrete backend identities are **Sixel** and **Kitty Graphics**.
2. Backend availability is independent from lifecycle and placement capability
   truth.
3. Frozen 1.11 lifecycle and 1.12 placement planners remain authoritative.
4. Frozen 1.13 runtime observations remain backend-neutral and unchanged.
5. Callers maintain separate 1.13 integration contexts when runtime verification
   is scoped to separate backends.
6. No terminal-name, emulator-brand, profile-name, enum-order, or candidate-input
   heuristic may infer backend support or preference.
7. No hidden backend ranking is permitted.
8. With no explicit caller preference and multiple viable candidates, planning
   returns `RequiresPreference`.
9. A complete explicit preference order is semantic fallback policy: an
   unverified preferred candidate blocks fallback until it is verified or becomes
   impossible.
10. Production `Icod.TermInfo.Inspection` remains free of any `Icod.Terminal`
    package/project dependency.
11. JSON versions 1 through 5 remain immutable; version 6 is additive only.
12. Stable `1.14.0` promotion changes release identity/documentation only and may
    not alter the accepted feature/API/schema/dependency/TFM/command/archive
    contract.

---

## 3. RB01 / Alpha-1 — Backend evidence and public model foundation

**Status:** COMPLETE  
**Accepted exact head:** `7f43c4ad27f1858f8648237cbcb26e6808142458`  
**Qualification:** workflow #865 / run `34862203053`, all 12 jobs green

RB01 establishes the bounded immutable raster-backend evidence/profile/request,
preference, candidate, evaluation, and plan vocabulary. It adds exactly 13
reviewed `RasterBackend*` public types while preserving every pre-1.14 Inspection
contract.

---

## 4. RB02 / Alpha-2 — Classification and conservative static inspection

**Status:** COMPLETE  
**Accepted exact head:** `6321de472a6e54a8382dd214996c22a4cf62e816`  
**Qualification:** workflow #875 / run `34865731077`, all 12 jobs green

`RasterBackendClassifier` uses:

```text
Verified > Declared > CapabilityDerived
```

At the highest evidence precedence present:

- positive only -> `Supported`;
- negative only -> `Unsupported`;
- both polarities -> `Contradicted`;
- no evidence -> `Unknown`.

`RasterBackendInspector` recognizes only exact Boolean Sixel metadata as positive
capability-derived Sixel evidence. Absence remains unknown. Kitty Graphics is
never inferred by brand/name/profile heuristics or unrelated capabilities.

RB02 adds two reviewed public types, bringing the cumulative 1.14 type addition
to 15.

---

## 5. RB03 / Alpha-3 — Candidate composition through frozen planners

**Status:** COMPLETE  
**Accepted exact head:** `22f38e9c6516bec9d5f6b8662f00e9b5bace7c2b`  
**Qualification:** workflow #885 / run `34871582203`, all 12 jobs green

`RasterBackendPlanner.Evaluate(...)` composes backend availability with the
already-frozen lifecycle and placement planners without duplicating their
semantic rules.

Candidate evaluation is frozen as:

1. backend `Unsupported` -> `Impossible`;
2. backend `Unknown` or `Contradicted` -> `RequiresRuntimeVerification`;
3. backend `Supported` -> delegate lifecycle planning;
4. lifecycle impossible -> `Impossible`;
5. lifecycle indeterminate -> `RequiresRuntimeVerification`;
6. lifecycle success with no placement request -> `Satisfied`;
7. lifecycle success with placement request -> delegate placement planning;
8. placement impossible -> `Impossible`;
9. placement verification-required/indeterminate ->
   `RequiresRuntimeVerification`; and
10. placement satisfied -> `Satisfied`.

The evaluation retains the actual lifecycle and placement plans used to explain
the result. RB03 adds the sixteenth and final reviewed 1.14 `RasterBackend*`
public type.

---

## 6. RB04 / Alpha-4 — Deterministic preference-aware selection

**Status:** COMPLETE  
**Accepted exact head:** `d202fd2e452b083de92b84521f1845a30354aede`  
**Qualification:** workflow #896 / run `34877834128`, all 12 jobs green

`RasterBackendPlanner.Plan(...)` snapshots and validates one or two unique
candidates, evaluates every candidate, and stores evaluations in canonical backend
order for deterministic output only.

Without explicit preference:

- all candidates impossible -> `Impossible`;
- exactly one non-impossible candidate -> reflect that candidate as selected or
  verification-required; and
- multiple non-impossible candidates -> `RequiresPreference`.

With a complete explicit preference order:

- impossible candidates are skipped;
- the first preferred verification-required candidate stops fallback;
- the first satisfied preferred candidate is selected; and
- all impossible candidates yield `Impossible`.

Canonical ordering never chooses a backend.

---

## 7. RB05 / Alpha-5 — Frozen 1.13 runtime-integration composition

**Status:** COMPLETE  
**Accepted exact head:** `a4aab5e7af4d554cad7c978cef20491744100c80`  
**Qualification:** workflow #900 / run `34880872852`, all 12 jobs green

RB05 adds the convenience composition boundary:

```text
RasterBackendProfile
+ PersistentRasterRuntimeIntegrationResult
    -> RasterBackendCandidate
```

The constructor reuses the strengthened lifecycle and placement profiles already
retained by the frozen 1.13 integration result. It does not re-integrate evidence,
inspect `Succeeded` as a trust shortcut, elevate backend availability, assign new
source ordinals, or manufacture verified evidence.

Consumers verifying more than one backend maintain separate backend-scoped 1.13
integration contexts.

---

## 8. RB06 / Alpha-6 — JSON version 6 backend automation

**Status:** COMPLETE  
**Accepted exact head:** `d20d1c7b9a727b20dbcb931f1ac980a0e961b372`  
**Qualification:** workflow #904 / run `34893643336`, all 12 jobs green

RB06 adds schema identifier:

```text
urn:icod:terminfo:inspection:json:6
```

with exactly two new document kinds:

```text
rasterBackendProfile
rasterBackendSelectionPlan
```

The profile document preserves backend identity, support status, canonical
evidence, provenance, and deterministic ordering. The selection-plan document
preserves the semantic request, optional placement request, caller preference,
overall status, selected backend only when selected, canonical candidate
evaluations, delegated lifecycle/placement plans, and the remaining blocker.

JSON versions 1 through 5 remain byte-frozen for their historical inputs. The
renderer retains the existing UTF-8 output-byte bounds and cancellation contract.

RB06 adds exactly six reviewed `TermInfoJsonRenderer` member lines and no new
public type.

---

## 9. RB07 / Alpha-7 — Published Terminal 1.13 qualification and sample

**Status:** COMPLETE  
**Accepted exact head:** `0ce8df7f08064de807db17a2662f38252a814ce5`  
**Qualification:** workflow #910 / run `34898618294`, all 12 jobs green

RB07 proves loose coupling against real published downstream packages. The
isolated package-only consumer references:

- freshly packed candidate `Icod.TermInfo.Inspection`; and
- published `Icod.Terminal 1.13.0`.

It has no project reference to production TermInfo projects. The focused sample
is:

```text
samples/Icod.TermInfo.RasterBackendSelection.Sample
```

Its default mode is deterministic and CI-safe. `--live` uses published
`TerminalSession.VerifyCapabilityAsync(...)` at the consumer boundary and keeps
Terminal-to-TermInfo mapping policy explicit. Production Inspection remains free
of a Terminal dependency.

---

## 10. RB08 / Alpha-8 — Adversarial hardening, exact freeze, and closure

**Status:** COMPLETE  
**Accepted exact head:** `911e8d44428a26b588193a27e07696c2489bcb93`  
**Qualification:** workflow #924 / run `34903967732`, all 12 jobs green

RB08 closes the feature line after adversarial coverage of:

- duplicate candidates and duplicate/incomplete preferences;
- invalid enum values and evidence bounds;
- maximum source-label length and overflow-by-one;
- compatible duplicates and evidence contradictions at multiple precedences;
- static Sixel positive advertisement and static absence;
- misleading terminal/profile names carrying no authoritative metadata;
- multiple viable no-preference candidates;
- verification-required preferred candidates blocking satisfied fallback;
- impossible preferred candidates allowing fallback;
- all-impossible selection;
- lifecycle/placement indeterminate and impossible propagation;
- candidate/evidence permutations;
- non-English cultures and repeated processes;
- cancellation and exact JSON UTF-8 byte boundaries; and
- package-only consumption across all supported TFMs.

### 10.1 Public API freeze

The complete `Icod.TermInfo.Inspection` 1.14 reflection manifest contains
**106 exported public types** and has normalized-LF SHA-256:

```text
e9f240a562aec5274d64fb2ec3647862fe4ef5684582af2b442ba3c55e189497
```

Removing the exact reviewed RB01/RB02/RB03 type additions and RB06 renderer
member additions reconstructs frozen 1.13 SHA-256:

```text
fd827a25abafb8e9ff3915567f45f9f2ec51b332bfd8a82dd4e4bca20290e764
```

### 10.2 JSON freeze

JSON schemas v1-v5 retain their accepted fingerprints. JSON v6 is frozen by
normalized-LF SHA-256:

```text
9d51ec6659f8978c867408881eefb106f3c4976dfcf2da12b23c374a222665c2
```

### 10.3 Documentation authorities

The release closure includes:

```text
docs/1.14.0-RASTER-BACKEND-SELECTION-GUIDE.md
docs/1.14.0-INSPECTION-PUBLIC-API-FREEZE.md
docs/1.14.0-RB08-FREEZE-FINGERPRINTS.txt
docs/1.14.0-RELEASE-AUDIT.md
```

and synchronizes the root README, Inspection package README, sample catalog,
versioning policy, compatibility policy, and post-1.0 development roadmap.

---

## 11. Stable 1.14.0 promotion

The feature roadmap is complete at the accepted Alpha-8 contract. Stable
`1.14.0` promotion is deliberately a version/status/documentation transition,
not a ninth feature tranche.

The stable head must independently pass the same complete Staging-equivalent
qualification matrix before PR #44 is considered release-ready:

- Windows Build/Test plus historical Inspection reconstruction;
- Linux Build/Test plus exact package verification;
- macOS Build/Test;
- three installed-tool package smokes; and
- all six standalone archive RID smokes.

If stable-promotion validation exposes a release defect, the defect is corrected
without widening the frozen feature/API/schema contract and the resulting exact
head is requalified from scratch.

## 12. Explicit exclusions

1.14 does not include graphics transport, image codecs, alpha/pixel-format
planning, performance/latency scoring, terminal-brand preference, scene/layout
policy, generic JSON import/deserialization, Berkeley DB acquisition, or
historical vendor binary formats.

Further backend families or richer graphics policy require a later explicitly
planned compatible release.
