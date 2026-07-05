---
status: todo
title: Add --settings-file CLI option to Versonify
created: 2026-05-18
priority: medium
reference: 4
---
# Reference
4 — Add --settings-file / -s CLI option to Versonify

# What
When the Versonify CLI is invoked with `--settings-file <path>` (or `-s <path>`), the tool reads a JSON settings file at that path and uses it to supply default values for the following configuration options if they are not explicitly provided on the command line:
- Version source (`-V`): Formed by concatenating `versionSettings.vstore.base` and `versionSettings.vstore.filename` from the file.
- File-match pattern list (`-M`): Taken from `versionSettings.fileMatches`.
  - `fileMatches` is a JSON object containing named sets of glob patterns (arrays of strings).
  - The default set name is `"all"`.
  - If only the `"all"` set is defined, those patterns are always used.
  - If multiple sets of file matches are defined:
    - If `--digit-group <group>` (or `--pre-release` which maps to the `"pre-release"` or `"prerelease"` group) is specified, the set with the matching name is used (matching is case-insensitive).
    - If no `--digit-group` or `--pre-release` is specified, Versonify falls back to using the `"all"` set if it exists; otherwise, it fails with an error.
    - If the specified group name is not found in `fileMatches`, Versonify fails with an error.
- Trace level (`--trace`): Taken from `versionSettings.trace`.
- Root path (`--root`): Taken from `versionSettings.root`.
- Debug flag (`--debug`): Taken from `versionSettings.debug`.

Any value explicitly provided on the command line takes precedence over the corresponding value from the settings file. If the path does not exist or the file contains invalid JSON, the tool exits with a non-zero code and an `Error >>` prefixed message.

A JSON schema document is added at `src/_Dependencies/automation/vsets.schema.json` describing the valid structure, and the existing example `.vsets` file at `src/_Dependencies/automation/pliskyverisoning.vsets` is updated to reference it via `$schema`.

# Why
A settings file lets teams commit a single source-of-truth for versioning configuration alongside the code, reducing command-line noise and the risk of drift between invocations. Adding trace, root, and debug to this file allows complete centralization of Versonify settings. The schema addition gives editors validation and autocompletion for `.vsets` files.

# Acceptance

- Given a valid settings file at the supplied path, when `versonify updatefiles -s <path>` is invoked without any of the overridden options, then the command completes successfully using the `vstore` path, patterns (from the `"all"` set or current digit group as matching rules), trace level, root path, and debug flag from the settings file.
- Given a settings file that provides a vstore value, when `-V <explicit-path>` is also passed on the command line, then the explicit `-V` value is used and the vstore from the settings file is ignored.
- Given a settings file that provides a `fileMatches` dictionary, when `-M <explicit-pattern>` is also passed on the command line, then the explicit `-M` value is used and `fileMatches` from the settings file is ignored.
- Given a settings file that provides a `trace` value, when `--trace <explicit-trace>` is also passed on the command line, then the explicit `--trace` value is used and the trace from the settings file is ignored.
- Given a settings file that provides a `root` value, when `--root <explicit-root>` (or `-Root`) is also passed on the command line, then the explicit root value is used and the root from the settings file is ignored.
- Given a settings file containing `"debug": true`, when the CLI is run without `--debug` (or any debug alias), then debug logging is enabled.
- Given a settings file containing `"debug": false` or `"debug": true`, when the CLI is run with `--debug` (or any debug alias), then debug logging is enabled (the CLI flag takes precedence to turn it on).
- Given a `--settings-file` path that does not exist on disk, when the tool is invoked, then it exits with a non-zero exit code and writes a message beginning with `Error >>` to standard output.
- Given a `--settings-file` path that exists but contains malformed JSON, when the tool is invoked, then it exits with a non-zero exit code and writes a message beginning with `Error >>` to standard output.
- Given a settings file containing only the `"all"` file matches set, when `versonify updatefiles -s <path>` is invoked, then the `"all"` file matches set is used.
- Given a settings file containing multiple file matches sets (e.g. `"all"` and `"prerelease"`), when `versonify updatefiles -s <path>` is invoked without any `--digit-group` or `--pre-release` option, then the `"all"` set is used.
- Given a settings file containing multiple file matches sets but no `"all"` set, when `versonify updatefiles -s <path>` is invoked without any `--digit-group` or `--pre-release` option, then it exits with a non-zero exit code and writes a message beginning with `Error >>` to standard output.
- Given a settings file containing multiple file matches sets (e.g. `"all"` and `"prerelease"`), when `versonify updatefiles -s <path> -g prerelease` is invoked, then the `"prerelease"` set is used.
- Given a settings file containing multiple file matches sets (e.g. `"all"` and `"pre-release"`), when `versonify updatefiles -s <path> -p` is invoked, then the `"pre-release"` set is used.
- Given a settings file containing multiple file matches sets, when `versonify updatefiles -s <path> -g missing-group` is invoked, then it exits with a non-zero exit code and writes a message beginning with `Error >>` to standard output.
- Given the file `src/_Dependencies/automation/vsets.schema.json`, when it is validated, then it defines a JSON Schema that includes `versionSettings` as an object with `vstore` (base/filename strings), `fileMatches` (object of string arrays, representing named sets of patterns), `trace` (optional string), `root` (optional string), and `debug` (optional boolean).
- Given the existing example `.vsets` file at `src/_Dependencies/automation/pliskyverisoning.vsets`, when it is opened, then it contains a `$schema` property pointing to `vsets.schema.json`.

# Out of Scope

- No plain-string shorthand for `vstore` — only the `base` + `filename` object form is supported.
- No settings file support for any CLI option other than `vstore` (→ `-V`), `fileMatches` (→ `-M`), `trace` (→ `--trace`), `root` (→ `--root`), and `debug` (→ `--debug`).
- No merging of multiple settings files or cascading/layered config lookup.
- No new unit tests beyond integration/unit tests validating the CLI options wireup.
- No changes to the existing option definitions or their validation logic.

# Assumptions & Constraints

- The concatenation of `vstore.base` and `vstore.filename` must produce a string that passes the existing `VersionPersistanceValue` validation unchanged; no new validation rules are introduced.
- The `fileMatches` array elements within any named set must each satisfy the existing `VersionTargetMinMatch` validation unchanged.
- Integration tests live in the `Versonify.ITest` project and follow that project's existing patterns and helpers.
- The short flag `-s` is not already in use by another Versonify CLI option.
- The `$schema` path in the updated `.vsets` example file is a relative reference to `vsets.schema.json` in the same directory.
- The command line option override check for boolean flags like `debug` must verify if the flag/alias was explicitly passed in the CLI argument list, rather than just checking if the parsed option value is `false` (since the default value of a boolean option is `false` when absent).

# Example JSON settings file

```json
{
  "$schema": "./vsets.schema.json",
  "versionSettings": {
    "vstore": {
      "base": "%NEXUSCONFIG%[R::plisky[L::https://<yournexus>/repository/<name>/vstore/",
      "filename": "versonify.vstore"
    },
    "fileMatches": {
      "all": [
        "**/src/**/changelog.md|TextFile",
        "**/src/**/Plisky.Versioning.csproj|StdAssembly",
        "**/src/**/Plisky.Versioning.csproj|StdInformational",
        "**/src/**/Plisky.Versioning.csproj|StdFile"
      ]
    },
    "trace": "verbose",
    "root": "./src",
    "debug": true
  }
}
```
