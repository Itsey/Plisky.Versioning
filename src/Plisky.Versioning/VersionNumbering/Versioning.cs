using System;
using System.Collections.Generic;
using System.IO;
using GlobExpressions;
using Plisky.Diagnostics;
using Plisky.Plumbing;
using Plisky.Versioning;

namespace Plisky.CodeCraft;

public class Versioning {
    protected Bilge b = new("Plisky-Versioning");
    protected CompleteVersion cv;
    protected Dictionary<FileUpdateType, List<string>> fileUpdateMinmatchers = [];
    protected List<Tuple<string, FileUpdateType>> filenamesRegistered = [];
    protected VersionStorage repo;
    protected bool testMode;
    protected VersionFileUpdater vfu;

    public Versioning(VersionStorage jvp, bool dryRun = false, Hub? hb = null) {
        b.Verbose.Log($"Versioning Online - DryRun {dryRun}");

        testMode = dryRun;
        repo = jvp;
        cv = repo.GetVersion();

        Messenger = hb ?? Hub.Current;

        if (dryRun) {
            vfu = new DryRunVersionFileUpdater(cv);
        } else {
            vfu = new VersionFileUpdater(cv);
        }
    }

    public string FileUpdateDisplayGroups { get; set; } = string.Empty;

    public Hub Messenger { get; set; }

    public CompleteVersion Version {
        get { return cv; }
    }

    public void AddCSharpFile(string targetCSFile) {
        ArgumentNullException.ThrowIfNull(targetCSFile);
        if ((string.IsNullOrEmpty(targetCSFile)) || (!File.Exists(targetCSFile))) {
            throw new FileNotFoundException("Filename not found", targetCSFile);
        }

        filenamesRegistered.Add(new Tuple<string, FileUpdateType>(targetCSFile, FileUpdateType.NetAssembly));
    }

    public void AddNugetFile(string targetNugetFile) {
        ArgumentNullException.ThrowIfNull(targetNugetFile);

        if ((string.IsNullOrEmpty(targetNugetFile)) || (!File.Exists(targetNugetFile))) {
            throw new FileNotFoundException("Filename not found", targetNugetFile);
        }

        filenamesRegistered.Add(new Tuple<string, FileUpdateType>(targetNugetFile, FileUpdateType.Nuspec));
    }

    public void ClearMiniMatchers() {
        b.Verbose.Log("Clearing Minimatchers");
        fileUpdateMinmatchers.Clear();
    }

    public string GetBehaviour(string digit) {
        b.Verbose.Log("Returning Behaviours");
        string result = cv.GetBehaviourString(digit);
        return result;
    }

    public string GetVersion() {
        string result = cv.ToString();
        b.Verbose.Log($"Returning Verison {result}");
        return result;
    }

    public void Increment(string? newReleaseName = null) {
        if ((!string.IsNullOrEmpty(newReleaseName)) && (newReleaseName != cv.ReleaseName)) {
            cv.ReleaseName = newReleaseName;
        }
        cv.Increment();
    }

    public void LoadMiniMatches(params string[] srcFile) {
        b.Verbose.Dump(srcFile, "Load MiniMatchers from Array");
        if (srcFile.Length == 1) {
            if (File.Exists(srcFile[0])) {
                Log($"Loading MM from file - {srcFile[0]}");
                srcFile = File.ReadAllLines(srcFile[0]);
            }
        }

        ClearMiniMatchers();

        foreach (string line in srcFile) {
            AddMMLine(line);
        }
    }

    public void SaveUpdatedVersion() {
        if (!testMode) {
            Log("Updating Version In Storage");
            repo.Persist(Version);
        } else {
            Log("DryRun enabled, not updating version storage.");
        }
    }

