namespace PliskyTool;

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Plisky.Plumbing;


internal class Program {
    public static CommandLineArguments options = new CommandLineArguments();
    private static CompleteVersion versionerUsed;
    private static VersionStorage storage;

    private static void Main(string[] args) {
        Console.WriteLine("Plisky Tool - Online.");

        var clas = new CommandArgumentSupport();

        clas.ArgumentPostfix = "=";
        try {
            clas.ProcessArguments(options, args);
        } catch (ArgumentOutOfRangeException aox) {
            Console.WriteLine("Fatal: Invalid Arguments Passed to Pliskytool.");
            Console.WriteLine($"{aox.ParamName} - {aox.Message}");
            return;
        } catch (TargetInvocationException tox) {
            if (tox.InnerException.GetType() == typeof(ArgumentOutOfRangeException)) {
                var axx = (ArgumentOutOfRangeException)tox.InnerException;
                Console.WriteLine("Fatal: Invalid Arguments Passed to Pliskytool.");
                Console.WriteLine($"{axx.ParamName} - {axx.Message}");
            } else {
                throw;
            }
        }

        if (!ValidateArgumentSettings(options)) {
            // TODO : Merge this in with the same code below
            Console.WriteLine("Fatal:  Argument Validation Failed.");
            Console.WriteLine();
            string s = clas.GenerateShortHelp(options, "Plisky Tool");
            Console.WriteLine(s);
            return;
        }

        if ((options.Debug) || (!string.IsNullOrEmpty(options.Trace))) {

            Console.WriteLine("Debug Mode, Adding Trace Handler");

            Bilge.AddHandler(new ConsoleHandler(), HandlerAddOptions.SingleType);

            Bilge.SetConfigurationResolver((name, inLevel) => {

                var returnLvl = SourceLevels.Verbose;

                if ((options.Trace != null) && (options.Trace.ToLowerInvariant() == "info")) {
                    returnLvl = SourceLevels.Information;
                }

                if ((name.Contains("Plisky-Versioning")) || (name.Contains("Plisky-Tool"))) {
                    return returnLvl;
                }
                return inLevel;
            });
        }

        var b = new Bilge("Plisky-Tool");
        Bilge.Alert.Online("Plisky-Tool");
        b.Verbose.Dump(options, "App Options");

        if (PerformActionsFromCommandline()) {
            if (versionerUsed != null) {
                var vo = new VersioningOutputter(versionerUsed);
                vo.ConsoleTemplate = options.ConsoleTemplate;
                vo.DoOutput(options.OutputsActive);
            }

            b.Info.Log("All Actions - Complete - Exiting.");
        } else {
            // TODO : Merge this in with the same code Above
            string s = clas.GenerateShortHelp(options, "Plisky Tool");
            Console.WriteLine(s);
        }

        b.Verbose.Log("Plisky Tool - Exit.");
        b.Flush();

    }

    private static bool ValidateArgumentSettings(CommandLineArguments options) {
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

        return valid;
    }

    private static bool PerformActionsFromCommandline() {
        Console.WriteLine("Performing Versioning Actions");

        GetVersionStorageFromCommandLine();
        if (!storage.IsValid) {
            return false;
        }

        string cmdCheck = options.Command.ToUpper();

        switch (cmdCheck) {
            case "CREATEVERSION":
                CreateNewVersionStore();
                return true;

            case "OVERRIDE":
                CreateNewPendingIncrement();
                return true;

            case "UPDATEFILES":
                ApplyVersionIncrement();
                return true;

            case "PASSIVE":
                LoadVersionStore();
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

        var ver = new Versioning(storage);
        versionerUsed = ver.Version;

        if (options.PerformIncrement) {
            Console.WriteLine("Version Increment Requested - Currently " + ver.GetVersion());
            ver.Increment(options.Release);

            ver.SaveUpdatedVersion();
        }


        Console.WriteLine($"Loaded [{ver.GetVersion()}]");
    }

    private static void CreateNewPendingIncrement() {

        var ver = new Versioning(storage);
        versionerUsed = ver.Version;

        string verPendPattern = options.QuickValue;

        Console.WriteLine($"Apply Delayed Incremenet. [{ver.ToString()}] using [{verPendPattern}]");
        ver.Version.ApplyPendingVersion(verPendPattern);

        if (!options.TestMode) {
            storage.Persist(ver.Version);
            ver.Increment();
            Console.WriteLine($"Saving Overriden Version [{ver.GetVersion()}]");
        } else {
            ver.Version.Increment();
            Console.WriteLine($"Would Save " + ver.Version.ToString());
        }
    }

    private static void ApplyVersionIncrement() {


        var ver = new Versioning(storage, options.TestMode);
        versionerUsed = ver.Version;

        ver.Logger = (v) => {
            Console.WriteLine(v);
        };

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
            ver.SearchForAllFiles(options.Root);
        } else {
            Console.WriteLine($"WARNING >> Path {options.Root} is invalid, skipping.");
        }


        ver.UpdateAllRegisteredFiles();

        ver.SaveUpdatedVersion();
    }

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
            Console.WriteLine("Dry Run Mode Active.");
            actionToPerform = (fn) => {
                Console.WriteLine("Would Update :" + fn);
            };
        }

        return actionToPerform;
    }

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
        if (!string.IsNullOrEmpty(options.QuickValue)) {
            Console.WriteLine($"Setting Release From Command Line: {options.Release}");
        }
        Console.WriteLine($"Creating New Version Store: {startVer}");

        var cv = new CompleteVersion(startVer);
        versionerUsed = cv;

        cv.ReleaseName = options.Release;


        Console.WriteLine($"Saving {cv.GetVersionString()}");
        storage.Persist(cv);

    }
}