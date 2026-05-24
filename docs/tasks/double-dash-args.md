---
status: todo
title: Migrate CLI options to double-dash POSIX-style naming
created: 2026-05-24
priority: high
reference: 5
---
# Reference
5 — Replace single-dash long options with double-dash kebab-case options and deprecate the old forms with a console warning

# What
When this task is complete, every long-form CLI option in Versonify uses a double-dash, kebab-case name as its primary/canonical form (e.g. `--dry-run`, `--no-error`, `--version-source`). All previously primary single-dash PascalCase options (e.g. `-DryRun`, `-NoError`, `-VersionSource`) and any single-dash lowercase long aliases (e.g. `-output`) still parse successfully but immediately emit a deprecation warning to `stderr` naming the old option and the new replacement. Single-character short aliases (`-D`, `-d`, `-O`, `-o`, `-I`, `-i`, `-Q`, `-R`, `-V`, `-v`, `-M`, `-m`, `-z`) continue to work silently with no warning. The positional `commandArg` argument is unchanged. Help output (`--help`) lists only the new double-dash canonical names (and single-character short aliases); deprecated names do not appear in help.

# Why
Versonify's option syntax currently uses non-standard single-dash long names (`-DryRun`, `-NoError`) that conflict with POSIX/GNU conventions and are inconsistent with the System.CommandLine library's intended double-dash design. Adopting `--kebab-case` long options improves discoverability, aligns with tooling conventions users expect, and makes the CLI behave consistently with other modern .NET CLI tools. Backward compatibility aliases prevent breakage in existing scripts while the deprecation warnings give users a clear migration path.

# Acceptance

## A — Canonical double-dash options are accepted and functional
- Given any previously supported command invocation, when the user replaces every old primary option name with its new canonical `--kebab-case` form, then Versonify executes identically to the old invocation (same exit code, same output, same side-effects).
- Given `--dry-run`, `--no-error`, `--no-override`, `--quick-value`, `--version-source`, `--min-match`, `--dry-run`, `--root`, `--trace`, `--command`, `--debug`, `--digits`, `--output`, `--increment`, `--release` are all present as registered primary option names on the root command, then `--help` lists all of them.

## B — Deprecated long-form aliases emit a warning and still function
- Given a user passes `-DryRun` (or any other old single-dash PascalCase primary name), when the command is parsed and executed, then:
  1. A warning line is written to `stderr` in the form: `WARNING: '-DryRun' is deprecated. Use '--dry-run' instead.`
  2. The command executes successfully as if `--dry-run` had been supplied.
- Given a user passes `-output` (single-dash lowercase long alias), when the command is parsed, then the same deprecation warning pattern applies (`WARNING: '-output' is deprecated. Use '--output' instead.`) and the option value is honoured.
- The following deprecated aliases and their expected replacement names are covered:

  | Deprecated alias   | Replacement canonical |
  |--------------------|-----------------------|
  | `-Command`         | `--command`           |
  | `-Debug`           | `--debug`             |
  | `-DryRun`          | `--dry-run`           |
  | `-Digits`          | `--digits`            |
  | `-NoError`         | `--no-error`          |
  | `-NoOverride`      | `--no-override`       |
  | `-Output`          | `--output`            |
  | `-Increment`       | `--increment`         |
  | `-QuickValue`      | `--quick-value`       |
  | `-Release`         | `--release`           |
  | `-Root`            | `--root`              |
  | `-Trace`           | `--trace`             |
  | `-VersionSource`   | `--version-source`    |
  | `-MinMatch`        | `--min-match`         |
  | `-output`          | `--output`            |

## C — Single-character short aliases are silent and unchanged
- Given a user passes any of `-D`, `-d`, `-O`, `-o`, `-I`, `-i`, `-Q`, `-R`, `-V`, `-v`, `-M`, `-m`, or `-z`, then:
  1. No deprecation warning is written to `stderr`.
  2. The option value is bound to the same underlying option as before.
- These aliases continue to appear alongside their canonical long-form in `--help` output.

## D — Deprecated names are hidden from help
- Given the user runs `versonify --help`, then the output does not list any of the deprecated single-dash PascalCase names or the `-output` alias.
- The help output does list every canonical `--kebab-case` option and every retained single-character short alias.

## E — Warning format and stream
- Deprecation warnings are written to `stderr`, not `stdout`, so they do not corrupt piped output.
- Each deprecated alias used in a single invocation produces exactly one warning line for that alias.
- Warning text matches the pattern: `WARNING: '<old-alias>' is deprecated. Use '<new-canonical>' instead.`

## F — No regression on existing tests
- All pre-existing passing tests continue to pass after the change.
- New tests cover: (a) each deprecated alias triggers the warning and correct behaviour, (b) each short alias triggers no warning, (c) each canonical double-dash option works without warning.

# Out of Scope
- Changes to the positional `commandArg` argument or its accepted values (`createversion`, `override`, `updatefiles`, `passive`, `behaviour`, `set`, `prefix`).
- Removal of deprecated aliases — they remain registered (hidden from help but still parseable).
- Adding new options or changing the semantics of any existing option.
- Changes to subcommands, if any exist outside the options listed above.
- Changes to documentation files other than those auto-generated by `--help`.
- Introducing a hard error (non-zero exit) when deprecated aliases are used — warnings only.

# Assumptions & Constraints
- The project targets .NET 9 and uses System.CommandLine 2.0.8; the deprecation warning mechanism must work within that library version (custom middleware or a `CommandLineBuilder` pipeline step is acceptable if the library does not natively support per-alias deprecation).
- "Deprecated alias still parses" means registering the old names as additional aliases on the same `Option<T>` object (or equivalent) so System.CommandLine binds the value normally; the warning is emitted via a middleware/parse-result inspection step, not by duplicating option handling logic.
- Warning output goes to `stderr` via `Console.Error` (or the System.CommandLine `IConsole` abstraction's error stream) to avoid corrupting piped `stdout`.
- The deprecated alias `-output` (single-dash, lowercase, long-form) is treated identically to the deprecated PascalCase aliases — it is a long alias, not a single-character alias, so it receives a warning.
- Single-character aliases are identified by their name being exactly one character long; no other criteria are used to decide whether a warning is shown.
- The task does not require a configuration flag to suppress deprecation warnings; warnings are always emitted when deprecated aliases are used.
- All changes are confined to `src/Versonify/Program.cs` and any directly related option/command setup files; no changes to library or business-logic layers.
