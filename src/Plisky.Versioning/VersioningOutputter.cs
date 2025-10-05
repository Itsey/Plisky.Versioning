namespace Plisky.CodeCraft;

using System;
using System.IO;
using System.Linq;
using Plisky.Diagnostics;
using Plisky.Versioning;

public class VersioningOutputter {
    public const string VERSIONING_PIPE_NAME = "plisky-versonify";
    public const string VERSION_MSG_SUBJECT = "version";
    public const string VERSION_REPLACE_TAG = "%VER%";
    public const string ALLDIGITSWILDCARD = "*";
    protected Bilge b = new Bilge("Plisky-Tool-Output");
    protected CompleteVersion versionToLog;
    protected string ValToWrite => ReleaseRequested
       ? versionToLog.ReleaseName ?? ""
       : versionToLog.GetVersionString();

    public string? FileTemplate { get; set; }
    public string? ConsoleTemplate { get; set; }
    public string? PverFileName { get; set; }
    public bool ReleaseRequested { get; set; }
    public string[] Digits { get; set; } = [ALLDIGITSWILDCARD];

    public string BehToWrite {
        get {
            if (Digits == null || Digits.Length == 0) {
                return string.Empty;
            }
            return string.Concat(Digits.Select(digit => versionToLog.GetBehaviourString(digit)));
        }
        set { }
    }

    protected virtual void SetEnvironmentWithValue() {
        string envVarName = ReleaseRequested ? "PVER-RELEASE" : "PVER-LATEST";
        b.Verbose.Log($"Attempting to set environment variable {envVarName} to {ValToWrite}");
        Environment.SetEnvironmentVariable(envVarName, ValToWrite, EnvironmentVariableTarget.User);
    }

    public VersioningOutputter(CompleteVersion ver, DisplayType dt = DisplayType.Full) {
        versionToLog = ver;
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
    }

    private void WriteBehaviourOutput(OutputPossibilities oo) {
        if ((oo & OutputPossibilities.File) == OutputPossibilities.File) {
            b.Verbose.Log("File output requested");
            PverFileName ??= "pbeh-latest.txt"; // Default file name if not set
            SetFileValue(BehToWrite);
        }
        if ((oo & OutputPossibilities.Console) == OutputPossibilities.Console) {
            b.Verbose.Log("Console output requested");
            WriteToConsole(BehToWrite);
        }
    }

    private void WritePassiveOutput(OutputPossibilities oo) {
        if ((oo & OutputPossibilities.Environment) == OutputPossibilities.Environment) {
            b.Verbose.Log("Environment output requested");
            SetEnvironmentWithValue();
        }
        if ((oo & OutputPossibilities.File) == OutputPossibilities.File) {
            b.Verbose.Log("File output requested");
            SetFileValue(ValToWrite);
        }

        if ((oo & OutputPossibilities.Console) == OutputPossibilities.Console) {
            string outputString;
            if (ConsoleTemplate != null) {
                outputString = ConsoleTemplate.Replace(VERSION_REPLACE_TAG, ValToWrite);
            } else {
                outputString = ValToWrite;
            }

            WriteToConsole(outputString);
        }

        if ((oo & OutputPossibilities.NukeFusion) == OutputPossibilities.NukeFusion) {
            string outputString = $"PNFV]{ValToWrite}";
            WriteToConsole(outputString);
            WriteToConsole($"PNF2]{versionToLog.GetVersionString(DisplayType.Short)}");
            WriteToConsole($"PNF3]{versionToLog.GetVersionString(DisplayType.ThreeDigit)}");
            WriteToConsole($"PN3D]{versionToLog.GetVersionString(DisplayType.ThreeDigitNumeric)}");
            WriteToConsole($"PNF4]{versionToLog.GetVersionString(DisplayType.Full)}");
            WriteToConsole($"PNQF]{versionToLog.GetVersionString(DisplayType.QueuedFull)}");
            WriteToConsole($"PN4D]{versionToLog.GetVersionString(DisplayType.FourDigitNumeric)}");
            WriteToConsole($"PNFN]{versionToLog.ReleaseName}");
        }
    }

    protected virtual void SetFileValue(string outputString) {
        string fileName = !string.IsNullOrWhiteSpace(PverFileName)
            ? PverFileName
            : ReleaseRequested ? "pver-release.txt" : "pver-latest.txt";
        string filePath = Path.Combine(Environment.CurrentDirectory, fileName);
        File.WriteAllText(filePath, outputString);
    }

    protected virtual void WriteToConsole(string outputString) {
        Console.WriteLine(outputString);
    }
}