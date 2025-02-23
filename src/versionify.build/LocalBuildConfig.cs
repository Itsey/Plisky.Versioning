
using Nuke.Common.IO;

public class LocalBuildConfig {
    public AbsolutePath ArtifactsDirectory { get; set; }
    public bool NonDestructive { get; set; } = true;
    public string VersioningPersistanceToken { get; set; }
    public string MainProjectName { get; internal set; }
    public string DependenciesDirectory { get; internal set; }
    public string MollyRulesToken { get; internal set; }
    public string MollyPrimaryToken { get; internal set; }
    public string MollyRulesVersion { get; internal set; }
}