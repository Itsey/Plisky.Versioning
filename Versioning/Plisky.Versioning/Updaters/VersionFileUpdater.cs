using Minimatch;
using Plisky.Plumbing;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Plisky.Diagnostics;

namespace Plisky.CodeCraft {
    public class VersionFileUpdater {
        private Bilge b = new Bilge();
        private const string ASMFILE_FILEVER_TAG = "AssemblyFileVersion";
        private const string ASMFILE_VER_TAG = "AssemblyVersion";
        private const string ASMFILE_INFVER_TAG = "AssemblyInformationalVersion";
        private const string ASM_STD_ASMVTAG = "AssemblyVersion";
        private const string ASM_STD_VERSTAG = "Version";
        private const string ASM_STD_FILETAG = "FileVersion";

        private Minimatcher AssemblyMM;
        private Minimatcher InfoMM;
        private Minimatcher WixMM;
        private IHookVersioningChanges hook;

        private CompleteVersion cv;
        private string RootPath;

        public VersionFileUpdater() {
        }

        public VersionFileUpdater(CompleteVersion cv, IHookVersioningChanges actions = null) {
            this.cv = cv;
        }

        public Regex GetRegex(string targetAttribute) {
            return new Regex("\\s*\\[\\s*assembly\\s*:\\s*" + targetAttribute + "\\s*\\(\\s*\\\"\\s*[0-9A-z\\-.*]*\\s*\\\"\\s*\\)\\s*\\]", RegexOptions.IgnoreCase);
        }

        public void PerformUpdate(string fl, FileUpdateType fut, DisplayType dt = DisplayType.Default) {
            b.Verbose.Log("Perform update requested " + fut.ToString(), fl);
            if (!File.Exists(fl)) {
                throw new FileNotFoundException($"Filename must be present for version update {fl}");
            }

            var dtx = cv.GetDisplayType(fut, dt);
            string versonToWrite = cv.GetVersionString(dtx);
            switch (fut) {
                case FileUpdateType.Assembly:
                    UpdateCSFileWithAttribute(fl, ASMFILE_VER_TAG, versonToWrite);
                    break;
                case FileUpdateType.AssemblyInformational:
                    UpdateCSFileWithAttribute(fl, ASMFILE_INFVER_TAG, versonToWrite);
                    break;
                case FileUpdateType.AssemblyFile:
                    UpdateCSFileWithAttribute(fl, ASMFILE_FILEVER_TAG, versonToWrite);
                    break;
                case FileUpdateType.Wix:
                    UpdateWixFile(fl, versonToWrite);
                    break;
                case FileUpdateType.Nuspec:
                    UpdateNuspecFile(fl, versonToWrite);
                    break;
                case FileUpdateType.NetStdAssembly:
                    UpdateStdCSPRoj(fl, versonToWrite, ASM_STD_ASMVTAG);
                    break;
                case FileUpdateType.NetStdFile:
                    UpdateStdCSPRoj(fl, versonToWrite, ASM_STD_FILETAG);
                    break;
                case FileUpdateType.NetStdInformational:
                    UpdateStdCSPRoj(fl, versonToWrite, ASM_STD_VERSTAG);
                    break;
                default:
                    throw new NotImplementedException("The file type requested does not have a way to update files currently.");                    
            }

        }

        private void UpdateStdCSPRoj(string fl, string versonToWrite, string propName) {
#if DEBUG
            if (!File.Exists(fl)) { throw new InvalidOperationException("Must not be possible, check this before you reach this code"); }
#endif
            b.Verbose.Log($"Updating CSPROJ style file with ver {versonToWrite} property {propName}");

            XDocument xd2 = XDocument.Load(fl);

            var el2 = xd2.Element("Project");
            if (el2 == null){
                b.Error.Log($"Unable to locate [Project] element in file [{fl}], version update failed.");
                return;
            }

            var el3 = el2?.Element("PropertyGroup");
            if(el3!=null) {
                var el4 = el3.Element(propName);
                if (el4==null) {
                    el4 = new XElement(propName);
                    el3.Add(el4);
                }
                el4.Value = versonToWrite;
            }

            xd2.Save(fl);
            
        }

        private void UpdateWixFile(string fileName, string versionToWrite) {
            const string WIXNAMESPACE = "http://schemas.microsoft.com/wix/2006/wi";
            XDocument xd = XDocument.Load(fileName);
            XNamespace ns = WIXNAMESPACE;
            var el = xd.Element(ns + "Wix")?.Element(ns + "Product");
            if (el == null) {
                b.Verbose.Log("No Wix/Product element found, nothing i can do");
                return;
            }

            var at1 = el?.Attribute("Version");
            if (at1 != null) {
                at1.Value = versionToWrite;
                var at2 = el?.Attribute("Name");
                if (at2 != null) {
                    b.Verbose.Log("Secondary name element found");
                    at2.Value = at2.Value.Replace("XXX_VERSION_XXX", versionToWrite);
                }
            } else {
                b.Verbose.Log("Invalid attribute, could not find Wix/Product [version]");
            }

            xd.Save(fileName);
        }

