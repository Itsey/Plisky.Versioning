using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Plumbing;
using Plisky.Versioning;

namespace Versonify;

internal static class Program {
    private const string ALL_DIGITS_WILDCARD = "*";
    private const string HELP_HINT_MESSAGE = "Use '--help' to display available options and commands, or '--get-md-help' to export documentation.";
    private static Bilge b = new();
    private static VersonifyOptions? opts;
    private static Hub outputContent = new(true);
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
            outputContent.Launch(new SimpleMessage($"Setting All Behaviours to {newBehaviour}"));
        } else {
            outputContent.Launch(new SimpleMessage($"Setting Behaviour for Digit[{string.Join(',', digitsToUpdate)}] to {newBehaviour}({(int)newBehaviour})"));
        }

        foreach (string digit in digitsToUpdate) {
            ver.UpdateBehaviour(digit, newBehaviour);
        }

        if (!opts.DryRunOnly) {
            outputContent.Launch(new SimpleMessage("Saving Updated Behaviour"));
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
            outputContent.Launch(new SimpleMessage($"Setting prefix for all digits to: {prefixToSet}"));
            ver.Version.SetPrefixForDigit(ALL_DIGITS_WILDCARD, prefixToSet!);
        } else {
            outputContent.Launch(new SimpleMessage($"Setting prefix for digit(s) [{string.Join(',', digitsToUpdate)}] to: {prefixToSet}"));
            foreach (string digit in digitsToUpdate) {
                ver.Version.SetPrefixForDigit(digit, prefixToSet!);
            }
        }

        if (!opts.DryRunOnly) {
            outputContent.Launch(new SimpleMessage("Saving updated digit prefixes"));
            ver.SaveUpdatedVersion();
            outputContent.Launch(new SimpleMessage($"[{ver.Version.GetVersionString()}]"));
        } else {
            outputContent.Launch(new SimpleMessage("DryRun - Would Save:"));
            outputContent.Launch(new SimpleMessage($"[{ver.Version.GetVersionString()}]"));
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
            outputContent.Launch(new SimpleMessage($"Set version to: {ver.Version.GetVersionString()}"));
        } else {
            if (!ver.Version.ValidateDigitOptions(digitsToUpdate)) {
                outputContent.Launch(new SimpleMessage("Error >> Invalid digit selection for value update."));
                return;
            }

            string? requestedGroupName = ResolveDigitGroupForSet();
            if (string.IsNullOrWhiteSpace(valueToSet) && requestedGroupName == null) {
                outputContent.Launch(new SimpleMessage("Error >> No value or digit-group specified for set command."));
                return;
            }

            if (!string.IsNullOrWhiteSpace(valueToSet) && digitsToUpdate.Length > 0 && digitsToUpdate[0] == ALL_DIGITS_WILDCARD) {
                outputContent.Launch(new SimpleMessage($"Setting all digits to value: {valueToSet}"));
                if (requestedGroupName != null) {
                    outputContent.Launch(new SimpleMessage($"  with group assignment: {requestedGroupName}"));
                }
            } else if (!string.IsNullOrWhiteSpace(valueToSet)) {
                outputContent.Launch(new SimpleMessage($"Setting digit(s) [{string.Join(',', digitsToUpdate)}] to value: {valueToSet}"));
                if (requestedGroupName != null) {
                    outputContent.Launch(new SimpleMessage($"  with group assignment: {requestedGroupName}"));
                }
            }

            if (!string.IsNullOrWhiteSpace(valueToSet)) {
                ver.Version.SetIndividualDigits(digitsToUpdate, valueToSet!);
            }

            if (requestedGroupName != null) {
                if (string.IsNullOrWhiteSpace(valueToSet)) {
                    outputContent.Launch(new SimpleMessage($"Assigning digit(s) [{string.Join(',', digitsToUpdate)}] to group: {requestedGroupName}"));
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
            outputContent.Launch(new SimpleMessage("Saving Updated Digit Values"));
            ver.SaveUpdatedVersion();
            outputContent.Launch(new SimpleMessage($"[{ver.Version.GetVersionString()}]"));
        } else {
            outputContent.Launch(new SimpleMessage("DryRun - Would Save:"));
            outputContent.Launch(new SimpleMessage($"[{ver.Version.GetVersionString()}]"));
        }
    }

    private static void ApplyReleaseNameUpdate() {
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));
        var ver = new Versioning(storage!, opts.DryRunOnly);
        versionerUsed = ver.Version;
        string? newReleaseName = opts.Release;

        ver.Version.SetReleaseName(newReleaseName!);
        if (!opts.DryRunOnly) {
            outputContent.Launch(new SimpleMessage($"Saving new Release Name as: {newReleaseName}"));
            ver.SaveUpdatedVersion();
        } else {
            outputContent.Launch(new SimpleMessage("DryRun - Would Save:"));
            outputContent.Launch(new SimpleMessage($"[{newReleaseName}]"));
        }
    }

    private static void ApplyVersionIncrement(ExecutionResult result) {
        b.Verbose.Flow();

        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        var ver = new Versioning(storage!, opts.DryRunOnly, outputContent);
        versionerUsed = ver.Version;

        ver.FileUpdateDisplayGroups = ResolveDigitGroupsForDisplay();

        if (opts.NoOverride) {
            outputContent.Launch(new SimpleMessage("Version Increment Override, Disabled"));
            foreach (var l in ver.Version.Digits) {
                l.IncrementOverride = null;
            }
        }
        if (opts.PerformIncrement) {
            outputContent.Launch(new SimpleMessage("Version Increment Requested - Currently " + ver.GetVersion()));

            if ((!string.IsNullOrWhiteSpace(opts.Release)) && (opts.Release != ver.Version.ReleaseName)) {
                ver.Version.ReleaseName = opts.Release;
            }
            ver.Version.IncrementByGroup(ResolveDigitGroupsForIncrement());
        } else {
            outputContent.Launch(new SimpleMessage("No Version Increment Requested."));
        }

        outputContent.Launch(new SimpleMessage("Version To Write: " + ver.GetVersion()));

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

    private static (bool shouldExit, int returnCode) CheckQuickReturns(string[] args) {
        int pnfShortCircuit = CheckPnfCompatibiliyRequest(args);
        if (pnfShortCircuit >= 200) {
            return (true, pnfShortCircuit);
        }

        if (IsVersionRequested(args)) {
            Console.WriteLine(GetAssemblyVersionString());
            return (true, 0);
        }

        if (CommandLineParser.IsHelpRequested(args)) {
            WriteGreetingMessage();
            CommandLineParser.DisplayHelp();
            return (true, 0);
        }

        return (false, 0);
    }

    private static void ConfigureOutput(VersonifyOptions options) {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        if (options.OutputsActive == OutputPossibilities.None) {
            b.Warning.Log("No output is set, there will be nothing written from the session.");
        }

        bool isJson = options.OutputsActive.HasFlag(OutputPossibilities.Json);
        bool isConsole = options.OutputsActive.HasFlag(OutputPossibilities.Console);

        if (isConsole) {
            outputContent.LookFor<SimpleMessage>(msg => {
                string outputString = msg.Content;

                if (isJson) {
                    if (options.RequestedCommand != VersioningCommand.GetDigitInformation ||
                        msg.MessageType != OutputMessageType.Result) {
                        var jsonOutput = new JsonOutputMessage {
                            MessageCategory = msg.MessageType switch {
                                OutputMessageType.Warning => "warning",
                                OutputMessageType.Error => "error",
                                OutputMessageType.Result => "result",
                                _ => "information"
                            },
                            MessageContent = msg.Content
                        };
                        outputString = JsonSerializer.Serialize(jsonOutput);
                    }
                }

                Console.WriteLine(outputString);
            });
        }
    }

    private static void CreateNewPendingIncrement() {
        b.Verbose.Flow();

        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        var ver = new Versioning(storage!, opts.DryRunOnly);
        versionerUsed = ver.Version;

        string? verPendPattern = opts.QuickValue;

        outputContent.Launch(new SimpleMessage($"Apply Delayed Increment. [{ver}] using [{verPendPattern}]"));
        ver.Version.ApplyPendingVersion(verPendPattern!);

        if (!opts.DryRunOnly) {
            storage!.Persist(ver.Version);
            ver.Increment();
            outputContent.Launch(new SimpleMessage($"Saving Overridden Version [{ver.GetVersion()}]"));
        } else {
            ver.Version.Increment();
            outputContent.Launch(new SimpleMessage($"DryRun - Would Save : {ver.Version}"));
        }
    }

    private static void CreateNewVersionStore() {
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        string startVer = "0.0.0.0";
        if (!string.IsNullOrEmpty(opts.QuickValue)) {
            outputContent.Launch(new SimpleMessage($"Using Value From Command Line: {opts.QuickValue}"));
            startVer = opts.QuickValue;
        }
        if (!string.IsNullOrEmpty(opts.Release)) {
            outputContent.Launch(new SimpleMessage($"Setting Release From Command Line: {opts.Release}"));
        }
        outputContent.Launch(new SimpleMessage($"Creating New Version Store: {startVer}"));

        var cv = new CompleteVersion(startVer) {
            ReleaseName = opts.Release
        };
        versionerUsed = cv;

        if (opts.DryRunOnly) {
            outputContent.Launch(new SimpleMessage($"DryRun - Would Save: {cv.GetVersionString()}"));
        } else {
            outputContent.Launch(new SimpleMessage($"Saving {cv.GetVersionString()}"));
            storage!.Persist(cv);
        }
    }

    private static void DisplayDryRunBehaviours(Versioning ver, string[] digitsToUpdate) {
        outputContent.Launch(new SimpleMessage("DryRun - Would Save:"));
        foreach (string digit in digitsToUpdate) {
            outputContent.Launch(new SimpleMessage(ver.GetBehaviour(digit)));
        }
    }

    private static string GetAssemblyVersionString() {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
    }

    private static string GetDisplayDigitGroupName(string? groupName) {
        string normalizedGroupName = CompleteVersion.NormalizeDigitGroup(groupName);
        return string.IsNullOrEmpty(normalizedGroupName) ? "default" : normalizedGroupName;
    }

    /// <summary>
    /// Most of the versioning approaches require a version store of some sort. This initialises the version store from the command line using the
    /// initialisation data that is passed in to determine which version store to load.
    /// </summary>
    private static void GetVersionStorageFromCommandLine() {
        b.Info.Flow();

        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        string vpv = Environment.ExpandEnvironmentVariables(opts.VersionPersistanceValue ?? string.Empty);
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
            outputContent.Launch(new SimpleMessage("Loading All Behaviours"));
            outputContent.Launch(new SimpleMessage(ver.GetBehaviour(digitsToLoad[0])));
        } else {
            outputContent.Launch(new SimpleMessage($"Loading Behaviour for Digits [{string.Join(',', digitsToLoad)}]"));
            foreach (string digit in digitsToLoad) {
                outputContent.Launch(new SimpleMessage(ver.GetBehaviour(digit)));
            }
        }
    }

    private static bool LoadDigitInformation(ExecutionResult result) {
        b.Verbose.Flow();
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        var ver = new Versioning(storage!, opts.DryRunOnly);
        versionerUsed = ver.Version;

        string groupName = opts.PreRelease ? "pre-release" : opts.DigitGroup ?? ALL_DIGITS_WILDCARD;
        b.Verbose.Log($"Resolving digit-group [{groupName}] for Get command");
        int[] groupDigitIndices = ver.Version.GetDigitsByGroup(groupName);
        if (groupDigitIndices.Length == 0) {
            b.Warning.Log($"Digit-group [{groupName}] contains no digits, cannot continue with Get command.");
            result.AddError($"Error >> The digit-group '{groupName}' does not contain any digits.", 1);
            return false;
        }

        int[]? selectedIndices = ResolveRequestedDigitIndices(ver.Version, groupDigitIndices, groupName, result);
        if (selectedIndices == null) {
            return false;
        }

        b.Verbose.Log($"Get command resolved digits [{string.Join(',', selectedIndices)}] for output");
        WriteDigitInformation(ver.Version, selectedIndices);

        return true;
    }

    private static void LoadReleaseName() {
        b.Verbose.Flow();
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        var ver = new Versioning(storage!, opts.DryRunOnly);
        versionerUsed = ver.Version;

        if (string.IsNullOrEmpty(ver.Version.ReleaseName)) {
            outputContent.Launch(new SimpleMessage("Release Name in version store is null or empty."));
            return;
        }
        outputContent.Launch(new SimpleMessage($"Loaded Release Name: {ver.Version.ReleaseName}"));
    }

    private static void LoadVersionStore() {
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));
        var ver = new Versioning(storage!, opts.DryRunOnly);
        versionerUsed = ver.Version;

        if (opts.PerformIncrement) {
            string v = ver.GetVersion();
            b.Verbose.Log($"Performing increment {v}");
            outputContent.Launch(new SimpleMessage("Version Increment Requested - Currently " + v));

            if ((!string.IsNullOrWhiteSpace(opts.Release)) && (opts.Release != ver.Version.ReleaseName)) {
                ver.Version.ReleaseName = opts.Release;
            }
            ver.Version.IncrementByGroup(ResolveDigitGroupsForIncrement());

            b.Verbose.Log("About to save version store");
            ver.SaveUpdatedVersion();
        }

        string outputVersion = ver.Version.GetVersionStringByGroup(ResolveDigitGroupsForDisplay());
        passiveOutputValue = outputVersion;
        outputContent.Launch(new SimpleMessage($"Loaded [{outputVersion}]"));
    }

    private static async Task<int> Main(string[] args) {
        try {
            (bool shouldexit, int returnCode) = CheckQuickReturns(args);
            if (shouldexit) {
                return returnCode;
            }

            (bool success, var options) = CommandLineParser.Parse(args);

            if (options.RequestedCommand != VersioningCommand.GetDigitInformation ||
                !options.OutputsActive.HasFlag(OutputPossibilities.Json)) {
                WriteGreetingMessage();
            }

            if (options.Debug) {
                Console.WriteLine("Debug Mode, Command Line Arguments:");

                for (int n = 0; n < args.Length; n++) {
                    Console.WriteLine($"args[{n}]: {args[n]}");
                }
            }

            if (!success) {
                WriteErrorConditions();
                return 11;
            }

            if (options.GetMdHelp) {
                return await WriteMarkdownHelpFileAsync();
            }

            ConfigureOutput(options);

            if (!ArgumentValidator.ValidateArgumentSettings(options)) {
                WriteErrorConditions();
                return 12;
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
                if (versionerUsed != null && opts.RequestedCommand != VersioningCommand.GetDigitInformation) {
                    b.Verbose.Log("All Actions - Complete - Outputting.");
                    var vo = new VersioningOutputter(versionerUsed, outputContent) {
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
                outputContent.Launch(new SimpleMessage("Errors Occurred:"));
                foreach (string e in result.Errors) {
                    outputContent.Launch(new SimpleMessage(e));
                }

                outputContent.Launch(new SimpleMessage(HELP_HINT_MESSAGE));
            }

            b.Verbose.Log("Versonify - Exit.");
            if (options.Flush) {
                await b.Flush();
            }

            if (options.ReturnZero) {
                outputContent.Launch(new SimpleMessage($"ReturnZero option specified:  ExitCode: {result.ExitCode} suppressed."));
                return 0;
            }

            return result.ExitCode;
        } catch (Exception ex) {
            outputContent.Launch(new SimpleMessage("Fatal: An unhandled exception was encountered. " + ex.Message));
            return 1;
        }
    }

    private static ExecutionResult PerformActionsFromCommandline() {
        var result = new ExecutionResult();
        b.Verbose.Flow();
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        passiveOutputValue = null;

        if (opts.RequestedCommand != VersioningCommand.GetDigitInformation) {
            outputContent.Launch(new SimpleMessage("Performing Versioning Actions"));
        }

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

            case VersioningCommand.GetDigitInformation:
                result.WasProcessedSuccessfully = LoadDigitInformation(result);
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

    private static int[]? ResolveRequestedDigitIndices(CompleteVersion version, int[] groupDigitIndices, string groupName, ExecutionResult result) {
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        string[] requestedDigits = opts.DigitManipulations ?? [];
        b.Verbose.Log($"Validating requested digits [{string.Join(',', requestedDigits)}] for Get command");
        bool allDigitsRequested = requestedDigits.Contains(ALL_DIGITS_WILDCARD);
        ValidateNoWildcardCombination(requestedDigits, allDigitsRequested);

        if (requestedDigits.Length == 0 || allDigitsRequested) {
            b.Verbose.Log("No specific digits requested, defaulting to all digits in group.");
            return groupDigitIndices;
        }

        var selected = new List<int>(requestedDigits.Length);
        foreach (string requestedDigit in requestedDigits) {
            int digitIndex = ValidateRequestedDigitIndex(requestedDigit, version.Digits.Length);

            if (!groupDigitIndices.Contains(digitIndex)) {
                b.Warning.Log($"Digit [{digitIndex}] requested for Get command is not part of digit-group [{groupName}].");
                result.AddError($"Error >> The digit [{requestedDigit}] is not in digit-group '{groupName}'.", 1);
                return null;
            }

            if (!selected.Contains(digitIndex)) {
                selected.Add(digitIndex);
            }
        }

        return [.. selected];
    }

    private static void ValidateNoWildcardCombination(string[] requestedDigits, bool allDigitsRequested) {
        if (allDigitsRequested && requestedDigits.Length > 1) {
            b.Warning.Log("Wildcard digit selection was combined with explicit digit indices, this is not supported.");
            throw new ArgumentException(
                "The wildcard digit selection cannot be combined with explicit digit indices.",
                nameof(requestedDigits));
        }
    }

    private static int ValidateRequestedDigitIndex(string requestedDigit, int digitCount) {
        if (!int.TryParse(requestedDigit, out int digitIndex) ||
            digitIndex < 0 ||
            digitIndex >= digitCount) {
            b.Warning.Log($"Digit [{requestedDigit}] requested for Get command is not valid.");
            throw new ArgumentOutOfRangeException(
                nameof(requestedDigit),
                $"The digit [{requestedDigit}] is not a valid digit.");
        }

        return digitIndex;
    }

    private static void WriteDigitInformation(CompleteVersion version, int[] selectedIndices) {
        ArgumentNullException.ThrowIfNull(opts, nameof(opts));

        if (opts.OutputsActive.HasFlag(OutputPossibilities.Json)) {
            b.Verbose.Log("Building JSON digit information response");
            var digitInformation = new Dictionary<int, object>();
            foreach (int digitIndex in selectedIndices) {
                var digit = version.Digits[digitIndex];
                digitInformation.Add(digitIndex, new {
                    digitValue = digit.Value,
                    digitBehaviour = digit.Behaviour.ToString(),
                    digitQueuedOverride = digit.IncrementOverride,
                    digitPrefix = digit.PreFix,
                    digitgroup = GetDisplayDigitGroupName(digit.GroupName)
                });
            }
            outputContent.Launch(new SimpleMessage(JsonSerializer.Serialize(digitInformation)) {
                MessageType = OutputMessageType.Result
            });
        } else {
            b.Verbose.Log("Building plain-text digit information response");
            foreach (int digitIndex in selectedIndices) {
                var digit = version.Digits[digitIndex];
                outputContent.Launch(new SimpleMessage(
                    $"Digit at position [{digitIndex}] has prefix \"{digit.PreFix}\", has value \"{digit.Value}\", and is set to {digit.Behaviour} behavior. It belongs to digit-group {GetDisplayDigitGroupName(digit.GroupName)}, and its Queued Override value is {digit.IncrementOverride ?? "null"}."));
            }
        }
    }

    private static void WriteErrorConditions() {
        Console.WriteLine("Fatal:  Argument Validation Failed.");
        Console.WriteLine();
        Console.WriteLine(HELP_HINT_MESSAGE);
    }

    private static void WriteGreetingMessage() {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        string verString = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
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
