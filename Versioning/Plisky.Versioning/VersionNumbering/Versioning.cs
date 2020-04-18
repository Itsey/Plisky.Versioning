using Minimatch;
using Plisky.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Plisky.CodeCraft {

    public class Versioning {
        protected Bilge b = new Bilge("Plisky-Versioning");
        protected List<Tuple<string, FileUpdateType>> filenamesRegistered = new List<Tuple<string, FileUpdateType>>();
        protected Dictionary<FileUpdateType, List<string>> fileUpdateMinmatchers = new Dictionary<FileUpdateType, List<string>>();
        protected CompleteVersion cv;
        protected VersionFileUpdater vfu;
        protected VersionStorage repo;
        protected bool testMode;

        public Versioning(VersionStorage jvp, bool dryRun = false) {
            b.Verbose.Log($"Versioning Online - DryRun {dryRun}");

            testMode = dryRun;
            repo = jvp;
            cv = repo.GetVersion();
            vfu = new VersionFileUpdater(cv);

            fileUpdateMinmatchers.Add(FileUpdateType.NetAssembly, new List<string>());
            fileUpdateMinmatchers[FileUpdateType.NetAssembly].Add("**\\properties\\assemblyinfo.cs");
            fileUpdateMinmatchers[FileUpdateType.NetAssembly].Add("**\\properties\\commonassemblyinfo.cs");

            fileUpdateMinmatchers.Add(FileUpdateType.Nuspec, new List<string>());
            fileUpdateMinmatchers[FileUpdateType.Nuspec].Add("**\\*.nuspec");
        }

        public void Increment(string newReleaseName=null) {
            if ((!string.IsNullOrEmpty(newReleaseName)) && (newReleaseName != cv.ReleaseName)) {
                cv.ReleaseName = newReleaseName;
            }
            cv.Increment();
        }

        public CompleteVersion Version {
            get { return cv; }
        }

        public Action<string> Logger { get; set; }

        public override string ToString() {
            return cv.ToString();
        }

        public void AddNugetFile(string targetNugetFile) {
            if (targetNugetFile == null) {
                throw new ArgumentNullException(nameof(targetNugetFile));
            }
            if ((string.IsNullOrEmpty(targetNugetFile)) || (!File.Exists(targetNugetFile))) {
                throw new FileNotFoundException("Filename not found", targetNugetFile);
            }

            filenamesRegistered.Add(new Tuple<string, FileUpdateType>(targetNugetFile, FileUpdateType.Nuspec));
        }

        public void AddCSharpFile(string targetCSFile) {
            if (targetCSFile == null) {
                throw new ArgumentNullException(nameof(targetCSFile));
            }
            if ((string.IsNullOrEmpty(targetCSFile)) || (!File.Exists(targetCSFile))) {
                throw new FileNotFoundException("Filename not found", targetCSFile);
            }

            filenamesRegistered.Add(new Tuple<string, FileUpdateType>(targetCSFile, FileUpdateType.NetAssembly));
        }

        public void UpdateAllRegisteredFiles() {
            Log("Update All Files");

            foreach (var f in filenamesRegistered) {
                Log("Updating : " + f);
                vfu.PerformUpdate(f.Item1, f.Item2);
            }
        }

        private void Log(string v) {
            b.Info.Log(v);
            Logger?.Invoke(v);
        }

        public string GetVersion() {
            string result = cv.ToString();
            b.Verbose.Log($"Returning Verison {result}");
            return result;
        }

        public void LoadMiniMatches(params string[] srcFile) {
            b.Verbose.Dump(srcFile, "Load MiniMatchers from Array");
            if (srcFile.Length == 1) {
                if (File.Exists(srcFile[0])) {
                    Log($"Loading MM from file - {srcFile}");
                    srcFile = File.ReadAllLines(srcFile[0]);
                }
            }

            ClearMiniMatchers();

            foreach (var line in srcFile) {
                AddMMLine(line);
            }
        }

        private void AddMMLine(string line) {
            Tuple<FileUpdateType, string> mmPattern = ParseMMStringToPattern(line);

            if (mmPattern != null) {
                if (!fileUpdateMinmatchers.ContainsKey(mmPattern.Item1)) {
                    fileUpdateMinmatchers.Add(mmPattern.Item1, new List<string>());
                }
                fileUpdateMinmatchers[mmPattern.Item1].Add(mmPattern.Item2);
                Log($"{mmPattern.Item1} Registered For {mmPattern.Item2}");
            } else {
                Log($"Invalid MM Line: {line}");
            }
        }

        private Tuple<FileUpdateType, string> ParseMMStringToPattern(string line) {
            Tuple<FileUpdateType, string> result = null;
            var ln = line.Split('|');
            if (ln.Length == 2) {
                // Valid
                if (Enum.TryParse<FileUpdateType>(ln[1], out FileUpdateType fut)) {
                    result = new Tuple<FileUpdateType, string>(fut, ln[0]);
                }
            }


            return result;
        }

        protected virtual IEnumerable<string> ActualGetFiles(string root) {
            return Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories);
        }

        public List<string> SearchForAllFiles(string root) {
            List<string> result = new List<string>();

            Log($"Searching in [{root}]");

            List<Tuple<Minimatcher, FileUpdateType>> mm = new List<Tuple<Minimatcher, FileUpdateType>>();

            foreach (var regmm in fileUpdateMinmatchers.Keys) {
                foreach (var m in fileUpdateMinmatchers[regmm]) {
                    mm.Add(new Tuple<Minimatcher, FileUpdateType>(new Minimatcher(m, new Options { AllowWindowsPaths = true, NoCase =  true }), regmm));
                }
            }

            int totalNoFiles = 0;
            int registered = 0;

            try {
                var fls = ActualGetFiles(root);

                foreach (var l in fls) {
                    totalNoFiles++;

                    for (int j = 0; j < mm.Count; j++) {
                        if (mm[j].Item1.IsMatch(l)) {
                            Log($"MM Match {l} - {mm[j].Item2}, queued for update.");
                            filenamesRegistered.Add(new Tuple<string, FileUpdateType>(l, mm[j].Item2));
                            registered++;
                            result.Add(l);
                        }
                    }
                }
            } catch (UnauthorizedAccessException) {
                Log($"Access Denied - File Searcher Stopped.");
            }
            b.Verbose.Log($"Total Files {totalNoFiles} registered for update {registered}");

            return result;
        }

        public void SaveUpdatedVersion() {
            Log("Updating Version In Storage");
            if (!testMode) {
                repo.Persist(Version);
            }
        }




        public void SetMiniMatches(FileUpdateType target, params string[] versionTargetMinMatch) {
            if (!fileUpdateMinmatchers.ContainsKey(target)) {
                fileUpdateMinmatchers.Add(target, new List<string>());
            }
            fileUpdateMinmatchers[target].AddRange(versionTargetMinMatch);
        }

        public void ClearMiniMatchers() {
            fileUpdateMinmatchers.Clear();
        }
    }
}