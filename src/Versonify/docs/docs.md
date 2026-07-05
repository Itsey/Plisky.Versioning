# Versonify CLI arguments and Help

AI/automation: run `versonify --get-md-help` to save this file as `docs.md` in the current working directory. This document covers the supported commands and options in the current version.

## Usage

`versonify [command] [options]`

## Commands

| Command | Purpose |
| --- | --- |
| `createversion` | Creates a new version store at the specified location.  Used with -v and a path to the version source.  The path can be either a nexus formatted path or a local system path or UNC file path.   -v Must be present for this to work, ask the user for the path if it is not already known. |
| `override` | Queues a pending version pattern ready for the next increment.  When override is specified then -q is also specified with either a version number or a version pattern, this will ensure that on the next increment the override is used. |
| `updatefiles` | Increments the version and updates matched files. |
| `passive` | Reads the current version without changing it. |
| `behaviour` | Reads or updates digit behaviour. |
| `set` | Sets a digit value or release name. |
| `prefix` | Sets a digit prefix. |

## Options

| Option | Short aliases | Purpose |
| --- | --- | --- |
| `--command` |  | Sets the command explicitly. |
| `--debug` |  | Echoes arguments and enables debug logging. |
| `--dry-run` |  | Runs without persisting changes. |
| `--digits` | `-d` | Selects digit indexes, or `*` for all digits. |
| `--no-error` | `-z` | Forces a zero exit code on failure. |
| `--no-override` |  | Ignores any saved pending override. |
| `--output` | `-o` | Selects output mode. |
| `--increment` | `-i` | Increments before other work. |
| `--quick-value` | `-Q` | Supplies the quick value or pattern text.  For a quick value then a standard version number is supplied with the relevant number of digits.  E.g. `1.0`  or `1.0.0` or `5.345.233.1.1.1`.  If a pattern is to be used then this will indicate actions to take on existing digits and will use + or - symbols to perform the activities.  e.g.  `+.-..` will increment the first digit, decrement the second digit and leave all other digits unchanged whereas `..+.+` would leave the first and second digits unchanged and increment the third and fourth digits. |
| `--release` | `-R` | Sets the release name. |
| `--root` |  | Sets the root directory for file updates. |
| `--trace` |  | Sets trace level.  This is used in debugging, if you are finding that it is not working as intended then this should be set to ensure that additional logging is present. |
| `--version` |  | Shows the application version and exits. |
| `--version-source` | `-v` | Points to the version store. |
| `--min-match` | `-m` | Supplies minmatch patterns for file updates.  The minmatch is a glob pattern used to identify files in the solution that should be updated with the version numbers.  This is usually csproj files and text files. |
| `--digit-group` | `-g` | Targets named digit groups. In `set`, assigns a group to selected digits. In `passive`, filters displayed digits. In increment flows, selects which group(s) to increment. |
| `--pre-release` | `-p` | Pre-release shortcut. For `passive`, behaves like `--digit-group=default,pre-release`. For mutation/increment flows, behaves like `--digit-group=pre-release`. Cannot be combined with `--digit-group`. |
| `--get-md-help` |  | Writes this markdown file to the current directory.  This only needs to be done once and then the mark down file can be used for the detailed documentation. |
| `--help` | `-h` | Shows the CLI help text. |
| `--qqpnf` |  | Returns an exit code indicating a compatibility level. This document is compatible with exit code 201. |





### File Update Statements

Versonify uses a series of file update min-matches to identify which files to update. These can be stored in a separate text file, in the configuration file or passed on the command line.  Each one looks like this:

`<minmatch path>|<versioningtype>`

The possible versioning types are:

| Versioning Type | Description |
| --- | --- |
| `Nuspec` | Suitable for a nuspec file, updating the version element in the nuspec directly. |
| `TextFile` | Generic text file replacement, replacing version tokens in the file. |
| `StdAssembly` | .NET standard csharp project, update with assembly version. |
| `StdInformational` | .NET standard csharp project, update with informational version. |
| `StdFile` | .NET standard csharp project, update with standard version. |
| `NetAssembly` | .NET framework assembly version attribute update. |
| `NetInformational` | .NET framework assembly informational version attribute update. |
| `NetFile` | .NET framework assembly file version attribute update. |
| `Wix` | Wix installation file product version update. |

An example set of file update statements:

```
**/src/**/*.nuspec|Nuspec
**/src/**/*.nuspec|TextFile
**/src/**/Plisky.Diagnostics.csproj|StdAssembly
**/src/**/Plisky.Diagnostics.csproj|StdInformational
**/src/**/Plisky.Diagnostics.csproj|StdFile
```

If the file type is not specified here then the text file type can be used for any type of file.  The text file type works by replacing the following tokens:

| Token | Description |
| --- | --- |
| `XXX-VERSION-XXX` | Replaced with the default/active version format. |
| `XXX-VERSION2-XXX` | Replaced with a 2-digit version format (Short). |
| `XXX-VERSION3-XXX` | Replaced with a 3-digit version format (ThreeDigitNumeric). |
| `XXX-VERSION4-XXX` | Replaced with a 4-digit version format (FourDigitNumeric). |
