namespace Plisky.CodeCraft;

public interface IKnowHowToVersion {

    VersionNumber GetBuildVersionNumberAfterIncrement(string g, string branchIndicator);

    VersionNumber GetBuildVersionNumberWithoutIncrement(string g, string branchIndicator);

    VersionNumber GetCompatibilityVersionNumberAfterIncrement(string g, string branchIndicator);

    VersionNumber GetCompatibilityVersionNumberWithoutIncrement(string g, string branchIndicator);
}