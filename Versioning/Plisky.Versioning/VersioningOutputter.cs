using System;
using System.IO;

namespace Plisky.CodeCraft.Test {
    public class VersioningOutputter {
        protected string valToWrite;

        protected virtual void SetEnvironmentWithValue() {
            Environment.SetEnvironmentVariable("PVER-LATEST", valToWrite, EnvironmentVariableTarget.User);
        }

        public VersioningOutputter(CompleteVersion ver) {
            valToWrite = ver.ToString();
        }

        public void DoOutput(OutputPossibilities oo) {
            if ((oo & OutputPossibilities.Environment) == OutputPossibilities.Environment) {
                SetEnvironmentWithValue();
            }
            if ((oo & OutputPossibilities.File) == OutputPossibilities.File) {
                SetFileValue();
            }
        }

        protected virtual void SetFileValue() {
            var fn = Path.Combine(Environment.CurrentDirectory, "pver-latest.txt");
            File.WriteAllText(fn, valToWrite);
        }
    }
}