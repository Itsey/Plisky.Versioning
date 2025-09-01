namespace Versonify;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Plumbing;
using Plisky.Versioning;

[CommandLineArguments]
public class VersonifyCommandline {
    protected Bilge b = new Bilge("CommandLineArguments");
    private OutputPossibilities outcache = OutputPossibilities.None;
    private string outOpts;
    private string pathPassed;

    public VersonifyCommandline() {
        VersionTargetMinMatch = null;
    }

    [CommandLineArg("Command", Description = "Choose one of: CreateVersion,Override,UpdateFiles,Passive,Behaviour,Set,Prefix", IsSingleParameterDefault = true)]
    public string Command { get; set; }

    public string ConsoleTemplate { get; private set; }
    public string PverFileName { get; set; }
    public DigitIncrementBehaviour IncrementBehaviour { get; set; }

    public static readonly Dictionary<string, string> deprecatedCommandMappings = new(StringComparer.OrdinalIgnoreCase) {
        { "-DG", "-d" },
        { "-VS", "-v" },
        { "-NO", "-NoOverride" },
        { "-MM", "-m" }
    };

    //Current bug in Plisky.Plumbing 1.7.25 means -debug and -dryrun must be above -Digits in the code to work correctly.
    [CommandLineArg("Debug", Description = "Enables Debug Logging")]
    public bool Debug { get; set; }

    [CommandLineArg("DryRun", Description = "Runs the tool in output mode only, no changes are made")]
    public bool DryRunOnly { get; set; }

    [CommandLineArg("DG")]  // Marked as deprecated. Retained for backward compatability
    [CommandLineArg("D")]
    [CommandLineArg("Digits", Description = "Separated characters to form digits for the version number", ArraySeparatorChar = ";")]
    public string[] DigitManipulations { get; set; }

    [CommandLineArg("NO")]  // Marked as deprecated. Retained for backward compatability
    [CommandLineArg("NoOverride", Description = "Allows you to ignore a saved override (see documentation).")]
    public bool NoOverride { get; set; }

    [CommandLineArg("O")]
    [CommandLineArg("Output", Description = "Specifies output options supports:  Env,Con,AzDo,File,Np,Npo")]
    public string RawOutputOptions { get; set; }

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

    [CommandLineArg("I")]
    [CommandLineArg("Increment", Description = "Perform increment prior to updating the files.")]
    public bool PerformIncrement { get; set; }

    [CommandLineArg("Q")]
    [CommandLineArg("QuickValue")]
    public string QuickValue { get; set; }

    [CommandLineArg("R")]
    [CommandLineArg("Release", Description = "The release name associated with this version number")]
    public string Release { get; set; }

    [CommandLineArg("Root", Description = "The root disk location to start searching for versionable files in")]
    public string Root {
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


    [CommandLineArg("Trace", Description = "Enables Debug Tracing, set to Info,Verbose,Off")]
    public string Trace { get; set; }

    [CommandLineArg("V")]
    [CommandLineArg("VS")]  // Marked as deprecated. Retained for backward compatability
    [CommandLineArg("VersionSource", Description = "Provides the source for retrieving version information")]
    public string VersionPersistanceValue { get; set; }

    [CommandLineArg("M")]
    [CommandLineArg("MM")]  // Marked as deprecated. Retained for backward compatability
    [CommandLineArg("MinMatch", ArraySeparatorChar = ";", Description = "A series of minmatch path descriptions to files to update")]
    public string[] VersionTargetMinMatch { get; set; }

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