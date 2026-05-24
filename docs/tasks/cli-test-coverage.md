---
status: done
completed: 2026-05-23
title: Add missing CLI argument and validation ITest coverage
created: 2026-05-18
priority: highest
reference: 3
---
# Reference
3 — Add missing CLI argument and validation ITest coverage

# What
Approximately 16 new integration tests are added to the Versonify.ITest project covering
five previously untested areas of the CLI: the `-Output` mode variants (`env`, `file`,
`file:<name>`, `azdo` default, `azdo:MyVar` custom, and the `vsts` alias); the boolean
`-Debug` flag's argument-echo behaviour; semicolon-separated arrays passed to the
`behaviour` command; the `prefix` command's store-and-apply flow; and the validation
error paths for every command that requires a mandatory argument it did not receive, plus
the conflicting-flags and invalid-directory paths. When complete, running the ITest suite
produces green results for all 16 new tests alongside the existing passing tests.

# Why
The Versonify CLI already has integration tests for the core versioning flow but leaves
output-mode routing, the debug flag, array argument parsing, the prefix command, and the
validation error paths entirely untested at the integration level. This means regressions
in those paths are invisible until a user reports them. Adding these tests closes the
coverage gap for the parts of the CLI most likely to break when argument-parsing or
output-routing code is touched, and provides a safety net for future refactoring of
`Program.cs` and the command-dispatch layer.

# Acceptance

## Group A — Output mode ITests
- Given a versioned store, when `-Output env` is passed, then the process exits with code 0
  and produces no error text on stdout.
- Given a versioned store, when `-Output file` is passed, then the process exits with code 0
  and a file with the `.pver` extension is created in the working directory.
- Given a versioned store, when `-Output file:<name>` is passed (e.g. `file:myver.txt`), then
  the process exits with code 0 and a file named `myver.txt` is created.
- Given a versioned store, when `-Output azdo` is passed (no custom variable name), then
  stdout contains the string `##vso[task.setvariable variable=CodeVersionNumber`.
- Given a versioned store, when `-Output azdo:MyVar` is passed, then stdout contains the
  string `##vso[task.setvariable variable=MyVar`.
- Given a versioned store, when `-Output vsts` is passed, then stdout contains the string
  `##vso[task.setvariable variable=CodeVersionNumber` (confirming the `vsts` alias behaves
  identically to `azdo` default).

## Group B — Debug flag ITest
- Given any valid invocation with `-Debug` appended (boolean flag, no value), then stdout
  contains the string `Command Line:` confirming each argument is echoed.

## Group C — Semicolon-array ITest
- Given a `behaviour` command where `-D=0;1` is passed, then stdout contains output for
  both digit 0 and digit 1, confirming both array elements were parsed and applied.

## Group D — Prefix command ITest
- Given a versioned store, when `prefix -V=<store> -D=2 -Q=-` is executed, then the
  process exits with code 0 and subsequent version output confirms the prefix character
  `-` has been applied (i.e. the version string contains `-` at the expected position).

## Group E — Validation error path ITests
- Given the `behaviour` command is invoked without a `-D` argument, then exit code is
  non-zero and stdout contains `Error >>`.
- Given the `override` command is invoked without a `-Q` argument, then exit code is
  non-zero and stdout contains `Error >>`.
- Given the `set` (digit) command is invoked without a `-Q` argument, then exit code is
  non-zero and stdout contains `Error >>`.
- Given the `prefix` command is invoked without a `-Q` argument, then exit code is
  non-zero and stdout contains `Error >>`.
- Given the `prefix` command is invoked without a `-D` argument, then exit code is
  non-zero and stdout contains `Error >>`.
- Given the `set` command is invoked with both `-Q` and `-R` supplied simultaneously,
  then exit code is non-zero and stdout contains `Error >>`.
- Given any command is invoked with a `-Root` value pointing to a directory that does
  not exist, then exit code is non-zero and stdout contains `Error >> Invalid Directory`.

# Out of Scope
- Changes to production code (`Program.cs`, command handlers, or output formatters) —
  tests must pass against the existing implementation as-is.
- Unit tests in `Plisky.Versioning.Test`; all new tests are integration tests in `Versonify.ITest`.
- Output mode variants beyond the six listed (`env`, `file`, `file:<name>`, `azdo`,
  `azdo:MyVar`, `vsts`).
- Testing the content or correctness of the version number itself — only the
  presence/absence of expected strings and exit codes is asserted.
- CI pipeline configuration changes.

# Assumptions & Constraints
- The `Versonify.ITest` project already has a working harness for invoking the CLI
  as a subprocess and capturing stdout, stderr, and exit code.
- A helper exists (or can be reused from existing tests) to create a minimal valid
  versioned store on disk for tests that require one.
- The `-Output file` and `-Output file:<name>` tests must clean up created files after
  the test run to avoid polluting the working directory.
- The `vsts` alias is confirmed to be a supported synonym for `azdo` in the current
  codebase.
- "Exit code non-zero" means any value other than 0; the exact non-zero value is not
  asserted unless the codebase documents a specific contract.
- All tests must pass on Windows (the primary CI platform); cross-platform behaviour
  of environment-variable output is not in scope.
