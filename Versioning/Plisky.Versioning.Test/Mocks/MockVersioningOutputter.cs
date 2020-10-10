namespace Plisky.CodeCraft.Test {
    public class MockVersioningOutputter : VersioningOutputter {
        public bool EnvWasSet { get; set; }
        public bool FileWasWritten { get; set; }


        protected override void SetEnvironmentWithValue() {
            EnvWasSet = true;
        }

        protected override void SetFileValue() {
            FileWasWritten = true;
        }

        public MockVersioningOutputter(CompleteVersion v) : base(v) {
            EnvWasSet = false;
            FileWasWritten = false;
        }
    }
}