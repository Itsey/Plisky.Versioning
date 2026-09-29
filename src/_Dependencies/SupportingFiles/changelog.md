## Versonify Change Log.

⬆️ **XXX-VERSION3-XXX** - Internal structure and CLI improvements.

  - ✅ Feature - Added digit-group targeting and the `--pre-release` shortcut for passive reads, updates, and queued version changes.
  - ✅ Feature - Added the `get` command to inspect digit values, behaviours, prefixes, queued overrides, and groups in plain text or `jcon` output.
  - ✅ Feature - Added structured JSON console output with explicit message categories, including machine-readable results, warnings, validation failures, and errors.
  - ✅ Feature - Added the built-in `--version` command.
  - ✅ Feature - Added embedded Markdown help via `--get-md-help` designed to support AI Agents.
  - ✅ Feature - Added --flush command, an edge case switch if trace and debug output is getting truncated, not expected to be needed.
  - ✅ Feature - Improved command-line parsing and validation, including canonical long options and deprecation handling for legacy aliases.
  - ✅ Feature - Added manual CLI prompt coverage and expanded integration tests for grouped digits, output modes, and validation scenarios.
  - 🐞 Fix - Corrected version-number comparison so a higher leading digit is not outweighed by later digits.
  - 🔧 Maintenance - Refactored the CLI and versioning output flow for nullable reference types, C# 12, and clearer build/test automation.



⬆️ **1.0.3** - Austen Compatibility Release.
  - ✅ Feature - Implemented --QQpnf quick version return exit codes to tell PNF what compatibility version Versonify is running. 
  - ✅ Feature - Implemented -z to suppress non zero return exit codes.
  - ✅ Feature - 💥Breaking Change💥 File Updates that do not update any files now default to returning non zero exit code.  Add -z for old functionality.

Note that the QQpnf feature is not really aimed at end users but purely at the Plisky.Nuke.Fusion library to ensure that it understands what parameters are available to different versions of Versonify.  File updates that update no files now default to an error, this was because it was more common that it was a mistake rather than intentional.

Changes to command line support are coming, with a view to standardising the command line in the way that is now more common.  These warnings are added to this version:

⚠️ -MM is now deprecated.  Use -M instead.
⚠️ -VS is now deprecated. Use -V instead.
⚠️ -NO is now deprecated. Use -NoOverride instead.
⚠️ -DG is now deprecated. Use -D instead. 



⬆️ **1.0.1** - Austen.
  - First Release ( Austen Release ).
  - Behaviour Update Added.

⬆️ 0.2.0 - Austen Pre-Release.
  - Moved to being a dotnet tool.
  - Now supports display of behaviours.  ( -Command=behaviour)

⬆️ 0.1.3 - Initial.
  - 🐞 Critical Bug - Typo in exe name in package corrected.  0.1.2. Superceeded.

⬆️  0.1.2 - Initial.
  - Added Nexus Support.      

⬆️  0.1.1 - Initial.
  - Updated documentation to remove references to PliskyTool.
  - 🐞 Fix - DryRun no longer updates the files on disk.
