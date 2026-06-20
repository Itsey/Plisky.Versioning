namespace Versonify;

using System;
using System.IO;
using Plisky.CodeCraft;
using Plisky.Versioning;

public static class ArgumentValidator {
    public static bool ValidateArgumentSettings(VersonifyOptions options) {
        bool valid = true;

        // Common checks
        if (!string.IsNullOrWhiteSpace(options.Root) && !Directory.Exists(options.Root)) {
            Console.WriteLine("Error >> Invalid Directory For Root:" + options.Root);
            valid = false;
        }
        if (string.IsNullOrWhiteSpace(options.VersionPersistanceValue)) {
            Console.WriteLine("Error >> A versioning store must be selected.  Use -V= and pass your initialisation data");
            valid = false;
        }
        if (!string.IsNullOrWhiteSpace(options.PverFileName)) {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (options.PverFileName.IndexOfAny(invalidChars) >= 0) {
                Console.WriteLine($"The output file name [{options.PverFileName}] contains invalid characters.");
                valid = false;
            }
        }

        // Command-specific checks
        switch (options.RequestedCommand) {
            case VersioningCommand.BehaviourOutput:
            case VersioningCommand.BehaviourUpdate:
                valid &= ValidateDigitsPresent(options.DigitManipulations, "Behaviour");
                break;
            case VersioningCommand.SetDigitValue:
                if (string.IsNullOrWhiteSpace(options.QuickValue)) {
                    Console.WriteLine("Error >> The Set command requires a value to set. Use -Q=<value> to set digit value. Use -Release=<value> to set release name.");
                    valid = false;
                } else if (!options.QuickValue.Contains('.')) {
                    // Only require digits if not setting the full version string
                    valid &= ValidateDigitsPresent(options.DigitManipulations, "Set");
                }
                break;
            case VersioningCommand.Override:
                if (string.IsNullOrWhiteSpace(options.QuickValue)) {
                    Console.WriteLine("Error >> The Override command requires a version pattern to apply. Use -Q=<pattern> to set the override pattern.");
                    valid = false;
                }
                break;
            case VersioningCommand.SetReleaseName:
                if (!string.IsNullOrEmpty(options.QuickValue)) {
                    Console.WriteLine("Error >> Both QuickValue (-Q) and Release (-R) cannot be provided for the Set command. Please specify only one.");
                    valid = false;
                }
                break;
            case VersioningCommand.SetDigitPrefix:
                valid &= ValidateDigitsPresent(options.DigitManipulations, "Prefix");
                if (options.QuickValue == null) {    // Allow empty string or whitespace as valid prefix
                    Console.WriteLine("Error >> The Prefix command requires a prefix value. Use -Q=<prefix> (can be empty string).");
                    valid = false;
                }
                break;
            case VersioningCommand.UpdateFiles:
                if ((options.VersionTargetMinMatch == null) || (options.VersionTargetMinMatch.Length == 0)) {
                    Console.WriteLine("Error >> The Update command requires a minmatch .txt file to be provided. Use -M=<path to minmatch file.>");
                    valid = false;
                }
                break;
        }

        return valid;
    }

    public static bool ValidateDigitsPresent(string[]? digits, string commandName) {
        if (digits == null || digits.Length == 0) {
            Console.WriteLine($"Error >> The {commandName} command requires at least one digit to update. Use -D=<digit> or -D=*.");
            return false;
        }
        return true;
    }

    public static bool ValidateVersionStorage(VersionStorage? storage, VersonifyOptions options) {
        if (storage == null || !storage.IsValid) {
            return false;
        }

        bool vstoreExists = storage.DoesVstoreExist();
        if (!vstoreExists && options.RequestedCommand != VersioningCommand.CreateNewVersion) {
            Console.WriteLine($"Error >> Version Store {options.VersionPersistanceValue} does not exist or is inaccessible.");
            return false;
        }
        if (vstoreExists && options.RequestedCommand == VersioningCommand.CreateNewVersion) {
            Console.WriteLine($"Error >> Version store {options.VersionPersistanceValue} already exists.");
            return false;
        }
        return true;
    }

    public static bool ShouldSetCompleteVersionFromString(string[] digitsToUpdate, string? valueToSet) {
        if (string.IsNullOrWhiteSpace(valueToSet)) {
            return false;
        }
        if (digitsToUpdate.Length == 0 && valueToSet.Contains('.')) {
            return true;
        }
        return false;
    }
}
