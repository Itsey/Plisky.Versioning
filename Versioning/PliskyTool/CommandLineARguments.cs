﻿using Plisky.Plumbing;

namespace PliskyTool {
    [CommandLineArguments]
    public class CommandLineArguments {

        [CommandLineArg("Command",IsSingleParameterDefault = true)]
        public string Command { get; set; }

        [CommandLineArg("VS")]
        [CommandLineArg("VersionSource", Description = "Provides the source for retrieving version information")]
        public string VersionPersistanceValue { get; set; }

        [CommandLineArg("I")]
        [CommandLineArg("Increment",Description = "Perform increment prior to updating the files.")]
        public bool PerformIncrement { get; set; }

        //-DG:
        [CommandLineArg("DG")]
        [CommandLineArg("Digits",ArraySeparatorChar =";")]
        public string[] DigitManipulations { get; set; }

        [CommandLineArg("Q")]
        [CommandLineArg("QuickValue")]
        public string QuickValue { get; set; }

        [CommandLineArg("MM")]
        [CommandLineArg("MinMatch",ArraySeparatorChar =";",Description ="A series of minmatch path descriptions to files to update")]
        public string[] VersionTargetMinMatch { get; set; }

        [CommandLineArg("Root", Description = "The root disk location to start searching for versionable files in")]
        public string Root { get; set; }


        [CommandLineArg("DryRun", Description ="Runs the tool in output mode only, no changes are made")]
        public bool TestMode { get; set; }


        public CommandLineArguments() {
            VersionTargetMinMatch = new string[] { 
                "**\\properties\\assemblyinfo.cs",              
                "**\\*.nuspec" };
        }
    }
}
