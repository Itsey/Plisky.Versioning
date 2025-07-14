namespace Plisky.CodeCraft;

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Plisky.Diagnostics;

public class DryRunVersionFileUpdater : VersionFileUpdater {

    protected override void UpdateCSFileWithAttribute(string fileName, string targetAttribute, string versionValue) {
        b.Info.Log($"DRYRUN - Would have updated {fileName} with {targetAttribute} to {versionValue}.  Instead Taking No Action.");
    }

    public DryRunVersionFileUpdater(CompleteVersion cv, IHookVersioningChanges? actions = null) : base(cv, actions) {
    }
}

public class VersionFileUpdater {
    protected Bilge b = new Bilge("Plisky-Versioning");
    private const string ASMFILE_FILEVER_TAG = "AssemblyFileVersion";
    private const string ASMFILE_VER_TAG = "AssemblyVersion";
    private const string ASMFILE_INFVER_TAG = "AssemblyInformationalVersion";
    private const string ASM_STD_ASMVTAG = "AssemblyVersion";
    private const string ASM_STD_VERSTAG = "Version";
    private const string ASM_STD_FILETAG = "FileVersion";
    private const string RELEASE_NAME_FILE_IDENTIFIER = "XXX-RELEASENAME-XXX";

    private IHookVersioningChanges? hook;
    private CompleteVersion cv;

    public VersionFileUpdater() {
        cv = new CompleteVersion();
    }

    public VersionFileUpdater(CompleteVersion cv, IHookVersioningChanges? actions = null) {
        this.cv = cv;
    }

    public Regex GetRegex(string targetAttribute) {
        return new Regex("\\s*\\[\\s*assembly\\s*:\\s*" + targetAttribute + "\\s*\\(\\s*\\\"\\s*[0-9A-z\\-.*]*\\s*\\\"\\s*\\)\\s*\\]", RegexOptions.IgnoreCase);
    }

    public string PerformUpdate(string fl, FileUpdateType fut, DisplayType dt = DisplayType.Default) {
        b.Verbose.Log("Perform update requested " + fut.ToString(), fl);

        if (!File.Exists(fl)) {
            throw new FileNotFoundException($"Filename must be present for version update {fl}");
        }

        string responseLog = string.Empty;

        var dtx = cv.GetDisplayType(fut, dt);
        string versonToWrite = cv.GetVersionString(dtx);
        switch (fut) {
            case FileUpdateType.NetAssembly:
                UpdateCSFileWithAttribute(fl, ASMFILE_VER_TAG, versonToWrite);
                responseLog = $"Updated {ASMFILE_FILEVER_TAG} to {versonToWrite}";
                break;

            case FileUpdateType.NetInformational:
                UpdateCSFileWithAttribute(fl, ASMFILE_INFVER_TAG, versonToWrite);
                responseLog = $"Updated {ASMFILE_INFVER_TAG} to {versonToWrite}";
                break;

            case FileUpdateType.NetFile:
                UpdateCSFileWithAttribute(fl, ASMFILE_FILEVER_TAG, versonToWrite);
                responseLog = $"Updated {ASMFILE_FILEVER_TAG} to {versonToWrite}";
                break;

            case FileUpdateType.Wix:
                UpdateWixFile(fl, versonToWrite);
                responseLog = $"Updated Wix file to {versonToWrite}";
                break;

            case FileUpdateType.Nuspec:
                UpdateNuspecFile(fl, versonToWrite);
                responseLog = $"Updated Nuspec file to {versonToWrite}";
                break;

            case FileUpdateType.StdAssembly:
                UpdateStdCSPRoj(fl, versonToWrite, ASM_STD_ASMVTAG);
                responseLog = $"Updated Std Assembly file to {versonToWrite}";
                break;

            case FileUpdateType.StdFile:
                UpdateStdCSPRoj(fl, versonToWrite, ASM_STD_FILETAG);
                responseLog = $"Updated Std File file to {versonToWrite}";
                break;

            case FileUpdateType.StdInformational:
                UpdateStdCSPRoj(fl, versonToWrite, ASM_STD_VERSTAG);
                responseLog = $"Updated Std Informational file to {versonToWrite}";
                break;

            case FileUpdateType.TextFile:
                responseLog = UpdateLiteralReplacer(fl, cv, dtx);
                break;

            default:
                throw new NotImplementedException("The file type requested does not have a way to update files currently.");
        }
        return responseLog;
    }

