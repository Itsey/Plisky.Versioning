namespace Plisky.CodeCraft.Test {
    public class MockVersioningOutputter : VersioningOutputter {
        public bool EnvWasSet { get; set; }
        public bool FileWasWritten { get; set; }

        public string WrittenToConsole { get; set; }

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
    }
}