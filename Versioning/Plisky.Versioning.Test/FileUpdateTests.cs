using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Plisky.Test;
using System.Collections.Generic;
using Xunit;

namespace Plisky.CodeCraft.Test {

    using CodeCraft;
    using System.IO;

    public class FileUpdateTests {
        private UnitTestHelper uth;
        private TestSupport ts;
        protected static InMemoryHandler imh = new InMemoryHandler(100000);
        protected static Bilge b;

        public FileUpdateTests() {
            if (b == null) {
                b = new Bilge();
                b.AddHandler(new TCPHandler("127.0.0.1", 5060, true));
                b.AddHandler(imh);
            }

            uth = new UnitTestHelper();

            ts = new TestSupport(uth);
            b.Info.Log("UTH Online");
        }

        ~FileUpdateTests() {
            uth.ClearUpTestFiles();
        }


        [Fact(DisplayName = nameof(LiteralReplace_DefaultReplacesVersionAndReleaseName))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void LiteralReplace_DefaultReplacesVersionAndReleaseName() {
            b.Info.Flow();

            var reid = TestResources.GetIdentifiers(TestResourcesReferences.ReleaseNameAndVerTxt);
            string srcFile = uth.GetTestDataFile(reid);
            CompleteVersion cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
            cv.ReleaseName = "Unicorn";

            VersionFileUpdater sut = new VersionFileUpdater(cv);
            sut.PerformUpdate(srcFile, FileUpdateType.TextFile,DisplayType.Release);


            string result = File.ReadAllText(srcFile);
            Assert.DoesNotContain("XXX-RELEASENAME-XXX", result);
            Assert.Contains("Unicorn", result);
            Assert.DoesNotContain("XXX-VERSION-XXX", result);
            Assert.Contains("1.1.1.1", result);
        }


        [Fact(DisplayName = nameof(LiteralReplace_Version3_IsThreeDigits))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void LiteralReplace_Version3_IsThreeDigits() {
            b.Info.Flow();

            var reid = TestResources.GetIdentifiers(TestResourcesReferences.VersionV3Txt);
            string srcFile = uth.GetTestDataFile(reid);
            CompleteVersion cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
     
            VersionFileUpdater sut = new VersionFileUpdater(cv);
            sut.PerformUpdate(srcFile, FileUpdateType.TextFile, DisplayType.Release);
            string result = File.ReadAllText(srcFile);

            Assert.DoesNotContain("XXX-VERSION3-XXX", result);
            Assert.DoesNotContain("1.1.1.1", result);
            Assert.Contains("1.1.1", result);
        }

        [Fact(DisplayName = nameof(LiteralReplace_Version2_IsTwoDigits))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void LiteralReplace_Version2_IsTwoDigits() {
            b.Info.Flow();

            var reid = TestResources.GetIdentifiers(TestResourcesReferences.VersionV3Txt);
            string srcFile = uth.GetTestDataFile(reid);
            File.WriteAllText(srcFile, File.ReadAllText(srcFile).Replace("XXX-VERSION3-XXX", "XXX-VERSION2-XXX"));
            CompleteVersion cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));

            VersionFileUpdater sut = new VersionFileUpdater(cv);
            sut.PerformUpdate(srcFile, FileUpdateType.TextFile, DisplayType.Release);
            string result = File.ReadAllText(srcFile);

            Assert.DoesNotContain("XXX-VERSION3-XXX", result);
            Assert.DoesNotContain("XXX-VERSION2-XXX", result);
            Assert.DoesNotContain("1.1.1.1", result);
            Assert.DoesNotContain("1.1.1", result);
            Assert.Contains("1.1", result);
        }

        [Fact(DisplayName = nameof(LiteralReplace_NoDisplay_DoesNotUpdateVersion))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void LiteralReplace_NoDisplay_DoesNotUpdateVersion() {
            b.Info.Flow();

            var reid = TestResources.GetIdentifiers(TestResourcesReferences.ReleaseNameAndVerTxt);
            string srcFile = uth.GetTestDataFile(reid);
            CompleteVersion cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
            cv.ReleaseName = "Unicorn";

            VersionFileUpdater sut = new VersionFileUpdater(cv);
            sut.PerformUpdate(srcFile, FileUpdateType.TextFile, DisplayType.NoDisplay);


            string result = File.ReadAllText(srcFile);
            Assert.DoesNotContain("XXX-RELEASENAME-XXX", result);
            Assert.Contains("Unicorn", result);
            Assert.Contains("XXX-VERSION-XXX",result);
            Assert.DoesNotContain("1.1.1.1", result);

        }



        [Fact(DisplayName = nameof(VersionFileUpdaterFindsFiles))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void VersionFileUpdaterFindsFiles() {
            b.Info.Flow();

            MockVersionFileUpdater msut = new MockVersionFileUpdater();
            VersionFileUpdater sut = (VersionFileUpdater)msut;

            msut.Mock.AddFilesystemFile("XX");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Regex_MatchesForAssembly() {
            b.Info.Flow();

            VersionFileUpdater sut = new VersionFileUpdater();
            var rx = sut.GetRegex("AssemblyVersion");
            Assert.True(rx.IsMatch("[assembly: AssemblyVersion(\"0.0.0.0\")]"), "1 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly: AssemblyVersion(\"0.0.0\")]"), "2 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly: AssemblyVersion(\"0.0\")]"), "3 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly: AssemblyVersion(\"0\")]"), "4 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly: AssemblyVersion(\"\")]"), "5 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly:      AssemblyVersion     (\"0.0.0.0\"   )     ]"), "7 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly     :AssemblyVersion(\"0.0.0.0\")]"), "8 Invalid match for an assembly version");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Regex_MatchesForInformational() {
            b.Info.Flow();

            VersionFileUpdater sut = new VersionFileUpdater();
            var rx = sut.GetRegex("AssemblyFileVersion");
            Assert.True(rx.IsMatch("[assembly: AssemblyFileVersion(\"0.0.0.0\")]"), "1 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly: AssemblyFileVersion(\"0.0.0\")]"), "2 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly: AssemblyFileVersion(\"0.0\")]"), "3 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly: AssemblyFileVersion(\"0\")]"), "4 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly: AssemblyFileVersion(\"\")]"), "5 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly:      AssemblyFileVersion     (\"0.0.0.0\"   )     ]"), "7 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly     :AssemblyFileVersion(\"0.0.0.0\")]"), "8 Invalid match for an assembly version");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Regex_MatchesForFile() {
            b.Info.Flow();

            VersionFileUpdater sut = new VersionFileUpdater();
            var rx = sut.GetRegex("AssemblyInformationalVersion");

            Assert.True(rx.IsMatch("[assembly: AssemblyInformationalVersion(\"0.0.0.0\")]"), "1 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly: AssemblyInformationalVersion(\"0.0.0\")]"), "2 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly: AssemblyInformationalVersion(\"0.0\")]"), "3 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly: AssemblyInformationalVersion(\"0\")]"), "4 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly: AssemblyInformationalVersion(\"\")]"), "5 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly:      AssemblyInformationalVersion     (\"0.0.0.0\"   )     ]"), "7 Invalid match for an assembly version");
            Assert.True(rx.IsMatch("[assembly     :AssemblyInformationalVersion(\"0.0.0.0\")]"), "8 Invalid match for an assembly version");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Update_AsmVersion_Works() {
            b.Info.Flow();

            var reid = TestResources.GetIdentifiers(TestResourcesReferences.JustAssemblyVer);

            string srcFile = uth.GetTestDataFile(reid);

            CompleteVersion cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
            string fn = ts.GetFileAsTemporary(srcFile);

            VersionFileUpdater sut = new VersionFileUpdater(cv);

            sut.PerformUpdate(fn, FileUpdateType.NetAssembly);

            Assert.False(ts.DoesFileContainThisText(fn, "0.0.0.0"), "No update was made to the file at all");
            Assert.True(ts.DoesFileContainThisText(fn, "1.1"), "The file does not appear to have been updated correctly.");
            Assert.True(ts.DoesFileContainThisText(fn, "AssemblyVersion(\"1.1\")"), "The file does not have the full version in it");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Update_DoesNotAlterOtherAttributes() {
            b.Info.Flow();

            var reid = TestResources.GetIdentifiers(TestResourcesReferences.NoChangeAssemInfo);
            string srcFile = uth.GetTestDataFile(reid);

            CompleteVersion cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
            string fn = ts.GetFileAsTemporary(srcFile);

            VersionFileUpdater sut = new VersionFileUpdater(cv);

            sut.PerformUpdate(fn, FileUpdateType.NetAssembly);

            Assert.False(ts.DoesFileContainThisText(fn, " AssemblyVersion(\"1.0.0.0\")"), "No update was made to the file at all");
            Assert.True(ts.DoesFileContainThisText(fn, "[assembly: AssemblyFileVersion(\"1.0.0.0\")]"), "The file does not appear to have been updated correctly.");
            Assert.True(ts.DoesFileContainThisText(fn, "[assembly: AssemblyCompany(\"\")]"), "Collatoral Damage - Another element in the file was updated - Company");
            Assert.True(ts.DoesFileContainThisText(fn, "[assembly: Guid(\"557cc26f-fcb2-4d0e-a34e-447295115fc3\")]"), "Collatoral Damage - Another element in the file was updated - Guid");
            Assert.True(ts.DoesFileContainThisText(fn, "// [assembly: AssemblyVersion(\"1.0.*\")]"), "Collatoral Damage - Another element in the file was updated - Comment");
            Assert.True(ts.DoesFileContainThisText(fn, "using System.Reflection;"), "Collatoral Damage - Another element in the file was updated - Reflection First Line");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Update_AsmInfVer_Works() {
            var reid = TestResources.GetIdentifiers(TestResourcesReferences.JustInformational);
            string srcFile = uth.GetTestDataFile(reid);

            CompleteVersion cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
            string fn = ts.GetFileAsTemporary(srcFile);

            VersionFileUpdater sut = new VersionFileUpdater(cv);

            sut.PerformUpdate(fn, FileUpdateType.NetInformational);

            Assert.False(ts.DoesFileContainThisText(fn, "0.0.0.0"), "No update was made to the file at all");
            Assert.True(ts.DoesFileContainThisText(fn, "1.1"), "The file does not appear to have been updated correctly.");
            Assert.True(ts.DoesFileContainThisText(fn, "AssemblyInformationalVersion(\"1.1.1.1\")"), "The file does not have the full version in it");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Update_AsmFileVer_Works() {
            b.Info.Flow();
            var reid = TestResources.GetIdentifiers(TestResourcesReferences.JustFileVer);
            string srcFile = uth.GetTestDataFile(reid);
            CompleteVersion cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
            string fn = ts.GetFileAsTemporary(srcFile);
            VersionFileUpdater sut = new VersionFileUpdater(cv);

            sut.PerformUpdate(fn, FileUpdateType.NetFile);

            Assert.False(ts.DoesFileContainThisText(fn, "0.0.0.0"), "No update was made to the file at all");
            Assert.True(ts.DoesFileContainThisText(fn, "1.1"), "The file does not appear to have been updated correctly.");
            Assert.True(ts.DoesFileContainThisText(fn, "AssemblyFileVersion(\"1.1.1.1\")"), "The file does not have the full version in it");
        }

        [Fact(DisplayName = nameof(Update_Nuspec_Works))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Update_Nuspec_Works() {
            var reid = TestResources.GetIdentifiers(TestResourcesReferences.NuspecSample1);
            string srcFile = uth.GetTestDataFile(reid);
            CompleteVersion cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
            VersionFileUpdater sut = new VersionFileUpdater(cv);
            var before = ts.GetVersion(FileUpdateType.Nuspec, srcFile);
            Assert.NotEmpty(before);

            sut.PerformUpdate(srcFile, FileUpdateType.Nuspec);

            var after = ts.GetVersion(FileUpdateType.Nuspec, srcFile);
            Assert.NotEqual<string>(before, after);
        }

        [Fact(DisplayName = nameof(Update_StdCSProjAsm_Works))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Update_StdCSProjAsm_Works() {
            var reid = TestResources.GetIdentifiers(TestResourcesReferences.NetStdAll3);
            string srcFile = uth.GetTestDataFile(reid);  // Value is zero
            CompleteVersion cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
            VersionFileUpdater sut = new VersionFileUpdater(cv);
            var before = ts.GetVersion(FileUpdateType.StdAssembly, srcFile);
            Assert.NotEmpty(before);

            sut.PerformUpdate(srcFile, FileUpdateType.StdAssembly);

            var after = ts.GetVersion(FileUpdateType.StdAssembly, srcFile);
            Assert.NotEqual<string>(before, after);
        }


        [Fact(DisplayName = nameof(UpdateStd_AddsFileWhenMissing))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void UpdateStd_AddsFileWhenMissing() {
            var reid = TestResources.GetIdentifiers(TestResourcesReferences.NetStdNone);
            string srcFile = uth.GetTestDataFile(reid); 
            CompleteVersion cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
            VersionFileUpdater sut = new VersionFileUpdater(cv);
            var before = ts.GetVersion(FileUpdateType.StdFile, srcFile);
            
            sut.PerformUpdate(srcFile, FileUpdateType.StdFile);

            var after = ts.GetVersion(FileUpdateType.StdFile, srcFile);
            Assert.True(string.IsNullOrEmpty(before));
            Assert.False(string.IsNullOrEmpty(after));
        }


        [Fact(DisplayName = nameof(UpdateStd_AddsAsmWhenMissing))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void UpdateStd_AddsAsmWhenMissing() {
            var reid = TestResources.GetIdentifiers(TestResourcesReferences.NetStdNone);
            string srcFile = uth.GetTestDataFile(reid);
            CompleteVersion cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
            VersionFileUpdater sut = new VersionFileUpdater(cv);
            var before = ts.GetVersion(FileUpdateType.StdAssembly, srcFile);

            sut.PerformUpdate(srcFile, FileUpdateType.StdAssembly);

            var after = ts.GetVersion(FileUpdateType.StdAssembly, srcFile);

            Assert.True(string.IsNullOrEmpty(before));
            Assert.False(string.IsNullOrEmpty(after));
        }




        [Fact(DisplayName = nameof(UpdateStd_AddsStdInfoWhenMissing))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void UpdateStd_AddsStdInfoWhenMissing() {
            var reid = TestResources.GetIdentifiers(TestResourcesReferences.NetStdNone);
            string srcFile = uth.GetTestDataFile(reid);
            CompleteVersion cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
            VersionFileUpdater sut = new VersionFileUpdater(cv);
            var before = ts.GetVersion(FileUpdateType.StdInformational, srcFile);

            sut.PerformUpdate(srcFile, FileUpdateType.StdInformational);

            var after = ts.GetVersion(FileUpdateType.StdInformational, srcFile);

            Assert.True(string.IsNullOrEmpty(before));
            Assert.False(string.IsNullOrEmpty(after));
        }






        [Fact(DisplayName = nameof(Update_StdCSProjFile_Works))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Update_StdCSProjFile_Works() {
            var reid = TestResources.GetIdentifiers(TestResourcesReferences.NetStdAll3);
            string srcFile = uth.GetTestDataFile(reid);  // Value is zero
            CompleteVersion cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
            VersionFileUpdater sut = new VersionFileUpdater(cv);
            var before = ts.GetVersion(FileUpdateType.StdFile, srcFile);
            Assert.NotEmpty(before);

            sut.PerformUpdate(srcFile, FileUpdateType.StdFile);

            var after = ts.GetVersion(FileUpdateType.StdFile, srcFile);
            Assert.NotEqual<string>(before, after);
        }

        [Fact(DisplayName = nameof(Update_StdCSProjInfo_Works))]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void Update_StdCSProjInfo_Works() {
            var reid = TestResources.GetIdentifiers(TestResourcesReferences.NetStdAll3);
            string srcFile = uth.GetTestDataFile(reid);
            CompleteVersion cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1", "."), new VersionUnit("1", "."), new VersionUnit("1", "."));
            VersionFileUpdater sut = new VersionFileUpdater(cv);
            var before = ts.GetVersion(FileUpdateType.StdInformational, srcFile);
            Assert.NotEmpty(before);

            sut.PerformUpdate(srcFile, FileUpdateType.StdInformational);

            var after = ts.GetVersion(FileUpdateType.StdInformational, srcFile);
            Assert.NotEqual<string>(before, after);
        }
    }

    public class MockVersionFileUpdater : VersionFileUpdater {
        private List<string> allFileSystemFiles = new List<string>();

        #region mocking implementation

        public Mocking Mock;

        public class Mocking {
            private MockVersionFileUpdater parent;

            public Mocking(MockVersionFileUpdater p) {
                parent = p;
            }

            public void Mock_MockingBird() {
            }

            public void AddFilesystemFile(string fname) {
                parent.allFileSystemFiles.Add(fname);
            }
        }

        #endregion mocking implementation

        protected Bilge b;

        public MockVersionFileUpdater(Bilge useThisTrace = null) {
            if (useThisTrace == null) {
                b = new Bilge();
            } else {
                b = useThisTrace;
            }

            Mock = new Mocking(this);
        }
    }
}