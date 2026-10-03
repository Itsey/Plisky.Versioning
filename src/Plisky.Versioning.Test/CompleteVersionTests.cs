namespace Plisky.CodeCraft.Test;

using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;
using Xunit;

public class CompleteVersionTests {
    private readonly Bilge b = new();
    private readonly TestSupport ts;
    private readonly UnitTestHelper uth;

    public CompleteVersionTests() {
        uth = new UnitTestHelper();
        ts = new TestSupport(uth);
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [InlineData("1.2-3.4", "", ".", "-", ".")]
    [InlineData("1.2.3.4", "", ".", ".", ".")]
    [InlineData("1-2-3-4", "", "-", "-", "-")]
    [InlineData("1-2+3.4", "", "-", "+", ".")]
    public void CompletedVerison_constructor_sets_prefix(string initialValue, string dg1, string dg2, string dg3, string dg4) {
        b.Info.Flow();
        var sut = new CompleteVersion(initialValue, '.', '-', '+');

        string? p0 = sut.Digits.Length > 0 ? sut.Digits[0].PreFix : null;
        string? p1 = sut.Digits.Length > 1 ? sut.Digits[1].PreFix : null;
        string? p2 = sut.Digits.Length > 2 ? sut.Digits[2].PreFix : null;
        string? p3 = sut.Digits.Length > 3 ? sut.Digits[3].PreFix : null;

        p0.ShouldBe(dg1);
        p1.ShouldBe(dg2);
        p2.ShouldBe(dg3);
        p3.ShouldBe(dg4);
    }

    [Theory(DisplayName = nameof(CompletedVersion_constructor_parses_correct_digitcount))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("0.0.0.0", 4)]
    [InlineData("9086334.2345.1234.111", 4)]
    [InlineData("94.0.0.0", 4)]
    [InlineData("94.0", 2)]
    [InlineData("94.0.1", 3)]
    [InlineData("94", 1)]
    public void CompletedVersion_constructor_parses_correct_digitcount(string initString, int expectedDigits) {
        b.Info.Flow();

        var cv = new CompleteVersion(initString);

        cv.Digits.Length.ShouldBe(expectedDigits);
    }

    [Theory]
    [InlineData(FileUpdateType.NetAssembly, DisplayType.FourDigitNumeric)]
    [InlineData(FileUpdateType.NetFile, DisplayType.FourDigitNumeric)]
    [InlineData(FileUpdateType.NetInformational, DisplayType.Full)]
    [InlineData(FileUpdateType.Wix, DisplayType.Full)]
    [InlineData(FileUpdateType.Nuspec, DisplayType.Full)]
    [InlineData(FileUpdateType.StdAssembly, DisplayType.FourDigitNumeric)]
    [InlineData(FileUpdateType.StdFile, DisplayType.FourDigitNumeric)]
    [InlineData(FileUpdateType.StdInformational, DisplayType.Full)]
    [InlineData(FileUpdateType.TextFile, DisplayType.Short)]
    public void CompleteVersion_constructor_adds_DisplayTypes(FileUpdateType fut, DisplayType dt) {
        b.Info.Flow();

        var cv = new CompleteVersion();

        cv.DisplayTypes.ShouldNotBeNull();
        cv.DisplayTypes.ShouldContainKeyAndValue(fut, dt);
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [InlineData("1.2-3.4", "1", "2", "3", "4")]
    [InlineData("1.2.3.4", "1", "2", "3", "4")]
    [InlineData("1-2-3-4", "1", "2", "3", "4")]
    [InlineData("1-2+3.4", "1", "2", "3", "4")]
    [InlineData("1234", "1234", null, null, null)]
    [InlineData("12.34", "12", "34", null, null)]
    [InlineData("12.3-4", "12", "3", "4", null)]
    public void Completeversion_constructor_sets_digit_values(string initialValue, string dg1, string? dg2, string? dg3, string? dg4) {
        b.Info.Flow();
        var sut = new CompleteVersion(initialValue, '.', '-', '+');

        string? d0 = sut.Digits.Length > 0 ? sut.Digits[0].Value : null;
        string? d1 = sut.Digits.Length > 1 ? sut.Digits[1].Value : null;
        string? d2 = sut.Digits.Length > 2 ? sut.Digits[2].Value : null;
        string? d3 = sut.Digits.Length > 3 ? sut.Digits[3].Value : null;

        d0.ShouldBe(dg1);

        if (dg2 != null) {
            d1.ShouldBe(dg2);
        } else {
            sut.Digits.Length.ShouldBeLessThan(2);
        }

        if (dg3 != null) {
            d2.ShouldBe(dg3);
        } else {
            sut.Digits.Length.ShouldBeLessThan(3);
        }

        if (dg4 != null) {
            d3.ShouldBe(dg4);
        } else {
            sut.Digits.Length.ShouldBeLessThan(4);
        }
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void DisplayType_DefaultsToFull() {
        var sut = new CompleteVersion(
           new VersionUnit("1", ""),
           new VersionUnit("0", "."),
           new VersionUnit("1", "."),
           new VersionUnit("0", ".", DigitIncrementBehaviour.AutoIncrementWithResetAny));

        string full = sut.GetVersionString(DisplayType.Full);
        string def = sut.GetVersionString();

        def.ShouldBe(full);
    }

    [Theory]
    [InlineData("1.2.3.4", "+.+.+.+", "2.3.4.5")]
    [InlineData("1.2..4", "+.+..+", "2.3..5")]
    [InlineData(null, "....", "")]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void DisplayType_QueuedFull_WorksCorrectly(string? startVer, string pattern, string endVer) {
        b.Info.Flow();
        var cv = new CompleteVersion(startVer ?? string.Empty, '.');

        cv.ApplyPendingVersion(pattern);
        string output = cv.GetVersionString(DisplayType.QueuedFull);

        output.ShouldBe(endVer);
    }

    [Theory(DisplayName = nameof(DisplayTypes_WorkCorrectly))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("1.1.1.1", "1.1.1.1", DisplayType.Full)]
    [InlineData("1.9.0.0", "1.9.0.0", DisplayType.Full)]
    [InlineData("1.0", "1.0", DisplayType.Full)]
    [InlineData("1000.1000.1000.1000", "1000.1000.1000.1000", DisplayType.Full)]
    [InlineData("1.1.1.1", "1.1", DisplayType.Short)]
    [InlineData("1.9.0.0", "1.9", DisplayType.Short)]
    [InlineData("1.0", "1.0", DisplayType.Short)]
    [InlineData("1000.1000.1000.1000", "1000.1000", DisplayType.Short)]
    [InlineData("1.1.1.1", "1.1.1", DisplayType.ThreeDigit)]
    [InlineData("1.9.0.0", "1.9.0", DisplayType.ThreeDigit)]
    [InlineData("1.0", "1.0", DisplayType.ThreeDigit)]
    [InlineData("1000.1000.1000.1000", "1000.1000.1000", DisplayType.ThreeDigit)]
    [InlineData("1.2.3.4", "1.2.3.4", DisplayType.FourDigitNumeric)]
    [InlineData("1000.20004", "1000.20004.0.0", DisplayType.FourDigitNumeric)]
    [InlineData("1001-20A004", "1001.0.0.0", DisplayType.FourDigitNumeric)]
    [InlineData("1", "1.0.0.0", DisplayType.FourDigitNumeric)]
    [InlineData("1.sometext.1.1", "1.0.0.0", DisplayType.FourDigitNumeric)]
    [InlineData("1.2.3.4", "1.2.3", DisplayType.ThreeDigitNumeric)]
    [InlineData("1", "1.0.0", DisplayType.ThreeDigitNumeric)]
    [InlineData("1001-20A004", "1001.0.0", DisplayType.ThreeDigitNumeric)]
    [InlineData("1001-20A004.1.1", "1001-20A004.1.1", DisplayType.FourDigit)]
    [InlineData("1.2.3.4.5", "1.2.3.4", DisplayType.FourDigit)]
    [InlineData("1.0", "1.0", DisplayType.FourDigit)]
    public void DisplayTypes_WorkCorrectly(string version, string expectedDisplay, DisplayType dtype) {
        b.Info.Flow();

        var cv = new CompleteVersion(version, '.', '-');

        string output = cv.GetVersionString(dtype);

        output.ShouldBe(expectedDisplay);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void FixedBehaviour_DoesNotIncrement() {
        var sut = new CompleteVersion(new VersionUnit("2"), new VersionUnit("0", "."));

        string before = sut.GetVersionString(DisplayType.Full);

        sut.Increment();

        string after = sut.GetVersionString(DisplayType.Full);

        after.ShouldBe(before);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void GetBehaviourString_ReturnsCorrectBehaviourForSingleDigit() {
        const DigitIncrementBehaviour BEHAVIOUR = DigitIncrementBehaviour.ContinualIncrement;
        const int BEHAVIOURVALUE = (int)BEHAVIOUR;
        string expectedResult = $"[0]:{BEHAVIOUR}({BEHAVIOURVALUE})";

        var vu = new VersionUnit("1", "", BEHAVIOUR);
        var sut = new CompleteVersion(vu);

        string result = sut.GetBehaviourString("0");

        result.ShouldBe(expectedResult);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void GetBehaviourString_ReturnsCorrectBehaviourForStar() {
        const DigitIncrementBehaviour BEHAVIOURFIXED = DigitIncrementBehaviour.Fixed;
        const int BEHAVIOURFIXEDVALUE = (int)BEHAVIOURFIXED;
        const DigitIncrementBehaviour BEHAVIOURINC = DigitIncrementBehaviour.ContinualIncrement;
        const int BEHAVIOURINCVALUE = (int)BEHAVIOURINC;
        string expectedResult =
            $"[0]:{BEHAVIOURFIXED}({BEHAVIOURFIXEDVALUE})\r\n" +
            $"[1]:{BEHAVIOURINC}({BEHAVIOURINCVALUE})\r\n" +
            $"[2]:{BEHAVIOURINC}({BEHAVIOURINCVALUE})";

        var vu1 = new VersionUnit("1", "");
        var vu2 = new VersionUnit("0", ".", BEHAVIOURINC);
        var vu3 = new VersionUnit("1", ".", BEHAVIOURINC);
        var sut = new CompleteVersion(vu1, vu2, vu3);

        string result = sut.GetBehaviourString("*");

        result.ShouldBe(expectedResult);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Increment_OverrideReplacesIncrement() {
        var vu = new VersionUnit("1", "", DigitIncrementBehaviour.ContinualIncrement) {
            IncrementOverride = "9"
        };
        var sut = new CompleteVersion(vu);

        sut.Increment();

        string result = sut.GetVersionString(DisplayType.Full);

        result.ShouldBe("9");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Increment_OverrideWorksForNames() {
        var vu = new VersionUnit("Monkey") {
            IncrementOverride = "Fish"
        };

        var sut = new CompleteVersion(vu);

        sut.Increment();

        string result = sut.GetVersionString(DisplayType.Full);

        result.ShouldBe("Fish");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Increment_OverrideWorksForNumbers() {
        var vu = new VersionUnit("1") {
            IncrementOverride = "5"
        };

        var sut = new CompleteVersion(vu);

        sut.Increment();

        string result = sut.GetVersionString(DisplayType.Full);

        result.ShouldBe("5");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Increment_OverrideWorksOnFixed() {
        var vu = new VersionUnit("1", "", DigitIncrementBehaviour.Fixed) {
            IncrementOverride = "Fish"
        };

        var sut = new CompleteVersion(vu);

        sut.Increment();

        string result = sut.GetVersionString(DisplayType.Full);

        result.ShouldBe("Fish");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Increment_ResentAnyWorks() {
        var vu2 = new VersionUnit("0", ".");
        var sut = new CompleteVersion(
           new VersionUnit("1", ""),
           vu2,
           new VersionUnit("1", "."),
           new VersionUnit("0", ".", DigitIncrementBehaviour.AutoIncrementWithResetAny));

        string before = sut.GetVersionString();

        before.ShouldBe("1.0.1.0");

        sut.Increment();

        string after = sut.GetVersionString();

        after.ShouldBe("1.0.1.1");

        vu2.IncrementOverride = "5";
        sut.Increment();

        string afterOverride = sut.GetVersionString();

        afterOverride.ShouldBe("1.5.1.0");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Increment_ResetAnyWorks() {
        var sut = new CompleteVersion(
            new VersionUnit("1", "", DigitIncrementBehaviour.ContinualIncrement),
            new VersionUnit("0", "."),
            new VersionUnit("1", "."),
            new VersionUnit("0", ".", DigitIncrementBehaviour.AutoIncrementWithResetAny));

        sut.Increment();

        string result = sut.GetVersionString(DisplayType.Full);

        result.ShouldBe("2.0.1.0");
    }

    [Theory(DisplayName = "ManipulateVersionTests")]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("1", "+", "2")]
    [InlineData("1", "-", "0")]
    [InlineData("1", "1", "1")]
    [InlineData("1", "2", "2")]
    [InlineData("1", "alpha", "alpha")]
    [InlineData("1", "brav+o", "brav+o")]
    [InlineData("3", "+", "4")]
    [InlineData("9", "", null)]
    [InlineData("9", "6", "6")]
    [InlineData("bannana", "pEEl", "pEEl")]
    public void ManipulateVersionTests(string value, string pattern, string? result) {
        var sut = new CompleteVersionMock();

        string? res = sut.Mock.ManipulateVersionBasedOnPattern(pattern, value);

        res.ShouldBe(result);
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData(DisplayType.FourDigitNumeric, "2.3.0.0")]
    [InlineData(DisplayType.ThreeDigitNumeric, "2.3.0")]
    public void NumericDisplay_grouped_digits_are_excluded_even_when_numeric_and_dot_prefixed(DisplayType dtype, string expected) {
        b.Info.Flow();

        var cv = new CompleteVersion("2.3.5.7", '.');
        cv.Digits[2].GroupName = "pre-release";
        cv.Digits[3].GroupName = "pre-release";

        string output = cv.GetVersionString(dtype);

        output.ShouldBe(expected);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Override_NoIncrement_DoesNotChangeValue() {
        var vu = new VersionUnit("1") {
            Value = "Monkey",
            IncrementOverride = "Fish"
        };
        var sut = new CompleteVersion(vu);

        string result = sut.GetVersionString(DisplayType.Full);

        result.ShouldBe("Monkey");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void PartiallyFixed_DoesIncrement() {
        var sut = new CompleteVersion(new VersionUnit("1", "", DigitIncrementBehaviour.ContinualIncrement), new VersionUnit("Monkey", "."));

        sut.Increment();

        string result = sut.GetVersionString(DisplayType.Full);

        result.ShouldBe("2.Monkey");
    }

    [Theory(DisplayName = nameof(PendingIncrement_IsAppliedCorrectly))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("1.0.0.0", "+...0", "2", null, null, "0")]
    [InlineData("1.0.0.0", "1.0.0.0", "1", "0", "0", "0")]
    [InlineData("1.0.0.0", "+.+.+.+", "2", "1", "1", "1")]
    [InlineData("2.2.2.2", "-.-.-.-", "1", "1", "1", "1")]
    [InlineData("2.2.2.2", "-..-.", "1", null, "1", null)]
    [InlineData("2.2.2.2", "...", null, null, null, null)]
    [InlineData("2.2.2.2", "..Bealzebub.-", null, null, "Bealzebub", "1")]
    [InlineData("2.2.2.2", "Unicorn.Peach.Applie.Pear", "Unicorn", "Peach", "Applie", "Pear")]
    public void PendingIncrement_IsAppliedCorrectly(string startVer, string pattern, string? d1Expected, string? d2Expected, string? d3Expected, string? d4Expected) {
        b.Info.Flow();

        var cv = new CompleteVersion(startVer);

        cv.ApplyPendingVersion(pattern);

        string? d1 = cv.Digits[0].IncrementOverride;
        string? d2 = cv.Digits[1].IncrementOverride;
        string? d3 = cv.Digits[2].IncrementOverride;
        string? d4 = cv.Digits[3].IncrementOverride;

        d1.ShouldBe(d1Expected);
        d2.ShouldBe(d2Expected);
        d3.ShouldBe(d3Expected);
        d4.ShouldBe(d4Expected);
    }

    [Theory(DisplayName = nameof(PendingIncrement_IsRemovedCorrectly))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("1.0.0.0", "+.0.0.0")]
    [InlineData("1.0.0.0", "1.0.0.0")]
    [InlineData("1.0.0.0", "+.+.+.+")]
    [InlineData("2.2.2.2", "-.-.-.-")]
    [InlineData("2.2.2.2", "-..-.-")]
    [InlineData("2.2.2.2", "...")]
    [InlineData("2.2.2.2", "..Bealzebub.-")]
    [InlineData("2.2.2.2", "Unicorn.Peach.Applie.Pear")]
    public void PendingIncrement_IsRemovedCorrectly(string startVer, string pattern) {
        b.Info.Flow();

        var cv = new CompleteVersion(startVer);

        cv.ApplyPendingVersion(pattern);
        cv.Increment();

        string? d1 = cv.Digits[0].IncrementOverride;
        string? d2 = cv.Digits[1].IncrementOverride;
        string? d3 = cv.Digits[2].IncrementOverride;
        string? d4 = cv.Digits[3].IncrementOverride;

        d1.ShouldBeNull();
        d2.ShouldBeNull();
        d3.ShouldBeNull();
        d4.ShouldBeNull();
    }

    [Theory(DisplayName = nameof(PendingIncrementPatterns_Work))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("1.0.0.0", "+.0.0.0", "2.0.0.0")]
    [InlineData("1.0.0.0", "1.0.0.0", "1.0.0.0")]
    [InlineData("1.0.0.0", "0.0.0.0", "0.0.0.0")]
    [InlineData("1.0.0.0", "+.+.+.+", "2.1.1.1")]
    [InlineData("2.2.2.2", "-.-.-.-", "1.1.1.1")]
    [InlineData("2.2.2.2", "-..-.-", "1.2.1.1")]
    [InlineData("2.2.2.2", "...", "2.2.2.2")]
    [InlineData("2.2.2.2", "..Bealzebub.-", "2.2.Bealzebub.1")]
    [InlineData("2.2.2.2", "Unicorn.Peach.Applie.Pear", "Unicorn.Peach.Applie.Pear")]
    [InlineData("2.Pear.Apple", ".0.0", "2.0.0")]
    [InlineData(null, ".0.0", "")]
    public void PendingIncrementPatterns_Work(string? startVer, string pattern, string endVer) {
        b.Info.Flow();

        var cv = new CompleteVersion(startVer ?? string.Empty);

        cv.ApplyPendingVersion(pattern);
        cv.Increment();

        string result = cv.ToString();

        result.ShouldBe(endVer);
    }

    [Theory(DisplayName = nameof(PendingIncrements_StackCorrectly))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("1.0.0.0", "+.0.0.0", ".+.0.0", "2.1.0.0")]
    [InlineData("1.1.1.1", "0.+.+.0", "0.-.-.0", "0.0.0.0")]
    [InlineData("1.0.0.0", "+.+.+.+", "+.+.+.+", "2.1.1.1")]
    [InlineData("2.2.2.2", "-.-.-.-", "-.-.-.-", "1.1.1.1")]
    [InlineData("2.2.2.2", "-..-.-", "-..-.-", "1.2.1.1")]
    [InlineData("2.2.2.2", "...", "...", "2.2.2.2")]
    [InlineData("2.2.2.2", "..Bealzebub.-", "..Demon.", "2.2.Demon.1")]
    [InlineData("2.2.2.2", "Unicorn.Peach.Applie.Pear", "..Berry.", "Unicorn.Peach.Berry.Pear")]
    public void PendingIncrements_StackCorrectly(string startVer, string pattern, string secondPattern, string endVer) {
        b.Info.Flow();
        var cv = new CompleteVersion(startVer);

        cv.ApplyPendingVersion(pattern);
        cv.ApplyPendingVersion(secondPattern);
        cv.Increment();

        string result = cv.ToString();

        result.ShouldBe(endVer);
    }

    [Fact(DisplayName = nameof(ReleaseVersion_StartsEmpty))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void ReleaseVersion_StartsEmpty() {
        b.Info.Flow();

        var sut = new CompleteVersion(new VersionUnit("2"), new VersionUnit("0", "."));

        string? releaseName = sut.ReleaseName;

        releaseName.ShouldBeNull();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void SimpleIncrement_Fixed_DoesNothing() {
        var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("2", "."));

        sut.Increment();

        string result = sut.ToString();

        result.ShouldBe("1.2");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void SimpleIncrement_Incrment_Works() {
        var vu = new VersionUnit("2", ".");
        vu.SetBehaviour(DigitIncrementBehaviour.AutoIncrementWithReset);
        var sut = new CompleteVersion(new VersionUnit("1"), vu);

        sut.Increment();

        string result = sut.ToString();

        result.ShouldBe("1.3");
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("*", true)]
    [InlineData("0", true)]
    [InlineData("1", true)]
    public void ValidateDigitOptions_ReturnsTrueForValidInput(string digitInput, bool expectedResult) {
        b.Info.Flow();
        var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("0", "."));

        bool actualResult = sut.ValidateDigitOptions([digitInput]);

        actualResult.ShouldBe(expectedResult);
    }

    public class DigitGroups {

        [Fact]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void IncrementByGroup_defaults_to_unnamed_group_only() {
            var sut = new CompleteVersion(
                new VersionUnit("1", "", DigitIncrementBehaviour.ContinualIncrement),
                new VersionUnit("2", ".", DigitIncrementBehaviour.ContinualIncrement),
                new VersionUnit("3", ".", DigitIncrementBehaviour.ContinualIncrement),
                new VersionUnit("4", ".", DigitIncrementBehaviour.ContinualIncrement));
            sut.Digits[2].GroupName = "pre-release";

            sut.IncrementByGroup(string.Empty);

            sut.GetVersionString().ShouldBe("2.3.3.5");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void IncrementByGroup_named_group_only_increments_target_group() {
            var sut = new CompleteVersion(
                new VersionUnit("2", "", DigitIncrementBehaviour.ContinualIncrement),
                new VersionUnit("3", ".", DigitIncrementBehaviour.ContinualIncrement),
                new VersionUnit("3", ".", DigitIncrementBehaviour.ContinualIncrement),
                new VersionUnit("5", ".", DigitIncrementBehaviour.ContinualIncrement));
            sut.Digits[2].GroupName = "pre-release";

            sut.IncrementByGroup("pre-release");

            sut.GetVersionString().ShouldBe("2.3.4.5");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void IncrementByGroup_wildcard_increments_all_groups() {
            var sut = new CompleteVersion(
                new VersionUnit("2", "", DigitIncrementBehaviour.ContinualIncrement),
                new VersionUnit("3", ".", DigitIncrementBehaviour.ContinualIncrement),
                new VersionUnit("4", ".", DigitIncrementBehaviour.ContinualIncrement),
                new VersionUnit("5", ".", DigitIncrementBehaviour.ContinualIncrement));
            sut.Digits[2].GroupName = "pre-release";

            sut.IncrementByGroup("*");

            sut.GetVersionString().ShouldBe("3.4.5.6");
        }

        [Theory]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("   ", "")]
        [InlineData("default", "")]
        [InlineData(" DEFAULT ", "")]
        [InlineData("pre-release", "pre-release")]
        [InlineData("  pre-release  ", "pre-release")]
        public void NormalizeDigitGroup_when_called_returns_expected_value(string? input, string expected) {
            string result = CompleteVersion.NormalizeDigitGroup(input);

            result.ShouldBe(expected);
        }

        [Fact]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Passive_can_select_multiple_groups_in_original_order() {
            var sut = new CompleteVersion("1.2.3.4");
            sut.Digits[2].GroupName = "pre-release";

            string result = sut.GetVersionStringByGroup("default,pre-release");

            result.ShouldBe("1.2.3.4");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Passive_can_select_named_group_only() {
            var sut = new CompleteVersion("1.2.3.4");
            sut.Digits[2].GroupName = "pre-release";

            string result = sut.GetVersionStringByGroup("pre-release");

            result.ShouldBe(".3");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Passive_defaults_to_unnamed_group_only() {
            var sut = new CompleteVersion("1.2.3.4");
            sut.Digits[2].GroupName = "pre-release";

            string result = sut.GetVersionStringByGroup(string.Empty);

            result.ShouldBe("1.2.4");
        }

        [Theory]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        [InlineData(DisplayType.ThreeDigit, "2.3-Alpha.1")]
        [InlineData(DisplayType.FourDigit, "2.3-Alpha.1")]
        public void VersionString_all_grouped_digits_produces_empty_main_and_all_grouped(DisplayType displayType, string expected) {
            var sut = new CompleteVersion("2.3-Alpha.1", '.', '-');
            sut.Digits[0].GroupName = "pre-release";
            sut.Digits[1].GroupName = "pre-release";
            sut.Digits[2].GroupName = "pre-release";
            sut.Digits[3].GroupName = "pre-release";

            string result = sut.GetVersionString(displayType);

            result.ShouldBe(expected);
        }

        [Theory]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        [InlineData(DisplayType.ThreeDigit, "2.3.1-Alpha.9")]
        [InlineData(DisplayType.FourDigit, "2.3.1.8-Alpha.9")]
        public void VersionString_grouped_digits_are_selected_by_membership_not_suffix(DisplayType displayType, string expected) {
            var sut = new CompleteVersion("2.3-Alpha.1.9.8", '.', '-');
            sut.Digits[2].GroupName = "my-custom-pr";
            sut.Digits[4].GroupName = "my-custom-pr";

            string result = sut.GetVersionString(displayType);

            result.ShouldBe(expected);
        }
    }

    public class DisplayTypes {

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Default_display_is_correct_for_two_digits() {
            var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("0", "."));

            string result = sut.ToString();

            result.ShouldBe("1.0");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Short_returns_two_digits() {
            var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("0", "."));
            const DisplayType DT = DisplayType.Short;

            string result = sut.GetVersionString(DT);

            result.ShouldBe("1.0");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Short_returns_two_digits_when_more_present() {
            var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("0", "."), new VersionUnit("1", "."));
            const DisplayType DT = DisplayType.Short;

            string result = sut.GetVersionString(DT);

            result.ShouldBe("1.0");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void ToString_equals_getversionstring() {
            var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("0", "."));

            string toString = sut.ToString();
            string getVersionString = sut.GetVersionString(DisplayType.Full);

            toString.ShouldBe(getVersionString);
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void ToString_respects_alternative_separator_characters() {
            var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("0", "-"), new VersionUnit("1", "-"));

            string result = sut.ToString();

            result.ShouldBe("1-0-1");
        }
    }

    public class UseCases {

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Plisky_semantic_versioning_is_supported() {
            var sut = new CompleteVersion(
                new VersionUnit("2"),
                new VersionUnit("0", "."),
                new VersionUnit("Unicorn", "-"),
                new VersionUnit("0", ".", DigitIncrementBehaviour.ContinualIncrement));

            string verString = sut.GetVersionString();
            verString.ShouldBe("2.0-Unicorn.0");

            sut.Increment();
            verString = sut.GetVersionString();
            verString.ShouldBe("2.0-Unicorn.1");

            sut.Increment();
            verString = sut.GetVersionString(DisplayType.Full);
            verString.ShouldBe("2.0-Unicorn.2");
        }
    }
}