namespace Plisky.CodeCraft.Test;

using System.IO;
using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Plisky.Test;
using Shouldly;
using Xunit;

public class FileUpdateTests {
    private readonly UnitTestHelper uth;
    private readonly TestSupport ts;
    protected static InMemoryHandler imh = new(100000);
    protected static Bilge b;

    public FileUpdateTests() {
        if (b == null) {
            b = new Bilge();
            b.AddHandler(new TCPHandler("127.0.0.1", 5060, true));
            b.AddHandler(imh);
        }

        uth = new UnitTestHelper();

        ts = new TestSupport(uth);
        b.Info.Log("UTH Online");
    }

    ~FileUpdateTests() {
        uth.ClearUpTestFiles();
    }

    [Fact(DisplayName = nameof(LiteralReplace_DefaultReplacesVersionAndReleaseName))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void LiteralReplace_DefaultReplacesVersionAndReleaseName() {
        b.Info.Flow();

        string reid = TestResources.GetIdentifiers(TestResourcesReferences.ReleaseNameAndVerTxt);
        string srcFile = uth.GetTestDataFile(reid);
        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", ".")) {
            ReleaseName = "Unicorn"
        };

        var sut = new VersionFileUpdater(cv);
        _ = sut.PerformUpdate(srcFile, FileUpdateType.TextFile, DisplayType.Release);

        string result = File.ReadAllText(srcFile);
        result.ShouldNotContain("XXX-RELEASENAME-XXX");
        result.ShouldContain("Unicorn");
        result.ShouldNotContain("XXX-VERSION-XXX");
        result.ShouldContain("1.1.1.1");
    }

    [Fact(DisplayName = nameof(LiteralReplace_Version3_IsThreeDigits))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void LiteralReplace_Version3_IsThreeDigits() {
        b.Info.Flow();

        string reid = TestResources.GetIdentifiers(TestResourcesReferences.VersionV3Txt);
        string srcFile = uth.GetTestDataFile(reid);
        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));

        var sut = new VersionFileUpdater(cv);
        _ = sut.PerformUpdate(srcFile, FileUpdateType.TextFile, DisplayType.Release);
        string result = File.ReadAllText(srcFile);

