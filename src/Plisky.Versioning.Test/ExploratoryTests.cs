namespace Plisky.CodeCraft.Test;

using System;
using System.Collections.Generic;
using System.IO;
using Minimatch;
using Plisky.Diagnostics;
using Plisky.Test;
using Xunit;

public class Exploratory {
    private readonly Bilge b = new();
    private readonly UnitTestHelper uth;
    private readonly TestSupport ts;

    public Exploratory() {
        uth = new UnitTestHelper();
        ts = new TestSupport(uth);
    }

    ~Exploratory() {
        uth.ClearUpTestFiles();
    }

    private static void CheckTheseMatches(List<Tuple<string, bool>> mtchs, string againstThis) {
        var mm = new Minimatcher(againstThis, new Options { AllowWindowsPaths = true, IgnoreCase = true });
        int i = 0;
        foreach (var v in mtchs) {
            i++;
            bool isMatch = mm.IsMatch(v.Item1);
            Assert.Equal(v.Item2, isMatch); //, "Mismatch " + i.ToString());
        }
    }

    private string CreateStoredVersionNumer() {
        string fn = uth.NewTemporaryFileName(true);
        var cv = GetDefaultVersion();
        var jvp = new JsonVersionPersister(fn);
        jvp.Persist(cv);
        return fn;
    }

    private static CompleteVersion GetDefaultVersion() {
        return new CompleteVersion(
            new VersionUnit("0", "", DigitIncremementBehaviour.ContinualIncrement),
            new VersionUnit("0", ".", DigitIncremementBehaviour.ContinualIncrement),
            new VersionUnit("0", ".", DigitIncremementBehaviour.ContinualIncrement),
            new VersionUnit("0", ".", DigitIncremementBehaviour.ContinualIncrement)
        );
    }

    [Theory]
    [InlineData("0.0.0.0", "0.0.0.0")]
    [InlineData("1.2.3.4", "1.2.3.4")]
    [InlineData("1234.1234.1234.1234", "1234.1234.1234.1234")]
    [InlineData("0", "0.0.0.0")]
    [InlineData("1.2.3", "1.2.3.0")]
    public void VersionNumberParseTests(string parseString, string expected) {
        var vnd = VersionNumber.Parse(parseString);
        Assert.Equal(expected, vnd.ToString());
    }

    [Theory]
    [InlineData("1.2.3.4", "1.2.3.4", false)]
    [InlineData("2.2.3.4", "1.2.3.4", true)]
    [InlineData("0.2.3.4", "1.2.3.4", false)]
    [InlineData("0.2.4.0", "0.2.3.400", true)]
    [InlineData("1.0.0.0", "0.999.999.999", true)]
    [InlineData("0.0.1.0", "0.0.0.400", true)]
    [InlineData("1.0", "0.0.0.400", true)]
    public void DoVersionComparisonsWork(string v1, string v2, bool v1IsGreater) {
        var vn1 = VersionNumber.Parse(v1);
        var vn2 = VersionNumber.Parse(v2);

        bool isGreater = vn1 > vn2;
        Assert.Equal(v1IsGreater, isGreater);
    }


