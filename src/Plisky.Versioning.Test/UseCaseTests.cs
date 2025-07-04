namespace Plisky.CodeCraft.Test;

using System;
using System.IO;
using CodeCraft;
using Plisky.Diagnostics;
using Plisky.Test;
using Xunit;

public class UseCaseTests {
    private readonly Bilge b = new();
    private readonly UnitTestHelper uth = new();
    private readonly TestSupport ts;

    public UseCaseTests() {
        uth = new UnitTestHelper();
        ts = new TestSupport(uth);
    }

    [Fact(DisplayName = nameof(Versioning_MultiMMSameFile_UpdatesMultipleTimes))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Versioning_MultiMMSameFile_UpdatesMultipleTimes() {
        b.Info.Flow();

        // Early cut of versioning only made a single update to a file, once it had been updated it was not updated again. Test case
        // ensures that this is no longer the case and now every MM based match is respected and triggers its correspoing update.
        string reid = TestResources.GetIdentifiers(TestResourcesReferences.NetStdNone);
        string srcFile = uth.GetTestDataFile(reid);

        // Find This file
        var v = new MockVersioning(new MockVersionStorage(""));
        v.mock.AddFilenameToFind(srcFile);
        v.LoadMiniMatches("**/csproj|StdFile", srcFile + "|StdFile", srcFile + "|StdAssembly", srcFile + "|StdInformational");
        _ = v.SearchForAllFiles("");
        v.UpdateAllRegisteredFiles();

        string s = File.ReadAllText(srcFile);

        // Use the presence of the tags to ensure that all updates have been made.
        Assert.Contains("<Version>", s);
        Assert.Contains("<AssemblyVersion>", s);
        Assert.Contains("<FileVersion>", s);
    }


    [Fact(DisplayName = nameof(Versioning_DefaultBehaviour_IsIncrementBuild))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Versioning_DefaultBehaviour_IsIncrementBuild() {
        b.Info.Flow();

        var jvp = new JsonVersionPersister(uth.NewTemporaryFileName(true));
        var ver = new Versioning(jvp);

        string before = ver.ToString();
        ver.Increment();
        string after = ver.ToString();

        Assert.Equal("0.0.0.0", before);
        Assert.Equal("0.0.1.0", after);
    }

    [Fact(DisplayName = nameof(Versioning_StartsAtZero))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Versioning_StartsAtZero() {
        b.Info.Flow();

        string fn = uth.NewTemporaryFileName(true);
        var jvp = new JsonVersionPersister(fn);
        string str = new Versioning(jvp).ToString();

        Assert.Equal("0.0.0.0", str);
    }

    [Fact(DisplayName = nameof(UC_NoIncrement_NoChange))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void UC_NoIncrement_NoChange() {
        var mvs = new MockVersionStorage("0.0.0.1");
        var sut = new Versioning(mvs);

        Assert.Equal("0.0.0.1", sut.ToString());
    }

    [Theory(DisplayName = nameof(UC_BehaviouralIncrement_Works))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("0.0.0.0", "0.0.1.0")]
    [InlineData("0.0.1.1", "0.0.2.0")]
    [InlineData("3.4.0.0", "3.4.1.0")]
    [InlineData("0.0.0.9", "0.0.1.0")]
    public void UC_BehaviouralIncrement_Works(string initial, string target) {
        var mvs = new MockVersionStorage(initial);
        var sut = new Versioning(mvs);

        mvs.mock.SetBehaviours(DigitIncrementBehaviour.Fixed, DigitIncrementBehaviour.Fixed, DigitIncrementBehaviour.AutoIncrementWithResetAny, DigitIncrementBehaviour.AutoIncrementWithResetAny);

        sut.Increment();
        Assert.Equal(target, sut.ToString());
    }

    [Fact(DisplayName = nameof(UC_UpdateNuspecFile_Works))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void UC_UpdateNuspecFile_Works() {
        b.Info.Flow();

        string reid = TestResources.GetIdentifiers(TestResourcesReferences.NuspecSample1);
        string srcFile = uth.GetTestDataFile(reid);

        var mvs = new MockVersionStorage("0.0.0.1");
        var sut = new Versioning(mvs);

        sut.AddNugetFile(srcFile);
    }

    [Fact(DisplayName = nameof(AddInvalid_NugetFile_Throws))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void AddInvalid_NugetFile_Throws() {
        b.Info.Flow();

        var mvs = new MockVersionStorage("0.0.0.1");
        var sut = new Versioning(mvs);

        _ = Assert.Throws<ArgumentNullException>(() => { sut.AddNugetFile(null); });
        _ = Assert.Throws<FileNotFoundException>(() => { sut.AddNugetFile(""); });
        _ = Assert.Throws<FileNotFoundException>(() => { sut.AddNugetFile("c:\\arflebarflegloop.txt"); });
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Integration)]
    public void UseCase_BuildVersionger_Exploratory() {
        string tfn1 = uth.NewTemporaryFileName(false);
        string tfn2 = ts.CreateStoredVersionNumer();
        var sut = new VersioningTask();
        string directory = Path.GetDirectoryName(tfn1);
        sut.BaseSearchDir = directory;
        sut.SetPersistanceValue(tfn2);
        sut.AddUpdateType(tfn1, FileUpdateType.NetAssembly);
        sut.AddUpdateType(tfn1, FileUpdateType.NetFile);
        sut.AddUpdateType(tfn1, FileUpdateType.NetInformational);
        sut.IncrementAndUpdateAll();

        Assert.Equal("1.1.1.1", sut.VersionString);
        var jp = new JsonVersionPersister(tfn2);
        Assert.Equal(sut.VersionString, jp.GetVersion().GetVersionString());
        Assert.True(ts.DoesFileContainThisText(tfn1, "AssemblyVersion(\"1.1"), "The target filename was not updated");
        Assert.True(ts.DoesFileContainThisText(tfn1, "AssemblyInformationalVersion(\"1.1.1.1"), "The target filename was not updated");
        Assert.True(ts.DoesFileContainThisText(tfn1, "AssemblyFileVersion(\"1.1.1.1"), "The target filename was not updated");
    }
}