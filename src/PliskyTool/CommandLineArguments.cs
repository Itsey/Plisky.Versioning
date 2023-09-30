namespace PliskyTool;

using System;
using System.IO;
using Plisky.CodeCraft;
using Plisky.Plumbing;

[CommandLineArguments]
public class CommandLineArguments {
    private OutputPossibilities outcache = OutputPossibilities.None;
    private string outOpts;
    private string pathPassed;

    public CommandLineArguments() {
        VersionTargetMinMatch = null;
    }

    [CommandLineArg("Command", Description = "Choose One of the following: CreateVersion,Override,UpdateFiles,Passive", IsSingleParameterDefault = true)]
    public string Command { get; set; }

    public string ConsoleTemplate { get; private set; }

    [CommandLineArg("Debug", Description = "Enables Debug Logging")]
    public bool Debug { get; set; }

    //-DG:
    [CommandLineArg("DG")]
    [CommandLineArg("Digits", Description = "Separated characters to form digits for the verison number", ArraySeparatorChar = ";")]
    public string[] DigitManipulations { get; set; }

    [CommandLineArg("NO", Description = "Allows you to ignore a saved override (see documentaiton).")]
    public bool NoOverride { get; set; }

    [CommandLineArg("O")]
    [CommandLineArg("Output", Description = "Specifies output options supports:  Env,Con,AzDo,File")]
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

    [CommandLineArg("DryRun", Description = "Runs the tool in output mode only, no changes are made")]
    public bool TestMode { get; set; }

    [CommandLineArg("Trace", Description = "Enables Debug Tracing, set to Info,Verbose,Off")]
    public string Trace { get; set; }

    [CommandLineArg("VS")]
    [CommandLineArg("VersionSource", Description = "Provides the source for retrieving version information")]
    public string VersionPersistanceValue { get; set; }

    [CommandLineArg("MM")]
    [CommandLineArg("MinMatch", ArraySeparatorChar = ";", Description = "A series of minmatch path descriptions to files to update")]
    public string[] VersionTargetMinMatch { get; set; }

    private void ParseOutputOptions() {
        if (string.IsNullOrEmpty(outOpts)) {
            outcache = OutputPossibilities.None;
            return;
        }

        if (outOpts == "env") {
            outcache = OutputPossibilities.Environment;
            return;
        }

        if (outOpts == "file") {
            outcache = OutputPossibilities.File;
            return;
        }

        if (outOpts.StartsWith("vsts") || (outOpts.StartsWith("azdo"))) {
            outcache = OutputPossibilities.Console;
            string varToReplace = "CodeVersionNumber";
            string outputTemplate = "##vso[task.setvariable variable=%BN%;]%VER%";
            if (outOpts.Contains(":")) {
                varToReplace = outOpts.Substring(outOpts.IndexOf(":"));
            }
            ConsoleTemplate = outputTemplate.Replace("%BN%", varToReplace);
            return;
        }

        if (outOpts.StartsWith("con")) {
            outcache = OutputPossibilities.Console;
            ConsoleTemplate = "%VER%";
            return;
        }

        throw new ArgumentOutOfRangeException("OutputOptions", $"The output option [{outOpts}] that were specified are invalid. Use (vsts|azdo|con|file|env).");
    }
}