using System;
using System.IO;

namespace Plisky.CodeCraft.Test {
    public class VersioningOutputter {
        protected string valToWrite;

        public string FileTemplate { get; set; }
        public string ConsoleTemplate { get; set; } 


        protected virtual void SetEnvironmentWithValue() {
            Environment.SetEnvironmentVariable("PVER-LATEST", valToWrite, EnvironmentVariableTarget.User);
        }

        public VersioningOutputter(CompleteVersion ver, DisplayType dt = DisplayType.Full) {
            valToWrite = ver.GetVersionString(dt);
        }

        public void DoOutput(OutputPossibilities oo) {
            if ((oo & OutputPossibilities.Environment) == OutputPossibilities.Environment) {
                SetEnvironmentWithValue();
            }
            if ((oo & OutputPossibilities.File) == OutputPossibilities.File) {
                SetFileValue();
            }

            if ((oo & OutputPossibilities.Console) == OutputPossibilities.Console) {
                WriteToConsole(ConsoleTemplate.Replace("%VER%",valToWrite));
            }


        }

        protected virtual void SetFileValue() {
            var fn = Path.Combine(Environment.CurrentDirectory, "pver-latest.txt");
            File.WriteAllText(fn, valToWrite);
        }

        protected virtual void WriteToConsole(string outputString) {
            Console.WriteLine(outputString);
        }
    }
}