using Plisky.Diagnostics;


namespace Plisky.CodeCraft.Test {
    using System;
    using Plisky.CodeCraft;
    using Plisky.Test;
    using Xunit;

    public class CompleteVersionTests {
        private Bilge b = new Bilge();

        [Theory(DisplayName = nameof(CompletedVersion_ConstructorStringParser_Works))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        [InlineData("0.0.0.0", 4)]
        [InlineData("9086334.2345.1234.111", 4)]
        [InlineData("94.0.0.0", 4)]
        [InlineData("94.0", 2)]
        [InlineData("94.0.1", 3)]
        [InlineData("94", 1)]
        public void CompletedVersion_ConstructorStringParser_Works(string initString, int expectedDigits) {
            b.Info.Flow();

            CompleteVersion cv = new CompleteVersion(initString);

            Assert.Equal(expectedDigits, cv.Digits.Length);
        }



        [Theory(DisplayName = (nameof(DisplayTypes_WorkCorrectly)))]
        [Trait(Traits.Age, Traits.Fresh)]
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
        public void DisplayTypes_WorkCorrectly(string version, string expectedDisplay, DisplayType dtype) {
            b.Info.Flow();

            CompleteVersion cv = new CompleteVersion(version);
            string output = cv.GetVersionString(dtype);

            Assert.Equal(expectedDisplay, output);
        }


        [Theory(DisplayName = nameof(PendingIncrementPatterns_Work))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        [InlineData("1.0.0.0", "+.0.0.0", "2.0.0.0")]
        [InlineData("1.0.0.0", "0.0.0.0", "0.0.0.0")]
        [InlineData("1.0.0.0", "+.+.+.+", "2.1.1.1")]
        [InlineData("2.2.2.2", "-.-.-.-", "1.1.1.1")]
        [InlineData("2.2.2.2", "-..-.-", "1.2.1.1")]
        [InlineData("2.2.2.2", "...", "2.2.2.2")]
        [InlineData("2.2.2.2", "..Bealzebub.-", "2.2.Bealzebub.1")]
        [InlineData("2.2.2.2", "Unicorn.Peach.Applie.Pear", "Unicorn.Peach.Applie.Pear")]
        public void PendingIncrementPatterns_Work(string startVer, string pattern, string endVer) {
            b.Info.Flow();

            CompleteVersion cv = new CompleteVersion(startVer);
            
            cv.ApplyPendingVersion(pattern);
            cv.Increment();

            Assert.Equal(endVer, cv.ToString());
        }

        [Theory(DisplayName ="ManipulateVersionTests")]
        [InlineData("1","+","2")]
        [InlineData("1", "-", "0")]
        [InlineData("1", "1", "1")]
        [InlineData("1", "2", "2")]
        [InlineData("1", "alpha", "alpha")]
        [InlineData("1", "brav+o", "brav+o")]
        [InlineData("3", "+", "4")]
        [InlineData("9", "", "9")]
        [InlineData("9", "6", "6")]
        [InlineData("bannana", "pEEl", "pEEl")]
        public void ManipulateVersionTests(string value, string pattern, string result) {
            var sut = new CompleteVersionMock();
            
            var res = sut.Mock.ManipulateVerisonBasedOnPattern(pattern, value);

            Assert.Equal(result, res);
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void FixedBehaviour_DoesNotIncrement() {
            var sut = new CompleteVersion(new VersionUnit("2"), new VersionUnit("0", "."));
            string before = sut.GetVersionString(DisplayType.Full);
            sut.Increment();
            Assert.Equal(before, sut.GetVersionString(DisplayType.Full)); //, "Digits should be fixed and not change when incremented");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void PartiallyFixed_DoesIncrement() {
            var sut = new CompleteVersion(new VersionUnit("1", "", DigitIncremementBehaviour.ContinualIncrement), new VersionUnit("Monkey", "."));
            sut.Increment();
            Assert.Equal("2.Monkey", sut.GetVersionString(DisplayType.Full)); //, "The increment for the first digit did not work in a mixed verison number");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Increment_ResetAnyWorks() {
            var sut = new CompleteVersion(
                new VersionUnit("1", "", DigitIncremementBehaviour.ContinualIncrement),
                new VersionUnit("0", "."),
                new VersionUnit("1", "."),
                new VersionUnit("0", ".", DigitIncremementBehaviour.AutoIncrementWithResetAny));
            sut.Increment();
            Assert.Equal("2.0.1.0", sut.GetVersionString(DisplayType.Full)); //, "The reset should prevent the last digit from incrementing");
        }


        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void DisplayType_DefaultsToFull() {
            var sut = new CompleteVersion(
               new VersionUnit("1", ""),
               new VersionUnit("0", "."),
               new VersionUnit("1", "."),
               new VersionUnit("0", ".", DigitIncremementBehaviour.AutoIncrementWithResetAny));
            Assert.Equal(sut.GetVersionString(DisplayType.Full), sut.GetVersionString()); //, "The default should be to display as full");
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
               new VersionUnit("0", ".", DigitIncremementBehaviour.AutoIncrementWithResetAny));
            Assert.Equal("1.0.1.0", sut.GetVersionString()); //, "Without an increment its wrong - invalid test");

            sut.Increment();
            Assert.Equal("1.0.1.1", sut.GetVersionString()); //, "Increment on all fixed should not change anything");
            vu2.IncrementOverride = "5";
            sut.Increment();

            Assert.Equal("1.5.1.0", sut.GetVersionString()); //, "The final digit should reset when a fixed digit changes.");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Increment_OverrideReplacesIncrement() {
            var vu = new VersionUnit("1", "", DigitIncremementBehaviour.ContinualIncrement);
            vu.IncrementOverride = "9";
            var sut = new CompleteVersion(vu);
            sut.Increment();
            Assert.Equal("9", sut.GetVersionString(DisplayType.Full)); //, "The overide on a word value did not work");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void SimpleIncrement_Fixed_DoesNothing() {
            var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("2", "."));
            sut.Increment();
            Assert.Equal("1.2", sut.ToString()); //, "The verison increment should do nothing for fixed");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void SimpleIncrement_Incrment_Works() {
            var vu = new VersionUnit("2", ".");
            vu.SetBehaviour(DigitIncremementBehaviour.AutoIncrementWithReset);
            var sut = new CompleteVersion(new VersionUnit("1"), vu);
            sut.Increment();
            Assert.Equal("1.3", sut.ToString()); //, "The verison increment should do nothing for fixed");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Override_NoIncrement_DoesNotChangeValue() {
            var vu = new VersionUnit("1");
            vu.Value = "Monkey";
            vu.IncrementOverride = "Fish";
            var sut = new CompleteVersion(vu);

            Assert.Equal("Monkey", sut.GetVersionString(DisplayType.Full));
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Increment_OverrideWorksForNumbers() {
            var vu = new VersionUnit("1");
            vu.IncrementOverride = "5";

            var sut = new CompleteVersion(vu);
            sut.Increment();

            Assert.Equal("5", sut.GetVersionString(DisplayType.Full));
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Increment_OverrideWorksForNames() {
            var vu = new VersionUnit("Monkey");
            vu.IncrementOverride = "Fish";

            var sut = new CompleteVersion(vu);
            sut.Increment();

            Assert.Equal("Fish", sut.GetVersionString(DisplayType.Full)); //, "The overide on a word value did not work");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Increment_OverrideWorksOnFixed() {
            var vu = new VersionUnit("1", "", DigitIncremementBehaviour.Fixed);
            vu.IncrementOverride = "Fish";

            var sut = new CompleteVersion(vu);
            sut.Increment();

            Assert.Equal("Fish", sut.GetVersionString(DisplayType.Full));
        }


        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void ToString_Equals_GetVersionString() {
            var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("0", "."));
            Assert.Equal(sut.ToString(), sut.GetVersionString(DisplayType.Full));
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void GetVersionString_Short_ShowsCorrect() {
            var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("0", "."));
            var dt = DisplayType.Short;
            Assert.Equal("1.0", sut.GetVersionString(dt));
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void GetVersionString_Short_EvenWhenMoreDigits() {
            var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("0", "."), new VersionUnit("1", "."));
            var dt = DisplayType.Short;
            Assert.Equal("1.0", sut.GetVersionString(dt));
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void GetString_Respects_AlternativeFormatter() {
            var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("0", "-"), new VersionUnit("1", "-"));
            Assert.Equal("1-0-1", sut.ToString());
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void BasicTwoDigitToString_ReturnsCorrectly() {
            var sut = new CompleteVersion(new VersionUnit("1"), new VersionUnit("0", "."));
            Assert.True(sut.ToString() == "1.0");
        }
    }
}
