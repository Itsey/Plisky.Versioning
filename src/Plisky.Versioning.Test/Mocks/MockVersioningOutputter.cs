using System.Collections.Generic;

namespace Plisky.CodeCraft.Test {
    public class MockVersioningOutputter : VersioningOutputter {
        protected List<string> outputRecieved = new List<string>();

        public bool EnvWasSet { get; set; }
        public bool FileWasWritten { get; set; }

        public string WrittenToConsole { get; set; }
        public string[] OutputLines { get { return outputRecieved.ToArray(); } }

        protected override void SetEnvironmentWithValue() {
            EnvWasSet = true;
        }

        protected override void SetFileValue() {
            FileWasWritten = true;
        }

        protected override void WriteToConsole(string outputString) {
            WrittenToConsole = outputString;
        }
        public MockVersioningOutputter(CompleteVersion v) : base(v) {
            EnvWasSet = false;
            FileWasWritten = false;
            WrittenToConsole = null;
        }

        public string GetTheValueRequestedToWrite() {
            return valToWrite;
        }
    }
}