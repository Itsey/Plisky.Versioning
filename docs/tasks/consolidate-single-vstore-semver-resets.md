---
status: todo
title: Consolidate Versonify to single vstore with SemVer resets
created: 2026-06-29
priority: high
---
# Reference
LFY-48 — Dependabot released `1.0.1-Austen.1.37` after `1.0.1`; unify version storage and enforce SemVer progression.

# What
Versonify and the build automation must use one shared vstore for both pre-release and release workflows. Version progression must follow semantic versioning so that when major, minor, or patch changes, prerelease numeric segments reset for the next prerelease cycle. With this behavior, after publishing `1.0.1`, the next prerelease is `1.0.2-<label>.1`. The prerelease label remains unchanged unless explicitly updated.

# Why
The current dual-vstore approach introduces state divergence and allows prerelease numbering to continue from stale prerelease state after a stable release, producing incorrect release order and confusing automation outcomes. A single source of truth removes synchronization drift and aligns release sequencing with SemVer expectations.

# Acceptance

- Given build configuration, when version persistence settings are defined, then only one vstore token/path is configured and used for both pre-release and release operations.
- Given release and prerelease runs in automation, when either mode executes, then both read and persist against the same vstore state.
- Given a stable release `1.0.1` has been produced, when the next prerelease is generated, then the result is `1.0.2-<label>.1`.
- Given major, minor, or patch is incremented, when prerelease numbering resumes, then prerelease numeric components restart from `1`.
- Given an existing legacy store that does not include digit group metadata, when loaded and saved, then compatibility is preserved and default-group behavior remains unchanged.
- Given regression tests for release-then-prerelease flow and digit-group behavior, when the test suite runs, then the new single-vstore SemVer flow is covered and passes.

# Out of Scope

- Changing prerelease label naming conventions or auto-generating new labels.
- Redesigning unrelated command-line options or non-versioning build steps.
- Historical migration tooling beyond maintaining compatibility when reading existing stores.

# Assumptions & Constraints

- Semantic versioning is the required version ordering model.
- The prerelease label (for example, `Austen`) persists across major/minor/patch changes unless explicitly changed by command input.
- Existing digit-group features remain supported under the unified storage model.
- The work must not include creating git commits.
