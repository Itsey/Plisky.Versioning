
using Nuke.Common.IO;

public class LocalBuildConfig {
    public required AbsolutePath ArtifactsDirectory { get; set; }
    public bool NonDestructive { get; set; } = true;
    public required string VersioningPersistanceToken { get; set; }
    public required string MainProjectName { get; set; }
    public required AbsolutePath DependenciesDirectory { get; set; }
    public required string MollyRulesToken { get; set; }
    public required string MollyPrimaryToken { get; set; }
    public required string MollyRulesVersion { get; set; }
    public required string VersioningPersistanceTokenRelease { get; set; }
}