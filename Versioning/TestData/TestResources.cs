namespace Plisky.Test {

    public enum TestResourcesReferences {
        Bug464RefContent,
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
        VersionV3Txt
    }

    public static class TestResources {

        public static string GetIdentifiers(TestResourcesReferences refNo) {
            switch (refNo) {
                case TestResourcesReferences.Bug464RefContent: return "B464_AsmInfo_Source";
                case TestResourcesReferences.JustAssemblyVer: return "JustAssemblyVersion.txt";
                case TestResourcesReferences.NoChangeAssemInfo: return "DoesNotChange.AssemblyInfo.txt";
                case TestResourcesReferences.PropertiesAssemInfo: return "Properties.AssemblyInfo.txt";
                case TestResourcesReferences.JustFileVer: return "JustFileVersion.txt";
                case TestResourcesReferences.JustInformational: return "JustInformationalVersion.txt";
                case TestResourcesReferences.NuspecSample1: return "sample1.nuspec";
                case TestResourcesReferences.NuspecSample2: return "sample2.nuspec";
                case TestResourcesReferences.NetStdAll3: return "MultipleVersNetStd.csproj";
                case TestResourcesReferences.NetStdNone: return "MissingAllElements.csproj";
                case TestResourcesReferences.MMTypeData: return "mmTypes.txt";
                case TestResourcesReferences.ReleaseNameAndVerTxt: return "ReleaseName.txt";
                case TestResourcesReferences.VersionV3Txt: return "ReleaseNameV3.txt";
                case TestResourcesReferences.MultiReleaseNameAndVer: return "ReleaseNameLorem.txt";
            }

            return null;
        }
    }
}