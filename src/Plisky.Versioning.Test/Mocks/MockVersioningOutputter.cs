using System.Collections.Generic;

namespace Plisky.CodeCraft.Test {
    public class MockVersioningOutputter : VersioningOutputter {
        protected List<string> outputReceived = new List<string>();

        public bool EnvWasSet { get; set; }
        public bool FileWasWritten { get; set; }

        public string WrittenToConsole { get; set; }
        public string[] OutputLines { get { return outputReceived.ToArray(); } }

        protected override void SetEnvironmentWithValue() {
            EnvWasSet = true;
        }

        protected override void SetFileValue(string outputString) {
            FileWasWritten = true;
            RecordOutputReceived(outputString);
        }

        protected override void WriteToConsole(string outputString) {
            WrittenToConsole = outputString;
            RecordOutputReceived(outputString);
        }
        public MockVersioningOutputter(CompleteVersion v) : base(v) {
            EnvWasSet = false;
            FileWasWritten = false;
            WrittenToConsole = null;
        }

        public string GetTheValueRequestedToWrite() {
            return ValToWrite;
        }

        private void RecordOutputReceived(string outputString) {
            if (!string.IsNullOrEmpty(outputString)) {
                string[] lines = outputString.Split("\r\n");
                outputReceived.AddRange(lines);
            }
        }
    }
}