using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using GlobExpressions;
using Plisky.Diagnostics;

namespace Plisky.CodeCraft;

public class VersioningTask {
    public delegate void LogEventHandler(object sender, LogEventArgs e);

    private readonly Bilge b = new();
    protected List<string> messageLog = new();
    protected Dictionary<string, List<FileUpdateType>> pendingUpdates = new();
    private string? persistanceValue;
    protected VersionStorage? storage;
    protected CompleteVersion? ver;

    public VersioningTask() {
    }

    public string? BaseSearchDir { get; set; }

    public string[] LogMessages {
        get { return messageLog.ToArray(); }
    }

    public string? VersionString { get; set; }

#pragma warning disable CS0067 // Event is never used

    public event LogEventHandler? Logger;

#pragma warning restore CS0067

    public void AddUpdateType(string minmatchPattern, FileUpdateType updateToPerform) {
        b.Verbose.Log("Adding Update Type " + minmatchPattern);
        if (!pendingUpdates.ContainsKey(minmatchPattern)) {
            pendingUpdates.Add(minmatchPattern, new List<FileUpdateType>());
        }
        pendingUpdates[minmatchPattern].Add(updateToPerform);
    }

    public void IncrementAndUpdateAll() {
        b.Verbose.Log("IncrementAndUpdateAll called");
        LoadVersioningComponent();
        ValidateForUpdate();
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
                        var mm = new Glob(chk.Replace('\\', '/'), GlobOptions.CaseInsensitive);
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
                // If you run through all the files in a directory you can hit areas of the filesystem
                // that you dont have access to - this skips those files and then continues.
                b.Verbose.Log("Unauthorised area of the filesystem, skipping");
            }
        }

        VersionString = ver.GetVersionString();
    }

    public void SetPersistanceValue(string pv) {
        persistanceValue = pv;
        storage = VersionStorage.CreateFromInitialisation(pv);
    }

    private void LoadVersioningComponent() {
        ValidateStorageSet();
        ver = storage.GetVersion();
    }

    private void SaveVersioningComponent() {
        ValidateForUpdate();
        ValidateStorageSet();
        storage.Persist(ver);
    }

    [MemberNotNull(nameof(ver))]
    [MemberNotNull(nameof(BaseSearchDir))]
    private void ValidateForUpdate() {
        if (string.IsNullOrEmpty(BaseSearchDir) || !Directory.Exists(BaseSearchDir)) {
            throw new DirectoryNotFoundException("The BaseSearchDirectory has to be specified");
        }
        if (ver == null) {
            throw new InvalidOperationException("The versioning component has not been loaded correctly.");
        }
    }

    [MemberNotNull(nameof(storage))]
    private void ValidateStorageSet() {
        if (storage == null) {
            throw new InvalidOperationException("The storage component has not been set, please call SetPersistanceValue first.");
        }
    }
}