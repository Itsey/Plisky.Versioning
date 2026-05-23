---
status: todo
title: Migrate Versonify CLI parsing from Plisky.Plumbing to System.CommandLine
created: 2026-05-02
priority: medium
reference: 1
---

# What
Migrate the `Versonify` command-line parsing layer from `Plisky.Plumbing` / `CommandArgumentSupport` to `System.CommandLine` without changing the underlying versioning behaviors in `Plisky.Versioning`. The migration is limited to the CLI surface in `src/Versonify`, especially `Program.cs` and `VersonifyCommandline.cs`, plus the related tests in `src/Versonify.ITest` and `src/Plisky.Versioning.Test`.

The replacement parser must keep the current command vocabulary (`createversion`, `override`, `updatefiles`, `passive`, `behaviour`, `set`, `prefix`) and the current non-deprecated options working for existing callers. For compatibility, the first token must continue to be interpreted as the command name rather than moving to real `System.CommandLine` subcommands. Existing single-dash long options such as `-VersionSource` and `-QuickValue` are part of the supported surface and must remain accepted.

# Why
The current CLI parser relies on `Plisky.Plumbing` reflection attributes and parser-specific normalization/warning code that is isolated to Versonify and makes the CLI harder to evolve. Moving to `System.CommandLine` modernizes argument handling in the tool while keeping the core versioning engine in `Plisky.Versioning` unchanged.

The migration also removes deprecated aliases that are no longer required (`-DG`, `-VS`, `-NO`, `-MM`) while preserving the active command/option surface that current users depend on, including compatibility probes such as `--QQpnf`.

# Acceptance
- Parsing for `Versonify` is implemented with `System.CommandLine`; the migration is scoped to the CLI layer under `src/Versonify` and must not move versioning business logic out of `Plisky.Versioning` or otherwise rewrite the core versioning engine.
- The CLI continues to recognize the existing command names `createversion`, `override`, `updatefiles`, `passive`, `behaviour`, `set`, and `prefix` case-insensitively, and maps them to the same behaviours now exposed by `RequestedCommand`.
- Command selection remains compatibility-first:
  - the first bare token is treated as the command argument;
  - the implementation does not introduce required `System.CommandLine` subcommands;
  - existing `-Command=<name>` input continues to work if it is currently accepted by tests/callers.
- The retained option surface remains available with current non-deprecated names and active short aliases, including:
  - `-Debug`
  - `-DryRun`
  - `-D` and `-Digits`
  - `-z` and `-NoError`
  - `-NoOverride`
  - `-O` and `-Output`
  - `-I` and `-Increment`
  - `-Q` and `-QuickValue`
  - `-R` and `-Release`
  - `-Root`
  - `-Trace`
  - `-V` and `-VersionSource`
  - `-M` and `-MinMatch`
- Single-dash long options are explicitly supported as first-class parser aliases under `System.CommandLine`; callers must still be able to invoke forms such as `-VersionSource=...`, `-QuickValue=...`, `-MinMatch=...`, `-NoError`, and `-NoOverride` without converting to `--long-name`.
- Deprecated aliases `-DG`, `-VS`, `-NO`, and `-MM` are removed from the supported surface:
  - they no longer parse successfully;
  - the migration removes the deprecated alias warning flow associated with them;
  - tests that currently cover these aliases are updated to assert the new contract instead of warning behavior.
- The parser replacement preserves the existing non-deprecated semantics that are currently layered on top of parsing, including:
  - `-NoError` still forces zero exit code on otherwise failing executions;
  - `-Trace=info` still enables trace setup;
  - `-QuickValue` / `-Release` / digits interactions continue to drive the same command-resolution and validation rules for `behaviour`, `set`, `override`, `prefix`, and `updatefiles`;
  - multi-value inputs such as digits and minmatch entries continue to support the currently expected separators and shapes used by tests.
- Help, usage, and error handling are intentionally redefined for the new parser but must remain user-usable and regression-safe:
  - running with no arguments still prints help/usage and exits non-zero;
  - invalid parse input prints a clear fatal/argument error and exits non-zero;
  - validation failures after parsing still print the specific validation errors plus help/usage and exit non-zero;
  - help/usage text documents the positional command token model and the supported retained option names/aliases;
  - output remains compatible with existing greeting/error flows where tests or callers rely on them, but it no longer depends on `CommandArgumentSupport.GenerateShortHelp`.
- The special compatibility probe `--QQpnf` remains supported as a case-insensitive short-circuit before normal parsing and still returns exit code `200`.
- Parser-specific plumbing is removed from `src/Versonify` once the `System.CommandLine` path is in place, including no-longer-needed reflection attributes, `CommandArgumentSupport`, parser normalization added only for the old parser, and deprecated-alias scanning/warning code.
- Project/package references are updated to match the new parser contract:
  - `src/Versonify/Versonify.csproj` adds the required `System.CommandLine` dependency;
  - parser-only dependency on `Plisky.Plumbing` is removed from `Versonify` if nothing else in that project still requires it;
  - any related using directives and parser-coupled comments are cleaned up.
- Automated coverage is updated in both test projects:
  - `src/Plisky.Versioning.Test/VersonifyCommandLineTests.cs` covers command resolution and any option-model behavior that remains unit-testable after the parser swap;
  - `src/Versonify.ITest/CommandLineArgumentCoverageTests.cs` and related CLI integration tests cover retained commands/options, positional command compatibility, single-dash long option acceptance, no-arguments help, validation failures, `-NoError`, `-Trace`, and `--QQpnf`;
  - tests that only prove deprecated alias warnings are removed or replaced with assertions for the retained contract.
- Completion means an engineer can implement the migration starting from `src/Versonify/Program.cs`, `src/Versonify/VersonifyCommandline.cs`, `src/Versonify/Versonify.csproj`, `src/Versonify.ITest/CommandLineArgumentCoverageTests.cs`, and `src/Plisky.Versioning.Test/VersonifyCommandLineTests.cs` without needing another discovery pass.
