using Minimatch;
using Plisky.Plumbing;
using System;
using System.Collections.Generic;
using System.IO;
using Plisky.Diagnostics;

namespace Plisky.CodeCraft {
    public class VersioningTask {
        private Bilge b = new Bilge();
        protected Dictionary<string, List<FileUpdateType>> pendingUpdates = new Dictionary<string, List<FileUpdateType>>();
        protected CompleteVersion ver;
        protected List<string> messageLog = new List<string>();

        public delegate void LogEventHandler(object sender, LogEventArgs e);
        public event LogEventHandler Logger=null;
        public string[] LogMessages {
            get {
                return messageLog.ToArray();
            } }
        private string Test(string thisone) {
            int idx = thisone.IndexOf("]}#");
            string write = thisone.Substring(idx + 7);
            if (Logger!=null) {
                //Console.WriteLine("Calling logger 3");
                Logger(this, new LogEventArgs() {
                    Severity = "INFO",
                    Text = "LOGGER"+write
                });
            }
            Console.WriteLine("CW "+write);
            //messageLog.Add("LG " + write);
            return thisone;
        }
        public VersioningTask() {
        }

        public string PersistanceValue { get; set; }
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
            string[] allLines = verItemsSimple.Split(new string[] { "\n" },StringSplitOptions.RemoveEmptyEntries);
            foreach(var ln in allLines) {
                string[] parts = ln.Split('!');
                if (parts.Length!=2) {
                    throw new InvalidOperationException($"The versioning item string was in the wrong format [{ln}] ");
                }
                FileUpdateType ft =  GetFileTypeFromString(parts[1]);
                AddUpdateType(parts[0], ft);
            }
        }

        private FileUpdateType GetFileTypeFromString(string v) {
            switch (v) {
                case "ASSEMBLY": return FileUpdateType.Assembly;
                case "INFO": return FileUpdateType.AssemblyInformational;
                case "FILE": return FileUpdateType.AssemblyFile;
                case "WIX": return FileUpdateType.Wix;
                default: throw new InvalidOperationException($"The verisoning string {v} is not valid.");                    
            }
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
                        var v = enumer.Current;

                        // Check every file that we have returned.
                        foreach (var chk in pendingUpdates.Keys) {
                            var mm = new Minimatcher(chk, new Options { AllowWindowsPaths = true, IgnoreCase = true });
                            b.Verbose.Log($"Checking {chk} against {v}");
                            if (mm.IsMatch(v)) {
                                b.Info.Log("Match...");
                                // TODO Cache this and make it less loopey
                                VersionFileUpdater sut = new VersionFileUpdater(ver);
                                foreach (var updateType in pendingUpdates[chk]) {
                                    b.Verbose.Log($"Perform update {v}");
                                    sut.PerformUpdate(v, updateType);
                                }

                            }
                        }
                    }

                } catch (System.UnauthorizedAccessException) {
                    // If you run through all the filles in a directory you can hit areas of the filesystem
                    // that you dont have access to - this skips those files and then continues.
                    b.Verbose.Log("Unauthorised area of the filesystem, skipping");
                }

            }
            
            VersionString = ver.GetVersionString();
        }

        private void SaveVersioningComponent() {
            var jvg = new JsonVersionPersister(PersistanceValue);
            jvg.Persist(ver);
        }

        private void ValidateForUpdate() {
            if ((String.IsNullOrEmpty(BaseSearchDir))||(!Directory.Exists(BaseSearchDir))) {
                throw new DirectoryNotFoundException("The BaseSearchDirectory has to be specified");
            }
        }

        private void LoadVersioningComponent() {
            var jvg = new JsonVersionPersister(PersistanceValue);
            ver = jvg.GetVersion();
        }
    }
}