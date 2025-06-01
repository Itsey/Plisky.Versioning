namespace Plisky.CodeCraft.Test;
using System.IO;
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

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Integration)]
    [Trait("Cause", "Bug:464")]
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
    public void IncrementAndUpdateThrowsIfNoDirectory() {
        _ = Assert.Throws<DirectoryNotFoundException>(() => {
            var sut = new VersioningTask();
            sut.IncrementAndUpdateAll();
        });
    }







}