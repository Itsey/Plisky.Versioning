namespace Versonify;

using System;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Plisky.Versioning;

internal class Program {
    private const string ALL_DIGITS_WILDCARD = "*";
    public static VersonifyCommandline options = new();
    private static CompleteVersion versionerUsed;
    private static VersionStorage storage;
    private static Bilge b = new Bilge();

    private static RootCommand rootCommand;
    private static Argument<string> commandArg;
    private static Option<string> commandOpt;
    private static Option<bool> debugOpt;
    private static Option<bool> dryRunOpt;
    private static Option<string> digitsOpt;
    private static Option<bool> noErrorOpt;
    private static Option<bool> noOverrideOpt;
    private static Option<string> outputOpt;
    private static Option<bool> incrementOpt;
    private static Option<string> quickValueOpt;
    private static Option<string> releaseOpt;
    private static Option<string> rootPathOpt;
    private static Option<string> traceOpt;
    private static Option<string> versionSourceOpt;
    private static Option<string> minMatchOpt;

    private static async Task<int> Main(string[] args) {
        try {
            int pnfShortCircuit = CheckPnfCompatibiliyRequest(args);
            if (pnfShortCircuit >= 200) {
                return pnfShortCircuit;
            }

            WriteGreetingMessage();

            if (!GetCommandLineArguments(args)) {
                WriteErrorConditions();
                return 1;
            }

            if (!ValidateArgumentSettings(options)) {
                WriteErrorConditions();
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

            var result = PerformActionsFromCommandline();
            if (result.WasProcessedSuccessfully) {
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
                Console.WriteLine("Errors Occurred:");
                foreach (string e in result.Errors) {
                    Console.WriteLine(e);
                }
                Console.WriteLine();
                rootCommand.Parse(new[] { "--help" }).Invoke(new System.CommandLine.InvocationConfiguration());
            }

            b.Verbose.Log("Versonify - Exit.");
            await b.Flush();

            if (options.ReturnZero) {
                Console.WriteLine($"ReturnZero option specified:  ExitCode: {result.ExitCode} suppressed.");
                return 0;
            }

            return result.ExitCode;
        } catch (Exception ex) {
            Console.WriteLine("Fatal: An unhandled exception was encountered. " + ex.Message);
            return 1;
        }
    }

    private static int CheckPnfCompatibiliyRequest(string[] args) {
        if (args.Length == 1 && args[0].Equals("--QQpnf", StringComparison.OrdinalIgnoreCase)) {
            // 200 is the first implemented compatibility exit code. Before this no compatibility exit codes existed
            // This equates to Versonify Release 1.0.1 Austen.
            return 200;
        }
        return 0;
    }

    private static void WriteGreetingMessage() {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        string verString = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        Console.WriteLine($"💖 Versioning By Versonify 💖 ({verString}).");
    }

    private static RootCommand BuildRootCommand() {
        var rc = new RootCommand("Parameter help for Versonify.");
        rc.TreatUnmatchedTokensAsErrors = true;

        commandArg = new Argument<string>("command");
        commandArg.Description = "Command to execute: createversion|override|updatefiles|passive|behaviour|set|prefix";
        commandArg.Arity = ArgumentArity.ZeroOrOne;
        commandArg.DefaultValueFactory = _ => null;
        rc.Add(commandArg);

        commandOpt = new Option<string>("-Command", Array.Empty<string>());
        commandOpt.Description = "Command name (legacy -Command=<name> form)";
        rc.Add(commandOpt);

        debugOpt = new Option<bool>("-Debug", Array.Empty<string>());
        debugOpt.Description = "Enables debug logging and echoes command-line arguments";
        rc.Add(debugOpt);

        dryRunOpt = new Option<bool>("-DryRun", Array.Empty<string>());
        dryRunOpt.Description = "Runs in output-only mode; no changes are persisted";
        rc.Add(dryRunOpt);

        digitsOpt = new Option<string>("-Digits", new[] { "-D", "-d" });
        digitsOpt.Description = "Semicolon-separated digit indices or * for all";
        rc.Add(digitsOpt);

        noErrorOpt = new Option<bool>("-NoError", new[] { "-z" });
        noErrorOpt.Description = "Forces zero exit code on otherwise failing executions";
        rc.Add(noErrorOpt);

        noOverrideOpt = new Option<bool>("-NoOverride", Array.Empty<string>());
        noOverrideOpt.Description = "Ignores any saved pending-increment override";
        rc.Add(noOverrideOpt);

        outputOpt = new Option<string>("-Output", new[] { "-O", "-o", "-output" });
        outputOpt.Description = "Output mode: env|con|azdo[:VarName]|file[:FileName]|con-nf";
        rc.Add(outputOpt);

        incrementOpt = new Option<bool>("-Increment", new[] { "-I", "-i" });
        incrementOpt.Description = "Performs a version increment before other operations";
        rc.Add(incrementOpt);

        quickValueOpt = new Option<string>("-QuickValue", new[] { "-Q" });
        quickValueOpt.Description = "Quick value parameter used by set/override/behaviour/prefix commands";
        rc.Add(quickValueOpt);

        releaseOpt = new Option<string>("-Release", new[] { "-R" });
        releaseOpt.Description = "Release name associated with this version";
        rc.Add(releaseOpt);

        rootPathOpt = new Option<string>("-Root", Array.Empty<string>());
        rootPathOpt.Description = "Root directory from which to search for versionable files";
        rc.Add(rootPathOpt);

        traceOpt = new Option<string>("-Trace", Array.Empty<string>());
        traceOpt.Description = "Trace level: info|verbose|off";
        rc.Add(traceOpt);

        versionSourceOpt = new Option<string>("-VersionSource", new[] { "-V", "-v" });
        versionSourceOpt.Description = "Version store initialisation string";
        rc.Add(versionSourceOpt);

        minMatchOpt = new Option<string>("-MinMatch", new[] { "-M", "-m" });
        minMatchOpt.Description = "Semicolon-separated minmatch patterns for file update";
        rc.Add(minMatchOpt);

        return rc;
    }

    private static bool GetCommandLineArguments(string[] args) {
        rootCommand = BuildRootCommand();
        var parseResult = rootCommand.Parse(args);

        if (parseResult.Errors.Count > 0) {
            Console.WriteLine("Fatal: Invalid Arguments Passed to Versonify.");
            foreach (var error in parseResult.Errors) {
                Console.WriteLine(error.Message);
            }
            return false;
        }

        string cmdFromPositional = parseResult.GetValue(commandArg);
        string cmdFromOption = parseResult.GetValue(commandOpt);
        options.Command = cmdFromPositional ?? cmdFromOption;

        options.Debug = parseResult.GetValue(debugOpt);
        options.DryRunOnly = parseResult.GetValue(dryRunOpt);
        options.ReturnZero = parseResult.GetValue(noErrorOpt);
        options.NoOverride = parseResult.GetValue(noOverrideOpt);
        options.PerformIncrement = parseResult.GetValue(incrementOpt);
        options.QuickValue = parseResult.GetValue(quickValueOpt);
        options.Release = parseResult.GetValue(releaseOpt);
        options.Root = parseResult.GetValue(rootPathOpt);
        options.Trace = parseResult.GetValue(traceOpt);
        options.VersionPersistanceValue = parseResult.GetValue(versionSourceOpt);

        string rawDigits = parseResult.GetValue(digitsOpt);
        options.DigitManipulations = rawDigits != null
            ? rawDigits.Split(';', StringSplitOptions.RemoveEmptyEntries)
            : null;

        string rawMinMatch = parseResult.GetValue(minMatchOpt);
        options.VersionTargetMinMatch = rawMinMatch != null
            ? rawMinMatch.Split(';', StringSplitOptions.RemoveEmptyEntries)
            : null;

        options.RawOutputOptions = parseResult.GetValue(outputOpt);
        options.OutputOptions = options.RawOutputOptions;

        return true;
    }

    private static void WriteErrorConditions() {
        Console.WriteLine("Fatal:  Argument Validation Failed.");
        Console.WriteLine();
        rootCommand.Parse(new[] { "--help" }).Invoke(new System.CommandLine.InvocationConfiguration());
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

    private static bool ValidateDigitsPresent(string[] digits, string commandName) {
        if (digits == null || digits.Length == 0) {
            Console.WriteLine($"Error >> The {commandName} command requires at least one digit to update. Use -D=<digit> or -D=*.");
            return false;
        }
        return true;
    }

    private static ExecutionResult PerformActionsFromCommandline() {
        var result = new ExecutionResult();
        b.Verbose.Flow();

        Console.WriteLine("Performing Versioning Actions");

        GetVersionStorageFromCommandLine();

        if (!ValidateVersionStorage()) {
            result.WasProcessedSuccessfully = false;
            result.ExitCode = 1;
            return result;
        }

        switch (options.RequestedCommand) {
            case VersioningCommand.CreateNewVersion:
                CreateNewVersionStore();
                result.WasProcessedSuccessfully = true;
                break;

            case VersioningCommand.Override:
                CreateNewPendingIncrement();
                result.WasProcessedSuccessfully = true;
                break;

            case VersioningCommand.UpdateFiles:
                if (options.VersionTargetMinMatch == null || options.VersionTargetMinMatch.Length == 0) {
                    result.AddError("Error >> The Update command requires a minmatch file to be provided. Use -M=<path to minmatch file.>¦-M=Minmatch glob");
                    // TODO : PRoper Exit Code Map
                    result.ExitCode = 7;
                    result.WasProcessedSuccessfully = false;
                } else {
                    ApplyVersionIncrement(result);
                    result.WasProcessedSuccessfully = true;
                }
                break;

            case VersioningCommand.PassiveOutput:
                if (options.Release != null) {
                    LoadReleaseName();
                } else {
                    LoadVersionStore();
                }
                result.WasProcessedSuccessfully = true;
                break;

            case VersioningCommand.BehaviourOutput:
                LoadDigitBehaviour();
                result.WasProcessedSuccessfully = true;
                break;

            case VersioningCommand.BehaviourUpdate:
                ApplyDigitBehaviour();
                result.WasProcessedSuccessfully = true;
                break;

            case VersioningCommand.SetDigitValue:
                ApplyDigitValueUpdate();
                result.WasProcessedSuccessfully = true;
                break;

            case VersioningCommand.SetReleaseName:
                ApplyReleaseNameUpdate();
                result.WasProcessedSuccessfully = true;
                break;

            case VersioningCommand.SetDigitPrefix:
                ApplyDigitPrefixUpdate();
                result.WasProcessedSuccessfully = true;
                break;

            default:
                result.AddError("Error >> Unrecognised Command: " + options.Command);
                result.ExitCode = 8;
                // Todo: Proper exit code map
                result.WasProcessedSuccessfully = false;
                break;
        }
        return result;
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

    private static void ApplyVersionIncrement(ExecutionResult result) {
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

        // Increment done, now persist and then update the pages 
        ver.LoadMiniMatches(options.VersionTargetMinMatch);

        if (!string.IsNullOrEmpty(options.Root) && Directory.Exists(options.Root)) {
            _ = ver.SearchForAllFiles(options.Root);
        } else {
            result.WasProcessedSuccessfully = false;
            result.AddError($"Invalid or Missing Root Path: {options.Root}.");
            // TODO: Consistant error code map
            result.ExitCode = 5;
        }

        int filesUpdated = ver.UpdateAllRegisteredFiles();

        if (filesUpdated == 0) {
            // TODO: Consistant error code map
            result.AddError("No files were updated, likely due to mismatches in the glob patterns.");
            result.ExitCode = 6;
        }

        ver.SaveUpdatedVersion();
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
