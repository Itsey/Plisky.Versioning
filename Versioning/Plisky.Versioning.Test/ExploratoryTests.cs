using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Minimatch;
using Plisky.Diagnostics;
using Plisky.Test;
using Xunit;

namespace Plisky.CodeCraft.Test {

    public class Exploratory {
        private Bilge b = new Bilge();
        UnitTestHelper uth;
        TestSupport ts;

        public Exploratory() {
            uth = new UnitTestHelper();
            ts = new TestSupport(uth);
        }



        ~Exploratory() {
            uth.ClearUpTestFiles();
        }

        private static void CheckTheseMatches(List<Tuple<string, bool>> mtchs, string againstThis) {
            var mm = new Minimatcher(againstThis, new Options { AllowWindowsPaths = true, IgnoreCase = true });
            int i = 0;
            foreach (var v in mtchs) {
                i++;
                bool isMatch = mm.IsMatch(v.Item1);
                Assert.Equal(v.Item2, isMatch); //, "Mismatch " + i.ToString());
            }
        }

        private string CreateStoredVersionNumer() {
            var fn = uth.NewTemporaryFileName(true);
            CompleteVersion cv = GetDefaultVersion();
            JsonVersionPersister jvp = new JsonVersionPersister(fn);
            jvp.Persist(cv);
            return fn;
        }

