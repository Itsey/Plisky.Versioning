using System.IO;

namespace Plisky.CodeCraft.Test {

    using CodeCraft;
    using Plisky.Diagnostics;
    using Plisky.Test;
    using System;
    using Xunit;

    public class UseCaseTests {
        private Bilge b = new Bilge();
        private UnitTestHelper uth = new UnitTestHelper();

        [Fact(DisplayName = nameof(Versioning_DefaultBehaviour_IsIncrementBuild))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Versioning_DefaultBehaviour_IsIncrementBuild() {
            b.Info.Flow();

            JsonVersionPersister jvp = new JsonVersionPersister(uth.NewTemporaryFileName(true));
            Versioning ver = new Versioning(jvp);

            string before = ver.ToString();
            ver.Increment();
            string after = ver.ToString();

            Assert.Equal("0.0.0.0", before);
            Assert.Equal("0.0.1.0", after);
        }

        [Fact(DisplayName = nameof(Versioning_StartsAtZero))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Versioning_StartsAtZero() {
            b.Info.Flow();

            string fn = uth.NewTemporaryFileName(true);
            JsonVersionPersister jvp = new JsonVersionPersister(fn);
            var str = new Versioning(jvp).ToString();

            Assert.Equal("0.0.0.0", str);
        }

        [Fact(DisplayName = nameof(UC_NoIncrement_NoChange))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void UC_NoIncrement_NoChange() {
            MockVersionStorage mvs = new MockVersionStorage("0.0.0.1");
            Versioning sut = new Versioning(mvs);

            Assert.Equal("0.0.0.1", sut.ToString());
        }

        [Theory(DisplayName = nameof(UC_BehaviouralIncrement_Works))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        [InlineData("0.0.0.0", "0.0.1.0")]
        [InlineData("0.0.1.1", "0.0.2.0")]
        [InlineData("3.4.0.0", "3.4.1.0")]
        [InlineData("0.0.0.9", "0.0.1.0")]
        public void UC_BehaviouralIncrement_Works(string initial, string target) {
            MockVersionStorage mvs = new MockVersionStorage(initial);
            Versioning sut = new Versioning(mvs);

            mvs.Mock.SetBehaviours(DigitIncremementBehaviour.Fixed, DigitIncremementBehaviour.Fixed, DigitIncremementBehaviour.AutoIncrementWithResetAny, DigitIncremementBehaviour.AutoIncrementWithResetAny);

            sut.Increment();
            Assert.Equal(target, sut.ToString());
        }

        [Fact(DisplayName = nameof(UC_UpdateNuspecFile_Works))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void UC_UpdateNuspecFile_Works() {
            b.Info.Flow();

            var reid = TestResources.GetIdentifiers(TestResourcesReferences.NuspecSample1);
            string srcFile = uth.GetTestDataFile(reid);

            MockVersionStorage mvs = new MockVersionStorage("0.0.0.1");
            Versioning sut = new Versioning(mvs);

            sut.AddNugetFile(srcFile);
        }

        [Fact(DisplayName = nameof(AddInvalid_NugetFile_Throws))]
        [Trait(Traits.Age, Traits.Fresh)]
        [Trait(Traits.Style, Traits.Unit)]
        public void AddInvalid_NugetFile_Throws() {
            b.Info.Flow();

            MockVersionStorage mvs = new MockVersionStorage("0.0.0.1");
            Versioning sut = new Versioning(mvs);

            Assert.Throws<ArgumentNullException>(() => { sut.AddNugetFile(null); });
            Assert.Throws<FileNotFoundException>(() => { sut.AddNugetFile(""); });
            Assert.Throws<FileNotFoundException>(() => { sut.AddNugetFile("c:\\arflebarflegloop.txt"); });
        }
    }
}