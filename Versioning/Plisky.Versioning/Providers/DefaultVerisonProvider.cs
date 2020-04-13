namespace Plisky.CodeCraft {

    public class DefaultVerisonProvider : IKnowHowToVersion {

        public VersionNumber GetBuildVersionNumberAfterIncrement(string g, string branchIndicator) {
            return new VersionNumber(9, 9, 9, 9);
        }

        public VersionNumber GetBuildVersionNumberWithoutIncrement(string g, string branchIndicator) {
            return new VersionNumber(1, 0, 0, 0);
        }

        public VersionNumber GetCompatibilityVersionNumberAfterIncrement(string g, string branchIndicator) {
            return new VersionNumber(9, 9, 9, 9);
        }

        public VersionNumber GetCompatibilityVersionNumberWithoutIncrement(string g, string branchIndicator) {
            return new VersionNumber(1, 0, 0, 0);
        }
    }
}