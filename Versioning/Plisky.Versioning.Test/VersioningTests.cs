using Plisky.CodeCraft.Test;
using Plisky.Diagnostics;
using Plisky.Test;
using Xunit;

namespace Plisky.Versioning.Test {

    public class VersioningTests {
        private Bilge b = new Bilge();
        private UnitTestHelper uth;
        private TestSupport ts;

        public VersioningTests() {
            uth = new UnitTestHelper();
            ts = new TestSupport(uth);
        }





        [Fact(DisplayName = nameof(GetDefaultMinimatchers_IsNotEmpty))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void GetDefaultMinimatchers_IsNotEmpty() {
            b.Info.Flow();

            MockVersioning v = new MockVersioning(new MockVersionStorage(""));

            Assert.True(v.Mock.ReturnMinMatchers().Length > 0, "There should be some default minmatchers loaded by versioning");
        }

        [Fact(DisplayName = nameof(SetMinMatchers_ReplacesAll))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void SetMinMatchers_ReplacesAll() {
            b.Info.Flow();

            MockVersioning v = new MockVersioning(new MockVersionStorage(""));

            v.ClearMiniMatchers();

            Assert.True(v.Mock.ReturnMinMatchers().Length == 0, "Clear should remove all minimatchers");
        }

        [Fact(DisplayName = nameof(Versioning_MMLoadedFromFile))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Versioning_MMLoadedFromFile() {
            b.Info.Flow();

            var reid = TestResources.GetIdentifiers(TestResourcesReferences.MMTypeData);
            string srcFile = uth.GetTestDataFile(reid);

            MockVersionStorage mva = new MockVersionStorage("");
            MockVersioning v = new MockVersioning(mva);

            v.LoadMiniMatches(srcFile);

            Assert.Equal(9, v.Mock.ReturnMinMatchers().Length);
        }
    }
}