    [Theory]
    [InlineData("1.2.3.4", "1.2.3.4", true)]
    [InlineData("0.0.0.0", "0.0.0.0", true)]
    [InlineData("0.0", "0.0", true)]
    [InlineData("1.0", "1.0.0.0", true)]
    [InlineData("2.2.3.4", "1.2.3.4", true)]
    [InlineData("1.0.3.4", "1.2.3.4", false)]
    public void DoVersionComparisonsWork2(string v1, string v2, bool isGreaterExpected) {
        var vn1 = VersionNumber.Parse(v1);
        var vn2 = VersionNumber.Parse(v2);

        bool isGreaterActual = vn1 >= vn2;
        Assert.Equal(isGreaterExpected, isGreaterActual);
    }


    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Developer)]
    public void Bug464_BuildVersionsNotUpdatedDuringBuild() {
        string ident = TestResources.GetIdentifiers(TestResourcesReferences.Bug464RefContent);

        string srcFile = uth.GetTestDataFile(ident);

        string fn = ts.GetFileAsTemporary(srcFile);
        var cv = new CompleteVersion(new VersionUnit("2"), new VersionUnit("0", "."),
            new VersionUnit("Unicorn", "-"),
            new VersionUnit("0", ".", DigitIncremementBehaviour.ContinualIncrement));
        var sut = new VersionFileUpdater(cv);

        _ = sut.PerformUpdate(fn, FileUpdateType.NetAssembly);
        _ = sut.PerformUpdate(fn, FileUpdateType.NetInformational);
        _ = sut.PerformUpdate(fn, FileUpdateType.NetFile);

        Assert.False(ts.DoesFileContainThisText(fn, "AssemblyFileVersion(\"2.0.0\""), "The file version should be three digits and present");
        Assert.True(ts.DoesFileContainThisText(fn, "AssemblyInformationalVersion(\"2.0-Unicorn.0\""), "The informational version should be present");
        Assert.True(ts.DoesFileContainThisText(fn, "AssemblyVersion(\"2.0\")"), "the assembly version should be two digits and present.");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void BuildTask_InvalidRuleType_Throws() {
        _ = Assert.Throws<InvalidOperationException>(() => {
            var sut = new TestableVersioningTask();
            string verItemsSimple = "**/assemblyinfo.cs;ASSXXEMBLY";
            sut.SetAllVersioningItems(verItemsSimple);
        });
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void BuildTask_PassInRules_Works() {
        var v = GetDefaultVersion();
        var sut = new TestableVersioningTask();
        sut.SetVersionNumber(v);
        string verItemsSimple = "**/assemblyinfo.cs!ASSEMBLY";
        sut.SetAllVersioningItems(verItemsSimple);

        Assert.True(sut.IsThisMinimatchIncluded("**/assemblyinfo.cs"), "The minimatch was not included");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void BuildTask_PassInMultipleRules_Works() {
        var v = GetDefaultVersion();
        var sut = new TestableVersioningTask();
        sut.SetVersionNumber(v);
        string verItemsSimple =
            $"**/assemblyinfo.cs!ASSEMBLY{Environment.NewLine}xxMonkey!FILE{Environment.NewLine}yyzzxxbannana!WIX{Environment.NewLine}";
        sut.SetAllVersioningItems(verItemsSimple);

        Assert.True(sut.IsThisMinimatchIncluded("**/assemblyinfo.cs"), "The minimatch was not included");
        Assert.True(sut.IsThisMinimatchIncluded("xxMonkey"), "The second minimatch was not included");
        Assert.True(sut.IsThisMinimatchIncluded("yyzzxxbannana"), "The third minimatch was not included");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void UseCase_Plisky_Works() {
        var sut = new CompleteVersion(new VersionUnit("2"), new VersionUnit("0", "."),
            new VersionUnit("Unicorn", "-"),
            new VersionUnit("0", ".", DigitIncremementBehaviour.ContinualIncrement));
        string verString = sut.GetVersionString();
        Assert.Equal("2.0-Unicorn.0", verString); //,"The initial string is not correct");
        sut.Increment();
        verString = sut.GetVersionString();
        Assert.Equal("2.0-Unicorn.1", verString); //, "The first increment string is not correct");
        sut.Increment();
        verString = sut.GetVersionString(DisplayType.Full);
        Assert.Equal("2.0-Unicorn.2", verString); //, "The second increment string is not correct");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void IncrementAndUpdateThrowsIfNoDirectory() {
        _ = Assert.Throws<DirectoryNotFoundException>(() => {
            var sut = new VersioningTask();
            sut.IncrementAndUpdateAll();
        });
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void UseCase_BuildVersionger_Exploratory() {
        string tfn1 = uth.NewTemporaryFileName(false);
        string tfn2 = CreateStoredVersionNumer();
        var sut = new VersioningTask();
        string directory = Path.GetDirectoryName(tfn1);
        sut.BaseSearchDir = directory;
        sut.SetPersistanceValue(tfn2);
        sut.AddUpdateType(tfn1, FileUpdateType.NetAssembly);
        sut.AddUpdateType(tfn1, FileUpdateType.NetFile);
        sut.AddUpdateType(tfn1, FileUpdateType.NetInformational);
        sut.IncrementAndUpdateAll();

        Assert.Equal("1.1.1.1", sut.VersionString); //, "The version string should be set post update");
        var jp = new JsonVersionPersister(tfn2);
        Assert.Equal(sut.VersionString, jp.GetVersion().GetVersionString()); //, "The update should be persisted");
        Assert.True(ts.DoesFileContainThisText(tfn1, "AssemblyVersion(\"1.1"), "The target filename was not updated");
        Assert.True(ts.DoesFileContainThisText(tfn1, "AssemblyInformationalVersion(\"1.1.1.1"), "The target filename was not updated");
        Assert.True(ts.DoesFileContainThisText(tfn1, "AssemblyFileVersion(\"1.1.1.1"), "The target filename was not updated");
    }

    [Theory]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData(@"C:\temp\verworking\assemblyinfo.cs", @"**\assemblyinfo.cs", true)]
    [InlineData(@"C:\temp\verworking\testing.csproj", @"**/*.csproj", true)]
    [InlineData(@"C:\temp\verworking\testing.csproj", @"**\verworking\*.csproj", true)]
    [InlineData(@"C:\temp\verworking\AsUbBy\testing.csproj", @"**\asubby\**\*.csproj", true)]
    [InlineData(@"C:\temp\verworking\AsUbBy\commonassemblyinfo.cs", @"**\asubby\**\common*.cs", true)]
    public void MinimatchSyntax_Research(string filename, string minimatch, bool shouldPass) {
        var mtchs = new List<Tuple<string, bool>> {
            new Tuple<string, bool>(filename, shouldPass)
        };
        CheckTheseMatches(mtchs, minimatch);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void MiniMatchSyntax_FindAssemblyInfo() {
        var mtchs = new List<Tuple<string, bool>> {
            new Tuple<string, bool>(@"C:\temp\te st\properties\assemblyinfo.cs", true),
            new Tuple<string, bool>(@"C:\te mp\test\assemblyinfo.cs", false),
            new Tuple<string, bool>(@"C:\te mp\t e s t\properties\notassemblyinfo.cs", false),
            new Tuple<string, bool>(@"C:\temp\test\properties\assemblyinfo.cs.txt", false),
            new Tuple<string, bool>(@"C:\a\1\s\PliskyLibrary\PliskyLib\Properties\AssemblyInfo.cs", true)
        };
        string againstThis = @"**\properties\assemblyinfo.cs";
        CheckTheseMatches(mtchs, againstThis);

        var mm2 = new Minimatcher(@"C:\temp\test\testfile.tst", new Options { AllowWindowsPaths = true });
        Assert.True(mm2.IsMatch(@"C:\temp\test\testfile.tst"), "Cant match on full filename");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void VersionStorage_SavesCorrectly() {
        var msut = new MockVersionStorage("itsamock");
        VersionStorage sut = msut;

        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1"), new VersionUnit("1"), new VersionUnit("1"));
        sut.Persist(cv);
        Assert.True(msut.PersistWasCalled, "The persist method was not called");
        Assert.Equal("1111", msut.VersionStringPersisted);
    }



    [Fact(DisplayName = nameof(Storage_DefaultValidationIsTrue))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Storage_DefaultValidationIsTrue() {
        b.Info.Flow();
        VersionStorage sut = new MockVersionStorage("itsamock");
        Assert.True(sut.ValidateInitialisation());
    }


    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void VersionStorage_Json_Saves() {
        string fn = uth.NewTemporaryFileName(true);
        var sut = new JsonVersionPersister(fn);
        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1"), new VersionUnit("1"), new VersionUnit("1"));
        sut.Persist(cv);
        Assert.True(File.Exists(fn), "The file must be created");
    }

    [Fact]
    public void VersionStorage_Json_Loads() {
        string fn = uth.NewTemporaryFileName(true);
        var sut = new JsonVersionPersister(fn);
        var cv = new CompleteVersion(new VersionUnit("1", ".", DigitIncremementBehaviour.ContinualIncrement), new VersionUnit("Alpha", "-"), new VersionUnit("1"), new VersionUnit("1", "", DigitIncremementBehaviour.ContinualIncrement));
        sut.Persist(cv);

        var cv2 = sut.GetVersion();

        Assert.Equal(cv.GetVersionString(), cv2.GetVersionString()); //, "The loaded type was not the same as the saved one, values");
        cv.Increment(); cv2.Increment();
        Assert.Equal(cv.GetVersionString(), cv2.GetVersionString()); //, "The loaded type was not the same as the saved one, behaviours");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void VersionStoreAndLoad_StoresUpdatedValues() {
        string fn = uth.NewTemporaryFileName(true);
        var sut = new JsonVersionPersister(fn);
        var cv = new CompleteVersion(new VersionUnit("1", "", DigitIncremementBehaviour.ContinualIncrement),
            new VersionUnit("1", ".", DigitIncremementBehaviour.ContinualIncrement),
            new VersionUnit("1", ".", DigitIncremementBehaviour.ContinualIncrement),
            new VersionUnit("1", ".", DigitIncremementBehaviour.ContinualIncrement));

        cv.Increment();
        _ = cv.GetVersionString();
        sut.Persist(cv);
        var cv2 = sut.GetVersion();

        Assert.Equal(cv.GetVersionString(), cv2.GetVersionString()); //, "The two version strings should match");
        Assert.Equal("2.2.2.2", cv2.GetVersionString()); //, "The loaded version string should keep the increment");

        cv.Increment(); cv2.Increment();
        Assert.Equal(cv.GetVersionString(), cv2.GetVersionString()); //, "The two version strings should match");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void VersionStoreAndLoad_StoresDisplayTypes() {
        string fn = uth.NewTemporaryFileName(true);
        var sut = new JsonVersionPersister(fn);
        var cv = new CompleteVersion(new VersionUnit("1", "", DigitIncremementBehaviour.ContinualIncrement),
            new VersionUnit("1", ".", DigitIncremementBehaviour.ContinualIncrement),
            new VersionUnit("1", ".", DigitIncremementBehaviour.ContinualIncrement),
            new VersionUnit("1", ".", DigitIncremementBehaviour.ContinualIncrement));

        // None of the defaults are no display, therefore this should set all to a new value
        cv.SetDisplayTypeForVersion(FileUpdateType.NetAssembly, DisplayType.NoDisplay);
        cv.SetDisplayTypeForVersion(FileUpdateType.NetFile, DisplayType.NoDisplay);
        cv.SetDisplayTypeForVersion(FileUpdateType.NetInformational, DisplayType.NoDisplay);
        cv.SetDisplayTypeForVersion(FileUpdateType.Wix, DisplayType.NoDisplay);
        cv.SetDisplayTypeForVersion(FileUpdateType.StdAssembly, DisplayType.NoDisplay);
        cv.SetDisplayTypeForVersion(FileUpdateType.StdFile, DisplayType.NoDisplay);
        cv.SetDisplayTypeForVersion(FileUpdateType.StdInformational, DisplayType.NoDisplay);
        cv.SetDisplayTypeForVersion(FileUpdateType.Nuspec, DisplayType.NoDisplay);

        sut.Persist(cv);
        var cv2 = sut.GetVersion();

        // Check that all of the display types come back as epxected
        Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.NetAssembly));
        Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.NetFile));
        Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.NetInformational));
        Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.Wix));
        Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.StdAssembly));
        Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.StdFile));
        Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.StdInformational));
        Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.Nuspec));
    }
}