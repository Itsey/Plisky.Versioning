namespace Plisky.CodeCraft;

using System;
using System.Collections.Generic;
using System.IO;
using Minimatch;
using Plisky.Diagnostics;

public class VersioningTask {
    private readonly Bilge b = new();
    protected Dictionary<string, List<FileUpdateType>> pendingUpdates = new();
    protected CompleteVersion ver;
    protected List<string> messageLog = new();

    public delegate void LogEventHandler(object sender, LogEventArgs e);

    public event LogEventHandler? Logger = null;

    public string[] LogMessages {
        get {
            return messageLog.ToArray();
        }
    }

    public VersioningTask() {
    }

    protected VersionStorage storage;
    private string persistanceValue;
    public void SetPersistanceValue(string pv) {
        persistanceValue = pv;
        storage = VersionStorage.CreateFromInitialisation(pv);
    }


    public string VersionString { get; set; }
    public string BaseSearchDir { get; set; }

    public void AddUpdateType(string minmatchPattern, FileUpdateType updateToPerform) {
        b.Verbose.Log("Adding Update Type " + minmatchPattern);
        if (!pendingUpdates.ContainsKey(minmatchPattern)) {
            pendingUpdates.Add(minmatchPattern, new List<FileUpdateType>());
        }
        pendingUpdates[minmatchPattern].Add(updateToPerform);
    }

    public void SetAllVersioningItems(string verItemsSimple) {
        b.Info.Log("SetAllVersioningITems");
        if (verItemsSimple.Contains(Environment.NewLine)) {
            // The TFS build agent uses \n not Environment.Newline for its line separator, however unit tests use Environment.Newline
            // so replacing them with \n to make the two consistant.
            verItemsSimple = verItemsSimple.Replace(Environment.NewLine, "\n");
        }
        string[] allLines = verItemsSimple.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string ln in allLines) {
            string[] parts = ln.Split('!');
            if (parts.Length != 2) {
                throw new InvalidOperationException($"The versioning item string was in the wrong format [{ln}] ");
            }
            var ft = GetFileTypeFromString(parts[1]);
            AddUpdateType(parts[0], ft);
        }
    }

    private FileUpdateType GetFileTypeFromString(string v) {
        return v switch {
            "ASSEMBLY" => FileUpdateType.NetAssembly,
            "INFO" => FileUpdateType.NetInformational,
            "FILE" => FileUpdateType.NetFile,
            "WIX" => FileUpdateType.Wix,
            _ => throw new InvalidOperationException($"The verisoning string {v} is not valid."),
        };
    }

    public void IncrementAndUpdateAll() {
        b.Verbose.Log("IncrementAndUpdateAll called");
        ValidateForUpdate();
        LoadVersioningComponent();
        b.Verbose.Log("Versioning Loaded ");
        ver.Increment();
        b.Verbose.Log("Saving");
        SaveVersioningComponent();
        b.Verbose.Log($"Searching {BaseSearchDir} there are {pendingUpdates.Count} pends.");

        var enumer = Directory.EnumerateFiles(BaseSearchDir, "*.*", SearchOption.AllDirectories).GetEnumerator();
        bool shouldContinue = true;
        while (shouldContinue) {
            try {
                shouldContinue = enumer.MoveNext();
                if (shouldContinue) {
                    string v = enumer.Current;

                    // Check every file that we have returned.
                    foreach (string chk in pendingUpdates.Keys) {
                        var mm = new Minimatcher(chk, new Options { AllowWindowsPaths = true, IgnoreCase = true });
                        b.Verbose.Log($"Checking {chk} against {v}");
                        if (mm.IsMatch(v)) {
                            b.Info.Log("Match...");
                            // TODO Cache this and make it less loopey
                            var sut = new VersionFileUpdater(ver);
                            foreach (var updateType in pendingUpdates[chk]) {
                                b.Verbose.Log($"Perform update {v}");
                                _ = sut.PerformUpdate(v, updateType);
                            }
                        }
                    }
                }
            } catch (UnauthorizedAccessException) {
                // If you run through all the filles in a directory you can hit areas of the filesystem
                // that you dont have access to - this skips those files and then continues.
                b.Verbose.Log("Unauthorised area of the filesystem, skipping");
            }
        }

        VersionString = ver.GetVersionString();
    }

    private void SaveVersioningComponent() {
        storage.Persist(ver);
    }

    private void ValidateForUpdate() {
        if (string.IsNullOrEmpty(BaseSearchDir) || !Directory.Exists(BaseSearchDir)) {
            throw new DirectoryNotFoundException("The BaseSearchDirectory has to be specified");
        }
    }

    private void LoadVersioningComponent() {
        ver = storage.GetVersion();
    }
}