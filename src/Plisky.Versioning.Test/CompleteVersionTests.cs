namespace Plisky.CodeCraft.Test;

using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;
using Xunit;

public class CompleteVersionTests {
    private Bilge b = new Bilge();
    private UnitTestHelper uth;
    private TestSupport ts;

    public CompleteVersionTests() {
        uth = new UnitTestHelper();
        ts = new TestSupport(uth);
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
    public void Completeversion_constructor_sets_digit_values(string initialValue, string dg1, string dg2, string dg3, string dg4) {
        b.Info.Flow();
        var sut = new CompleteVersion(initialValue, '.', '-', '+');

        string d0 = sut.Digits.Length > 0 ? sut.Digits[0].Value : null;
        string d1 = sut.Digits.Length > 1 ? sut.Digits[1].Value : null;
        string d2 = sut.Digits.Length > 2 ? sut.Digits[2].Value : null;
        string d3 = sut.Digits.Length > 3 ? sut.Digits[3].Value : null;

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

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [InlineData("1.2-3.4", "", ".", "-", ".")]
    [InlineData("1.2.3.4", "", ".", ".", ".")]
    [InlineData("1-2-3-4", "", "-", "-", "-")]
    [InlineData("1-2+3.4", "", "-", "+", ".")]
    public void CompletedVerison_constructor_sets_prefix(string initialValue, string dg1, string dg2, string dg3, string dg4) {
        b.Info.Flow();
        var sut = new CompleteVersion(initialValue, '.', '-', '+');

        string p0 = sut.Digits.Length > 0 ? sut.Digits[0].PreFix : null;
        string p1 = sut.Digits.Length > 1 ? sut.Digits[1].PreFix : null;
        string p2 = sut.Digits.Length > 2 ? sut.Digits[2].PreFix : null;
        string p3 = sut.Digits.Length > 3 ? sut.Digits[3].PreFix : null;

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

    [Theory(DisplayName = (nameof(DisplayTypes_WorkCorrectly)))]
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
    public void DisplayTypes_WorkCorrectly(string version, string expectedDisplay, DisplayType dtype) {
        b.Info.Flow();

        var cv = new CompleteVersion(version, '.', '-');

        string output = cv.GetVersionString(dtype);

        output.ShouldBe(expectedDisplay);
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
    public void PendingIncrementPatterns_Work(string startVer, string pattern, string endVer) {
        b.Info.Flow();

        var cv = new CompleteVersion(startVer);

        cv.ApplyPendingVersion(pattern);
        cv.Increment();

        string result = cv.ToString();

        result.ShouldBe(endVer);
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
    public void PendingIncrement_IsAppliedCorrectly(string startVer, string pattern, string d1Expected, string d2Expected, string d3Expected, string d4Expected) {
        b.Info.Flow();

        var cv = new CompleteVersion(startVer);

        cv.ApplyPendingVersion(pattern);

        string d1 = cv.Digits[0].IncrementOverride;
        string d2 = cv.Digits[1].IncrementOverride;
        string d3 = cv.Digits[2].IncrementOverride;
        string d4 = cv.Digits[3].IncrementOverride;

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

        string d1 = cv.Digits[0].IncrementOverride;
        string d2 = cv.Digits[1].IncrementOverride;
        string d3 = cv.Digits[2].IncrementOverride;
        string d4 = cv.Digits[3].IncrementOverride;

        d1.ShouldBeNull();
        d2.ShouldBeNull();
        d3.ShouldBeNull();
        d4.ShouldBeNull();
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
    public void ManipulateVersionTests(string value, string pattern, string result) {
        var sut = new CompleteVersionMock();

        string res = sut.Mock.ManipulateVersionBasedOnPattern(pattern, value);

        res.ShouldBe(result);
    }

    [Fact(DisplayName = nameof(ReleaseVersion_StartsEmpty))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void ReleaseVersion_StartsEmpty() {
        b.Info.Flow();

        var sut = new CompleteVersion(new VersionUnit("2"), new VersionUnit("0", "."));

        string releaseName = sut.ReleaseName;

        releaseName.ShouldBeNull();
    }

    [Fact(DisplayName = nameof(SetReleaseName_Works))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void SetReleaseName_Works() {
        const string RELEASENAME = "Unicorn";
        b.Info.Flow();

        var sut = new CompleteVersion(new VersionUnit("2"), new VersionUnit("0", "."), new VersionUnit("", "+", DigitIncrementBehaviour.ReleaseName));

        sut.ReleaseName = RELEASENAME;

        string releaseName = sut.ReleaseName;
        string versionString = sut.ToString();

        releaseName.ShouldBe(RELEASENAME);
        versionString.ShouldBe("2.0+" + RELEASENAME);
    }

    [Fact(DisplayName = nameof(Increment_DoesNotChangeReleaseName))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Increment_DoesNotChangeReleaseName() {
        const string RELEASENAME = "Unicorn";
        b.Info.Flow();

        var sut = new CompleteVersion(new VersionUnit("2"), new VersionUnit("0", "."), new VersionUnit("", "+", DigitIncrementBehaviour.ReleaseName));

        sut.ReleaseName = RELEASENAME;
        sut.Increment();

        string releaseName = sut.ReleaseName;
        string versionString = sut.ToString();

        releaseName.ShouldBe(RELEASENAME);
        versionString.ShouldBe("2.0+" + RELEASENAME);
    }

    [Fact(DisplayName = nameof(PendingReleaseName_AppliedOnIncrement))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void PendingReleaseName_AppliedOnIncrement() {
        const string RELEASENAME = "Unicorn";
        const string NEWRELEASE = "Phoenix";
        b.Info.Flow();

        var sut = new CompleteVersion(new VersionUnit("2"), new VersionUnit("0", "."), new VersionUnit("", "+", DigitIncrementBehaviour.ReleaseName));

        sut.ReleaseName = RELEASENAME;
        sut.ApplyPendingRelease(NEWRELEASE);
        sut.Increment();

        string releaseName = sut.ReleaseName;
        string versionString = sut.ToString();

        releaseName.ShouldBe(NEWRELEASE);
        versionString.ShouldBe("2.0+" + NEWRELEASE);
    }

    [Fact(DisplayName = nameof(PendingReleaseName_IgnoredNoIncrement))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void PendingReleaseName_IgnoredNoIncrement() {
        const string RELEASENAME = "Unicorn";
        const string NEWRELEASE = "Phoenix";
        b.Info.Flow();

        var sut = new CompleteVersion(new VersionUnit("2"), new VersionUnit("0", "."), new VersionUnit("", "+", DigitIncrementBehaviour.ReleaseName));

        sut.ReleaseName = RELEASENAME;
        sut.ApplyPendingRelease(NEWRELEASE);

        string releaseName = sut.ReleaseName;
        string versionString = sut.ToString();

        releaseName.ShouldBe(RELEASENAME);
        versionString.ShouldBe("2.0+" + RELEASENAME);
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
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void PartiallyFixed_DoesIncrement() {
        var sut = new CompleteVersion(new VersionUnit("1", "", DigitIncrementBehaviour.ContinualIncrement), new VersionUnit("Monkey", "."));

        sut.Increment();

        string result = sut.GetVersionString(DisplayType.Full);

        result.ShouldBe("2.Monkey");
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
    public void Increment_OverrideReplacesIncrement() {
        var vu = new VersionUnit("1", "", DigitIncrementBehaviour.ContinualIncrement);
        vu.IncrementOverride = "9";
        var sut = new CompleteVersion(vu);

        sut.Increment();

        string result = sut.GetVersionString(DisplayType.Full);

        result.ShouldBe("9");
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

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Override_NoIncrement_DoesNotChangeValue() {
        var vu = new VersionUnit("1");
        vu.Value = "Monkey";
        vu.IncrementOverride = "Fish";
        var sut = new CompleteVersion(vu);

        string result = sut.GetVersionString(DisplayType.Full);

        result.ShouldBe("Monkey");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Increment_OverrideWorksForNumbers() {
        var vu = new VersionUnit("1");
        vu.IncrementOverride = "5";

        var sut = new CompleteVersion(vu);

        sut.Increment();

        string result = sut.GetVersionString(DisplayType.Full);

        result.ShouldBe("5");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Increment_OverrideWorksForNames() {
        var vu = new VersionUnit("Monkey");
        vu.IncrementOverride = "Fish";

        var sut = new CompleteVersion(vu);

        sut.Increment();

        string result = sut.GetVersionString(DisplayType.Full);

        result.ShouldBe("Fish");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Increment_OverrideWorksOnFixed() {
        var vu = new VersionUnit("1", "", DigitIncrementBehaviour.Fixed);
        vu.IncrementOverride = "Fish";

        var sut = new CompleteVersion(vu);

        sut.Increment();

        string result = sut.GetVersionString(DisplayType.Full);

        result.ShouldBe("Fish");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void GetBehaviourString_ReturnsCorrectBehaviourForSingleDigit() {
        var behaviour = DigitIncrementBehaviour.ContinualIncrement;
        int behaviourValue = (int)behaviour;
        string expectedResult = $"[0]:{behaviour}({behaviourValue})";

        var vu = new VersionUnit("1", "", behaviour);
        var sut = new CompleteVersion(vu);

        string result = sut.GetBehaviourString("0");

        result.ShouldBe(expectedResult);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void GetBehaviourString_ReturnsCorrectBehaviourForStar() {
        var behaviourFixed = DigitIncrementBehaviour.Fixed;
        int behaviourFixedValue = (int)behaviourFixed;
        var behaviourInc = DigitIncrementBehaviour.ContinualIncrement;
        int behaviourIncValue = (int)behaviourInc;
        string expectedResult =
            $"[0]:{behaviourFixed}({behaviourFixedValue})\r\n" +
            $"[1]:{behaviourInc}({behaviourIncValue})\r\n" +
            $"[2]:{behaviourInc}({behaviourIncValue})";

        var vu1 = new VersionUnit("1", "");
        var vu2 = new VersionUnit("0", ".", behaviourInc);
        var vu3 = new VersionUnit("1", ".", behaviourInc);
        var sut = new CompleteVersion(vu1, vu2, vu3);

        string result = sut.GetBehaviourString("*");

        result.ShouldBe(expectedResult);
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("0", true)]
    [InlineData("*", true)]
    public void ValidateDigitOptions_ReturnsTrueForValidInput(string digitInput, bool expectedResult) {
        b.Info.Flow();
        var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("0", "."));

        bool actualResult = sut.ValidateDigitOptions([digitInput]);

        actualResult.ShouldBe(expectedResult);
    }

    public class DisplayTypes {

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
        public void Short_returns_two_digits() {
            var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("0", "."));
            var dt = DisplayType.Short;

            string result = sut.GetVersionString(dt);

            result.ShouldBe("1.0");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Short_returns_two_digits_when_more_present() {
            var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("0", "."), new VersionUnit("1", "."));
            var dt = DisplayType.Short;

            string result = sut.GetVersionString(dt);

            result.ShouldBe("1.0");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void ToString_respects_alternative_separator_characters() {
            var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("0", "-"), new VersionUnit("1", "-"));

            string result = sut.ToString();

            result.ShouldBe("1-0-1");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Default_display_is_correct_for_two_digits() {
            var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("0", "."));

            string result = sut.ToString();

            result.ShouldBe("1.0");
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