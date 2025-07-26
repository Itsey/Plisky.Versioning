namespace Versonify;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Plisky.Plumbing;
using Plisky.Versioning;

internal class Program {
    private const string ALL_DIGITS_WILDCARD = "*";
    public static VersonifyCommandline options = new();
    private static CompleteVersion versionerUsed;
    private static VersionStorage storage;
    private static Bilge b = new Bilge();

    private static async Task<int> Main(string[] args) {
        WriteGreetingMessage();

        CommandArgumentSupport clas = null;
        try {
            clas = GetCommandLineArguments(args);

            if (!ValidateArgumentSettings(options)) {
                WriteErrorConditions(clas);
                return 1;
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return 1;
        }

        if (options.Debug) {
            foreach (string a in args) {
                Console.WriteLine($"Command Line: {a}");
            }
        }

        if (options.Debug || (!string.IsNullOrEmpty(options.Trace))) {
            ConfigureTrace();
        }

        b = new Bilge("Versonify");
        Bilge.Alert.Online("Versonify");
        b.Verbose.Dump(options, "App Options");

        if (PerformActionsFromCommandline()) {
            if (versionerUsed != null) {
                b.Verbose.Log($"All Actions - Complete - Outputting.");
                var vo = new VersioningOutputter(versionerUsed) {
                    ConsoleTemplate = options.ConsoleTemplate,
                    PverFileName = options.PverFileName,
                    Digits = options.GetDigits(),
                    ReleaseRequested = options.Release != null,
                };

                vo.DoOutput(options.OutputsActive, options.RequestedCommand);
            }

            b.Info.Log("All Actions - Complete - Exiting.");
        } else {
            // TODO : Merge this in with the same code Above
            string s = clas.GenerateShortHelp(options, "Versonify");
            Console.WriteLine(s);
        }

        b.Verbose.Log("Versonify - Exit.");
        await b.Flush();
        return 0;
    }

    private static void WriteGreetingMessage() {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        string verString = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        Console.WriteLine($"💖 Versioning By Versonify 💖 ({verString}).");
    }

    private static CommandArgumentSupport GetCommandLineArguments(string[] args) {
        b.Verbose.Flow();

        var result = new CommandArgumentSupport {
            ArgumentPostfix = "="
        };
        try {
            b.Verbose.Log("Processing Arguments");
            result.ProcessArguments(options, args);
            options.OutputOptions = options.RawOutputOptions;
            //b.Verbose.Log($"Seting OO {options.RawOutputOptions} as {options.OutputOptions}");
        } catch (ArgumentOutOfRangeException aox) {
            Console.WriteLine("Fatal: Invalid Arguments Passed to Versonify.");
            Console.WriteLine($"{aox.ParamName} - {aox.Message}");
            throw;
        } catch (TargetInvocationException tox) {
            if (tox.InnerException.GetType() == typeof(ArgumentOutOfRangeException)) {
                var axx = (ArgumentOutOfRangeException)tox.InnerException;
                Console.WriteLine("Fatal: Invalid Arguments Passed to Versonify.");
                Console.WriteLine($"{axx.ParamName} - {axx.Message}");
            } else {
                throw;
            }
        }
        return result;
    }

    private static void WriteErrorConditions(CommandArgumentSupport clas) {
        // TODO : Merge this in with the same code below
        Console.WriteLine("Fatal:  Argument Validation Failed.");
        Console.WriteLine();
        string s = clas.GenerateShortHelp(options, "Versonify.");
        Console.WriteLine(s);
    }

    private static void ConfigureTrace() {
        Console.WriteLine("Debug Mode, Adding Trace Handler");

        _ = Bilge.AddHandler(new ConsoleHandler(), HandlerAddOptions.SingleType);

        Bilge.SetConfigurationResolver((name, inLevel) => {
            var returnLvl = SourceLevels.Verbose;

            if ((options.Trace != null) && (options.Trace.ToLowerInvariant() == "info")) {
                returnLvl = SourceLevels.Information;
            }

            return name.Contains("Plisky-Versioning") || name.Contains("Versonify") ? returnLvl : inLevel;
        });
    }

    private static bool ValidateArgumentSettings(VersonifyCommandline options) {
        bool valid = true;

        // Common checks
        if (!string.IsNullOrWhiteSpace(options.Root) && !Directory.Exists(options.Root)) {
            Console.WriteLine("Error >> Invalid Directory For Root:" + options.Root);
            valid = false;
        }
        if (string.IsNullOrWhiteSpace(options.VersionPersistanceValue)) {
            Console.WriteLine("Error >> A versioning store must be selected.  Use -VS= and pass your initialisation data");
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
        }

        return valid;
    }

    private static bool ValidateDigitsPresent(string[] digits, string commandName) {
        if (digits == null || digits.Length == 0) {
            Console.WriteLine($"Error >> The {commandName} command requires at least one digit to update. Use -DG=<digit> or -DG=*.");
            return false;
        }
        return true;
    }

    private static bool PerformActionsFromCommandline() {
        b.Verbose.Flow();

        Console.WriteLine("Performing Versioning Actions");

        GetVersionStorageFromCommandLine();

        if (!ValidateVersionStorage()) {
            return false;
        }

        switch (options.RequestedCommand) {
            case VersioningCommand.CreateNewVersion:
                CreateNewVersionStore();
                return true;

            case VersioningCommand.Override:
                CreateNewPendingIncrement();
                return true;

            case VersioningCommand.UpdateFiles:
                ApplyVersionIncrement();
                return true;

            case VersioningCommand.PassiveOutput:
                if (options.Release != null) {
                    LoadReleaseName();
                } else {
                    LoadVersionStore();
                }
                return true;

            case VersioningCommand.BehaviourOutput:
                LoadDigitBehaviour();
                return true;

            case VersioningCommand.BehaviourUpdate:
                ApplyDigitBehaviour();
                return true;

            case VersioningCommand.SetDigitValue:
                ApplyDigitValueUpdate();
                return true;

            case VersioningCommand.SetReleaseName:
                ApplyReleaseNameUpdate();
                return true;

            case VersioningCommand.SetDigitPrefix:
                ApplyDigitPrefixUpdate();
                return true;

            default:
                Console.WriteLine("Error >> Unrecognised Command: " + options.Command);
                return false;
        }
    }

    /// <summary>
    /// Most of the versioning approaches require a version store of some sort. This initialises the version store from the command line using the
    /// initialisation data that is passed in to determine which version store to load.
    /// </summary>
    private static void GetVersionStorageFromCommandLine() {
        string vpv = Environment.ExpandEnvironmentVariables(options.VersionPersistanceValue);
        storage = VersionStorage.CreateFromInitialisation(vpv);
    }

    private static void LoadVersionStore() {
        var ver = new Versioning(storage, options.DryRunOnly);
        versionerUsed = ver.Version;

        if (options.PerformIncrement) {
            string v = ver.GetVersion();
            b.Verbose.Log($"Performing increment {v}");
            Console.WriteLine("Version Increment Requested - Currently " + v);
            ver.Increment(options.Release);

            b.Verbose.Log("About to save version store");
            ver.SaveUpdatedVersion();
        }
        Console.WriteLine($"Loaded [{ver.GetVersion()}]");
    }

    private static bool ValidateVersionStorage() {
        if (!storage.IsValid) {
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

    private static void LoadReleaseName() {
        b.Verbose.Flow();
        var ver = new Versioning(storage, options.DryRunOnly);
        versionerUsed = ver.Version;

        if (string.IsNullOrEmpty(ver.Version.ReleaseName)) {
            Console.WriteLine("Release Name in version store is null or empty.");
            return;
        }
        Console.WriteLine($"Loaded Release Name: {ver.Version.ReleaseName}");
    }


    private static void CreateNewPendingIncrement() {
        b.Verbose.Flow();

        var ver = new Versioning(storage, options.DryRunOnly);
        versionerUsed = ver.Version;

        string verPendPattern = options.QuickValue;

        Console.WriteLine($"Apply Delayed Increment. [{ver.ToString()}] using [{verPendPattern}]");
        ver.Version.ApplyPendingVersion(verPendPattern);

        if (!options.DryRunOnly) {
            storage.Persist(ver.Version);
            ver.Increment();
            Console.WriteLine($"Saving Overridden Version [{ver.GetVersion()}]");
        } else {
            ver.Version.Increment();
            Console.WriteLine($"DryRun - Would Save :" + ver.Version.ToString());
        }
    }

    private static void ApplyVersionIncrement() {
        b.Verbose.Flow();

        var ver = new Versioning(storage, options.DryRunOnly);
        versionerUsed = ver.Version;

        ver.Logger = Console.WriteLine;

        if (options.NoOverride) {
            Console.WriteLine("Version Increment Override, Disabled");
            foreach (var l in ver.Version.Digits) {
                l.IncrementOverride = null;
            }
        }
        if (options.PerformIncrement) {
            Console.WriteLine("Version Increment Requested - Currently " + ver.GetVersion());
            ver.Increment(options.Release);
        } else {
            Console.WriteLine("No Version Increment Requested.");
        }

        Console.WriteLine("Version To Write: " + ver.GetVersion());

        // Increment done, now persist and then update the pages - first check if the command line ovverrides the minimatchers
        if ((options.VersionTargetMinMatch != null) && (options.VersionTargetMinMatch.Length > 0)) {
            ver.LoadMiniMatches(options.VersionTargetMinMatch);
        }

        if (!string.IsNullOrEmpty(options.Root) && Directory.Exists(options.Root)) {
            _ = ver.SearchForAllFiles(options.Root);
        } else {
            Console.WriteLine($"WARNING >> Path {options.Root} is invalid, skipping.");
        }

        ver.UpdateAllRegisteredFiles();

        ver.SaveUpdatedVersion();
    }

#if FALSE
    private static Action<string> GetActionToPerform(Versioning ver) {
        Action<string> actionToPerform;
        if (!options.TestMode) {
            actionToPerform = (fn) => {
                if (fn.EndsWith(".cs")) {
                    ver.AddCSharpFile(fn);
                }
                if (fn.EndsWith(".nuspec")) {
                    ver.AddNugetFile(fn);
                }
            };
        } else {
            actionToPerform = (fn) => {
                Console.WriteLine("DryRun - Would Update :" + fn);
            };
        }

        return actionToPerform;
    }
#endif

    [Conditional("DEBUG")]
    private static void DebugLog(string l) {
#if DEBUG
        Console.WriteLine(l);
#endif
    }

    private static void CreateNewVersionStore() {
        string startVer = "0.0.0.0";
        if (!string.IsNullOrEmpty(options.QuickValue)) {
            Console.WriteLine($"Using Value From Command Line: {options.QuickValue}");
            startVer = options.QuickValue;
        }
        if (!string.IsNullOrEmpty(options.Release)) {
            Console.WriteLine($"Setting Release From Command Line: {options.Release}");
        }
        Console.WriteLine($"Creating New Version Store: {startVer}");

        var cv = new CompleteVersion(startVer);
        versionerUsed = cv;

        cv.ReleaseName = options.Release;

        Console.WriteLine($"Saving {cv.GetVersionString()}");
        storage.Persist(cv);
    }

    private static void LoadDigitBehaviour() {
        var ver = new Versioning(storage, options.DryRunOnly);
        versionerUsed = ver.Version;
        if (!ver.Version.ValidateDigitOptions(options.DigitManipulations)) {
            return;
        }

        string[] digitsToLoad = options.GetDigits();
        if (digitsToLoad[0] == ALL_DIGITS_WILDCARD) {
            Console.WriteLine("Loading All Behaviours");
            Console.WriteLine(ver.GetBehaviour(digitsToLoad[0]));
        } else {
            Console.WriteLine($"Loading Behaviour for Digits [{string.Join(',', digitsToLoad)}]");
            foreach (string digit in digitsToLoad) {
                Console.WriteLine(ver.GetBehaviour(digit));
            }
        }
    }

    private static void ApplyDigitBehaviour() {
        var newBehaviour = options.IncrementBehaviour;
        var ver = new Versioning(storage, options.DryRunOnly);
        versionerUsed = ver.Version;

        if (!ver.Version.ValidateDigitOptions(options.DigitManipulations)) {
            return;
        }

        string[] digitsToUpdate = options.GetDigits();
        if (digitsToUpdate.Length > 0 && digitsToUpdate[0] == ALL_DIGITS_WILDCARD) {
            Console.WriteLine($"Setting All Behaviours to {newBehaviour}");
        } else {
            Console.WriteLine($"Setting Behaviour for Digit[{string.Join(',', digitsToUpdate)}] to {newBehaviour}({(int)newBehaviour})");
        }

        foreach (string digit in digitsToUpdate) {
            ver.UpdateBehaviour(digit, newBehaviour);
        }

        if (!options.DryRunOnly) {
            Console.WriteLine("Saving Updated Behaviour");
            ver.SaveUpdatedVersion();
        } else {
            DisplayDryRunBehaviours(ver, digitsToUpdate);
        }
    }

    private static void DisplayDryRunBehaviours(Versioning ver, string[] digitsToUpdate) {
        Console.WriteLine("DryRun - Would Save:");
        foreach (string digit in digitsToUpdate) {
            Console.WriteLine(ver.GetBehaviour(digit));
        }
    }

    private static void ApplyDigitValueUpdate() {
        var ver = new Versioning(storage, options.DryRunOnly);
        versionerUsed = ver.Version;

        string[] digitsToUpdate = options.GetDigits();
        string valueToSet = options.QuickValue;

        if (ShouldSetCompleteVersionFromString(digitsToUpdate, valueToSet)) {
            ver.Version.SetCompleteVersionFromString(valueToSet);
            Console.WriteLine($"Set version to: {ver.Version.GetVersionString()}");
        } else {
            if (!ver.Version.ValidateDigitOptions(digitsToUpdate)) {
                Console.WriteLine("Error >> Invalid digit selection for value update.");
                return;
            }
            if (digitsToUpdate.Length > 0 && digitsToUpdate[0] == ALL_DIGITS_WILDCARD) {
                Console.WriteLine($"Setting all digits to value: {valueToSet}");
            } else {
                Console.WriteLine($"Setting digit(s) [{string.Join(',', digitsToUpdate)}] to value: {valueToSet}");
            }
            ver.Version.SetIndividualDigits(digitsToUpdate, valueToSet);
        }

        if (!options.DryRunOnly) {
            Console.WriteLine("Saving Updated Digit Values");
            ver.SaveUpdatedVersion();
            Console.WriteLine($"[{ver.Version.GetVersionString()}]");
        } else {
            Console.WriteLine("DryRun - Would Save:");
            Console.WriteLine($"[{ver.Version.GetVersionString()}]");
        }
    }

    private static bool ShouldSetCompleteVersionFromString(string[] digitsToUpdate, string valueToSet) {
        if (string.IsNullOrWhiteSpace(valueToSet)) {
            return false;
        }
        if (digitsToUpdate.Length == 0 && valueToSet.Contains('.')) {
            return true;
        }
        return false;
    }
 
    private static void ApplyReleaseNameUpdate() {
        var ver = new Versioning(storage, options.DryRunOnly);
        versionerUsed = ver.Version;
        string newReleaseName = options.Release;

        ver.Version.SetReleaseName(newReleaseName);
        if (!options.DryRunOnly) {
            Console.WriteLine($"Saving new Release Name as: {newReleaseName}");
            ver.SaveUpdatedVersion();
        } else {
            Console.WriteLine("DryRun - Would Save:");
            Console.WriteLine($"[{newReleaseName}]");
        }
    }

    private static void ApplyDigitPrefixUpdate() {
        var ver = new Versioning(storage, options.DryRunOnly);
        versionerUsed = ver.Version;

        string[] digitsToUpdate = options.GetDigits();
        string prefixToSet = options.QuickValue;

        if (digitsToUpdate.Length > 0 && digitsToUpdate[0] == ALL_DIGITS_WILDCARD) {
            Console.WriteLine($"Setting prefix for all digits to: {prefixToSet}");
            ver.Version.SetPrefixForDigit(ALL_DIGITS_WILDCARD, prefixToSet);
        } else {
            Console.WriteLine($"Setting prefix for digit(s) [{string.Join(',', digitsToUpdate)}] to: {prefixToSet}");
            foreach (string digit in digitsToUpdate) {
                ver.Version.SetPrefixForDigit(digit, prefixToSet);
            }
        }

        if (!options.DryRunOnly) {
            Console.WriteLine("Saving updated digit prefixes");
            ver.SaveUpdatedVersion();
            Console.WriteLine($"[{ver.Version.GetVersionString()}]");
        } else {
            Console.WriteLine("DryRun - Would Save:");
            Console.WriteLine($"[{ver.Version.GetVersionString()}]");
        }
    }
}