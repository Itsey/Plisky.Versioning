using Plisky.Versioning;

namespace Plisky.CodeCraft.Test;

using System;
using System.Linq;
using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;
using Versonify;
using Xunit;

public class ExploratoryTests {
    private readonly Bilge b = new();
    private readonly UnitTestHelper uth;
    private readonly TestSupport ts;

    public ExploratoryTests() {
        uth = new UnitTestHelper();
        ts = new TestSupport(uth);
    }

    ~ExploratoryTests() {
        uth.ClearUpTestFiles();
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("1.2.3.4", "1.2.3.4")]
    [InlineData("1.2.3.4.0", "1.2.3.4")]
    [InlineData("1.2.3.4.0.0", "1.2.3.4")]
    [InlineData("1.2", "1.2.0.0")]
    [InlineData("1", "1.0.0.0")]
    [InlineData("1.2-Release.3.4", "1.2.0.0")]
    [InlineData("1-2-3-4", "1.0.0.0")]
    public void Fourdigit_display_only_shows_four_digits(string verStr, string outStr) {
        b.Info.Flow();
        CompleteVersion sut = new(verStr, '.', '-', '+');

        string versionString = sut.GetVersionString(DisplayType.FourDigitNumeric);

        versionString.ShouldBe(outStr);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    public void Commandline_digits_allows_multiple_digits() {
        VersonifyCommandline sut = new();
        sut.DigitManipulations = new[] { "1", "2", "3" };

        string[] gd = sut.GetDigits();

        gd.Length.ShouldBe(3);
        gd[0].ShouldBe("1");
        gd[1].ShouldBe("2");
        gd[2].ShouldBe("3");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    public void Validate_digitoptions_throws_when_invalid_digit_passed() {
        var cv = CompleteVersion.GetDefault();

        Action act = () => { _ = cv.ValidateDigitOptions(new[] { "monkey" }); };

        act.ShouldThrow<ArgumentOutOfRangeException>();
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
        VersonifyCommandline sut = new();
        sut.Command = "behaviour";
        sut.QuickValue = quickValue;

        var result = sut.RequestedCommand;

        result.ShouldBe(cmd);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Integration)]
    [Trait("Cause", "Bug:464")]
    public void Build_version_does_not_update_during_build() {
        string ident = TestResources.GetIdentifiers(TestResourcesReferences.Bug464RefContent);
        string srcFile = uth.GetTestDataFile(ident);
        string fn = ts.GetFileAsTemporary(srcFile);
        CompleteVersion cv = new(new VersionUnit("2"), new VersionUnit("0", "."), new VersionUnit("Unicorn", "-"), new VersionUnit("0", ".", DigitIncrementBehaviour.ContinualIncrement));
        VersionFileUpdater sut = new(cv);

        _ = sut.PerformUpdate(fn, FileUpdateType.NetAssembly);
        _ = sut.PerformUpdate(fn, FileUpdateType.NetInformational);
        _ = sut.PerformUpdate(fn, FileUpdateType.NetFile);

        bool fileVer = ts.DoesFileContainThisText(fn, "AssemblyFileVersion(\"2.0.0\"");
        bool infoVer = ts.DoesFileContainThisText(fn, "AssemblyInformationalVersion(\"2.0-Unicorn.0\"");
        bool asmVer = ts.DoesFileContainThisText(fn, "AssemblyVersion(\"2.0\")");

        fileVer.ShouldBeFalse();
        infoVer.ShouldBeTrue();
        asmVer.ShouldBeTrue();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void IncrementAndUpdateThrowsIfNoDirectory() {
        Action act = () => {
            VersioningTask sut = new();
            sut.IncrementAndUpdateAll();
        };

        act.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void ApplyBehaviourUpdate_UpdatesSingleDigitBehaviour() {
        CompleteVersion version = new(new VersionUnit("1"), new VersionUnit("2"), new VersionUnit("3"));

        bool allFixed = version.Digits.All(d => d.Behaviour == DigitIncrementBehaviour.Fixed);

        allFixed.ShouldBeTrue();

        version.ApplyBehaviourUpdate("1", DigitIncrementBehaviour.AutoIncrementWithReset);

        var d0 = version.Digits[0].Behaviour;
        var d1 = version.Digits[1].Behaviour;
        var d2 = version.Digits[2].Behaviour;

        d0.ShouldBe(DigitIncrementBehaviour.Fixed);
        d1.ShouldBe(DigitIncrementBehaviour.AutoIncrementWithReset);
        d2.ShouldBe(DigitIncrementBehaviour.Fixed);
    }

    [Fact]
    public void ApplyBehaviourUpdate_UpdatesAllDigitsWhenWildcard() {
        CompleteVersion version = new(new VersionUnit("1"), new VersionUnit("2"), new VersionUnit("3"));

        version.ApplyBehaviourUpdate("*", DigitIncrementBehaviour.ContinualIncrement);

        bool allContinual = version.Digits.All(d => d.Behaviour == DigitIncrementBehaviour.ContinualIncrement);

        allContinual.ShouldBeTrue();
    }

    [Theory]
    [Trait("Cause", "Bug:LFY-17")]
    [InlineData("FIXED", DigitIncrementBehaviour.Fixed, true)]
    [InlineData("0", DigitIncrementBehaviour.Fixed, true)]
    [InlineData("2", DigitIncrementBehaviour.DaysSinceDate, true)]
    [InlineData("dailyautoincrement", DigitIncrementBehaviour.DailyAutoIncrement, true)]
    [InlineData("autoIncreMentWithreset", DigitIncrementBehaviour.AutoIncrementWithReset, true)]
    [InlineData("AutoIncrementWithReset", DigitIncrementBehaviour.AutoIncrementWithReset, true)]
    [InlineData("AutoIncrementWithResetAny", DigitIncrementBehaviour.AutoIncrementWithResetAny, true)]
    [InlineData("ContinualIncrement", DigitIncrementBehaviour.ContinualIncrement, true)]
    [InlineData("NotAValidBehaviour", default(DigitIncrementBehaviour), false)]
    [InlineData("", default(DigitIncrementBehaviour), false)]
    public void TryParseDigitIncrementBehaviour_ParsesExpectedValues(string input, DigitIncrementBehaviour expected, bool shouldParse) {
        bool parseResult = VersonifyCommandline.TryParseDigitIncrementBehaviour(input, out var result);

        parseResult.ShouldBe(shouldParse);
        if (shouldParse) {
            result.ShouldBe(expected);
        }
    }

    [Fact]
    public void ApplyValueUpdate_SetsAllDigitsToValue() {
        CompleteVersion version = new(new VersionUnit("1"), new VersionUnit("2"), new VersionUnit("3"));
        string valueToSet = "42";

        version.ApplyValueUpdate("*", valueToSet);

        bool allSet = version.Digits.All(d => d.Value == valueToSet);

        allSet.ShouldBeTrue();
    }

    [Fact]
    public void ApplyValueUpdate_SetsSingleDigitToValue() {
        CompleteVersion version = new(new VersionUnit("1"), new VersionUnit("2"), new VersionUnit("3"));
        string valueToSet = "99";

        version.ApplyValueUpdate("1", valueToSet);

        string d0 = version.Digits[0].Value;
        string d1 = version.Digits[1].Value;
        string d2 = version.Digits[2].Value;

        d0.ShouldBe("1");
        d1.ShouldBe(valueToSet);
        d2.ShouldBe("3");
    }

    [Fact]
    public void ApplyValueUpdate_SetsMultipleDigitsToSameValue() {
        CompleteVersion version = new(new VersionUnit("1"), new VersionUnit("2"), new VersionUnit("3"));
        string valueToSet = "7";

        foreach (string idx in new[] { "0", "2" }) {
            version.ApplyValueUpdate(idx, valueToSet);
        }

        string d0 = version.Digits[0].Value;
        string d1 = version.Digits[1].Value;
        string d2 = version.Digits[2].Value;

        d0.ShouldBe(valueToSet);
        d1.ShouldBe("2");
        d2.ShouldBe(valueToSet);
    }

    [Theory]
    [InlineData(DigitIncrementBehaviour.ReleaseName, "ReleaseA")]
    [InlineData(DigitIncrementBehaviour.Fixed, "123")]
    [InlineData(DigitIncrementBehaviour.DaysSinceDate, "77")]
    public void ApplyValueUpdate_SetsValueForBehaviour(DigitIncrementBehaviour behaviour, string valueToSet) {
        CompleteVersion version = new(new VersionUnit("1", "", behaviour), new VersionUnit("2"));

        version.ApplyValueUpdate("0", valueToSet);

        string d0 = version.Digits[0].Value;

        d0.ShouldBe(valueToSet);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("2")]
    [InlineData("5")]
    public void ApplyValueUpdate_InvalidIndex_Throws(string index) {
        CompleteVersion version = new(new VersionUnit("1"), new VersionUnit("2"));

        Action act = () => version.ApplyValueUpdate(index, "100");

        act.ShouldThrow<Exception>();
    }

    [Theory]
    [InlineData(DigitIncrementBehaviour.DaysSinceDate, "NotANumber")]
    [InlineData(DigitIncrementBehaviour.DailyAutoIncrement, "NotANumber")]
    public void ApplyValueUpdate_NonIntegerValueForIntegerBehaviour_Throws(DigitIncrementBehaviour behaviour, string valueToSet) {
        CompleteVersion version = new(new VersionUnit("1", "", behaviour), new VersionUnit("2"));

        Action act = () => version.ApplyValueUpdate("0", valueToSet);

        act.ShouldThrow<Exception>();
    }

    [Fact]
    public void ApplyValueUpdate_FixedBehaviour_ReleaseNameValue_SetsToReleaseName() {
        CompleteVersion version = new(new VersionUnit("1", "", DigitIncrementBehaviour.Fixed), new VersionUnit("2"));
        string expectedReleaseName = "MyRelease";
        version.ReleaseName = expectedReleaseName;

        version.ApplyValueUpdate("0", "ReleaseName");

        string d0 = version.Digits[0].Value;

        d0.ShouldBe(expectedReleaseName);
    }

    [Fact]
    public void Outputter_ValToWrite_IsReleaseName_WhenReleaseRequestedTrue() {
        CompleteVersion version = new(new VersionUnit("1"), new VersionUnit("2"));
        string expectedReleaseName = "ReleaseX";
        version.ReleaseName = expectedReleaseName;

        MockVersioningOutputter outputter = new(version) { ReleaseRequested = true };

        string valToWrite = outputter.GetTheValueRequestedToWrite();

        valToWrite.ShouldBe(expectedReleaseName);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(null, "")]
    [InlineData("myNewRelease", "myNewRelease")]
    [InlineData("release123", "release123")]
    public void Outputter_File_WritesReleaseNameToFile(string releaseName, string expectedOutput) {
        b.Info.Flow();

        MockVersionStorage mvs = new("0.0.0.1");
        Versioning sut = new(mvs);
        var v = sut.Version;
        v.ReleaseName = releaseName;

        MockVersioningOutputter op = new(v) { ReleaseRequested = true };

        op.DoOutput(OutputPossibilities.File, VersioningCommand.PassiveOutput);

        bool fileWasWritten = op.FileWasWritten;
        bool envWasSet = op.EnvWasSet;
        string valToWrite = op.GetTheValueRequestedToWrite();

        fileWasWritten.ShouldBeTrue();
        envWasSet.ShouldBeFalse();
        valToWrite.ShouldBe(expectedOutput);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Outputter_Console_Writes_Correct_Output(bool releaseRequest) {
        b.Info.Flow();

        string releaseName = "testReleaseName";
        string versionNumber = "2.0.1.0";

        MockVersionStorage mvs = new(versionNumber);
        Versioning sut = new(mvs);
        var v = sut.Version;
        v.ReleaseName = releaseName;
        MockVersioningOutputter op = new(v) { ReleaseRequested = releaseRequest };
        string expectedOutput = releaseRequest ? releaseName : versionNumber;

        op.DoOutput(OutputPossibilities.File, VersioningCommand.PassiveOutput);

        bool fileWasWritten = op.FileWasWritten;
        string valToWrite = op.GetTheValueRequestedToWrite();

        fileWasWritten.ShouldBeTrue();
        valToWrite.ShouldBe(expectedOutput);
    }

    [Fact]
    public void Outputter_Environment_WritesReleaseNameToEnvironment() {
        b.Info.Flow();

        MockVersionStorage mvs = new("0.0.0.1");
        Versioning sut = new(mvs);
        var v = sut.Version;
        v.ReleaseName = "testReleaseName";
        MockVersioningOutputter op = new(v) { ReleaseRequested = true };

        op.DoOutput(OutputPossibilities.Environment, VersioningCommand.PassiveOutput);

        bool fileWasWritten = op.FileWasWritten;
        bool envWasSet = op.EnvWasSet;
        string valToWrite = op.GetTheValueRequestedToWrite();

        fileWasWritten.ShouldBeFalse();
        envWasSet.ShouldBeTrue();
        valToWrite.ShouldBe("testReleaseName");
    }

    [Theory]
    [InlineData("releaseA", "NewRelease")]
    [InlineData(null, "ImNoLongerNull")]
    [InlineData("", "ImNoLongerEmpty")]
    [InlineData("releaseA", "release space123")]
    public void SetReleaseNameCommand_SetsReleaseNameInVersionStore(string currentRelease, string newRelease) {
        CompleteVersion cv = new("1.2.3.4") { ReleaseName = currentRelease };

        cv.SetReleaseName(newRelease);

        string releaseName = cv.ReleaseName;

        releaseName.ShouldBe(newRelease);
    }

    [Fact]
    public void SetReleaseNameCommand_SetsCorrectly() {
        string releaseName = "QuantumBanana";
        VersonifyCommandline cmd = new() {
            Command = "set",
            Release = releaseName
        };

        var requestedCommand = cmd.RequestedCommand;

        requestedCommand.ShouldBe(VersioningCommand.SetReleaseName);
    }

    [Fact]
    public void SetPrefixForDigit_SetsPrefixForAllDigitsWithWildcard() {
        var version = new CompleteVersion(
            new VersionUnit("1"),
            new VersionUnit("2"),
            new VersionUnit("3")
        );
        version.SetPrefixForDigit("*", "+");
        foreach (var digit in version.Digits) {
            digit.PreFix.ShouldBe("+", "Prefix should be set to '+' for all digits.");
        }
    }

    [Fact]
    public void SetPrefixForDigit_ThrowsOnInvalidIndex() {
        var version = new CompleteVersion(
            new VersionUnit("1"),
            new VersionUnit("2")
        );
        Should.Throw<Exception>(() => version.SetPrefixForDigit("5", "!"), "Should throw for invalid digit index '5'.");
    }

    [Theory]
    [InlineData("-", "-")]
    [InlineData(".", ".")]
    [InlineData("", "")]
    [InlineData(" ", " ")]
    [InlineData("#", "#")]
    [InlineData("ver-", "ver-")]
    public void SetPrefixForDigit_SetsPrefixForSingleDigit(string prefixToSet, string expectedPrefix) {
        var version = new CompleteVersion(
            new VersionUnit("1"),
            new VersionUnit("2"),
            new VersionUnit("3"),
            new VersionUnit("4")
        );
        version.Digits[2].PreFix.ShouldBe("", "Prefix should initially be empty for digit[2].");

        version.SetPrefixForDigit("2", prefixToSet);

        version.Digits[2].PreFix.ShouldBe(expectedPrefix, $"Prefix should be set to '{expectedPrefix}' for digit[2].");
    }
}