namespace Plisky.CodeCraft.Test;

using System;
using System.IO;
using CodeCraft;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Plisky.Test;
using Xunit;

public class UseCaseTests {
    private readonly Bilge b = new();
    private readonly UnitTestHelper uth = new();

    [Obsolete]
    public UseCaseTests() {
        Bilge.AddMessageHandler(new TCPHandler("127.0.0.1", 9060));
        Bilge.SetConfigurationResolver((a, b) => {
            return System.Diagnostics.SourceLevels.Verbose;
        });
    }

    [Fact(DisplayName = nameof(Versioning_MultiMMSameFile_UpdatesMultipleTimes))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Versioning_MultiMMSameFile_UpdatesMultipleTimes() {
        b.Info.Flow();

        // Get the Std CSroj with no attributes.
        string reid = TestResources.GetIdentifiers(TestResourcesReferences.NetStdNone);
        string srcFile = uth.GetTestDataFile(reid);

        // Find This file
        var v = new MockVersioning(new MockVersionStorage(""));
        v.Mock.AddFilenameToFind(srcFile);
        v.LoadMiniMatches("**/csproj|StdFile", srcFile + "|StdFile", srcFile + "|StdAssembly", srcFile + "|StdInformational");
        _ = v.SearchForAllFiles("");
        v.UpdateAllRegisteredFiles();

        string s = File.ReadAllText(srcFile);

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

        mvs.Mock.SetBehaviours(DigitIncremementBehaviour.Fixed, DigitIncremementBehaviour.Fixed, DigitIncremementBehaviour.AutoIncrementWithResetAny, DigitIncremementBehaviour.AutoIncrementWithResetAny);

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
}