    public List<string> SearchForAllFiles(string root) {
        var result = new List<string>();

        Log($"Searching in [{root}]");

        var mm = new List<Tuple<Glob, FileUpdateType>>();

        foreach (var regmm in fileUpdateMinmatchers.Keys) {
            foreach (string m in fileUpdateMinmatchers[regmm]) {
                mm.Add(new Tuple<Glob, FileUpdateType>(new Glob(m.Replace('\\', '/'), GlobOptions.CaseInsensitive), regmm));
            }
        }

        int totalNoFiles = 0;
        int registered = 0;

        try {
            var fls = ActualGetFiles(root);

            foreach (string l in fls) {
                totalNoFiles++;

                foreach (var t in mm) {
                    if (t.Item1.IsMatch(l)) {
                        Log($"MM Match {l} - {t.Item2}, queued for update.");
                        filenamesRegistered.Add(new Tuple<string, FileUpdateType>(l, t.Item2));
                        registered++;
                        result.Add(l);
                    } else {
                        b.Verbose.Log($"No Match > {l}");
                    }
                }
            }
        } catch (UnauthorizedAccessException) {
            Warning("Access Denied - File Searcher Stopped.");
        }
        b.Verbose.Log($"Total Files {totalNoFiles} registered for update {registered}");

        return result;
    }

    public void SetMiniMatches(FileUpdateType target, params string[] versionTargetMinMatch) {
        if (!fileUpdateMinmatchers.ContainsKey(target)) {
            fileUpdateMinmatchers.Add(target, []);
        }
        fileUpdateMinmatchers[target].AddRange(versionTargetMinMatch);
    }

    public override string ToString() {
        return cv.ToString();
    }

    public int UpdateAllRegisteredFiles() {
        Log("Update All Files");

        int numberFilesUpdated = 0;

        foreach (var f in filenamesRegistered) {
            Log("Updating : " + f);
            string s = vfu.PerformUpdate(f.Item1, f.Item2, groupNamesForDisplay: FileUpdateDisplayGroups);
            if (!testMode) {
                Log($"Updated : {s}");
                b.Verbose.Log($"Update Completed {f.Item1} : {f.Item2}");
            } else {
                Log($"DRYRUN - Would have updated : {s}. Instead taking no action.");
            }

            numberFilesUpdated++;
        }

        if (numberFilesUpdated == 0) {
            Warning("WARNING - No files found to update.");
        }
        return numberFilesUpdated;
    }

    public void UpdateBehaviour(string digitToUpdate, DigitIncrementBehaviour newBehaviour) {
        b.Verbose.Log($"Updating Behaviour for digit {digitToUpdate} to behaviour {newBehaviour}");
        cv.ApplyBehaviourUpdate(digitToUpdate, newBehaviour);
    }

    protected virtual IEnumerable<string> ActualGetFiles(string root) {
        return Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories);
    }

    private static Tuple<FileUpdateType, string>? ParseMMStringToPattern(string line) {
        Tuple<FileUpdateType, string>? result = null;
        string[] ln = line.Split('|');
        if (ln.Length == 2) {
            // Valid
            if (Enum.TryParse<FileUpdateType>(ln[1], out var fut)) {
                result = new Tuple<FileUpdateType, string>(fut, ln[0]);
            }
        }
        return result;
    }

    private void AddMMLine(string line) {
        var mmPattern = ParseMMStringToPattern(line);

        if (mmPattern != null) {
            if (!fileUpdateMinmatchers.ContainsKey(mmPattern.Item1)) {
                fileUpdateMinmatchers.Add(mmPattern.Item1, []);
            }
            fileUpdateMinmatchers[mmPattern.Item1].Add(mmPattern.Item2);
            Log($"{mmPattern.Item1} Registered For {mmPattern.Item2}");
        } else {
            Warning($"Invalid MM Line: {line}");
        }
    }

    private void Log(string v) {
        b.Info.Log(v);
        Messenger.Launch(new SimpleMessage(v));
    }

    private void Warning(string v) {
        b.Warning.Log(v);
        Messenger.Launch(new SimpleMessage(v));
    }
}