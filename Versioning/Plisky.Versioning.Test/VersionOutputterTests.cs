using System.Collections.Generic;
using System.Text;
using Plisky.CodeCraft;
using Plisky.CodeCraft.Test;
using Plisky.Diagnostics;
using Plisky.Test;
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


        [Fact(DisplayName = nameof(Outputter_Environment_WritesToEnvironment))]
        [Trait(Traits.Age, Traits.Fresh)]
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
