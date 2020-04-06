using System;
using System.Collections.Generic;
using System.IO;

namespace Plisky.CodeCraft {
    public class Versioning {
        private List<string> NugetFilenames = new List<string>();
        private List<string> CSFilenames = new List<string>();
        private CompleteVersion cv;
        private VersionFileUpdater vfu;

        public Versioning(VersionStorage jvp) {
            cv = jvp.GetVersion();
            vfu = new VersionFileUpdater(cv);
        }

        public void Increment() {
            cv.Increment();
        }

        public CompleteVersion Version {
            get { return cv; }
        }

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

            NugetFilenames.Add(targetNugetFile);
        }

        public void AddCSharpFile(string targetCSFile) {
            if (targetCSFile == null) {
                throw new ArgumentNullException(nameof(targetCSFile));
            }
            if ((string.IsNullOrEmpty(targetCSFile)) || (!File.Exists(targetCSFile))) {
                throw new FileNotFoundException("Filename not found", targetCSFile);
            }

            CSFilenames.Add(targetCSFile);
        }

        public void UpdateAllRegisteredFiles() {
            foreach (var l in NugetFilenames) {
                vfu.PerformUpdate(l, FileUpdateType.Nuspec);
            }

            foreach (var x in CSFilenames) {
                vfu.PerformUpdate(x, FileUpdateType.AssemblyFile);
                vfu.PerformUpdate(x, FileUpdateType.AssemblyInformational);
                vfu.PerformUpdate(x, FileUpdateType.Assembly);

            }
        }

        public string GetVersion() {
            return cv.ToString();
        }
    }
}