using System;
using Plisky.Diagnostics;
using Plisky.Test;
using Plisky.Versioning;
using Shouldly;
using Versonify;
using Xunit;

namespace Plisky.CodeCraft.Test;

public class VersonifyCommandLineTests {
    private readonly Bilge b = new();
    private readonly TestSupport ts;
    private readonly UnitTestHelper uth;

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
    [InlineData("GET", VersioningCommand.GetDigitInformation)]
    [InlineData("CreateVersion", VersioningCommand.CreateNewVersion)]
    [InlineData("override", VersioningCommand.Override)]
    [InlineData("updateFiles", VersioningCommand.UpdateFiles)]
    [InlineData("passIVE", VersioningCommand.PassiveOutput)]
    [InlineData("behaviour", VersioningCommand.BehaviourOutput)]
    [InlineData("get", VersioningCommand.GetDigitInformation)]
    public void CommandLine_correctly_sets_command_from_argument(string commandString, VersioningCommand cmd) {
        var sut = new VersonifyOptions {
            Command = commandString
        };
        sut.RequestedCommand.ShouldBe(cmd, "The command should be set correctly from the command line argument.");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    public void CommandLine_will_only_allow_asterisk_once() {
        var sut = new VersonifyOptions {
            DigitManipulations = [
                "1", "*", "2", "*"
            ]
        };

        string[] result = sut.GetDigits();

        result.Length.ShouldBe(1, "There should only be one digit returned from the command line, even though two were specified.");
        result[0].ShouldBe("*", "The only digit returned should be an asterisk, as that is the only valid digit in this case.");
    }

    [Fact]
    public void CommandLine_will_return_empty_array_when_no_digits_are_specified() {
        var sut = new VersonifyOptions {
            DigitManipulations = []
        };

        string[] result = sut.GetDigits();

        result.Length.ShouldBe(0, "There should be no digits returned from the command line, as none were specified.");
    }

    [Fact]
    public void CommandLine_get_option_requires_double_dash() {
        CommandLineParser.Parse(["get"]).Options.RequestedCommand.ShouldBe(VersioningCommand.GetDigitInformation);
        CommandLineParser.Parse(["--command=get"]).Success.ShouldBeTrue();
        CommandLineParser.Parse(["-get"]).Options.RequestedCommand.ShouldBe(VersioningCommand.Invalid);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Output_defaults_to_none() {
        b.Info.Flow();

        var sut = new VersonifyOptions();
        Assert.Equal(OutputPossibilities.None, sut.OutputsActive);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Output_environment_selected_works() {
        b.Info.Flow();

        var sut = new VersonifyOptions {
            OutputOptions = "env"
        };

        Assert.Equal(OutputPossibilities.Environment, sut.OutputsActive & OutputPossibilities.Environment);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Output_file_selected_works() {
        b.Info.Flow();

        var sut = new VersonifyOptions {
            OutputOptions = "file"
        };

        Assert.Equal(OutputPossibilities.File, sut.OutputsActive & OutputPossibilities.File);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Output_incorrect_value_throws() {
        b.Info.Flow();

        _ = Assert.Throws<ArgumentOutOfRangeException>(() => {
            var sut = new VersonifyOptions {
                OutputOptions = "MyIncrediblyWrongArgument"
            };
        });
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Output_null_is_same_as_none() {
        b.Info.Flow();

        var sut = new VersonifyOptions {
            OutputOptions = ""
        };
        Assert.Equal(OutputPossibilities.None, sut.OutputsActive);

        sut.OutputOptions = null!;
        Assert.Equal(OutputPossibilities.None, sut.OutputsActive);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Output_json_console_selected_works() {
        b.Info.Flow();

        var sut = new VersonifyOptions {
            OutputOptions = "jcon"
        };

        Assert.Equal(OutputPossibilities.Json, sut.OutputsActive & OutputPossibilities.Json);
        Assert.Equal(OutputPossibilities.Console, sut.OutputsActive & OutputPossibilities.Console);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void JsonOutputMessage_defaults_correctly() {
        b.Info.Flow();

        var msg = new JsonOutputMessage {
            MessageContent = "Test message"
        };

        msg.MessageCategory.ShouldBe("information");
        msg.MessageContent.ShouldBe("Test message");
        msg.Meta.ShouldNotBeNull();
        msg.Meta.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("--help", true)]
    [InlineData("-h", true)]
    [InlineData("-H", true)]
    [InlineData("--HELP", true)]
    [InlineData("--get-md-help", false)]
    [InlineData("passive", false)]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void IsHelpRequested_identifies_help_flag_accurately(string argument, bool expected) {
        b.Info.Flow();
        CommandLineParser.IsHelpRequested([argument]).ShouldBe(expected);
    }

    [Fact]
    public void IsHelpRequested_identifies_help_flag_accurately_with_no_arguments() {
        b.Info.Flow();
        CommandLineParser.IsHelpRequested([]).ShouldBeTrue();
    }
}