        result.ShouldNotContain("XXX-VERSION3-XXX");
        result.ShouldNotContain("1.1.1.1");
        result.ShouldContain("1.1.1");
    }

    [Fact(DisplayName = nameof(LiteralReplace_Version2_IsTwoDigits))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void LiteralReplace_Version2_IsTwoDigits() {
        b.Info.Flow();

        string reid = TestResources.GetIdentifiers(TestResourcesReferences.VersionV3Txt);
        string srcFile = uth.GetTestDataFile(reid);
        File.WriteAllText(srcFile, File.ReadAllText(srcFile).Replace("XXX-VERSION3-XXX", "XXX-VERSION2-XXX"));
        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));

        var sut = new VersionFileUpdater(cv);
        _ = sut.PerformUpdate(srcFile, FileUpdateType.TextFile, DisplayType.Release);
        string result = File.ReadAllText(srcFile);

        result.ShouldNotContain("XXX-VERSION3-XXX");
        result.ShouldNotContain("XXX-VERSION2-XXX");
        result.ShouldNotContain("1.1.1.1");
        result.ShouldNotContain("1.1.1");
        result.ShouldContain("1.1");
    }

    [Fact(DisplayName = nameof(LiteralReplace_NoDisplay_DoesNotUpdateVersion))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void LiteralReplace_NoDisplay_DoesNotUpdateVersion() {
        b.Info.Flow();

        string reid = TestResources.GetIdentifiers(TestResourcesReferences.ReleaseNameAndVerTxt);
        string srcFile = uth.GetTestDataFile(reid);
        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", ".")) {
            ReleaseName = "Unicorn"
        };

        var sut = new VersionFileUpdater(cv);
        _ = sut.PerformUpdate(srcFile, FileUpdateType.TextFile, DisplayType.NoDisplay);

        string result = File.ReadAllText(srcFile);
        result.ShouldNotContain("XXX-RELEASENAME-XXX");
        result.ShouldContain("Unicorn");
        result.ShouldContain("XXX-VERSION-XXX");
        result.ShouldNotContain("1.1.1.1");
    }

    [Fact(DisplayName = nameof(VersionFileUpdaterFindsFiles))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void VersionFileUpdaterFindsFiles() {
        b.Info.Flow();

        var msut = new MockVersionFileUpdater();
        msut.mock.AddFilesystemFile("XX");

        msut.mock.ContainsFilesystemFile("XX").ShouldBeTrue();
    }
    [Theory]
    [InlineData("AssemblyVersion", "[assembly: AssemblyVersion(\"1.2.3.4\")]")]
    [InlineData("AssemblyFileVersion", "[ assembly : AssemblyFileVersion ( \" 1.2.* \" ) ]")]
    [InlineData("AssemblyInformationalVersion", "[assembly: AssemblyInformationalVersion(\"Beta-1.2\")] // trailing comment")]
    [InlineData("AssemblyVersion", "   \t[assembly:\tAssemblyVersion(\"1.2.3.4\")]\t")]
    [InlineData("AssemblyVersion", "[assembly: aSSeMbLyVeRsIoN(\"1.2.3.4\")]")]
    [InlineData("AssemblyVersion", "//  \t[assembly: AssemblyVersion(\"1.2.3.4\")]")] // Note: regex matches; comment filtering is handled outside the regex.
    public void GetRegex_MatchesExpectedAssemblyAttributeLines_WhenValidInput(string attributeName, string line) {
        var sut = VersionFileUpdater.GetRegex(attributeName);

        bool result = sut.IsMatch(line);

        result.ShouldBeTrue();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Regex_MatchesForAssembly() {
        b.Info.Flow();

        var rx = VersionFileUpdater.GetRegex("AssemblyVersion");
        rx.IsMatch("[assembly: AssemblyVersion(\"0.0.0.0\")]").ShouldBeTrue("1 Invalid match for an assembly version");
        rx.IsMatch("[assembly: AssemblyVersion(\"0.0.0\")]").ShouldBeTrue("2 Invalid match for an assembly version");
        rx.IsMatch("[assembly: AssemblyVersion(\"0.0\")]").ShouldBeTrue("3 Invalid match for an assembly version");
        rx.IsMatch("[assembly: AssemblyVersion(\"0\")] ").ShouldBeTrue("4 Invalid match for an assembly version");
        rx.IsMatch("[assembly: AssemblyVersion(\"\")] ").ShouldBeTrue("5 Invalid match for an assembly version");
        rx.IsMatch("[assembly:      AssemblyVersion     (\"0.0.0.0\"   )     ]").ShouldBeTrue("7 Invalid match for an assembly version");
        rx.IsMatch("[assembly     :AssemblyVersion(\"0.0.0.0\")]").ShouldBeTrue("8 Invalid match for an assembly version");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Regex_MatchesForInformational() {
        b.Info.Flow();

        var rx = VersionFileUpdater.GetRegex("AssemblyFileVersion");
        rx.IsMatch("[assembly: AssemblyFileVersion(\"0.0.0.0\")]").ShouldBeTrue("1 Invalid match for an assembly version");
        rx.IsMatch("[assembly: AssemblyFileVersion(\"0.0.0\")]").ShouldBeTrue("2 Invalid match for an assembly version");
        rx.IsMatch("[assembly: AssemblyFileVersion(\"0.0\")]").ShouldBeTrue("3 Invalid match for an assembly version");
        rx.IsMatch("[assembly: AssemblyFileVersion(\"0\")] ").ShouldBeTrue("4 Invalid match for an assembly version");
        rx.IsMatch("[assembly: AssemblyFileVersion(\"\")] ").ShouldBeTrue("5 Invalid match for an assembly version");
        rx.IsMatch("[assembly:      AssemblyFileVersion     (\"0.0.0.0\"   )     ]").ShouldBeTrue("7 Invalid match for an assembly version");
        rx.IsMatch("[assembly     :AssemblyFileVersion(\"0.0.0.0\")]").ShouldBeTrue("8 Invalid match for an assembly version");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Regex_MatchesForFile() {
        b.Info.Flow();

        var rx = VersionFileUpdater.GetRegex("AssemblyInformationalVersion");

        rx.IsMatch("[assembly: AssemblyInformationalVersion(\"0.0.0.0\")]").ShouldBeTrue("1 Invalid match for an assembly version");
        rx.IsMatch("[assembly: AssemblyInformationalVersion(\"0.0.0\")]").ShouldBeTrue("2 Invalid match for an assembly version");
        rx.IsMatch("[assembly: AssemblyInformationalVersion(\"0.0\")]").ShouldBeTrue("3 Invalid match for an assembly version");
        rx.IsMatch("[assembly: AssemblyInformationalVersion(\"0\")] ").ShouldBeTrue("4 Invalid match for an assembly version");
        rx.IsMatch("[assembly: AssemblyInformationalVersion(\"\")] ").ShouldBeTrue("5 Invalid match for an assembly version");
        rx.IsMatch("[assembly:      AssemblyInformationalVersion     (\"0.0.0.0\"   )     ]").ShouldBeTrue("7 Invalid match for an assembly version");
        rx.IsMatch("[assembly     :AssemblyInformationalVersion(\"0.0.0.0\")]").ShouldBeTrue("8 Invalid match for an assembly version");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Update_AsmVersion_Works() {
        b.Info.Flow();

        string reid = TestResources.GetIdentifiers(TestResourcesReferences.JustAssemblyVer);

        string srcFile = uth.GetTestDataFile(reid);

        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
        string fn = ts.GetFileAsTemporary(srcFile);

        var sut = new VersionFileUpdater(cv);

        _ = sut.PerformUpdate(fn, FileUpdateType.NetAssembly);

        ts.DoesFileContainThisText(fn, "0.0").ShouldBeFalse("No update was made to the file at all");
        ts.DoesFileContainThisText(fn, "1.1.1.1").ShouldBeTrue("The file does not appear to have been updated correctly.");
        ts.DoesFileContainThisText(fn, "AssemblyVersion(\"1.1.1.1\")").ShouldBeTrue("The file does not have the full version in it");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Update_DoesNotAlterOtherAttributes() {
        b.Info.Flow();

        string reid = TestResources.GetIdentifiers(TestResourcesReferences.NoChangeAssemInfo);
        string srcFile = uth.GetTestDataFile(reid);

        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
        string fn = ts.GetFileAsTemporary(srcFile);

        var sut = new VersionFileUpdater(cv);

        _ = sut.PerformUpdate(fn, FileUpdateType.NetAssembly);

        ts.DoesFileContainThisText(fn, " AssemblyVersion(\"1.0.0.0\")").ShouldBeFalse("No update was made to the file at all");
        ts.DoesFileContainThisText(fn, "[assembly: AssemblyFileVersion(\"1.0.0.0\")]").ShouldBeTrue("The file does not appear to have been updated correctly.");
        ts.DoesFileContainThisText(fn, "[assembly: AssemblyCompany(\"\")]").ShouldBeTrue("Collatoral Damage - Another element in the file was updated - Company");
        ts.DoesFileContainThisText(fn, "[assembly: Guid(\"557cc26f-fcb2-4d0e-a34e-447295115fc3\")]").ShouldBeTrue("Collatoral Damage - Another element in the file was updated - Guid");
        ts.DoesFileContainThisText(fn, "// [assembly: AssemblyVersion(\"1.0.*\")]").ShouldBeTrue("Collatoral Damage - Another element in the file was updated - Comment");
        ts.DoesFileContainThisText(fn, "using System.Reflection;").ShouldBeTrue("Collatoral Damage - Another element in the file was updated - Reflection First Line");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Update_AsmInfVer_Works() {
        string reid = TestResources.GetIdentifiers(TestResourcesReferences.JustInformational);
        string srcFile = uth.GetTestDataFile(reid);

        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
        string fn = ts.GetFileAsTemporary(srcFile);

        var sut = new VersionFileUpdater(cv);

        _ = sut.PerformUpdate(fn, FileUpdateType.NetInformational);

        ts.DoesFileContainThisText(fn, "0.0.0.0").ShouldBeFalse("No update was made to the file at all");
        ts.DoesFileContainThisText(fn, "1.1").ShouldBeTrue("The file does not appear to have been updated correctly.");
        ts.DoesFileContainThisText(fn, "AssemblyInformationalVersion(\"1.1.1.1\")").ShouldBeTrue("The file does not have the full version in it");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Update_AsmFileVer_Works() {
        b.Info.Flow();
        string reid = TestResources.GetIdentifiers(TestResourcesReferences.JustFileVer);
        string srcFile = uth.GetTestDataFile(reid);
        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
        string fn = ts.GetFileAsTemporary(srcFile);
        var sut = new VersionFileUpdater(cv);

        string response = sut.PerformUpdate(fn, FileUpdateType.NetFile);

        ts.DoesFileContainThisText(fn, "0.0.0.0").ShouldBeFalse("No update was made to the file at all");
        ts.DoesFileContainThisText(fn, "1.1").ShouldBeTrue("The file does not appear to have been updated correctly.");
        ts.DoesFileContainThisText(fn, "AssemblyFileVersion(\"1.1.1.1\")").ShouldBeTrue("The file does not have the full version in it");
        response.ShouldContain($"Updated AssemblyFileVersion");
    }

    [Fact(DisplayName = nameof(Update_Nuspec_Works))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Update_Nuspec_Works() {
        string reid = TestResources.GetIdentifiers(TestResourcesReferences.NuspecSample1);
        string srcFile = uth.GetTestDataFile(reid);
        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
        var sut = new VersionFileUpdater(cv);
        string before = ts.GetVersion(FileUpdateType.Nuspec, srcFile);
        before.ShouldNotBeNullOrEmpty();

        string response = sut.PerformUpdate(srcFile, FileUpdateType.Nuspec);

        string after = ts.GetVersion(FileUpdateType.Nuspec, srcFile);
        after.ShouldNotBe(before);
        response.ShouldContain("Updated Nuspec");
    }

    [Fact(DisplayName = nameof(Update_StdCSProjAsm_Works))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Update_StdCSProjAsm_Works() {
        string reid = TestResources.GetIdentifiers(TestResourcesReferences.NetStdAll3);
        string srcFile = uth.GetTestDataFile(reid); // Value is zero
        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
        var sut = new VersionFileUpdater(cv);
        string before = ts.GetVersion(FileUpdateType.StdAssembly, srcFile);
        before.ShouldNotBeNullOrEmpty();

        string response = sut.PerformUpdate(srcFile, FileUpdateType.StdAssembly);

        string after = ts.GetVersion(FileUpdateType.StdAssembly, srcFile);
        after.ShouldNotBe(before);
        response.ShouldContain("Updated Std Assembly");
    }

    [Fact(DisplayName = nameof(UpdateStd_AddsFileWhenMissing))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void UpdateStd_AddsFileWhenMissing() {
        string reid = TestResources.GetIdentifiers(TestResourcesReferences.NetStdNone);
        string srcFile = uth.GetTestDataFile(reid);
        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
        var sut = new VersionFileUpdater(cv);
        string before = ts.GetVersion(FileUpdateType.StdFile, srcFile);

        _ = sut.PerformUpdate(srcFile, FileUpdateType.StdFile);

        string after = ts.GetVersion(FileUpdateType.StdFile, srcFile);
        before.ShouldBeNullOrEmpty();
        after.ShouldNotBeNullOrEmpty();
    }

    [Fact(DisplayName = nameof(UpdateStd_AddsAsmWhenMissing))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void UpdateStd_AddsAsmWhenMissing() {
        string reid = TestResources.GetIdentifiers(TestResourcesReferences.NetStdNone);
        string srcFile = uth.GetTestDataFile(reid);
        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
        var sut = new VersionFileUpdater(cv);
        string before = ts.GetVersion(FileUpdateType.StdAssembly, srcFile);

        _ = sut.PerformUpdate(srcFile, FileUpdateType.StdAssembly);

        string after = ts.GetVersion(FileUpdateType.StdAssembly, srcFile);

        before.ShouldBeNullOrEmpty();
        after.ShouldNotBeNullOrEmpty();
    }

    [Fact(DisplayName = nameof(UpdateStd_AddsStdInfoWhenMissing))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void UpdateStd_AddsStdInfoWhenMissing() {
        string reid = TestResources.GetIdentifiers(TestResourcesReferences.NetStdNone);
        string srcFile = uth.GetTestDataFile(reid);
        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
        var sut = new VersionFileUpdater(cv);
        string before = ts.GetVersion(FileUpdateType.StdInformational, srcFile);

        _ = sut.PerformUpdate(srcFile, FileUpdateType.StdInformational);

        string after = ts.GetVersion(FileUpdateType.StdInformational, srcFile);

        before.ShouldBeNullOrEmpty();
        after.ShouldNotBeNullOrEmpty();
    }

    [Fact(DisplayName = nameof(Update_StdCSProjFile_Works))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Update_StdCSProjFile_Works() {
        string reid = TestResources.GetIdentifiers(TestResourcesReferences.NetStdAll3);
        string srcFile = uth.GetTestDataFile(reid); // Value is zero
        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
        var sut = new VersionFileUpdater(cv);
        string before = ts.GetVersion(FileUpdateType.StdFile, srcFile);
        before.ShouldNotBeNullOrEmpty();

        string response = sut.PerformUpdate(srcFile, FileUpdateType.StdFile);

        string after = ts.GetVersion(FileUpdateType.StdFile, srcFile);
        after.ShouldNotBe(before);
        response.ShouldContain("Updated Std File");
    }

    [Fact(DisplayName = nameof(Update_StdCSProjInfo_Works))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Update_StdCSProjInfo_Works() {
        string reid = TestResources.GetIdentifiers(TestResourcesReferences.NetStdAll3);
        string srcFile = uth.GetTestDataFile(reid);
        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
        var sut = new VersionFileUpdater(cv);
        string before = ts.GetVersion(FileUpdateType.StdInformational, srcFile);
        before.ShouldNotBeNullOrEmpty();

        string response = sut.PerformUpdate(srcFile, FileUpdateType.StdInformational);

        string after = ts.GetVersion(FileUpdateType.StdInformational, srcFile);
        after.ShouldNotBe(before);
        response.ShouldContain("Updated Std Informational");
    }

    [Fact(DisplayName = nameof(Update_Wix_Works))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Update_Wix_Works() {
        string reid = TestResources.GetIdentifiers(TestResourcesReferences.WixSample1);
        string srcFile = uth.GetTestDataFile(reid);
        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
        var sut = new VersionFileUpdater(cv);
        string before = ts.GetVersion(FileUpdateType.Wix, srcFile);
        before.ShouldNotBeNullOrEmpty();

        string response = sut.PerformUpdate(srcFile, FileUpdateType.Wix);

        string after = ts.GetVersion(FileUpdateType.Wix, srcFile);
        after.ShouldNotBe(before);
        response.ShouldContain("Updated Wix");
    }

    [Fact(DisplayName = nameof(Update_Nuspec_BugNoUpdate))]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Update_Nuspec_BugNoUpdate() {
        b.Info.Flow();
        // BUG Case - for some reason nuspec was not being updated. B_NuspecUpdateFailed

        string reid = TestResources.GetIdentifiers(TestResourcesReferences.BugNuspecUpdateFail);
        string srcFile = uth.GetTestDataFile(reid);

        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
        var sut = new VersionFileUpdater(cv);

        string knownStartPoint = "<version>1.7.2.0</version>";
        string destinationPoint = "<version>1.1.1.1</version>";
        string txt = File.ReadAllText(srcFile);

        _ = sut.PerformUpdate(srcFile, FileUpdateType.Nuspec, DisplayType.Release);
        string txt2 = File.ReadAllText(srcFile);

        (txt.IndexOf(knownStartPoint) > 0).ShouldBeTrue();
        (txt.IndexOf(destinationPoint) > 0).ShouldBeFalse();
        (txt2.IndexOf(knownStartPoint) > 0).ShouldBeFalse();
        (txt2.IndexOf(destinationPoint) > 0).ShouldBeTrue();
    }

    [Fact]
    public void PerformUpdate_Throws_WhenFileDoesNotExist() {
        b.Info.Flow();
        var cv = new CompleteVersion();
        var sut = new VersionFileUpdater(cv);
        string nonExistentFile = Path.Combine(Path.GetTempPath(), "ThisFileDoesNotExist12345.txt");

        var ex = Should.Throw<FileNotFoundException>(() => {
            _ = sut.PerformUpdate(nonExistentFile, FileUpdateType.TextFile);
        });

        ex.Message.ShouldContain("Filename must be present");
    }
}
