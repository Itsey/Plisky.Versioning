namespace Plisky.CodeCraft;

using System;
using System.IO;
using Plisky.Diagnostics;
using Plisky.Versioning;

public class VersioningOutputter {
    public const string VERSIONING_PIPE_NAME = "plisky-versonify";
    public const string VERSION_MSG_SUBJECT = "version";
    public const string VERSION_REPLACE_TAG = "%VER%";
    protected Bilge b = new Bilge("Plisky-Tool-Output");
    protected CompleteVersion versionToLog;
    protected string valToWrite;

    public string? FileTemplate { get; set; }
    public string? ConsoleTemplate { get; set; }
    public string? PverFileName { get; set; }


    protected virtual void SetEnvironmentWithValue() {
        b.Verbose.Log($"Attempting to set environment variable PVER-LATEST to {valToWrite}");
        Environment.SetEnvironmentVariable("PVER-LATEST", valToWrite, EnvironmentVariableTarget.User);
    }

    public VersioningOutputter(CompleteVersion ver, DisplayType dt = DisplayType.Full) {
        versionToLog = ver;
        valToWrite = ver.GetVersionString(dt);
    }

    public void DoOutput(OutputPossibilities oo, VersioningCommand command) {
        b.Verbose.Flow($"{oo}");

        if (command == VersioningCommand.PassiveOutput) {
            b.Verbose.Log("Passive output requested, writing to passive output");
            WritePassiveOutput(oo);
        } else if (command == VersioningCommand.BehaviourOutput) {
            b.Verbose.Log("Behaviour output requested, writing to behaviour output");
            WriteBehaviourOutput(oo);
        } else {
            b.Error.Log($"Invalid command for output: {command}");
        }
        WritePassiveOutput(oo);
    }

    private void WriteBehaviourOutput(OutputPossibilities oo) {
        // TODO: Something Like  if output is console writ to console etc.
        WriteToConsole("TODO: Correct Output");
    }

    private void WritePassiveOutput(OutputPossibilities oo) {
        if ((oo & OutputPossibilities.Environment) == OutputPossibilities.Environment) {
            b.Verbose.Log("Environment output requested");
            SetEnvironmentWithValue();
        }
        if ((oo & OutputPossibilities.File) == OutputPossibilities.File) {
            b.Verbose.Log("File output requested");
            SetFileValue();
        }

        if ((oo & OutputPossibilities.Console) == OutputPossibilities.Console) {
            string outputString;
            if (ConsoleTemplate != null) {
                outputString = ConsoleTemplate.Replace(VERSION_REPLACE_TAG, valToWrite);
            } else {
                outputString = valToWrite;
            }


            WriteToConsole(outputString);
        }

        if ((oo & OutputPossibilities.NukeFusion) == OutputPossibilities.NukeFusion) {
            b.Verbose.Log("Named Pipe output requested");
            string outputString = $"PNFV]{valToWrite}";
            WriteToConsole(outputString);
            WriteToConsole($"PNF4]{versionToLog.GetVersionString(DisplayType.Full)}");
            WriteToConsole($"PNF2]{versionToLog.GetVersionString(DisplayType.Short)}");
            WriteToConsole($"PNFN]{versionToLog.ReleaseName}");
        }
    }

    protected virtual void SetFileValue() {
        string fileName = string.IsNullOrWhiteSpace(PverFileName) ? "pver-latest.txt" : PverFileName;
        string filePath = Path.Combine(Environment.CurrentDirectory, fileName);
        File.WriteAllText(filePath, valToWrite);
    }

    protected virtual void WriteToConsole(string outputString) {
        Console.WriteLine(outputString);
    }
}