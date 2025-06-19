using Plisky.Versioning;

namespace Plisky.CodeCraft.Test;

using System;
using System.IO;
using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;
using Versonify;
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
    [Trait(Traits.Age, Traits.Fresh)]
    public void Commandline_digits_allows_multiple_digits() {
        var sut = new VersonifyCommandline();
        sut.DigitManipulations = new[] { "1", "2", "3" };

        string[] gd = sut.GetDigits();

        gd.Length.ShouldBe(3, "Three digits were passed in.");
        gd[0].ShouldBe("1");
        gd[1].ShouldBe("2");
        gd[2].ShouldBe("3");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    public void Validate_digitoptions_throws_when_invalid_digit_passed() {
        var cv = CompleteVersion.GetDefault();

        Assert.Throws<ArgumentOutOfRangeException>(() => {
            _ = cv.ValidateDigitOptions(new[] { "monkey" });
        });
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    public void Validate_digitoptions_returns_false_when_null_options() {
        var cv = CompleteVersion.GetDefault();

        bool result = cv.ValidateDigitOptions(null);
        result.ShouldBeFalse();
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [InlineData(null, VersioningCommand.BehaviourOutput)]
    [InlineData("Fixed", VersioningCommand.BehaviourUpdate)]
    [InlineData("0", VersioningCommand.BehaviourUpdate)]
    [InlineData("AutoIncrementWithReset", VersioningCommand.BehaviourUpdate)]
    [InlineData("DailyAutoIncrement", VersioningCommand.BehaviourUpdate)]
    [InlineData("ContinualIncrement", VersioningCommand.BehaviourUpdate)]
    [InlineData("Bannana", VersioningCommand.Invalid)]
    public void CommandLine_correctly_sets_behviourtypes(string quickValue, VersioningCommand cmd) {
        var sut = new VersonifyCommandline();
        sut.Command = "behaviour";
        sut.QuickValue = quickValue;

        sut.RequestedCommand.ShouldBe(cmd, "Behaviour should be output unless a quick value is passed.");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Integration)]
    [Trait("Cause", "Bug:464")]
    public void Build_version_does_not_update_during_build() {
        string ident = TestResources.GetIdentifiers(TestResourcesReferences.Bug464RefContent);

        string srcFile = uth.GetTestDataFile(ident);

        string fn = ts.GetFileAsTemporary(srcFile);
        var cv = new CompleteVersion(new VersionUnit("2"), new VersionUnit("0", "."),
            new VersionUnit("Unicorn", "-"),
            new VersionUnit("0", ".", DigitIncrementBehaviour.ContinualIncrement));
        var sut = new VersionFileUpdater(cv);

        _ = sut.PerformUpdate(fn, FileUpdateType.NetAssembly);
        _ = sut.PerformUpdate(fn, FileUpdateType.NetInformational);
        _ = sut.PerformUpdate(fn, FileUpdateType.NetFile);

        ts.DoesFileContainThisText(fn, "AssemblyFileVersion(\"2.0.0\"").ShouldBeFalse("The file version should be three digits and present");
        ts.DoesFileContainThisText(fn, "AssemblyInformationalVersion(\"2.0-Unicorn.0\"").ShouldBeTrue("The informational version should be present");
        ts.DoesFileContainThisText(fn, "AssemblyVersion(\"2.0\")").ShouldBeTrue("the assembly version should be two digits and present.");
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
    public void ApplyBehaviourUpdate_UpdatesSingleDigitBehaviour() {
        var version = new CompleteVersion(
            new VersionUnit("1"),
            new VersionUnit("2"),
            new VersionUnit("3")
        );
        // Precondition: All digits start as Fixed
        version.Digits.ShouldAllBe(d => d.Behaviour == DigitIncrementBehaviour.Fixed);

        version.ApplyBehaviourUpdate("1", DigitIncrementBehaviour.AutoIncrementWithReset);

        version.Digits[0].Behaviour.ShouldBe(DigitIncrementBehaviour.Fixed);
        version.Digits[1].Behaviour.ShouldBe(DigitIncrementBehaviour.AutoIncrementWithReset);
        version.Digits[2].Behaviour.ShouldBe(DigitIncrementBehaviour.Fixed);
    }

    [Fact]
    public void ApplyBehaviourUpdate_UpdatesAllDigitsWhenWildcard() {
        var version = new CompleteVersion(
            new VersionUnit("1"),
            new VersionUnit("2"),
            new VersionUnit("3")
        );

        version.ApplyBehaviourUpdate("*", DigitIncrementBehaviour.ContinualIncrement);

        version.Digits.ShouldAllBe(d => d.Behaviour == DigitIncrementBehaviour.ContinualIncrement);
    }
}