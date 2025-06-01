using System;
using System.IO;
using System.Xml.Linq;
using Plisky.Diagnostics;
using Plisky.Test;

namespace Plisky.CodeCraft.Test {

    public class TestSupport {
        private Bilge b = new Bilge();
        private UnitTestHelper uth;

        public string CreateStoredVersionNumer() {
            string fn = uth.NewTemporaryFileName(true);
            var cv = GetDefaultVersion();
            var jvp = new JsonVersionPersister(fn);
            jvp.Persist(cv);
            return fn;
        }


        public CompleteVersion GetDefaultVersion() {
            return new CompleteVersion(
                new VersionUnit("0", "", DigitIncremementBehaviour.ContinualIncrement),
                new VersionUnit("0", ".", DigitIncremementBehaviour.ContinualIncrement),
                new VersionUnit("0", ".", DigitIncremementBehaviour.ContinualIncrement),
                new VersionUnit("0", ".", DigitIncremementBehaviour.ContinualIncrement)
            );
        }

        public string GetVersion(FileUpdateType fut, string srcFile) {
            switch (fut) {
                case FileUpdateType.Nuspec: return GetVersionFromNuspec(srcFile);
                case FileUpdateType.StdAssembly: return GetVersionFromCSProj(srcFile, "AssemblyVersion");
                case FileUpdateType.StdInformational: return GetVersionFromCSProj(srcFile, "Version");
                case FileUpdateType.StdFile: return GetVersionFromCSProj(srcFile, "FileVersion");
            }
            // case FileUpdateType.Wix:
            //        break;
            //default:
            //        break;
            //        case FileUpdateType.Assembly:
            //    break;
            //case FileUpdateType.AssemblyInformational:
            //    break;
            //case FileUpdateType.AssemblyFile:
            //    break;
            throw new NotImplementedException();
        }

        public string GetVersionFromCSProj(string srcFile, string propName) {
            var xd2 = XDocument.Load(srcFile);
            var el2 = xd2.Element("Project")?.Element("PropertyGroup")?.Element(propName);
            if (el2 == null) { return null; }
            string after = el2.Value;
            return after;
        }

        public string GetVersionFromNuspec(string srcFile) {
            XNamespace ns = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd";
            var xd2 = XDocument.Load(srcFile);
            var el2 = xd2.Element(ns + "package")?.Element(ns + "metadata")?.Element(ns + "version");
            string after = el2.Value;
            return after;
        }

        public TestSupport(UnitTestHelper newuth) {
            uth = newuth;
        }

        public string GetFileAsTemporary(string srcFile) {
            string fn = uth.NewTemporaryFileName(true);
            File.Copy(srcFile, fn);
            return fn;
        }

        public bool DoesFileContainThisText(string fn, string v) {
            return File.ReadAllText(fn).Contains(v);
        }
    }
}