    private string UpdateLiteralReplacer(string fileToCheck, CompleteVersion versonToWrite, DisplayType displayStyle) {
#if DEBUG
        if (!File.Exists(fileToCheck)) { throw new InvalidOperationException("Must not be possible, check this before you reach this code"); }
#endif
        string response = string.Empty;

        Func<string, string> replacer;

        if (displayStyle == DisplayType.NoDisplay) {
            replacer = new Func<string, string>((inney) => {
                if (!inney.Contains(RELEASE_NAME_FILE_IDENTIFIER)) {
                    response = "WARNING - No Release Name Identifier Found";
                    return inney;
                } else {
                    response = $"Replacing {RELEASE_NAME_FILE_IDENTIFIER} with {versonToWrite.ReleaseName}";
                    return inney.Replace(RELEASE_NAME_FILE_IDENTIFIER, versonToWrite.ReleaseName);
                }
            });
        } else {
            replacer = new Func<string, string>((inney) => {
                if (!inney.Contains("XXX-VERSION") && !inney.Contains(RELEASE_NAME_FILE_IDENTIFIER)) {
                    response = "WARNING - No Versioning or Release Name Identifier Found, no updates possible.";
                    return inney;
                }
                response = "Replacing XXX-VERSION* with " + versonToWrite.GetVersionString(displayStyle);

                return inney.Replace(RELEASE_NAME_FILE_IDENTIFIER, versonToWrite.ReleaseName)
                .Replace("XXX-VERSION-XXX", versonToWrite.GetVersionString(displayStyle))
                .Replace("XXX-VERSION3-XXX", versonToWrite.GetVersionString(DisplayType.ThreeDigit))
                .Replace("XXX-VERSION2-XXX", versonToWrite.GetVersionString(DisplayType.Short))
                .Replace("XXX-VERSION4-XXX", versonToWrite.GetVersionString(DisplayType.Full));
            });
        }

        string fileText = replacer(File.ReadAllText(fileToCheck));
        File.WriteAllText(fileToCheck, fileText);
        return response;
    }

    private void UpdateStdCSPRoj(string fl, string versonToWrite, string propName) {
        const string PROPERTYGROUP_ELNAME = "PropertyGroup";
        const string PROJECT_ELNAME = "Project";
#if DEBUG
        if (!File.Exists(fl)) { throw new InvalidOperationException("Must not be possible, validate that the file exists prior to this point in the code."); }
#endif
        b.Info.Log($"Updating NetStd style file with ver {versonToWrite} property {propName}", fl);

        var xd2 = XDocument.Load(fl);

        var el2 = xd2.Element(PROJECT_ELNAME);
        if (el2 == null) {
            b.Error.Log($"Unable to locate [{PROJECT_ELNAME}] element in file [{fl}], version update failed.", "Likely this is not a .net standard csproj but a framework one.");
            return;
        }

        var propGroupElement = el2?.Element(PROPERTYGROUP_ELNAME);
        if (propGroupElement != null) {
            b.Verbose.Log("PropertyGroup Matched");
            var versionElementToUpdate = propGroupElement.Element(propName);
            if (versionElementToUpdate == null) {
                b.Verbose.Log($"Element {propName} not found, Adding.");
                versionElementToUpdate = new XElement(propName);
                propGroupElement.Add(versionElementToUpdate);
            }
            versionElementToUpdate.Value = versonToWrite;
        } else {
            b.Warning.Log($"Unable to locate [{PROPERTYGROUP_ELNAME}] element in the file [{fl}], version update failed");
        }

        xd2.Save(fl);
    }

    private void UpdateWixFile(string fileName, string versionToWrite) {
        const string WIXNAMESPACE = "http://schemas.microsoft.com/wix/2006/wi";
        var xd = XDocument.Load(fileName);
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

    private string UpdateNuspecFile(string fileName, string versionText) {
        const string NUGETNAMESPACE = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd";
        string result = string.Empty;

        b.Verbose.Log("About to load filename", fileName);

        var xd = XDocument.Load(fileName);
        XNamespace ns = NUGETNAMESPACE;
        var el2 = xd.Element(ns + "package")?.Element(ns + "metadata")?.Element(ns + "version");

        if (el2 == null) {
            b.Warning.Log("element package/metadata/version using namespace not found, trying alternative");
            el2 = xd.Element("package")?.Element("metadata")?.Element("version");
        }

        if (el2 != null) {
            result = $"{el2.Name} with {el2.Value} being set to {versionText}";
            try {
                b.Verbose.Log($"Performing Update - {result}");
                el2.Value = versionText;
                b.Verbose.Log($"About to save. - {fileName}");

                xd.Save(fileName);
                string[] x = File.ReadAllLines(fileName);
                foreach (string n in x) {
                    b.Verbose.Log("LINE: " + n);
                }

                b.Verbose.Log(xd.Element(ns + "package")?.Element(ns + "metadata")?.Element(ns + "version")?.Value);
            } catch (Exception ex) {
                b.Warning.Dump(ex, "Unable to save nuspec file");
                result = "WARNING >> Unable to save nuspec file, update failed.";
            }
        } else {
            b.Warning.Log("Invalid element in the Nuget file, can not update.");
            result = "WARNING >> Element not found in nuspec, no changes made.";
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
    protected virtual void UpdateCSFileWithAttribute(string fileName, string targetAttribute, string versionValue) {

        #region entry code

        b.Assert.True(!string.IsNullOrEmpty(fileName), "fileName is null, internal consistancy error.");
        b.Assert.True(!string.IsNullOrEmpty(targetAttribute), "target attribute cant be null, internal consistancy error");
        b.Assert.True(versionValue != null, "vn cant be null, internal consistancy error");

        #endregion entry code

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

            var r = GetRegex(targetAttribute);
            using (var sr = new StreamReader(fileName)) {
                string? nextLine = null;
                while ((nextLine = sr.ReadLine()) != null) {
                    if ((!nextLine.Trim().StartsWith("//")) && (r.IsMatch(nextLine))) {
                        if (replacementMade) {
                            // One would hope that this would not occur outside of testing, yet surprisingly enough this is not the case.
                            throw new ArgumentException($"Invalid CSharp File, duplicate verison attribute ({targetAttribute}) discovered", fileName);
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