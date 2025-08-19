namespace Plisky.CodeCraft;

using System;
using System.IO;
using System.Xml.Linq;

public class DryRunVersionFileUpdater(CompleteVersion cv, IHookVersioningChanges? actions = null) : VersionFileUpdater(cv, actions) {
    protected override void UpdateCSFileWithAttribute(string fileName, string targetAttribute, string versionValue) {
        #region entry code

        b.Assert.True(!string.IsNullOrEmpty(fileName), "fileName is null, internal consistancy error.");
        b.Assert.True(!string.IsNullOrEmpty(targetAttribute), "target attribute cant be null, internal consistancy error");
        b.Assert.True(versionValue != null, "vn cant be null, internal consistancy error");

        #endregion entry code

        b.Info.Log($"VersionSupport, Asked to update CS file with the {targetAttribute} attribute", "Full Filename:" + fileName);

        if (!File.Exists(fileName)) {
            b.Verbose.Log("There was no file, creating file and adding attribute");
        } else {
            // If it does exist we need to verify that it is not readonly.
            if ((File.GetAttributes(fileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                b.Warning.Log("The file is readonly, removing attributes to enable write access", "fname [" + fileName + "]");
            }

            // Put this in to identify if there were duplicate entries discovered in the file, this should not be valid but helps to reassure that its not the versioner that has
            // introduced a compile error into the code.
            bool replacementMade = false;

            var r = VersionFileUpdater.GetRegex(targetAttribute);
            using var sr = new StreamReader(fileName);
            string? nextLine = null;
            while ((nextLine = sr.ReadLine()) != null) {
                if ((!nextLine.Trim().StartsWith("//")) && (r.IsMatch(nextLine))) {
                    if (replacementMade) {
                        // One would hope that this would not occur outside of testing, yet surprisingly enough this is not the case.
                        throw new ArgumentException($"Invalid CSharp File, duplicate version attribute ({targetAttribute}) discovered", fileName);
                    }
                    //  its the line we are to replace
                    replacementMade = true;
                }
            }
            if (!replacementMade) {
                b.Warning.Log("No " + targetAttribute + " found in file, appending new one.");
            }
        }
        b.Info.Log($"DRYRUN - Would have applied the attribute {targetAttribute} to the file {fileName}. Instead Taking No Action.");
    }

    protected override void UpdateWixFile(string fileName, string versionToWrite) {
        const string WIXNAMESPACE = "http://schemas.microsoft.com/wix/2006/wi";
        var xd = XDocument.Load(fileName);
        XNamespace ns = WIXNAMESPACE;
        var el = xd.Element(ns + "Wix")?.Element(ns + "Product");
        if (el == null) {
            b.Verbose.Log("No Wix/Product element found, nothing to do");
            return;
        }

        var at1 = el?.Attribute("Version");
        if (at1 != null) {
            at1.Value = versionToWrite;
            var at2 = el?.Attribute("Name");
            if (at2 != null) {
                b.Verbose.Log("Secondary name element found.");
            }
            b.Info.Log($"DRYRUN - Would have updated Wix file {fileName} to {versionToWrite}.  Instead Taking No Action.");
        } else {
            b.Verbose.Log("Invalid attribute, could not find Wix/Product [version]");
        }
    }

    protected override string UpdateNuspecFile(string fileName, string versionText) {
        const string NUGETNAMESPACE = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd";
        string result;

        b.Verbose.Log("About to load filename", fileName);

        var xd = XDocument.Load(fileName);
        XNamespace ns = NUGETNAMESPACE;
        var el2 = xd.Element(ns + "package")?.Element(ns + "metadata")?.Element(ns + "version");

        if (el2 == null) {
            b.Warning.Log("element package/metadata/version using namespace not found, trying alternative");
            el2 = xd.Element("package")?.Element("metadata")?.Element("version");
        }

        if (el2 != null) {
            result = $"{el2.Name} with {el2.Value} would be set to {versionText}";
            b.Info.Log($"DRYRUN - Would have updated {fileName} to {versionText}.  Instead Taking No Action.");
        } else {
            b.Warning.Log("Invalid element in the Nuget file, can not update.");
            result = "WARNING >> Element not found in nuspec, no changes made.";
        }
        return result;
    }

    protected override void UpdateStdCSPRoj(string fl, string versonToWrite, string propName) {
        const string PROPERTYGROUP_ELNAME = "PropertyGroup";
        const string PROJECT_ELNAME = "Project";
#if DEBUG
        if (!File.Exists(fl)) { throw new InvalidOperationException("Must not be possible, validate that the file exists prior to this point in the code."); }
#endif
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
                b.Verbose.Log($"DRYRUN - Element {propName} not found, would add. Instead taking no action");
            }
            b.Info.Log($"DRYRUN - Would have updated Std C# Project file {fl} with {propName} to {versonToWrite}.  Instead Taking No Action.");
        } else {
            b.Warning.Log($"Unable to locate [{PROPERTYGROUP_ELNAME}] element in the file [{fl}], version update failed");
        }
    }

    protected override string UpdateLiteralReplacer(string fileToCheck, CompleteVersion versonToWrite, DisplayType displayStyle) {
        string inney = File.ReadAllText(fileToCheck);

        string response;
        if (displayStyle == DisplayType.NoDisplay) {
            if (!inney.Contains(RELEASE_NAME_FILE_IDENTIFIER)) {
                response = "WARNING - No Release Name Identifier Found";
            } else {
                response = $"Replacing {RELEASE_NAME_FILE_IDENTIFIER} with {versonToWrite.ReleaseName}";
            }
        } else if (!inney.Contains("XXX-VERSION") && !inney.Contains(RELEASE_NAME_FILE_IDENTIFIER)) {
            response = "WARNING - No Versioning or Release Name Identifier Found, no updates possible";
        } else {
            response = "Replacing XXX-VERSION* with " + versonToWrite.GetVersionString(displayStyle);
            b.Info.Log($"DRYRUN - Would have updated XXX-VERSION* with {versonToWrite.GetVersionString(displayStyle)}");
        }
        return response;
    }
}