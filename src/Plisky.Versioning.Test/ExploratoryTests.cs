using Plisky.Versioning;

namespace Plisky.CodeCraft.Test;

using System;
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
        _ = Assert.Throws<InvalidOperationException>(() => {
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
        var version = new CompleteVersion(
            new VersionUnit("1"),
            new VersionUnit("2"),
            new VersionUnit("3")
        );
        string valueToSet = "42";

        version.ApplyValueUpdate("*", valueToSet);

        version.Digits.ShouldAllBe(d => d.Value == valueToSet, $"All digits should be set to '{valueToSet}' after wildcard value update.");
    }

    [Fact]
    public void ApplyValueUpdate_SetsSingleDigitToValue() {
        var version = new CompleteVersion(
            new VersionUnit("1"),
            new VersionUnit("2"),
            new VersionUnit("3")
        );
        string valueToSet = "99";

        version.ApplyValueUpdate("1", valueToSet);

        version.Digits[0].Value.ShouldBe("1", "First digit should remain unchanged after single digit update.");
        version.Digits[1].Value.ShouldBe(valueToSet, $"Second digit should be updated to '{valueToSet}'.");
        version.Digits[2].Value.ShouldBe("3", "Third digit should remain unchanged after single digit update.");
    }

    [Fact]
    public void ApplyValueUpdate_SetsMultipleDigitsToSameValue() {
        var version = new CompleteVersion(
            new VersionUnit("1"),
            new VersionUnit("2"),
            new VersionUnit("3")
        );
        string valueToSet = "7";

        foreach (string idx in new[] { "0", "2" }) {
            version.ApplyValueUpdate(idx, valueToSet);
        }

        version.Digits[0].Value.ShouldBe(valueToSet, $"First digit should be updated to '{valueToSet}'.");
        version.Digits[1].Value.ShouldBe("2", "Second digit should remain unchanged after multiple digit update.");
        version.Digits[2].Value.ShouldBe(valueToSet, $"Third digit should be updated to '{valueToSet}'.");
    }

    [Theory]
    [InlineData(DigitIncrementBehaviour.ReleaseName, "ReleaseA")]
    [InlineData(DigitIncrementBehaviour.Fixed, "123")]
    [InlineData(DigitIncrementBehaviour.DaysSinceDate, "77")]
    public void ApplyValueUpdate_SetsValueForBehaviour(DigitIncrementBehaviour behaviour, string valueToSet) {
        var version = new CompleteVersion(
            new VersionUnit("1", "", behaviour),
            new VersionUnit("2")
        );

        version.ApplyValueUpdate("0", valueToSet);

        version.Digits[0].Value.ShouldBe(valueToSet, $"Digit with behaviour {behaviour} should be set to '{valueToSet}'.");
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("2")]
    [InlineData("5")]
    public void ApplyValueUpdate_InvalidIndex_Throws(string index) {
        var version = new CompleteVersion(
            new VersionUnit("1"),
            new VersionUnit("2")
        );

        Should.Throw<Exception>(() => version.ApplyValueUpdate(index, "100"), $"Should throw for invalid digit index '{index}'.");
    }

    [Theory]
    [InlineData(DigitIncrementBehaviour.DaysSinceDate, "NotANumber")]
    [InlineData(DigitIncrementBehaviour.DailyAutoIncrement, "NotANumber")]
    public void ApplyValueUpdate_NonIntegerValueForIntegerBehaviour_Throws(DigitIncrementBehaviour behaviour, string valueToSet) {
        var version = new CompleteVersion(
            new VersionUnit("1", "", behaviour),
            new VersionUnit("2")
        );

        Should.Throw<Exception>(() => version.ApplyValueUpdate("0", valueToSet), $"Non-integer value '{valueToSet}' provided for behaviour '{behaviour}' should throw an exception.");
    }

    [Fact]
    public void ApplyValueUpdate_FixedBehaviour_ReleaseNameValue_SetsToReleaseName() {
        var version = new CompleteVersion(
            new VersionUnit("1", "", DigitIncrementBehaviour.Fixed),
            new VersionUnit("2")
        );
        string expectedReleaseName = "MyRelease";
        version.ReleaseName = expectedReleaseName;

        version.ApplyValueUpdate("0", "ReleaseName");

        version.Digits[0].Value.ShouldBe(expectedReleaseName, "Digit with Fixed behaviour and value 'ReleaseName' should be set to the ReleaseName property value.");
    }
}