namespace Versonify;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Versioning;

internal class Program {
    private const string ALL_DIGITS_WILDCARD = "*";
    public static VersonifyOptions options = new();
    private static CompleteVersion? versionerUsed;
    private static VersionStorage? storage;
    private static Bilge b = new Bilge();
    private static string? passiveOutputValue;

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

            var parseResult = CommandLineParser.Parse(args);
            if (!parseResult.Success) {
                WriteErrorConditions();
                return 1;
            }
            options = parseResult.Options;

            if (options.GetMdHelp) {
                return await WriteMarkdownHelpFileAsync();
            }

            if (!ArgumentValidator.ValidateArgumentSettings(options)) {
                WriteErrorConditions();
                return 1;
            }

            if (options.Debug) {
                foreach (string a in args) {
                    Console.WriteLine($"Command Line: {a}");
                }
            }

            if (options.Debug || (!string.IsNullOrEmpty(options.Trace))) {
                DiagnosticsConfig.ConfigureTrace(options);
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
                        PassiveOutputOverride = options.RequestedCommand == VersioningCommand.PassiveOutput ? passiveOutputValue : null,
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

    private static int CheckPnfCompatibiliyRequest(string[] args) {
        if (args.Length == 1 && args[0].Equals("--QQpnf", StringComparison.OrdinalIgnoreCase)) {
            // 200 is the first implemented compatibility exit code. Before this no compatibility exit codes existed  - Versonify Release 1.0.1 Austen.
            // 201 is the new command line interface.  Versonify Release 2.0 Bronte.
            return 201;
        }
        return 0;
    }

    private static void WriteGreetingMessage() {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        string verString = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
        Console.WriteLine($"💖 Versioning By Versonify 💖 ({verString}).");
    }

    private static string GetAssemblyVersionString() {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
    }

    private static bool IsVersionRequested(string[] args) {
        return args.Any(arg => arg.Equals("--version", StringComparison.OrdinalIgnoreCase));
    }

    private static void WriteErrorConditions() {
        Console.WriteLine("Fatal:  Argument Validation Failed.");
        Console.WriteLine();
        CommandLineParser.DisplayHelp();
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

    private static ExecutionResult PerformActionsFromCommandline() {
        var result = new ExecutionResult();
        b.Verbose.Flow();
        passiveOutputValue = null;

        Console.WriteLine("Performing Versioning Actions");

        GetVersionStorageFromCommandLine();

        if (!ArgumentValidator.ValidateVersionStorage(storage, options)) {
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
                    // TODO : Proper Exit Code Map
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
        string vpv = Environment.ExpandEnvironmentVariables(options.VersionPersistanceValue ?? "");
        storage = VersionStorage.CreateFromInitialisation(vpv);
    }

    private static void LoadVersionStore() {
        var ver = new Versioning(storage!, options.DryRunOnly);
        versionerUsed = ver.Version;

        if (options.PerformIncrement) {
            string v = ver.GetVersion();
            b.Verbose.Log($"Performing increment {v}");
            Console.WriteLine("Version Increment Requested - Currently " + v);

            if ((!string.IsNullOrWhiteSpace(options.Release)) && (options.Release != ver.Version.ReleaseName)) {
                ver.Version.ReleaseName = options.Release;
            }
            ver.Version.IncrementByGroup(ResolveDigitGroupsForIncrement());

            b.Verbose.Log("About to save version store");
            ver.SaveUpdatedVersion();
        }

        string outputVersion = ver.Version.GetVersionStringByGroup(ResolveDigitGroupsForDisplay());
        passiveOutputValue = outputVersion;
        Console.WriteLine($"Loaded [{outputVersion}]");
    }



    private static void LoadReleaseName() {
        b.Verbose.Flow();
        var ver = new Versioning(storage!, options.DryRunOnly);
        versionerUsed = ver.Version;

        if (string.IsNullOrEmpty(ver.Version.ReleaseName)) {
            Console.WriteLine("Release Name in version store is null or empty.");
            return;
        }
        Console.WriteLine($"Loaded Release Name: {ver.Version.ReleaseName}");
    }


    private static void CreateNewPendingIncrement() {
        b.Verbose.Flow();

        var ver = new Versioning(storage!, options.DryRunOnly);
        versionerUsed = ver.Version;

        string? verPendPattern = options.QuickValue;

        Console.WriteLine($"Apply Delayed Increment. [{ver.ToString()}] using [{verPendPattern}]");
        ver.Version.ApplyPendingVersion(verPendPattern!);

        if (!options.DryRunOnly) {
            storage!.Persist(ver.Version);
            ver.Increment();
            Console.WriteLine($"Saving Overridden Version [{ver.GetVersion()}]");
        } else {
            ver.Version.Increment();
            Console.WriteLine($"DryRun - Would Save :" + ver.Version.ToString());
        }
    }

    private static void ApplyVersionIncrement(ExecutionResult result) {
        b.Verbose.Flow();

        var ver = new Versioning(storage!, options.DryRunOnly);
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

            if ((!string.IsNullOrWhiteSpace(options.Release)) && (options.Release != ver.Version.ReleaseName)) {
                ver.Version.ReleaseName = options.Release;
            }
            ver.Version.IncrementByGroup(ResolveDigitGroupsForIncrement());
        } else {
            Console.WriteLine("No Version Increment Requested.");
        }

        Console.WriteLine("Version To Write: " + ver.GetVersion());

        // Increment done, now persist and then update the pages 
        ver.LoadMiniMatches(options.VersionTargetMinMatch!);

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

        var cv = new CompleteVersion(startVer) {
            ReleaseName = options.Release
        };
        versionerUsed = cv;

        Console.WriteLine($"Saving {cv.GetVersionString()}");
        storage!.Persist(cv);
    }

    private static void LoadDigitBehaviour() {
        var ver = new Versioning(storage!, options.DryRunOnly);
        versionerUsed = ver.Version;
        if (!ver.Version.ValidateDigitOptions(options.DigitManipulations!)) {
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
        var ver = new Versioning(storage!, options.DryRunOnly);
        versionerUsed = ver.Version;

        if (!ver.Version.ValidateDigitOptions(options.DigitManipulations!)) {
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
        var ver = new Versioning(storage!, options.DryRunOnly);
        versionerUsed = ver.Version;

        string[] digitsToUpdate = options.GetDigits();
        string? valueToSet = options.QuickValue;

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

        if (!options.DryRunOnly) {
            Console.WriteLine("Saving Updated Digit Values");
            ver.SaveUpdatedVersion();
            Console.WriteLine($"[{ver.Version.GetVersionString()}]");
        } else {
            Console.WriteLine("DryRun - Would Save:");
            Console.WriteLine($"[{ver.Version.GetVersionString()}]");
        }
    }

    private static string ResolveDigitGroupsForDisplay() {
        if (options.PreRelease) {
            return "default,pre-release";
        }

        if (string.IsNullOrWhiteSpace(options.DigitGroup)) {
            return string.Empty;
        }

        return options.DigitGroup;
    }

    private static string ResolveDigitGroupsForIncrement() {
        if (options.PreRelease) {
            return "pre-release";
        }

        if (string.IsNullOrWhiteSpace(options.DigitGroup)) {
            return string.Empty;
        }

        return options.DigitGroup;
    }

    private static string? ResolveDigitGroupForSet() {
        if (options.PreRelease) {
            return "pre-release";
        }

        if (options.DigitGroup == null) {
            return null;
        }

        return CompleteVersion.NormalizeDigitGroup(options.DigitGroup);
    }

    private static void ApplyReleaseNameUpdate() {
        var ver = new Versioning(storage!, options.DryRunOnly);
        versionerUsed = ver.Version;
        string? newReleaseName = options.Release;

        ver.Version.SetReleaseName(newReleaseName!);
        if (!options.DryRunOnly) {
            Console.WriteLine($"Saving new Release Name as: {newReleaseName}");
            ver.SaveUpdatedVersion();
        } else {
            Console.WriteLine("DryRun - Would Save:");
            Console.WriteLine($"[{newReleaseName}]");
        }
    }

    private static void ApplyDigitPrefixUpdate() {
        var ver = new Versioning(storage!, options.DryRunOnly);
        versionerUsed = ver.Version;

        string[] digitsToUpdate = options.GetDigits();
        string? prefixToSet = options.QuickValue;

        if (digitsToUpdate.Length > 0 && digitsToUpdate[0] == ALL_DIGITS_WILDCARD) {
            Console.WriteLine($"Setting prefix for all digits to: {prefixToSet}");
            ver.Version.SetPrefixForDigit(ALL_DIGITS_WILDCARD, prefixToSet!);
        } else {
            Console.WriteLine($"Setting prefix for digit(s) [{string.Join(',', digitsToUpdate)}] to: {prefixToSet}");
            foreach (string digit in digitsToUpdate) {
                ver.Version.SetPrefixForDigit(digit, prefixToSet!);
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
