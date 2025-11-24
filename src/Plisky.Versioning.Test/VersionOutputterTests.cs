namespace Plisky.CodeCraft.Test;

using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Test;
using Plisky.Versioning;
using Shouldly;
using Versonify;
using Xunit;

public class VersionOutputterTests {
    private readonly Bilge b = new();
    private readonly TestSupport ts;
    private readonly UnitTestHelper uth;

    public VersionOutputterTests() {
        uth = new UnitTestHelper();
        ts = new TestSupport(uth);
    }

    ~VersionOutputterTests() {
        uth.ClearUpTestFiles();
    }

    [Fact(DisplayName = nameof(Args_OutputterParseConsole_Works))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Args_OutputterParseConsole_Works() {
        b.Info.Flow();

        var cla = new VersonifyCommandline {
            OutputOptions = "con"
        };

        Assert.True((cla.OutputsActive & OutputPossibilities.Console) == OutputPossibilities.Console);
    }

    [Theory(DisplayName = nameof(Args_OutputterParseSetsConsoleString))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("con:variable", "%VER%")]
    [InlineData("vsts:variable", "variable")]
    [InlineData("con", "%VER%")]
    [InlineData("vsts:variable", "%VER%")]
    [InlineData("vsts", "%VER%")]
    [InlineData("vsts:bone", "bone")]
    public void Args_OutputterParseSetsConsoleString(string argument, string contains) {
        b.Info.Flow();

        var cla = new VersonifyCommandline {
            OutputOptions = argument
        };

        Assert.Contains(contains, cla.ConsoleTemplate);
    }

    [Fact(DisplayName = nameof(Args_OutputterParseSetsFileName))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Args_OutputterParseSetsFileName() {
        b.Info.Flow();

        string argument = "file:myfile.txt";
        string expectedFilename = "myfile.txt";

        var cla = new VersonifyCommandline {
            OutputOptions = argument
        };

        cla.PverFileName.ShouldBe(expectedFilename);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
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

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Behaviour_output_works_for_console_output_possibility() {
        b.Info.Flow();

        var sut = new CompleteVersion("1.2.3");
        var op = new MockVersioningOutputter(sut);

        op.DoOutput(OutputPossibilities.Console, VersioningCommand.BehaviourOutput);

        op.OutputLines.Length.ShouldBe(3, "There should be three lines of output for the behaviour output command when using Console output.");
        op.OutputLines[0].ShouldStartWith("[0]:");
        op.OutputLines[1].ShouldStartWith("[1]:");
        op.OutputLines[2].ShouldStartWith("[2]:");
    }

    [Theory]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("99", DigitIncrementBehaviour.Fixed, "Fixed", 0)]
    [InlineData("5", DigitIncrementBehaviour.Fixed, "Fixed", 0)]
    [InlineData("68", DigitIncrementBehaviour.AutoIncrementWithReset, "AutoIncrementWithReset", 4)]
    [InlineData("0", DigitIncrementBehaviour.DaysSinceDate, "DaysSinceDate", 2)]
    [InlineData("0", DigitIncrementBehaviour.DailyAutoIncrement, "DailyAutoIncrement", 3)]
    [InlineData("0", DigitIncrementBehaviour.AutoIncrementWithReset, "AutoIncrementWithReset", 4)]
    [InlineData("0", DigitIncrementBehaviour.AutoIncrementWithResetAny, "AutoIncrementWithResetAny", 5)]
    [InlineData("0", DigitIncrementBehaviour.ContinualIncrement, "ContinualIncrement", 6)]
    [InlineData("0", DigitIncrementBehaviour.WeeksSinceDate, "WeeksSinceDate", 7)]
    [InlineData("0", DigitIncrementBehaviour.ReleaseName, "ReleaseName", 8)]
    public void Behaviour_output_works_for_fixed(string value, DigitIncrementBehaviour behaviour, string expectedOutput, int behaviourValue) {
        // Behaviour value is coded into the test here because its on the public interface, change the enum values introduce a breaking change.
        b.Info.Flow();

        var sut = new CompleteVersion(value);
        sut.Digits[0].Behaviour = behaviour;
        var op = new MockVersioningOutputter(sut);
        op.DoOutput(OutputPossibilities.File, VersioningCommand.BehaviourOutput);

        op.OutputLines.Length.ShouldBe(1, "There should be one line of output for the behaviour output command.");
        op.OutputLines[0].ShouldBe($"[0]:{expectedOutput}({behaviourValue})");
    }

    [Theory]
    [Trait(Traits.Age, Traits.Regression)]
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

    [Fact(DisplayName = nameof(Outputter_Environment_WritesToEnvironment))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Outputter_Environment_WritesToEnvironment() {
        b.Info.Flow();

        var mvs = new MockVersionStorage("0.0.0.1");
        var sut = new Versioning(mvs);
        var v = sut.Version;

        var op = new MockVersioningOutputter(v);
        op.DoOutput(OutputPossibilities.Environment, VersioningCommand.PassiveOutput);

        op.FileWasWritten.ShouldBeFalse("FileWasWritten should be false when writing to environment.");
        op.EnvWasSet.ShouldBeTrue("EnvWasSet should be true when writing to environment.");
    }

    [Fact(DisplayName = nameof(Outputter_Environment_WritesToEnvironment))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Outputter_File_WritesToFile() {
        b.Info.Flow();

        var mvs = new MockVersionStorage("0.0.0.1");
        var sut = new Versioning(mvs);
        var v = sut.Version;

        var op = new MockVersioningOutputter(v);
        op.DoOutput(OutputPossibilities.File, VersioningCommand.PassiveOutput);

        op.FileWasWritten.ShouldBeTrue("FileWasWritten should be true when writing to file.");
        op.EnvWasSet.ShouldBeFalse("EnvWasSet should be false when writing to file.");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Outputter_PNF_WritesPNFToConsole() {
        b.Info.Flow();
        string version = "1.1.oranges.0.0";
        string release = "Fruit";
        var mvs = new MockVersionStorage(version);
        var sut = new Versioning(mvs);
        var v = sut.Version;
        v.ReleaseName = release;

        v.ApplyPendingVersion("+...");
        var op = new MockVersioningOutputter(v);
        op.DoOutput(OutputPossibilities.NukeFusion, VersioningCommand.PassiveOutput);
        op.OutputLines.Length.ShouldBe(8, "There should be eight lines of output for the nuke fusion output.");
        op.OutputLines[0].ShouldBe($"PNFV]{version}");
        op.OutputLines[1].ShouldBe($"PNF2]1.1");
        op.OutputLines[2].ShouldBe($"PNF3]1.1.oranges");
        op.OutputLines[3].ShouldBe($"PN3D]1.1.0");
        op.OutputLines[4].ShouldBe($"PNF4]1.1.oranges.0");
        op.OutputLines[5].ShouldBe($"PNQF]2.1.oranges.0.0");
        op.OutputLines[6].ShouldBe($"PN4D]1.1.0.0");
        op.OutputLines[7].ShouldBe($"PNFN]{release}");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.LiveBug, "LFY-30")]
    public void Version_output_information_written_after_file_update() {
        b.Info.Flow();
        string version = "1.0";
        var mvs = new MockVersionStorage(version);
        var sut = new Versioning(mvs);
        var v = sut.Version;

        var op = new MockVersioningOutputter(v);
        op.DoOutput(OutputPossibilities.NukeFusion, VersioningCommand.UpdateFiles);

        op.OutputLines.Length.ShouldBe(8, "The versioning output should be written on file update as well as passive.");
    }
}