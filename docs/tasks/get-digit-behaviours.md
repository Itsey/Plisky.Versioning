---
status: todo
title: Add Get command line option to return individual and all digit informaton
created: 2026-08-11
priority: medium
reference: LFY-74
---

# Reference
LFY-74 — Add `--Command=Get` and top-level `Get` command line options to Versonify to retrieve individual and all digit increment information in plain English and JSON formats.

# What
Add a `Get` command option to `Versonify` (supported via `--command=get` or `get`) that queries and outputs digit information metadata for specified digits or all digits.

The `Get` command supports two output modes:
1. **Plain English Output (Default)**: Outputs human-readable sentence(s) describing each queried digit's status (e.g. `"Digit at position [0] has prefix "", value "1", and  is set to Fixed behaviour. It belongs to the digit-group default and its Queued Override value is null."` This output is repeated for each digit queried. If multiple digits are queried, they are separated by new lines.
2. **JSON Output (`--json`)**: Outputs a JSON dictionary keyed by digit position, containing detailed properties for each digit: keyed on digit position with the values being `digitValue`, `digitBehaviour`, `digitQueuedOverride`, `digitgroup` and `digitPrefix`.

When invoked with specific digit indices (e.g. `-Digits=0` or `-Digits=0,2`), only the specified digits are returned. If `-Digits` is omitted or specified as a wildcard (`*`), it defaults to returning all digits in the active version store.

Where a digit-group is specified then this is used to select the digits.

Note the implementation of the `--json` command line argument is for a different, prerequisite task.



# Why
There is no consistent way of reading the values from a version store currently without reading the file directly.

Setting the digits requires quite a detailed knowledge of the expected behaviour that is hard to glean from the current output methods therefore its very difficult for an AI to manipulate the digits in the file accurately.  In order to determine what the approach would be for an update a read is required for all of the digits.

For example, when combining a pre and a release version to use the new digit grouping capability.  You would need to add digits to the release one from the pre-release file.  There is no simple way of retrieving the information about what the values, behaviours and approach is for each digit.  Adding a dedicated `Get` option (`-Command=Get` / `Get`) provides an explicit, intuitive read API that supports both human inspection in plain English and machine consumption via a structured JSON dictionary. 

# Acceptance

- Given `Versonify` is executed with `-Command=Get` or `-Get` in plain text mode (default):
  - When `-Digits=0` is specified, it writes a plain English sentence: `"Digit at position [0] has prefix "<prefix>", has value "<DigitValue>", and is set to <BehaviourName> behavior. It belongs to digit-group <DigitGroup>, and its Queued Override value is <QueuedOverrideValue>"`.
  - When `-Digits=*` or no `-Digits` parameter is specified, it writes sentences for all digits.
  
- Given `Versonify` is executed with `--command=Get` or `Get` in JSON mode (`--json`):
  - It writes a JSON object dictionary keyed by digit position containing:
    - `digitValue`: current string value of the digit
    - `digitBehaviour`: `DigitIncrementBehaviour` enum string (e.g. `"Fixed"`, `"AutoIncrementWithReset"`)
    - `digitQueuedOverride`: string or null representing pending/queued increment override
    - `digitPrefix`: string prefix character (e.g. `"."` or `"-"`)
    - `digitgroup`: the name of the digit group.
  - Example structure:
    ```json
    {
      "0": {
        "digitValue": "1",
        "digitBehaviour": "Fixed",
        "digitQueuedOverride": null,
        "digitPrefix": "",
        "digitgroup" : "default"
      },
      "1": {
        "digitValue": "0",
        "digitBehaviour": "AutoIncrementWithReset",
        "digitQueuedOverride": "+",
        "digitPrefix": ".",
        "digitgroup" : "default"
      }
    }
    ```
  
- Given multiple digit indices (e.g. `-Digits=0;2`), only the requested digit entries are included in the plain text sentences or JSON dictionary output.

- Given an invalid or out-of-range digit index is provided (e.g. `-Digits=9`), `Versonify` outputs an error message and terminates with a non-zero exit code.

- Existing commands and output options remain backwards-compatible.

- Given a digit-group is provided this will limit the return of the digits to that grouping where groups are supported.  
  e.g. 
  `versonify get -digit-group=pre -d=2 -v=<store>`   > Retrieves the second digit from the pre grouped digits.
  `versonify get -digit-group=pre -d=* -v=<store>`   > Retrieves all pre digits, same as not specifying -d.
  `versonify get -v=<store>`   > Retrieves all digits for all groups.

  Where the digit is out of range of the group, an error occurs.

  Where the group is invalid an error occurs.
  Where the group is not specified it applies to all digits.

# Out of Scope

- Modifying digit behaviours or values during a `Get` command execution.
- Changing JSON output format for non-`Get` commands.

# Assumptions & Constraints

- **Prerequisite Dependency**: Task #2 (`docs/tasks/json-output.md`: "Add JSON console output mode for Versonify") MUST be completed and `--json` console output support implemented prior to building this task's JSON output capability.
- `Get` command follows standard `Versonify` CLI parameter parsing rules (`--command=Get` and `Get` alias).
- Defaults to querying all digits (`*`) if `-Digits` is omitted.
- Respects active version storage initialized via `--version-source=` / `-v=`.
