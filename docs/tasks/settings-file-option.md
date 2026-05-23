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
When the Versonify CLI is invoked with `--settings-file <path>` (or `-s <path>`), the tool reads a
JSON settings file at that path and uses it to supply default values for the version source (`-V`)
and the file-match pattern list (`-M`). The version source is formed by concatenating
`versionSettings.vstore.base` and `versionSettings.vstore.filename` from the file. The file-match
list is taken from `versionSettings.fileMatches`. Any value explicitly provided on the command line
(`-V` or `-M`) takes precedence over the corresponding value from the settings file. If the path
does not exist or the file contains invalid JSON, the tool exits with a non-zero code and an
`Error >>` prefixed message. A JSON schema document is added at
`src/_Dependencies/automation/vsets.schema.json` describing the valid structure, and the existing
example `.vsets` file is updated to reference it via `$schema`.

# Why
A settings file lets teams commit a single source-of-truth for these values alongside the code, reducing command-line noise and the risk of drift between invocations. The schema addition gives editors validation and autocompletion for `.vsets` files.  This will allow other tools to identify the versioning settings more simply.

# Acceptance

- Given a valid settings file at the supplied path, when `versonify updatefiles -s <path>` is
  invoked without `-V` or `-M`, then the `updatefiles` command completes successfully using the
  vstore path formed from `versionSettings.vstore.base` + `versionSettings.vstore.filename` and
  the patterns from `versionSettings.fileMatches`.

- Given a settings file that provides a vstore value, when `-V <explicit-path>` is also passed on
  the command line, then the explicit `-V` value is used and the vstore from the settings file is
  ignored.

- Given a settings file that provides a `fileMatches` array, when `-M <explicit-pattern>` is also
  passed on the command line, then the explicit `-M` value is used and the `fileMatches` from the
  settings file is ignored.

- Given a `--settings-file` path that does not exist on disk, when the tool is invoked, then it
  exits with a non-zero exit code and writes a message beginning with `Error >>` to standard output.

- Given a `--settings-file` path that exists but contains malformed JSON, when the tool is invoked,
  then it exits with a non-zero exit code and writes a message beginning with `Error >>` to standard
  output.

- Given the file `src/_Dependencies/automation/vsets.schema.json`, when it is validated, then it
  defines a JSON Schema that requires `versionSettings.vstore` as an object with `base` (string)
  and `filename` (string) properties, and `versionSettings.fileMatches` as an array of strings.

- Given the existing example `.vsets` file at `src/_Dependencies/automation/pliskyverisoning.vsets`,
  when it is opened, then it contains a `$schema` property pointing to `vsets.schema.json`.

# Out of Scope

- No plain-string shorthand for `vstore` — only the `base` + `filename` object form is supported.
- No settings file support for any CLI option other than `vstore` (→ `-V`) and `fileMatches` (→ `-M`).
- No merging of multiple settings files or cascading/layered config lookup.
- No new unit tests beyond the ITests listed above — the feature is thin CLI wiring over existing validated paths.
- No changes to the existing `-V` or `-M` option definitions or their validation logic.

# Assumptions & Constraints

- The concatenation of `vstore.base` and `vstore.filename` must produce a string that passes the
  existing `VersionPersistanceValue` validation unchanged; no new validation rules are introduced.
- The `fileMatches` array elements must each satisfy the existing `VersionTargetMinMatch` validation
  unchanged.
- Integration tests live in the `Versonify.ITest` project and follow that project's existing
  patterns and helpers.
- The short flag `-s` is not already in use by another Versonify CLI option — implementer must
  verify this before implementation.
- The `$schema` path in the updated `.vsets` example file is a relative reference to
  `vsets.schema.json` in the same directory.
