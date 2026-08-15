namespace Versonify;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Plumbing;
using Plisky.Versioning;

internal class Program {
    public static VersonifyOptions? opts;
    private const string ALL_DIGITS_WILDCARD = "*";
    private static Bilge b = new();
    private static Hub outputContent = new();
    private static string? passiveOutputValue;
    private static VersionStorage? storage;
    private static CompleteVersion? versionerUsed;

    private static void ApplyDigitBehaviour() {
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        var newBehaviour = opts.IncrementBehaviour;
        var ver = new Versioning(storage!, opts.DryRunOnly);
        versionerUsed = ver.Version;

        if (!ver.Version.ValidateDigitOptions(opts.DigitManipulations!)) {
            return;
        }

        string[] digitsToUpdate = opts.GetDigits();
        if (digitsToUpdate.Length > 0 && digitsToUpdate[0] == ALL_DIGITS_WILDCARD) {
            Console.WriteLine($"Setting All Behaviours to {newBehaviour}");
        } else {
            Console.WriteLine($"Setting Behaviour for Digit[{string.Join(',', digitsToUpdate)}] to {newBehaviour}({(int)newBehaviour})");
        }

        foreach (string digit in digitsToUpdate) {
            ver.UpdateBehaviour(digit, newBehaviour);
        }

        if (!opts.DryRunOnly) {
            Console.WriteLine("Saving Updated Behaviour");
            ver.SaveUpdatedVersion();
        } else {
            DisplayDryRunBehaviours(ver, digitsToUpdate);
        }
    }

    private static void ApplyDigitPrefixUpdate() {
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        var ver = new Versioning(storage!, opts.DryRunOnly);
        versionerUsed = ver.Version;

        string[] digitsToUpdate = opts.GetDigits();
        string? prefixToSet = opts.QuickValue;

        if (digitsToUpdate.Length > 0 && digitsToUpdate[0] == ALL_DIGITS_WILDCARD) {
            Console.WriteLine($"Setting prefix for all digits to: {prefixToSet}");
            ver.Version.SetPrefixForDigit(ALL_DIGITS_WILDCARD, prefixToSet!);
        } else {
            Console.WriteLine($"Setting prefix for digit(s) [{string.Join(',', digitsToUpdate)}] to: {prefixToSet}");
            foreach (string digit in digitsToUpdate) {
                ver.Version.SetPrefixForDigit(digit, prefixToSet!);
            }
        }

        if (!opts.DryRunOnly) {
            Console.WriteLine("Saving updated digit prefixes");
            ver.SaveUpdatedVersion();
            Console.WriteLine($"[{ver.Version.GetVersionString()}]");
        } else {
            Console.WriteLine("DryRun - Would Save:");
            Console.WriteLine($"[{ver.Version.GetVersionString()}]");
        }
    }

    private static void ApplyDigitValueUpdate() {
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        var ver = new Versioning(storage!, opts.DryRunOnly);
        versionerUsed = ver.Version;

        string[] digitsToUpdate = opts.GetDigits();
        string? valueToSet = opts.QuickValue;

        if (ArgumentValidator.ShouldSetCompleteVersionFromString(digitsToUpdate, valueToSet)) {
            ver.Version.SetCompleteVersionFromString(valueToSet!);
            Console.WriteLine($"Set version to: {ver.Version.GetVersionString()}");
        } else {
            if (!ver.Version.ValidateDigitOptions(digitsToUpdate)) {
                Console.WriteLine("Error >> Invalid digit selection for value update.");
                return;
            }

            string? requestedGroupName = ResolveDigitGroupForSet();
            if (string.IsNullOrWhiteSpace(valueToSet) && requestedGroupName == null) {
                Console.WriteLine("Error >> No value or digit-group specified for set command.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(valueToSet) && digitsToUpdate.Length > 0 && digitsToUpdate[0] == ALL_DIGITS_WILDCARD) {
                Console.WriteLine($"Setting all digits to value: {valueToSet}");
                if (requestedGroupName != null) {
                    Console.WriteLine($"  with group assignment: {requestedGroupName}");
                }
            } else if (!string.IsNullOrWhiteSpace(valueToSet)) {
                Console.WriteLine($"Setting digit(s) [{string.Join(',', digitsToUpdate)}] to value: {valueToSet}");
                if (requestedGroupName != null) {
                    Console.WriteLine($"  with group assignment: {requestedGroupName}");
                }
            }

            if (!string.IsNullOrWhiteSpace(valueToSet)) {
                ver.Version.SetIndividualDigits(digitsToUpdate, valueToSet!);
            }

            if (requestedGroupName != null) {
                if (string.IsNullOrWhiteSpace(valueToSet)) {
                    Console.WriteLine($"Assigning digit(s) [{string.Join(',', digitsToUpdate)}] to group: {requestedGroupName}");
                }
                foreach (string digitStr in digitsToUpdate) {
                    if (digitStr != ALL_DIGITS_WILDCARD && int.TryParse(digitStr, out int digitIdx)) {
                        ver.Version.Digits[digitIdx].GroupName = requestedGroupName;
                    } else if (digitStr == ALL_DIGITS_WILDCARD) {
                        for (int i = 0; i < ver.Version.Digits.Length; i++) {
                            ver.Version.Digits[i].GroupName = requestedGroupName;
                        }
                    }
                }
            }
        }

        if (!opts.DryRunOnly) {
            Console.WriteLine("Saving Updated Digit Values");
            ver.SaveUpdatedVersion();
            Console.WriteLine($"[{ver.Version.GetVersionString()}]");
        } else {
            Console.WriteLine("DryRun - Would Save:");
            Console.WriteLine($"[{ver.Version.GetVersionString()}]");
        }
    }

    private static void ApplyReleaseNameUpdate() {
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));
        var ver = new Versioning(storage!, opts.DryRunOnly);
        versionerUsed = ver.Version;
        string? newReleaseName = opts.Release;

