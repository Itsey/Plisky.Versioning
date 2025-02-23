namespace Plisky.CodeCraft;

using System;
using System.IO;
using Plisky.Diagnostics;

public class VersioningOutputter {
    protected Bilge b = new Bilge("Plisky-Tool-Output");
    protected string valToWrite;

    public string FileTemplate { get; set; }
    public string ConsoleTemplate { get; set; }


    protected virtual void SetEnvironmentWithValue() {
        b.Verbose.Log($"Attempting to set environment variable PVER-LATEST to {valToWrite}");
        Environment.SetEnvironmentVariable("PVER-LATEST", valToWrite, EnvironmentVariableTarget.User);
    }

    public VersioningOutputter(CompleteVersion ver, DisplayType dt = DisplayType.Full) {
        valToWrite = ver.GetVersionString(dt);
    }

    public void DoOutput(OutputPossibilities oo) {
        b.Verbose.Flow($"{oo}");

        if ((oo & OutputPossibilities.Environment) == OutputPossibilities.Environment) {
            b.Verbose.Log("Environment output requested");
            SetEnvironmentWithValue();
        }
        if ((oo & OutputPossibilities.File) == OutputPossibilities.File) {
            b.Verbose.Log("File output requested");
            SetFileValue();
        }


        if ((oo & OutputPossibilities.Console) == OutputPossibilities.Console) {

            string outputString = ConsoleTemplate.Replace("%VER%", valToWrite);
            //b.Verbose.Log($"Console Output: {outputString}");            
            WriteToConsole(outputString);
        }


    }

    protected virtual void SetFileValue() {
        string fn = Path.Combine(Environment.CurrentDirectory, "pver-latest.txt");
        File.WriteAllText(fn, valToWrite);
    }

    protected virtual void WriteToConsole(string outputString) {
        Console.WriteLine(outputString);
    }
}