        private static CompleteVersion GetDefaultVersion() {
            return new CompleteVersion(
                new VersionUnit("0", "", DigitIncremementBehaviour.ContinualIncrement),
                new VersionUnit("0", ".", DigitIncremementBehaviour.ContinualIncrement),
                new VersionUnit("0", ".", DigitIncremementBehaviour.ContinualIncrement),
                new VersionUnit("0", ".", DigitIncremementBehaviour.ContinualIncrement)
            );
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Developer)]
        public void Bug464_BuildVersionsNotUpdatedDuringBuild() {

            var ident = TestResources.GetIdentifiers(TestResourcesReferences.Bug464RefContent);

            string srcFile = uth.GetTestDataFile(ident);

            string fn = ts.GetFileAsTemporary(srcFile);
            var cv = new CompleteVersion(new VersionUnit("2"), new VersionUnit("0", "."),
                new VersionUnit("Unicorn", "-"),
                new VersionUnit("0", ".", DigitIncremementBehaviour.ContinualIncrement));
            VersionFileUpdater sut = new VersionFileUpdater(cv);

            sut.PerformUpdate(fn, FileUpdateType.Assembly);
            sut.PerformUpdate(fn, FileUpdateType.AssemblyInformational);
            sut.PerformUpdate(fn, FileUpdateType.AssemblyFile);

            Assert.False(ts.DoesFileContainThisText(fn, "AssemblyFileVersion(\"2.0.0\""),"The file version should be three digits and present");
            Assert.True(ts.DoesFileContainThisText(fn, "AssemblyInformationalVersion(\"2.0-Unicorn.0\""),"The informational version should be present");
            Assert.True(ts.DoesFileContainThisText(fn, "AssemblyVersion(\"2.0\")"),"the assembly version should be two digits and present.");
        }



       


        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void BuildTask_InvalidRuleType_Throws() {
            Assert.Throws<InvalidOperationException>(() => {
                TestableVersioningTask sut = new TestableVersioningTask();
                string verItemsSimple = "**/assemblyinfo.cs;ASSXXEMBLY";
                sut.SetAllVersioningItems(verItemsSimple);
            });
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void BuildTask_PassInRules_Works() {
            var v = GetDefaultVersion();
            TestableVersioningTask sut = new TestableVersioningTask();
            sut.SetVersionNumber(v);
            string verItemsSimple = "**/assemblyinfo.cs!ASSEMBLY";
            sut.SetAllVersioningItems(verItemsSimple);

            Assert.True(sut.IsThisMinimatchIncluded("**/assemblyinfo.cs"), "The minimatch was not included");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void BuildTask_PassInMultipleRules_Works() {
            var v = GetDefaultVersion();
            TestableVersioningTask sut = new TestableVersioningTask();
            sut.SetVersionNumber(v);
            string verItemsSimple =
                $"**/assemblyinfo.cs!ASSEMBLY{Environment.NewLine}xxMonkey!FILE{Environment.NewLine}yyzzxxbannana!WIX{Environment.NewLine}";
            sut.SetAllVersioningItems(verItemsSimple);

            Assert.True(sut.IsThisMinimatchIncluded("**/assemblyinfo.cs"), "The minimatch was not included");
            Assert.True(sut.IsThisMinimatchIncluded("xxMonkey"), "The second minimatch was not included");
            Assert.True(sut.IsThisMinimatchIncluded("yyzzxxbannana"), "The third minimatch was not included");

        }


        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void UseCase_Plisky_Works() {
            var sut = new CompleteVersion(new VersionUnit("2"), new VersionUnit("0", "."),
                new VersionUnit("Unicorn", "-"),
                new VersionUnit("0", ".", DigitIncremementBehaviour.ContinualIncrement));
            var verString = sut.GetVersionString();
            Assert.Equal("2.0-Unicorn.0", verString); //,"The initial string is not correct");
            sut.Increment();
            verString = sut.GetVersionString();
            Assert.Equal("2.0-Unicorn.1", verString); //, "The first increment string is not correct");
            sut.Increment();
            verString = sut.GetVersionString(DisplayType.Full);
            Assert.Equal("2.0-Unicorn.2", verString); //, "The second increment string is not correct");

        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void IncrementAndUpdateThrowsIfNoDirectory() {
            Assert.Throws<DirectoryNotFoundException>(() => {
                VersioningTask sut = new VersioningTask();
                sut.IncrementAndUpdateAll();
            });
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void UseCase_BuildVersionger_Exploratory() {
            string tfn1 = uth.NewTemporaryFileName(false);
            string tfn2 = CreateStoredVersionNumer();
            VersioningTask sut = new VersioningTask();
            string directory = Path.GetDirectoryName(tfn1);
            sut.BaseSearchDir = directory;
            sut.PersistanceValue = tfn2;
            sut.AddUpdateType(tfn1, FileUpdateType.Assembly);
            sut.AddUpdateType(tfn1, FileUpdateType.AssemblyFile);
            sut.AddUpdateType(tfn1, FileUpdateType.AssemblyInformational);
            sut.IncrementAndUpdateAll();

            Assert.Equal("1.1.1.1", sut.VersionString); //, "The version string should be set post update");
            var jp = new JsonVersionPersister(tfn2);
            Assert.Equal(sut.VersionString, jp.GetVersion().GetVersionString()); //, "The update should be persisted");
            Assert.True(ts.DoesFileContainThisText(tfn1, "AssemblyVersion(\"1.1"), "The target filename was not updated");
            Assert.True(ts.DoesFileContainThisText(tfn1, "AssemblyInformationalVersion(\"1.1.1.1"), "The target filename was not updated");
            Assert.True(ts.DoesFileContainThisText(tfn1, "AssemblyFileVersion(\"1.1.1.1"), "The target filename was not updated");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void MinimatchSyntax_Research() {
            var mtchs = new List<Tuple<string, bool>>();
            mtchs.Add(new Tuple<string, bool>(@"C:\temp\verworking\assemblyinfo.cs", true));
            CheckTheseMatches(mtchs, @"**\assemblyinfo.cs");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void MiniMatchSyntax_FindAssemblyInfo() {
            var mtchs = new List<Tuple<string, bool>>();
            mtchs.Add(new Tuple<string, bool>(@"C:\temp\te st\properties\assemblyinfo.cs", true));
            mtchs.Add(new Tuple<string, bool>(@"C:\te mp\test\assemblyinfo.cs", false));
            mtchs.Add(new Tuple<string, bool>(@"C:\te mp\t e s t\properties\notassemblyinfo.cs", false));
            mtchs.Add(new Tuple<string, bool>(@"C:\temp\test\properties\assemblyinfo.cs.txt", false));
            mtchs.Add(new Tuple<string, bool>(@"C:\a\1\s\PliskyLibrary\PliskyLib\Properties\AssemblyInfo.cs", true));
            string againstThis = @"**\properties\assemblyinfo.cs";
            CheckTheseMatches(mtchs, againstThis);

            var mm2 = new Minimatcher(@"C:\temp\test\testfile.tst", new Options { AllowWindowsPaths = true });
            Assert.True(mm2.IsMatch(@"C:\temp\test\testfile.tst"), "Cant match on full filename");
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void VersionStorage_SavesCorrectly() {
            var msut = new MockVersionStorage("itsamock");
            VersionStorage sut = msut;

            var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1"), new VersionUnit("1"), new VersionUnit("1"));
            sut.Persist(cv);
            Assert.True(msut.PersistWasCalled, "The persist method was not called");
            Assert.Equal("1111", msut.VersionStringPersisted);
        }

        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void VersionStorage_Json_Saves() {
            string fn = uth.NewTemporaryFileName(true);
            var sut = new JsonVersionPersister(fn);
            var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1"), new VersionUnit("1"), new VersionUnit("1"));
            sut.Persist(cv);
            Assert.True(File.Exists(fn), "The file must be created");
        }

        [Fact]
        public void VersionStorage_Json_Loads() {
            string fn = uth.NewTemporaryFileName(true);
            var sut = new JsonVersionPersister(fn);
            var cv = new CompleteVersion(new VersionUnit("1", ".", DigitIncremementBehaviour.ContinualIncrement), new VersionUnit("Alpha", "-"), new VersionUnit("1"), new VersionUnit("1", "", DigitIncremementBehaviour.ContinualIncrement));
            sut.Persist(cv);

            var cv2 = sut.GetVersion();

            Assert.Equal(cv.GetVersionString(), cv2.GetVersionString()); //, "The loaded type was not the same as the saved one, values");
            cv.Increment(); cv2.Increment();
            Assert.Equal(cv.GetVersionString(), cv2.GetVersionString()); //, "The loaded type was not the same as the saved one, behaviours");
        }


        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void VersionStoreAndLoad_StoresUpdatedValues() {
            string fn = uth.NewTemporaryFileName(true);
            var sut = new JsonVersionPersister(fn);
            var cv = new CompleteVersion(new VersionUnit("1", "", DigitIncremementBehaviour.ContinualIncrement),
                new VersionUnit("1", ".", DigitIncremementBehaviour.ContinualIncrement),
                new VersionUnit("1", ".", DigitIncremementBehaviour.ContinualIncrement),
                new VersionUnit("1", ".", DigitIncremementBehaviour.ContinualIncrement));

            cv.Increment();
            var beforeStore = cv.GetVersionString();
            sut.Persist(cv);
            var cv2 = sut.GetVersion();

            Assert.Equal(cv.GetVersionString(), cv2.GetVersionString()); //, "The two version strings should match");
            Assert.Equal("2.2.2.2", cv2.GetVersionString()); //, "The loaded version string should keep the increment");

            cv.Increment(); cv2.Increment();
            Assert.Equal(cv.GetVersionString(), cv2.GetVersionString()); //, "The two version strings should match");
        }


        [Fact]
        [Trait(Traits.Age, Traits.Regression)]
        [Trait(Traits.Style, Traits.Unit)]
        public void VersionStoreAndLoad_StoresDisplayTypes() {
            string fn = uth.NewTemporaryFileName(true);
            var sut = new JsonVersionPersister(fn);
            var cv = new CompleteVersion(new VersionUnit("1", "", DigitIncremementBehaviour.ContinualIncrement),
                new VersionUnit("1", ".", DigitIncremementBehaviour.ContinualIncrement),
                new VersionUnit("1", ".", DigitIncremementBehaviour.ContinualIncrement),
                new VersionUnit("1", ".", DigitIncremementBehaviour.ContinualIncrement));

            // None of the defaults are no display, therefore this should set all to a new value
            cv.SetDisplayTypeForVersion(FileUpdateType.Assembly, DisplayType.NoDisplay);
            cv.SetDisplayTypeForVersion(FileUpdateType.AssemblyFile, DisplayType.NoDisplay);
            cv.SetDisplayTypeForVersion(FileUpdateType.AssemblyInformational, DisplayType.NoDisplay);
            cv.SetDisplayTypeForVersion(FileUpdateType.Wix, DisplayType.NoDisplay);
            cv.SetDisplayTypeForVersion(FileUpdateType.NetStdAssembly, DisplayType.NoDisplay);
            cv.SetDisplayTypeForVersion(FileUpdateType.NetStdFile, DisplayType.NoDisplay);
            cv.SetDisplayTypeForVersion(FileUpdateType.NetStdInformational, DisplayType.NoDisplay);
            cv.SetDisplayTypeForVersion(FileUpdateType.Nuspec, DisplayType.NoDisplay);

            sut.Persist(cv);
            var cv2 = sut.GetVersion();

            // Check that all of the display types come back as epxected
            Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.Assembly)); 
            Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.AssemblyFile)); 
            Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.AssemblyInformational)); 
            Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.Wix)); 
            Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.NetStdAssembly));
            Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.NetStdFile));
            Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.NetStdInformational));
            Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.Nuspec));
        }

    }
}
