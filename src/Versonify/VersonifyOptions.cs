using System;
using System.IO;
using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Versioning;

namespace Versonify;

public class VersonifyOptions {
    protected Bilge b = new("Options");
    private string outOpts = string.Empty;

    private string? pathPassed;

    public VersonifyOptions() {
        VersionTargetMinMatch = null!;
    }

    public string? Command { get; set; }

    public string? ConsoleTemplate { get; private set; }
    public bool Debug { get; set; }
    public string? DigitGroup { get; set; }
    public string[]? DigitManipulations { get; set; }
    public bool DryRunOnly { get; set; }
    public bool Flush { get; set; }
    public bool GetMdHelp { get; set; }
    public DigitIncrementBehaviour IncrementBehaviour { get; set; }
    public bool NoOverride { get; set; }

    public string OutputOptions {
        get { return outOpts; }
        set {
            value ??= string.Empty;
            outOpts = value.Trim().ToLowerInvariant();
            ParseOutputOptions();
        }
    }

    public OutputPossibilities OutputsActive { get; private set; } = OutputPossibilities.None;

    public bool PerformIncrement { get; set; }
    public bool PreRelease { get; set; }
    public string? PverFileName { get; set; }
    public string? QuickValue { get; set; }
    public string? RawOutputOptions { get; set; }
    public string? Release { get; set; }
    public bool ReleaseValueMissing { get; set; }

    public VersioningCommand RequestedCommand {
        get {
            if (string.IsNullOrEmpty(Command)) {
                return VersioningCommand.Invalid;
            }
            switch (Command.ToLowerInvariant()) {
                case "createversion":
                    return VersioningCommand.CreateNewVersion;

                case "override":
                    return VersioningCommand.Override;

                case "updatefiles":
                    return VersioningCommand.UpdateFiles;

                case "passive":
                    return VersioningCommand.PassiveOutput;

                case "behaviour":
                    if (string.IsNullOrEmpty(QuickValue)) {
                        return VersioningCommand.BehaviourOutput;
                    } else {
                        if (TryParseDigitIncrementBehaviour(QuickValue, out var parsedBehaviour)) {
                            IncrementBehaviour = parsedBehaviour;
                            return VersioningCommand.BehaviourUpdate;
                        }
                    }
                    return VersioningCommand.Invalid;

                case "get":
                    return VersioningCommand.GetDigitInformation;

                case "set":
                    if (Release != null) {
                        return VersioningCommand.SetReleaseName;
                    } else {
                        return VersioningCommand.SetDigitValue;
                    }
                case "prefix":
                    return VersioningCommand.SetDigitPrefix;

                default:
                    return VersioningCommand.Invalid;
            }
        }
    }

    public bool ReturnZero { get; set; }

    public string? Root {
        get {
            if (string.IsNullOrEmpty(pathPassed)) {
                return null;
            }
#if DEBUG
            if (pathPassed == ArgumentValidator.TEST_VALID_ARGUMENT) {
                return pathPassed;
            }
#endif
            return Path.GetFullPath(pathPassed);
        }
        set { pathPassed = value; }
    }

    public string? Trace { get; set; }

    public string? VersionPersistanceValue { get; set; }

    public string[]? VersionTargetMinMatch { get; set; }

    public static bool TryParseDigitIncrementBehaviour(string value, out DigitIncrementBehaviour behaviour) {
        if (Enum.TryParse<DigitIncrementBehaviour>(value, true, out behaviour) &&
            Enum.IsDefined(typeof(DigitIncrementBehaviour), behaviour)) {
            return true;
        }
        Console.WriteLine($"Error: '{value}' is not a valid digit increment behaviour.");
        return false;
    }

    public string[] GetDigits() {
        if (DigitManipulations == null || DigitManipulations.Length == 0) {
            b.Verbose.Log("No digits specified");
            return [];
        } else if (DigitManipulations.Contains("*")) {
            return ["*"];
        }

        return DigitManipulations;
    }

    private void ParseOutputOptions() {
        b.Verbose.Flow();

        string outRaw = outOpts.Trim();
        outOpts = outRaw.ToLowerInvariant();
        if (outOpts.EndsWith("-nf")) {
            outOpts = outOpts[..^3];
            OutputsActive = OutputPossibilities.NukeFusion;
        } else if (outOpts.EndsWith("-pf")) {
            outOpts = outOpts[..^3];
            OutputsActive = OutputPossibilities.PliskyFusion;
        } else {
            OutputsActive = OutputPossibilities.None;
        }

        if (string.IsNullOrEmpty(outOpts)) {
            b.Verbose.Log("No output options specified, defaulting to none.");
            OutputsActive |= OutputPossibilities.None;
            return;
        }

        if (outOpts == "jcon") {
            OutputsActive = OutputPossibilities.Json | OutputPossibilities.Console;
            return;
        }

        if (outOpts == "env") {
            OutputsActive |= OutputPossibilities.Environment;
            return;
        }

        if (outOpts.StartsWith("file")) {
            OutputsActive |= OutputPossibilities.File;
            if (outRaw.Contains(':')) {
                int markerPos = outRaw.IndexOf(':') + 1;
                if (markerPos < outRaw.Length) {
                    PverFileName = outRaw[markerPos..].Trim();
                }
            }
            return;
        }

        if (outOpts.StartsWith("vsts") || outOpts.StartsWith("azdo")) {
            b.Verbose.Log("VSTS/AzDo output options specified.");

            OutputsActive |= OutputPossibilities.Console;

            string varToReplace = "CodeVersionNumber";
            const string OUTPUT_TEMPLATE = "##vso[task.setvariable variable=XXVARIABLENAMEXX;isOutput=true]%VER%";

            if (outOpts.Contains(':')) {
                int markerPos = outRaw.IndexOf(':') + 1;
                if (markerPos < outRaw.Length) {
                    varToReplace = outRaw[markerPos..];
                }
            }

            ConsoleTemplate = OUTPUT_TEMPLATE.Replace("XXVARIABLENAMEXX", varToReplace);
            b.Verbose.Log($"Console Template Updated to {ConsoleTemplate.Replace("##vso", "dummy")}");

            return;
        }

        if (outOpts.StartsWith("con")) {
            OutputsActive |= OutputPossibilities.Console;
            ConsoleTemplate = "%VER%";
            return;
        }

        throw new ArgumentOutOfRangeException("OutputOptions", $"The output option [{outOpts}] that were specified are invalid. Use (vsts|azdo|con|con-nf|con-pf|jcon|file|env).");
    }
}