        ver.Version.SetReleaseName(newReleaseName!);
        if (!opts.DryRunOnly) {
            Console.WriteLine($"Saving new Release Name as: {newReleaseName}");
            ver.SaveUpdatedVersion();
        } else {
            Console.WriteLine("DryRun - Would Save:");
            Console.WriteLine($"[{newReleaseName}]");
        }
    }

    private static void ApplyVersionIncrement(ExecutionResult result) {
        b.Verbose.Flow();

        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        var ver = new Versioning(storage!, opts.DryRunOnly);
        versionerUsed = ver.Version;

        ver.Logger = Console.WriteLine;
        ver.FileUpdateDisplayGroups = ResolveDigitGroupsForDisplay();

        if (opts.NoOverride) {
            Console.WriteLine("Version Increment Override, Disabled");
            foreach (var l in ver.Version.Digits) {
                l.IncrementOverride = null;
            }
        }
        if (opts.PerformIncrement) {
            Console.WriteLine("Version Increment Requested - Currently " + ver.GetVersion());

            if ((!string.IsNullOrWhiteSpace(opts.Release)) && (opts.Release != ver.Version.ReleaseName)) {
                ver.Version.ReleaseName = opts.Release;
            }
            ver.Version.IncrementByGroup(ResolveDigitGroupsForIncrement());
        } else {
            Console.WriteLine("No Version Increment Requested.");
        }

        Console.WriteLine("Version To Write: " + ver.GetVersion());

        // Increment done, now persist and then update the pages
        ver.LoadMiniMatches(opts.VersionTargetMinMatch!);

        if (!string.IsNullOrEmpty(opts.Root) && Directory.Exists(opts.Root)) {
            _ = ver.SearchForAllFiles(opts.Root);
        } else {
            result.WasProcessedSuccessfully = false;
            result.AddError($"Invalid or Missing Root Path: {opts.Root}.");
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

    private static int CheckPnfCompatibiliyRequest(string[] args) {
        if (args.Length == 1 && args[0].Equals("--QQpnf", StringComparison.OrdinalIgnoreCase)) {
            // 200 is the first implemented compatibility exit code. Before this no compatibility exit codes existed  - Versonify Release 1.0.1 Austen.
            // 201 is the new command line interface.  Versonify Release 2.0 Bronte.
            return 201;
        }
        return 0;
    }

    private static void CreateNewPendingIncrement() {
        b.Verbose.Flow();

        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        var ver = new Versioning(storage!, opts.DryRunOnly);
        versionerUsed = ver.Version;

        string? verPendPattern = opts.QuickValue;

        Console.WriteLine($"Apply Delayed Increment. [{ver}] using [{verPendPattern}]");
        ver.Version.ApplyPendingVersion(verPendPattern!);

        if (!opts.DryRunOnly) {
            storage!.Persist(ver.Version);
            ver.Increment();
            Console.WriteLine($"Saving Overridden Version [{ver.GetVersion()}]");
        } else {
            ver.Version.Increment();
            Console.WriteLine($"DryRun - Would Save :" + ver.Version.ToString());
        }
    }

    private static void CreateNewVersionStore() {
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        string startVer = "0.0.0.0";
        if (!string.IsNullOrEmpty(opts.QuickValue)) {
            Console.WriteLine($"Using Value From Command Line: {opts.QuickValue}");
            startVer = opts.QuickValue;
        }
        if (!string.IsNullOrEmpty(opts.Release)) {
            Console.WriteLine($"Setting Release From Command Line: {opts.Release}");
        }
        Console.WriteLine($"Creating New Version Store: {startVer}");

        var cv = new CompleteVersion(startVer) {
            ReleaseName = opts.Release
        };
        versionerUsed = cv;

        Console.WriteLine($"Saving {cv.GetVersionString()}");
        storage!.Persist(cv);
    }

    private static void DisplayDryRunBehaviours(Versioning ver, string[] digitsToUpdate) {
        Console.WriteLine("DryRun - Would Save:");
        foreach (string digit in digitsToUpdate) {
            Console.WriteLine(ver.GetBehaviour(digit));
        }
    }

    private static string GetAssemblyVersionString() {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
    }

    /// <summary>
    /// Most of the versioning approaches require a version store of some sort. This initialises the version store from the command line using the
    /// initialisation data that is passed in to determine which version store to load.
    /// </summary>
    private static void GetVersionStorageFromCommandLine() {
        b.Info.Flow();

        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        string vpv = Environment.ExpandEnvironmentVariables(opts.VersionPersistanceValue ?? "");
        b.Verbose.Log($"Expanded versionstore :{vpv}");
        storage = VersionStorage.CreateFromInitialisation(vpv);
    }

    private static bool IsVersionRequested(string[] args) {
        return args.Any(arg => arg.Equals("--version", StringComparison.OrdinalIgnoreCase));
    }

    private static void LoadDigitBehaviour() {
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        var ver = new Versioning(storage!, opts.DryRunOnly);
        versionerUsed = ver.Version;
        if (!ver.Version.ValidateDigitOptions(opts.DigitManipulations!)) {
            return;
        }

        string[] digitsToLoad = opts.GetDigits();
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

    private static void LoadReleaseName() {
        b.Verbose.Flow();
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        var ver = new Versioning(storage!, opts.DryRunOnly);
        versionerUsed = ver.Version;

        if (string.IsNullOrEmpty(ver.Version.ReleaseName)) {
            Console.WriteLine("Release Name in version store is null or empty.");
            return;
        }
        Console.WriteLine($"Loaded Release Name: {ver.Version.ReleaseName}");
    }

    private static void LoadVersionStore() {
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));
        var ver = new Versioning(storage!, opts.DryRunOnly);
        versionerUsed = ver.Version;

        if (opts.PerformIncrement) {
            string v = ver.GetVersion();
            b.Verbose.Log($"Performing increment {v}");
            Console.WriteLine("Version Increment Requested - Currently " + v);

            if ((!string.IsNullOrWhiteSpace(opts.Release)) && (opts.Release != ver.Version.ReleaseName)) {
                ver.Version.ReleaseName = opts.Release;
            }
            ver.Version.IncrementByGroup(ResolveDigitGroupsForIncrement());

            b.Verbose.Log("About to save version store");
            ver.SaveUpdatedVersion();
        }

        string outputVersion = ver.Version.GetVersionStringByGroup(ResolveDigitGroupsForDisplay());
        passiveOutputValue = outputVersion;
        Console.WriteLine($"Loaded [{outputVersion}]");
    }

    private static async Task<int> Main(string[] args) {
        try {
            int pnfShortCircuit = CheckPnfCompatibiliyRequest(args);
            if (pnfShortCircuit >= 200) {
                return pnfShortCircuit;
            }

            if (IsVersionRequested(args)) {
                Console.WriteLine(GetAssemblyVersionString());
                return 0;
            }

            WriteGreetingMessage();

            if (CommandLineParser.IsHelpRequested(args)) {
                CommandLineParser.DisplayHelp();
                return 0;
            }

            var (success, options) = CommandLineParser.Parse(args);

            if (options.Debug) {
                Console.WriteLine("Debug Mode, Command Line Arguments:");

                for (int n = 0; n < args.Length; n++) {
                    Console.WriteLine($"args[{n}]: {args[n]}");
                }
            }

            if (!success) {
                WriteErrorConditions();
                return 1;
            }

            if (options.GetMdHelp) {
                return await WriteMarkdownHelpFileAsync();
            }

            if (!ArgumentValidator.ValidateArgumentSettings(options)) {
                WriteErrorConditions();
                return 1;
            }

            if (options.Debug || (!string.IsNullOrEmpty(options.Trace))) {
                DiagnosticsConfig.ConfigureTrace(options);
            }

            opts = options;
            b = new Bilge("Versonify");
            Bilge.Alert.Online("Versonify");
            b.Verbose.Dump(options, "App Options");

            var result = PerformActionsFromCommandline();
            if (result.WasProcessedSuccessfully) {
                if (versionerUsed != null) {
                    b.Verbose.Log($"All Actions - Complete - Outputting.");
                    var vo = new VersioningOutputter(versionerUsed) {
                        ConsoleTemplate = opts.ConsoleTemplate,
                        PverFileName = opts.PverFileName,
                        Digits = opts.GetDigits(),
                        ReleaseRequested = opts.Release != null,
                        PassiveOutputOverride = opts.RequestedCommand == VersioningCommand.PassiveOutput ? passiveOutputValue : null,
                    };

                    vo.DoOutput(opts.OutputsActive, opts.RequestedCommand);
                }

                b.Info.Log("All Actions - Complete - Exiting.");
            } else {
                Console.WriteLine("Errors Occurred:");
                foreach (string e in result.Errors) {
                    Console.WriteLine(e);
                }
                Console.WriteLine();
                CommandLineParser.DisplayHelp();
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

    private static ExecutionResult PerformActionsFromCommandline() {
        var result = new ExecutionResult();
        b.Verbose.Flow();
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        passiveOutputValue = null;

        Console.WriteLine("Performing Versioning Actions");

        GetVersionStorageFromCommandLine();

        if (!ArgumentValidator.ValidateVersionStorage(storage, opts)) {
            // Do not like this at all - LFY-68 created.
            result.WasProcessedSuccessfully = false;
            result.ExitCode = 1;
            return result;
        }

        switch (opts.RequestedCommand) {
            case VersioningCommand.CreateNewVersion:
                CreateNewVersionStore();
                result.WasProcessedSuccessfully = true;
                break;

            case VersioningCommand.Override:
                CreateNewPendingIncrement();
                result.WasProcessedSuccessfully = true;
                break;

            case VersioningCommand.UpdateFiles:
                if (opts.VersionTargetMinMatch == null || opts.VersionTargetMinMatch.Length == 0) {
                    result.AddError("Error >> The Update command requires a minmatch file to be provided. Use -M=<path to minmatch file.>¦-M=Minmatch glob");
                    // TODO : Proper Exit Code Map
                    result.ExitCode = 7;
                    result.WasProcessedSuccessfully = false;
                } else {
                    ApplyVersionIncrement(result);
                    result.WasProcessedSuccessfully = true;
                }
                break;

            case VersioningCommand.PassiveOutput:
                if (opts.Release != null) {
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
                result.AddError("Error >> Unrecognised Command: " + opts.Command);
                result.ExitCode = 8;
                // Todo: Proper exit code map
                result.WasProcessedSuccessfully = false;
                break;
        }
        return result;
    }

    private static string? ResolveDigitGroupForSet() {
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        if (opts.PreRelease) {
            return "pre-release";
        }

        if (opts.DigitGroup == null) {
            return null;
        }

        return CompleteVersion.NormalizeDigitGroup(opts.DigitGroup);
    }

    private static string ResolveDigitGroupsForDisplay() {
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        if (opts.PreRelease) {
            return "default,pre-release";
        }

        if (string.IsNullOrWhiteSpace(opts.DigitGroup)) {
            return string.Empty;
        }

        return opts.DigitGroup;
    }

    private static string ResolveDigitGroupsForIncrement() {
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        if (opts.PreRelease) {
            return "pre-release";
        }

        if (string.IsNullOrWhiteSpace(opts.DigitGroup)) {
            return string.Empty;
        }

        return opts.DigitGroup;
    }

    private static void WriteErrorConditions() {
        Console.WriteLine("Fatal:  Argument Validation Failed.");
        Console.WriteLine();
        CommandLineParser.DisplayHelp();
    }

    private static void WriteGreetingMessage() {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        string verString = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
#if DEBUG
        Console.WriteLine($"💖 Versioning -DEBUG- By Versonify 💖 ({verString}).");
#else
        Console.WriteLine($"💖 Versioning By Versonify 💖 ({verString}).");
#endif
    }

    private static async Task<int> WriteMarkdownHelpFileAsync() {
        const string RESOURCE_NAME = "Versonify.docs.md";
        const string FILE_NAME = "docs.md";

        using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RESOURCE_NAME);
        if (resourceStream == null) {
            Console.WriteLine($"Fatal: Embedded markdown resource '{RESOURCE_NAME}' was not found.");
            return 1;
        }

        using var reader = new StreamReader(resourceStream);
        string markdown = await reader.ReadToEndAsync();
        string outputPath = Path.Combine(Environment.CurrentDirectory, FILE_NAME);

        await File.WriteAllTextAsync(outputPath, markdown);
        Console.WriteLine($"Wrote markdown help to {outputPath}");
        return 0;
    }
}