        private void UpdateNuspecFile(string fileName, string versionText) {
            const string NUGETNAMESPACE = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd";
            XDocument xd = XDocument.Load(fileName);
            XNamespace ns = NUGETNAMESPACE;
            var el2 = xd.Element(ns + "package")?.Element(ns + "metadata")?.Element(ns + "version");
            if (el2 != null) {
                el2.Value = versionText;
            } else {
                b.Verbose.Log("Invalid element in the Nuget file, can not update.");
            }
            xd.Save(fileName);
        }

       

        private bool CheckForAssemblyVersion(string fl) {
            if (AssemblyMM == null) { return false; }
            string assemblyVerString = cv.GetVersionString(DisplayType.Short);
            return CheckAndUpdate(fl, AssemblyMM, assemblyVerString, (theFile, theVn) => {
                UpdateCSFileWithAttribute(fl, ASMFILE_VER_TAG, theVn);
            });
        }

        private bool CheckForInformationalVersion(string fl) {

            if (InfoMM == null) { return false; }
            string assemblyVerString = cv.GetVersionString(DisplayType.Short);

            return CheckAndUpdate(fl, InfoMM, assemblyVerString, (theFile, theVn) => {
                UpdateCSFileWithAttribute(fl, ASMFILE_INFVER_TAG, theVn);
            });

        }

        private bool CheckForWix(string fl) {

            if (WixMM == null) { return false; }
            string assemblyVerString = cv.GetVersionString(DisplayType.Short);

            return CheckAndUpdate(fl, WixMM, assemblyVerString, (theFile, theVn) => {
                // TODO : UpdateWixFileWithVersion(fl, theVn);
            });
        }



        private bool CheckAndUpdate(string fl, Minimatcher assemblyMM, string versionValue, Action<string, string> p) {
            b.Assert.True(p != null, "The action used cant be null");

            b.Verbose.Log("Checking file :" + fl);

            bool result = assemblyMM.IsMatch(fl);
            if ((result) && (File.Exists(fl))) {
                b.Info.Log($"Updating VersioningFile File ({fl}) to ({versionValue})");

                hook?.PreUpdateFileAction(fl); // PreUpdateAllAction?.Invoke(fl);
                //PreUpdateAction?.Invoke(fl);

                p(fl, versionValue);

                hook?.PostUpdateFileAction(fl);


            }
            return result;
        }

        /// <summary>
        /// Either updates an existing version number in a file or creates a new (very basic) assembly info file and adds the verison number to it.  The
        /// version is stored in the attribute that is supplied as the second parameter.
        /// </summary>
        /// <param name="fileName">The full path to the file to either update or create</param>
        /// <param name="targetAttribute">The name of the attribute to write the verison number into</param>
        /// <param name="vn">The verison number to apply to the code</param>
        private void UpdateCSFileWithAttribute(string fileName, string targetAttribute, string versionValue) {
            #region entry code
            b.Assert.True(!string.IsNullOrEmpty(fileName), "fileName is null, internal consistancy error.");
            b.Assert.True(!string.IsNullOrEmpty(targetAttribute), "target attribute cant be null, internal consistancy error");
            b.Assert.True(versionValue != null, "vn cant be null, internal consistancy error");
            #endregion (entry code)

            b.Info.Log($"VersionSupport, Asked to update CS file with the {targetAttribute} attribute", "Full Filename:" + fileName);

            var outputFile = new StringBuilder();


            if (!File.Exists(fileName)) {
                b.Verbose.Log("There was no file, creating file and adding attribute");
                outputFile.Append("using System.Reflection;\r\n");
                outputFile.Append($"[assembly: {targetAttribute}(\"{versionValue}\")]\r\n");
            } else {

                // If it does exist we need to verify that it is not readonly.
                if ((File.GetAttributes(fileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                    b.Warning.Log("The file is readonly, removing attribs so I can write on it", "fname [" + fileName + "]");
                    File.SetAttributes(fileName, (File.GetAttributes(fileName) ^ FileAttributes.ReadOnly));
                }


                // Put this in to identify if there were duplicate entries discovered in the file, this should not be valid but helps to reassure that its not the verisoner that has
                // introduced a compile error into the code.                
                bool replacementMade = false;


                Regex r = GetRegex(targetAttribute);
                using (StreamReader sr = new StreamReader(fileName)) {
                    string nextLine = null;
                    while ((nextLine = sr.ReadLine()) != null) {

                        if ((!nextLine.Trim().StartsWith("//")) && (r.IsMatch(nextLine))) {

                            if (replacementMade) {
                                // One would hope that this would not occur outside of testing, yet surprisingly enough this is not the case.
                                throw new ArgumentException("Invalid CSharp File, duplicate verison attribute discovered", fileName);
                            }

                            //  its the line we are to replace
                            outputFile.Append("[assembly: " + targetAttribute + "(\"");
                            outputFile.Append(versionValue);
                            outputFile.Append("\")]\r\n");
                            replacementMade = true;
                        } else {
                            // All lines except the one we are interested in are copied across.
                            outputFile.Append(nextLine + "\r\n");
                        }

                    }

                    if (!replacementMade) {
                        b.Warning.Log("No " + targetAttribute + " found in file, appending new one.");
                        outputFile.Append($"\r\n[assembly: {targetAttribute}(\"{versionValue}\")]\r\n");
                    }
                }
            }

            File.WriteAllText(fileName, outputFile.ToString(), Encoding.UTF8);

            b.Info.Log("The attribute " + targetAttribute + " was applied to the file " + fileName + " Successfully.");
        }
    }
}