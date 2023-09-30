namespace Plisky.CodeCraft.Test {
    using Plisky.Versioning.VersionNumbering;

    internal class TestableVersioningTask : VersioningTask {

        public void SetVersionNumber(CompleteVersion v) {
            ver = v;
        }

        public bool IsThisMinimatchIncluded(string mm) {
            foreach (var v in pendingUpdates.Keys) {
                if (v == mm) {
                    return true;
                }
            }
            return false;
        }
    }
}