using Minimatch;
using System;
using System.Collections.Generic;
using System.IO;

namespace Plisky.CodeCraft {

    public class Versioning {
        protected List<Tuple<string, FileUpdateType>> filenamesRegistered = new List<Tuple<string, FileUpdateType>>();
        protected Dictionary<FileUpdateType, List<string>> fileUpdateMinmatchers = new Dictionary<FileUpdateType, List<string>>();
        protected CompleteVersion cv;
        protected VersionFileUpdater vfu;
        protected VersionStorage repo;
        protected bool testMode;

        public Versioning(VersionStorage jvp, bool dryRun = false) {
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

        public void Increment() {
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
            foreach (var f in filenamesRegistered) {
                vfu.PerformUpdate(f.Item1, f.Item2);
            }
        }

        public string GetVersion() {
            return cv.ToString();
        }

        public void LoadVersioningMinmatchersFromSourceFile(string srcFile) {
            if (!File.Exists(srcFile)) {
                throw new FileNotFoundException("Versioning File Must Exist", srcFile);
            }

            ClearMiniMatchers();

            foreach (var line in File.ReadAllLines(srcFile)) {
                var ln = line.Split('|');
                if (ln.Length == 2) {
                    // Valid
                    if (Enum.TryParse<FileUpdateType>(ln[1], out FileUpdateType fut)) {
                        if (!fileUpdateMinmatchers.ContainsKey(fut)) {
                            fileUpdateMinmatchers.Add(fut, new List<string>());
                        }
                        fileUpdateMinmatchers[fut].Add(ln[0]);
                    }
                } else {
                    // TODO : Log invalid
                }
            }
        }

        public List<string> SearchForAllFiles(string root) {
            List<string> result = new List<string>();

            Logger?.Invoke($"Searching in [{root}]");

            List<Tuple<Minimatcher, FileUpdateType>> mm = new List<Tuple<Minimatcher, FileUpdateType>>();

            foreach (var regmm in fileUpdateMinmatchers.Keys) {
                foreach (var m in fileUpdateMinmatchers[regmm]) {
                    mm.Add(new Tuple<Minimatcher, FileUpdateType>(new Minimatcher(m, new Options { AllowWindowsPaths = true }), regmm));
                }
            }

            var fls = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories);

            foreach (var l in fls) {
                for (int j = 0; j < mm.Count; j++) {
                    if (mm[j].Item1.IsMatch(l)) {
                        Logger?.Invoke("Match :" + l);
                        filenamesRegistered.Add(new Tuple<string, FileUpdateType>(l, mm[j].Item2));
                        result.Add(l);
                        break;
                    }
                }
            }

            return result;
        }

        public void SaveUpdatedVersion() {
            Logger?.Invoke("Updating Version In Storage");
            if (!testMode) {
                repo.Persist(Version);
            }
        }

        public void ApplyUpdatesToAllFiles() {
            /* Action<string> ActionToPerform = GetActionToPerform(ver);

             foreach (var l in allFiles) {
                 ActionToPerform(l);
             }

     */
            UpdateAllRegisteredFiles();
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