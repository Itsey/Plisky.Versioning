namespace Plisky.CodeCraft.Test {

    internal class TestableVersioningTask : VersioningTask {

        public void SetVersionNumber(CompleteVersion v) {
            ver = v;
        }

        public bool IsThisMinimatchIncluded(string mm) {
            foreach (string v in pendingUpdates.Keys) {
                if (v == mm) {
                    return true;
                }
            }
            return false;
        }
    }
}