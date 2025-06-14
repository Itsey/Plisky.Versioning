namespace Plisky.CodeCraft.Test;

using System;
using Plisky.Diagnostics;
using Plisky.Test;
using Plisky.Versioning;
using Shouldly;
using Versonify;
using Xunit;

public class VersonifyCommandLineTests {
    private readonly Bilge b = new();
    private readonly UnitTestHelper uth;
    private readonly TestSupport ts;

    public VersonifyCommandLineTests() {
        uth = new UnitTestHelper();
        ts = new TestSupport(uth);
    }

    [Theory]
    [Trait(Traits.Age, Traits.Regression)]
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
    public void CommandLine_will_only_allow_asterisk_once() {
        var sut = new VersonifyCommandline();
        sut.DigitManipulations = new[] { "1", "*", "2", "*" };

        string[] gd = sut.GetDigits();

        gd.Length.ShouldBe(1, "There should only be one digit returned from the command line, even though two were specified.");
        gd[0].ShouldBe("*", "The only digit returned should be an asterisk, as that is the only valid digit in this case.");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Output_environment_selected_works() {
        b.Info.Flow();

        var sut = new VersonifyCommandline {
            OutputOptions = "env"
        };

        Assert.True((sut.OutputsActive & OutputPossibilities.Environment) == OutputPossibilities.Environment);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Output_file_selected_works() {
        b.Info.Flow();

        var sut = new VersonifyCommandline {
            OutputOptions = "file"
        };

        Assert.True((sut.OutputsActive & OutputPossibilities.File) == OutputPossibilities.File);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Output_incorrect_value_throws() {
        b.Info.Flow();

        _ = Assert.Throws<ArgumentOutOfRangeException>(() => {
            var sut = new VersonifyCommandline {
                OutputOptions = "MyIncrediblyWrongArgument"
            };
        });
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Output_defaults_to_none() {
        b.Info.Flow();

        var sut = new VersonifyCommandline();
        Assert.Equal(OutputPossibilities.None, sut.OutputsActive);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Output_null_is_same_as_none() {
        b.Info.Flow();

        var sut = new VersonifyCommandline {
            OutputOptions = ""
        };
        Assert.Equal(OutputPossibilities.None, sut.OutputsActive);

        sut.OutputOptions = null;
        Assert.Equal(OutputPossibilities.None, sut.OutputsActive);
    }
}