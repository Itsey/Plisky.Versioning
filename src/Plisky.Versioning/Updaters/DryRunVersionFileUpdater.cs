using System.Xml.Linq;

namespace Plisky.CodeCraft;

public class DryRunVersionFileUpdater(CompleteVersion cv) : VersionFileUpdater(cv) {
    protected override void SaveUpdatedReplacedFile(string fileToCheck, string fileText) {
        b.Info.Log($"DRYRUN - Would have updated file {fileToCheck}.  Instead Taking No Action.");
    }

    protected override void SaveUpdatedNuspecFile(string fileName, XDocument xd) {
        b.Info.Log($"DRYRUN - Would have updated Nuspec file {fileName}.  Instead Taking No Action.");
    }

    protected override void SaveUpdatedCSProj(string fl, XDocument xd2) {
        b.Info.Log($"DRYRUN - Would have updated Std C# Project file {fl}.  Instead Taking No Action.");
    }

    protected override void SaveUpdateWix(string fileName, XDocument xd) {
        b.Info.Log($"DRYRUN - Would have updated Wix file {fileName}.  Instead Taking No Action.");
    }
}
