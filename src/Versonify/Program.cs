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
    public static VersonifyCommandline options = new();
    private static CompleteVersion versionerUsed;
    private static VersionStorage storage;
    private static Bilge b = new Bilge();

    private static async Task<int> Main(string[] args) {
        Console.WriteLine("Versonify - Online.");



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
                    Digits = options.GetDigits()
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

        if (!string.IsNullOrWhiteSpace(options.Root)) {
            if (!Directory.Exists(options.Root)) {
                Console.WriteLine("Error >> Invalid Directory For Root:" + options.Root);
                valid = false;
            }
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

        if (options.RequestedCommand == VersioningCommand.BehaviourOutput || options.RequestedCommand == VersioningCommand.BehaviourUpdate) {
            if (options.DigitManipulations is null || options.DigitManipulations.Length == 0) {
                Console.WriteLine("Error >> The Behaviour command requires at least one digit to be specified. Use -DG=<digit> or -DG=* .");
                valid = false;
            }
        }

        if (options.RequestedCommand == VersioningCommand.SetDigitValue) {
            if (string.IsNullOrWhiteSpace(options.QuickValue)) {
                Console.WriteLine("Error >> The Set command requires a value to set. Use -Q=<value>.");
                valid = false;
            }
            if (options.DigitManipulations == null || options.DigitManipulations.Length == 0) {
                Console.WriteLine("Error >> The Set command requires at least one digit to update. Use -DG=<digit> or -DG=*.");
                valid = false;
            }
        }

        return valid;
    }

    private static bool PerformActionsFromCommandline() {
        b.Verbose.Flow();

        Console.WriteLine("Performing Versioning Actions");

        GetVersionStorageFromCommandLine();
        if (!storage.IsValid) {
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
                LoadVersionStore();
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
        if (digitsToLoad[0] == "*") {
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
        if (digitsToUpdate.Length > 0 && digitsToUpdate[0] == "*") {
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

        if (!ver.Version.ValidateDigitOptions(digitsToUpdate)) {
            Console.WriteLine("Error >> Invalid digit selection for value update.");
            return;
        }

        if (digitsToUpdate.Length > 0 && digitsToUpdate[0] == "*") {
            Console.WriteLine($"Setting all digits to value: {valueToSet}");
            ver.Version.ApplyValueUpdate("*", valueToSet);
        } else {
            Console.WriteLine($"Setting digit(s) [{string.Join(',', digitsToUpdate)}] to value: {valueToSet}");
            foreach (string digit in digitsToUpdate) {
                ver.Version.ApplyValueUpdate(digit, valueToSet);
            }
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
}