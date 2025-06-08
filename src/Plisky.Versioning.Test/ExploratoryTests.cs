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



    [Fact(Skip = "Todo")]
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

    [Fact(Skip = "Todo")]
    [Trait(Traits.Age, Traits.Fresh)]
    public void CommandLine_will_only_allow_asterisk_once() {
        var sut = new VersonifyCommandline();
        sut.DigitManipulations = new[] { "1", "*", "2", "*" };

        string[] gd = sut.GetDigits();

        gd.Length.ShouldBe(1, "There should only be one digit returned from the command line, even though two were specified.");
        gd[0].ShouldBe("*", "The only digit returned should be an asterisk, as that is the only valid digit in this case.");
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


    [Theory(Skip = "Todo")]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("88.99", 2)]
    [InlineData("88.99.77", 3)]
    [InlineData("5.6.7.8", 4)]
    [InlineData("0", 1)]
    [InlineData("0.0.0.0", 4)]
    public void Behaviour_returns_correct_number_digits(string verInit, int digits) {
        b.Info.Flow();

        var mvs = new MockVersionStorage(verInit);
        var sut = new Versioning(mvs);
        var v = sut.Version;

        var op = new MockVersioningOutputter(v);
        op.DoOutput(OutputPossibilities.File, VersioningCommand.BehaviourOutput);

        op.OutputLines.Length.ShouldBe(digits, "Correct number of lines of output should follow behaviour output.");
    }


    [Theory(Skip = "Todo")]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("99", DigitIncremementBehaviour.Fixed, "Fixed", 0)]
    [InlineData("5.6.7.8", DigitIncremementBehaviour.Fixed, "Fixed", 0)]
    [InlineData("5.68", DigitIncremementBehaviour.AutoIncrementWithReset, "AutoIncrementWithReset", 4)]
    [InlineData("0", DigitIncremementBehaviour.DaysSinceDate, "DaysSinceDate", 1)]
    [InlineData("0", DigitIncremementBehaviour.MajorDeterminesVersionNumber, "MajorDeterminesVersionNumber", 2)]
    [InlineData("0", DigitIncremementBehaviour.DailyAutoIncrement, "DailyAutoIncremement", 3)]
    [InlineData("0", DigitIncremementBehaviour.AutoIncrementWithReset, "AutoIncrementWithReset", 4)]
    [InlineData("0", DigitIncremementBehaviour.AutoIncrementWithResetAny, "AutoIncrementWithResetAny", 5)]
    [InlineData("0", DigitIncremementBehaviour.ContinualIncrement, "ContinualIncrement", 6)]
    [InlineData("0", DigitIncremementBehaviour.WeeksSinceDate, "WeeksSinceDate", 7)]
    [InlineData("0", DigitIncremementBehaviour.ReleaseName, "ReleaseName", 8)]
    public void Behaviour_output_works_for_fixed(string value, DigitIncremementBehaviour behaviour, string expectedOutput, int behaviourValue) {
        // Behaviour value is coded into the test here because its on the public interface, change the enum values introduce a breaking change.
        b.Info.Flow();

        var sut = new CompleteVersion(value);
        sut.Digits[0].Behaviour = behaviour;
        var op = new MockVersioningOutputter(sut);
        op.DoOutput(OutputPossibilities.File, VersioningCommand.BehaviourOutput);

        op.OutputLines.Length.ShouldBe(1, "There should be two lines of output for the behaviour output command.");
        op.OutputLines[0].ShouldBe($"[0]:{expectedOutput}({behaviourValue})");
    }


    [Fact(Skip = "Todo")]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Behaviour_output_does_not_have_digit_values() {
        b.Info.Flow();

        var sut = new CompleteVersion("99.89");
        var op = new MockVersioningOutputter(sut);

        op.DoOutput(OutputPossibilities.File, VersioningCommand.BehaviourOutput);

        op.OutputLines.Length.ShouldBe(2, "There should be two lines of output for the behaviour output command.");
        op.OutputLines[0].ShouldNotContain("99");
        op.OutputLines[1].ShouldNotContain("89");

    }

    [Theory(Skip = "Todo")]
    [Trait(Traits.Age, Traits.Fresh)]
    [InlineData(null, VersioningCommand.BehaviourOutput)]
    [InlineData("Fixed", VersioningCommand.BehaviourUpdate)]
    [InlineData("Auto", VersioningCommand.BehaviourUpdate)]
    public void CommandLine_correctly_sets_behviourtypes(string quickValue, VersioningCommand cmd) {
        var sut = new VersonifyCommandline();
        sut.Command = "behaviour";
        sut.QuickValue = quickValue;

        sut.RequestedCommand.ShouldBe(cmd, "Behaviour should be output unless a quick value is passed.");
    }



    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [InlineData("", VersioningCommand.Invalid)]
    [InlineData("bimajkasdercas;erasdf1!!assdf asda", VersioningCommand.Invalid)]
    [InlineData("=", VersioningCommand.Invalid)]
    [InlineData("CREATEVERSION", VersioningCommand.CreateNewVersion)]
    [InlineData("OVERRIDE", VersioningCommand.Override)]
    [InlineData("UPDATEFILES", VersioningCommand.UpdateFiles)]
    [InlineData("PASSIVE", VersioningCommand.PassiveOutput)]
    [InlineData("BEHAVIOUR", VersioningCommand.BehaviourOutput)]
    [InlineData("CreateVersion", VersioningCommand.CreateNewVersion)]
    [InlineData("override", VersioningCommand.Override)]
    [InlineData("updateFiles", VersioningCommand.UpdateFiles)]
    [InlineData("passIVE", VersioningCommand.PassiveOutput)]
    [InlineData("behaviour", VersioningCommand.BehaviourOutput)]
    public void CommandLine_correctly_sets_command_from_argument(string commandString, VersioningCommand cmd) {
        var sut = new VersonifyCommandline();
        sut.Command = commandString;
        sut.RequestedCommand.ShouldBe(cmd, "The command should be set correctly from the command line argument.");
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
            new VersionUnit("0", ".", DigitIncremementBehaviour.ContinualIncrement));
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







}