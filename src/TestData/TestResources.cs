namespace Plisky.Test;

public enum TestResourcesReferences {
    Bug464RefContent,
    BugNuspecUpdateFail,
    JustAssemblyVer,
    NoChangeAssemInfo,
    PropertiesAssemInfo,
    JustFileVer,
    JustInformational,
    NuspecSample1,
    NuspecSample2,
    NetStdAll3,
    MMTypeData,
    NetStdNone,
    ReleaseNameAndVerTxt,
    MultiReleaseNameAndVer,
    VersionV3Txt,
    DefaultVersionStore,
    OneEachBehaviourStore,
}

public static class TestResources {

    public static string GetIdentifiers(TestResourcesReferences refNo) {
        return refNo switch {
            TestResourcesReferences.Bug464RefContent => "B464_AsmInfo_Source",
            TestResourcesReferences.JustAssemblyVer => "JustAssemblyVersion.txt",
            TestResourcesReferences.NoChangeAssemInfo => "DoesNotChange.AssemblyInfo.txt",
            TestResourcesReferences.PropertiesAssemInfo => "Properties.AssemblyInfo.txt",
            TestResourcesReferences.JustFileVer => "JustFileVersion.txt",
            TestResourcesReferences.JustInformational => "JustInformationalVersion.txt",
            TestResourcesReferences.NuspecSample1 => "sample1.nuspec",
            TestResourcesReferences.NuspecSample2 => "sample2.nuspec",
            TestResourcesReferences.NetStdAll3 => "MultipleVersNetStd.csproj",
            TestResourcesReferences.NetStdNone => "MissingAllElements.csproj",
            TestResourcesReferences.MMTypeData => "mmTypes.txt",
            TestResourcesReferences.ReleaseNameAndVerTxt => "ReleaseName.txt",
            TestResourcesReferences.VersionV3Txt => "ReleaseNameV3.txt",
            TestResourcesReferences.MultiReleaseNameAndVer => "ReleaseNameLorem.txt",
            TestResourcesReferences.BugNuspecUpdateFail => "B_NuspecUpdateFailed.nuspec",
            TestResourcesReferences.DefaultVersionStore => "default_fxfxaifx.vstore",
            TestResourcesReferences.OneEachBehaviourStore => "one_each_behaviour.vstore",
            _ => null,
        };
    }
}