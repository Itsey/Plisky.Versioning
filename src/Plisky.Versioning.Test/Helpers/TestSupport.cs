using System;
using System.IO;
using System.Xml.Linq;
using Plisky.Diagnostics;
using Plisky.Test;

namespace Plisky.CodeCraft.Test;

public class TestSupport(UnitTestHelper newuth) {
    private readonly UnitTestHelper uth = newuth;

    public string CreateStoredVersionNumer() {
        string fn = uth.NewTemporaryFileName(true);
        var cv = GetDefaultVersion();
        var jvp = new JsonVersionPersister(fn);
        jvp.Persist(cv);
        return fn;
    }

    public static bool DoesFileContainThisText(string fn, string v) {
        return File.ReadAllText(fn).Contains(v);
    }

    public static CompleteVersion GetDefaultVersion() {
        return new CompleteVersion(
            new VersionUnit("0", "", DigitIncrementBehaviour.ContinualIncrement),
            new VersionUnit("0", ".", DigitIncrementBehaviour.ContinualIncrement),
            new VersionUnit("0", ".", DigitIncrementBehaviour.ContinualIncrement),
            new VersionUnit("0", ".", DigitIncrementBehaviour.ContinualIncrement)
        );
    }

    public string GetFileAsTemporary(string srcFile) {
        string fn = uth.NewTemporaryFileName(true);
        File.Copy(srcFile, fn);
        return fn;
    }

    public static string GetVersion(FileUpdateType fut, string srcFile) {
        return fut switch {
            FileUpdateType.Nuspec => GetVersionFromNuspec(srcFile),
            FileUpdateType.StdAssembly => GetVersionFromCSProj(srcFile, "AssemblyVersion"),
            FileUpdateType.StdInformational => GetVersionFromCSProj(srcFile, "Version"),
            FileUpdateType.StdFile => GetVersionFromCSProj(srcFile, "FileVersion"),
            FileUpdateType.Wix => GetVersionFromWix(srcFile),
            _ => throw new NotImplementedException(),
        };
    }

    public static string GetVersionFromCSProj(string srcFile, string propName) {
        var xd2 = XDocument.Load(srcFile);
        var el2 = xd2.Element("Project")?.Element("PropertyGroup")?.Element(propName);
        if (el2 == null) { return null!; }
        string after = el2!.Value;
        return after;
    }

    public static string GetVersionFromNuspec(string srcFile) {
        XNamespace ns = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd";
        var xd2 = XDocument.Load(srcFile);
        var el2 = xd2.Element(ns + "package")?.Element(ns + "metadata")?.Element(ns + "version");
        string after = el2!.Value;
        return after;
    }

    public static string GetVersionFromWix(string srcFile) {
        XNamespace ns = "http://schemas.microsoft.com/wix/2006/wi";
        var xd2 = XDocument.Load(srcFile);
        var el2 = xd2.Element(ns + "Wix")?.Element(ns + "Product")?.Attribute("Version");
        string after = el2!.Value;
        return after;
    }
}