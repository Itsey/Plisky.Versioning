using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.CoreUtilities.Helpers;
using Plisky.CodeCraft;
using Plisky.CodeCraft.Test;
using Plisky.Diagnostics;
using Plisky.Test;
using PliskyTool;
using Xunit;

namespace Plisky.Versioning.Test {
    public class PliskyToolTests {
        private Bilge b = new Bilge();
        private readonly UnitTestHelper uth;
        private TestSupport ts;

        public PliskyToolTests() {
            uth = new UnitTestHelper();
            ts = new TestSupport(uth);
        }



        [Fact(DisplayName = nameof(ParseOptions_EnvSelected_Works))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void ParseOptions_EnvSelected_Works() {
            b.Info.Flow();

            var sut = new CommandLineArguments();
            sut.OutputOptions = "env";

            Assert.True( (sut.OutputsActive & OutputPossibilities.Environment) == OutputPossibilities.Environment);
        }

        [Fact(DisplayName = nameof(ParseOptions_FileSelected_Works))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void ParseOptions_FileSelected_Works() {
            b.Info.Flow();

            var sut = new CommandLineArguments();
            sut.OutputOptions = "file";

            Assert.True((sut.OutputsActive & OutputPossibilities.File) == OutputPossibilities.File);
        }


        [Fact(DisplayName = nameof(ParseOptions_Invalid_ThrowsError))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void ParseOptions_Invalid_ThrowsError() {
            b.Info.Flow();

            Assert.Throws<ArgumentOutOfRangeException>(() => {
                var sut = new CommandLineArguments();
                sut.OutputOptions = "MyIncrediblyWrongArgument";
            });
        }


        [Fact(DisplayName = nameof(ParseOptions_DefaultsToNone))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void ParseOptions_DefaultsToNone() {
            b.Info.Flow();

            var sut = new CommandLineArguments();
            Assert.Equal(OutputPossibilities.None, sut.OutputsActive); 
        }


        [Fact(DisplayName = nameof(ParseOptions_NullIsNone))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void ParseOptions_NullIsNone() {
            b.Info.Flow();

            var sut = new CommandLineArguments();
            sut.OutputOptions = "";
            Assert.Equal(OutputPossibilities.None, sut.OutputsActive);

            sut.OutputOptions = null;
            Assert.Equal(OutputPossibilities.None, sut.OutputsActive);
        }

    }
}
