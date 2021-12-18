using System.Collections.Generic;
using System.Text;
using Plisky.CodeCraft;
using Plisky.CodeCraft.Test;
using Plisky.Diagnostics;
using Plisky.Test;
using PliskyTool;
using Xunit;

namespace Plisky.CodeCraft.Test {
    public class VersionOutputterTests {

        private Bilge b = new Bilge();
        private readonly UnitTestHelper uth;
        private TestSupport ts;

        public VersionOutputterTests() {
            uth = new UnitTestHelper();
            ts = new TestSupport(uth);
        }


        [Fact(DisplayName = nameof(Args_OutputterParseConsole_Works))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Args_OutputterParseConsole_Works() {
            b.Info.Flow();

            CommandLineArguments cla = new CommandLineArguments();
            cla.OutputOptions = "con";

            Assert.True( (cla.OutputsActive & OutputPossibilities.Console) == OutputPossibilities.Console);
        }


        [Theory(DisplayName = nameof(Args_OutputterParseSetsConsoleString))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        [InlineData("con:variable","variable")]
        [InlineData("vsts:variable", "variable")]
        [InlineData("con", "%VER%")]
        [InlineData("vsts:variable", "%VER%")]
        [InlineData("vsts", "%VER%")]
        [InlineData("vsts:bone", "bone")]
        public void Args_OutputterParseSetsConsoleString(string argument, string contains) {
            b.Info.Flow();

            CommandLineArguments cla = new CommandLineArguments();
            cla.OutputOptions = argument;

            Assert.Contains(contains, cla.ConsoleTemplate);
        }


        [Fact(DisplayName = nameof(Outputter_Environment_WritesToEnvironment))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Outputter_Environment_WritesToEnvironment() {
            b.Info.Flow();


            MockVersionStorage mvs = new MockVersionStorage("0.0.0.1");
            var sut = new Versioning(mvs);
            var v = sut.Version;
            
            var op = new MockVersioningOutputter(v);
            op.DoOutput(OutputPossibilities.File);

            Assert.True(op.FileWasWritten);
            Assert.False(op.EnvWasSet);
        }

    }
}
