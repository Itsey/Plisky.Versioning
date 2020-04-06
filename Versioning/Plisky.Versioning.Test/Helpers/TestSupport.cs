using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Plisky.Diagnostics;
using Plisky.Test;

namespace Plisky.CodeCraft.Test {
    public class TestSupport {
        private Bilge b = new Bilge();
        private UnitTestHelper uth;

        
        public string GetVersion(FileUpdateType fut, string srcFile) {
            
            switch (fut) {

                case FileUpdateType.Nuspec: return GetVersionFromNuspec(srcFile);                    
                case FileUpdateType.NetStdAssembly: return GetVersionFromCSProj(srcFile, "AssemblyVersion");               
                case FileUpdateType.NetStdInformational: return GetVersionFromCSProj(srcFile, "Version");
                case FileUpdateType.NetStdFile: return GetVersionFromCSProj(srcFile, "FileVersion");

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

        public string GetVersionFromCSProj(string srcFile, string propName ) {
           
            XDocument xd2 = XDocument.Load(srcFile);
            var el2 = xd2.Element("Project")?.Element("PropertyGroup")?.Element(propName);
            var after = el2.Value;
            return after;
        }

        public string GetVersionFromNuspec(string srcFile) {
            XNamespace ns = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd";
            XDocument xd2 = XDocument.Load(srcFile);
            var el2 = xd2.Element(ns + "package")?.Element(ns + "metadata")?.Element(ns + "version");
            var after = el2.Value;
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

        public  bool DoesFileContainThisText(string fn, string v) {
            return File.ReadAllText(fn).Contains(v);
        }
    }
}
