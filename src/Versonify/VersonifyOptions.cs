namespace Versonify;

using System;
using System.IO;
using System.Linq;
using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Versioning;

public class VersonifyOptions {
    protected Bilge b = new Bilge("Options");

    private OutputPossibilities outcache = OutputPossibilities.None;
    private string outOpts = string.Empty;
    private string? pathPassed;

    public VersonifyOptions() {
        VersionTargetMinMatch = null!;
    }

    public string? Command { get; set; }

    public string? ConsoleTemplate { get; private set; }
    public string? PverFileName { get; set; }
    public DigitIncrementBehaviour IncrementBehaviour { get; set; }

    public bool Debug { get; set; }

    public bool DryRunOnly { get; set; }

    public string[]? DigitManipulations { get; set; }

    public bool ReturnZero { get; set; }

    public bool NoOverride { get; set; }

    public string? RawOutputOptions { get; set; }

    public string OutputOptions {
        get { return outOpts; }
        set {
            value ??= "";
            outOpts = value.Trim().ToLowerInvariant();
            ParseOutputOptions();
        }
    }

    public OutputPossibilities OutputsActive {
        get {
            return outcache;
        }
    }

    public bool PerformIncrement { get; set; }

    public bool GetMdHelp { get; set; }

    public string? QuickValue { get; set; }

    public string? Release { get; set; }

    public string? Root {
        get {
            if (string.IsNullOrEmpty(pathPassed)) {
                return null;
            }
            return Path.GetFullPath(pathPassed);
        }
        set {
            pathPassed = value;
        }
    }


    public string? Trace { get; set; }

    public string? VersionPersistanceValue { get; set; }

    public string[]? VersionTargetMinMatch { get; set; }

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

    public string[] GetDigits() {
        if (DigitManipulations == null || DigitManipulations.Length == 0) {
            b.Verbose.Log("No digits specified");
            return [];
        } else if (DigitManipulations.Contains("*")) {
            return ["*"];
        }

        return DigitManipulations;
    }

    public static bool TryParseDigitIncrementBehaviour(string value, out DigitIncrementBehaviour behaviour) {
        if (Enum.TryParse<DigitIncrementBehaviour>(value, true, out behaviour) &&
            Enum.IsDefined(typeof(DigitIncrementBehaviour), behaviour)) {
            return true;
        }
        Console.WriteLine($"Error: '{value}' is not a valid digit increment behaviour.");
        return false;
    }

    private void ParseOutputOptions() {
        b.Verbose.Flow();

        outOpts = outOpts.Trim().ToLowerInvariant();
        if (outOpts.EndsWith("-nf")) {
            outOpts = outOpts.Substring(0, outOpts.Length - 3);
            outcache = OutputPossibilities.NukeFusion;
        } else {
            outcache = OutputPossibilities.None;
        }


        if (string.IsNullOrEmpty(outOpts)) {
            b.Verbose.Log("No output options specified, defaulting to none.");
            outcache |= OutputPossibilities.None;
            return;
        }

        if (outOpts == "env") {
            outcache |= OutputPossibilities.Environment;
            return;
        }

        if (outOpts.StartsWith("file")) {
            outcache |= OutputPossibilities.File;
            if (outOpts.Contains(':')) {
                int markerPos = outOpts.IndexOf(':') + 1;
                if (markerPos < outOpts.Length) {
                    PverFileName = outOpts.Substring(markerPos).Trim();
                }
            }
            return;
        }

        if (outOpts.StartsWith("vsts") || (outOpts.StartsWith("azdo"))) {
            b.Verbose.Log("VSTS/AzDo output options specified.");

            outcache |= OutputPossibilities.Console;

            string varToReplace = "CodeVersionNumber";
            string outputTemplate = "##vso[task.setvariable variable=XXVARIABLENAMEXX;isOutput=true]%VER%";

            if (outOpts.Contains(":")) {
                int markerPos = outOpts.IndexOf(":") + 1;
                if (markerPos < outOpts.Length) {
                    varToReplace = outOpts.Substring(markerPos);
                }
            }

            ConsoleTemplate = outputTemplate.Replace("XXVARIABLENAMEXX", varToReplace);
            b.Verbose.Log($"Console Template Updated to {ConsoleTemplate.Replace("##vso", "dummy")}");

            return;
        }

        if (outOpts.StartsWith("con")) {
            outcache |= OutputPossibilities.Console;
            ConsoleTemplate = "%VER%";
            return;
        }

        throw new ArgumentOutOfRangeException("OutputOptions", $"The output option [{outOpts}] that were specified are invalid. Use (vsts|azdo|con|file|env).");
    }
}
