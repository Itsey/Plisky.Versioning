---
status: todo
title: Add JSON console output mode for Versonify
created: 2026-05-02
priority: medium
reference: 2
---

# What
Add a console output mode that lets `Versonify` emit either the current human-readable text stream or newline-delimited JSON events, selected by a new `--json` command-line option.

The versioning execution path must stop treating console formatting as the primary output contract. Instead, `Plisky.Versioning` must expose shared structured result/output types that capture the information currently written during versioning operations, and `Versonify` must render that information either as existing text lines or as JSON events.

This change is limited to the console message stream. Existing non-console outputs such as file output, environment variable output, and NukeFusion output must keep their current behavior unless they currently depend on console-only formatting concerns.

# Why
`Program.cs` currently mixes command execution, validation, progress reporting, warning/error reporting, banner output, and final result formatting directly through `Console.WriteLine`. That makes the CLI hard to automate because machine-readable output is incomplete and inconsistent.

Introducing a structured shared result contract allows the core versioning library to describe what happened without owning CLI presentation. `Versonify` can then own the rendering layer and provide both backward-compatible text output and an opt-in JSON message stream for automation scenarios.

# Acceptance
- By default, running `Versonify` without `--json` preserves the current human-readable console experience:
  - existing text output remains the default;
  - the greeting/banner still appears in text mode;
  - current text result formatting for passive output, behaviour output, update flows, warnings, validation failures, and fatal errors remains materially unchanged unless a test must be updated for an intentional wording cleanup.
- A new `--json` CLI option is added in `src/Versonify/VersonifyCommandline.cs` and is wired through `src/Versonify/Program.cs` as an output-mode selector for console rendering only.
- When `--json` is present, all user-facing console output is emitted as newline-delimited JSON (one JSON object per emitted message) and no human-only banner/decorative line is written before, between, or after those JSON objects.
- Greeting/banner output is suppressed entirely in JSON mode.
- Shared output/result contract ownership is split correctly:
  - shared structured result/message types that describe versioning activity and outcomes live in `src/Plisky.Versioning`;
  - CLI-specific wrappers, message envelopes, and rendering logic for text vs JSON live in `src/Versonify`;
  - the change does not move core versioning logic into `Versonify`, and does not move CLI presentation concerns into `Plisky.Versioning`.
- `Plisky.Versioning` is refactored so versioning execution returns structured output/result information instead of relying on `VersioningOutputter` or `Program.cs` to be the only place where meaningful output is assembled.
- `VersioningOutputter` and surrounding output flow are updated so console output can be derived from structured shared results while non-console outputs (`File`, `Environment`, `NukeFusion`) remain behaviorally unchanged.
- `src/Versonify/Program.cs` no longer owns scattered direct `Console.WriteLine` calls for normal command reporting. Current console messages are represented as structured messages/events so JSON mode covers the same operational surface, including:
  - startup/progress/info lines;
  - warnings;
  - validation failures;
  - fatal/unhandled errors;
  - successful final results.
- The JSON event shape is defined at a practical contract level and used consistently across commands. Each emitted JSON object includes, at minimum:
  - a machine-usable message category or event type;
  - a human-readable text/message field;
  - severity or equivalent classification where relevant;
  - command/context fields when relevant to understand the event;
  - structured payload fields for result-specific data when relevant, such as version value, release value, behaviour output, updated file details, or validation/failure details.
- No schema/version field is introduced in this task.
- JSON mode covers representative command flows rather than only passive success paths. Acceptance is satisfied only if structured output is available for flows such as:
  - `passive`;
  - `behaviour`;
  - `updatefiles`;
  - `set`;
  - `createversion`;
  - and command-validation/failure paths before execution completes successfully.
- Validation and error handling remain machine-usable in JSON mode:
  - validation failures emit JSON error events instead of plain text;
  - fatal/unhandled exceptions emit JSON error events instead of plain text;
  - the process exit code remains correct and unchanged by JSON rendering alone;
  - `--json` does not silently force success or alter `-NoError` / return-zero semantics beyond the existing contract.
- Text mode and JSON mode both render from the same structured execution/result information so the two modes stay aligned in coverage even if the textual wording differs from JSON payload fields.
- Existing console-only decoration or incidental text that cannot be represented meaningfully as structured output is either removed from JSON mode or converted into a deliberate informational event; JSON mode must not emit stray non-JSON lines.
- Automated coverage is updated to prove both modes:
  - `src/Plisky.Versioning.Test/VersonifyCommandLineTests.cs` covers the new option/model behavior that remains unit-testable;
  - `src/Versonify.ITest/CommandLineArgumentCoverageTests.cs` or equivalent CLI integration coverage verifies default text mode, `--json`, banner suppression in JSON mode, JSON validation/failure output, JSON success output, and representative command flows;
  - tests assert newline-delimited JSON objects rather than a single aggregated JSON document.
- Completion means an engineer can implement the feature starting from `src/Plisky.Versioning/VersioningOutputter.cs`, `src/Versonify/Program.cs`, `src/Versonify/VersonifyCommandline.cs`, `src/Versonify.ITest/CommandLineArgumentCoverageTests.cs`, and `src/Plisky.Versioning.Test/VersonifyCommandLineTests.cs` without needing another discovery pass.
