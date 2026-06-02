# Backlog

- Versonify can still print a version-store access/error message but exit with code `0` for some missing or inaccessible version-store flows; investigate and align the exit code with the failure state.
- Embed a skill as a resource into versonify and support --get-skill to extract the skill as an md file. Source the skill from the existing versonify skill and place it in _Dependencies for adding as a resource.
- [ ] **Fix finalizer in OutputModeAndValidationTests** — `OutputModeAndValidationTests.cs:19` uses a finalizer `~OutputModeAndValidationTests()` for cleanup instead of `IDisposable.Dispose()`; should implement `IDisposable` and move cleanup to `Dispose()`. _(found during: Migrate Versonify CLI parsing from Plisky.Plumbing to System.CommandLine)_
- [ ] Add double dashes to all of the commands as aliases to enable the standard --parameter-bit type format.  Currently only single dashes are used for backward compatibility but we should support the better target state of double dashes.
- [ ] **Fix failing Exploratory nuke-marker tests** — `Versonify.ITest/Exploratory.cs` tests `Console_with_nuke_has_markers` and `Console_does_not_have_nuke_markers` currently fail baseline (exit code/assertion mismatch) and should be aligned with current CLI behavior. _(found during: Migrate CLI options to double-dash POSIX-style naming)_


