using Plisky.CodeCraft;
using Plisky.CodeCraft.Test;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Plisky.Plumbing;
using System;
using System.Diagnostics;

namespace PliskyTool {

    internal class Program {
        public static CommandLineArguments options = new CommandLineArguments();
        private static CompleteVersion versionerUsed;

        private static void Main(string[] args) {
            Console.WriteLine("Plisky Tool - Online");
            
            var clas = new CommandArgumentSupport();

            clas.ArgumentPostfix = "=";
            clas.ProcessArguments(options, args);

            if ((options.Debug)||(!string.IsNullOrEmpty(options.Trace))) {

                Console.WriteLine("Debug Mode, Adding Trace Handler");
                
                Bilge.AddHandler(new ConsoleHandler(),HandlerAddOptions.SingleType);

                Bilge.SetConfigurationResolver((name, inLevel) => {

                    SourceLevels returnLvl = SourceLevels.Verbose;

                    if ((options.Trace !=null) && (options.Trace.ToLowerInvariant()=="info")) {
                        returnLvl = SourceLevels.Information;
                    }
                    
                    if (name == "Plisky-Versioning") {
                        return returnLvl;
                    }
                    return inLevel;
                });
            }

            Bilge b = new Bilge("Plisky-Tool");
            Bilge.Alert.Online("Plisky-Tool");
            b.Verbose.Dump(options, "App Options");          

            if (PerformActionsFromCommandline()) {
                if (versionerUsed != null) {
                    VersioningOutputter vo = new VersioningOutputter(versionerUsed);
                    vo.DoOutput(options.OutputsActive);
                }

                b.Info.Log("All Actions - Complete - Exiting.");
            } else {
                string s = clas.GenerateShortHelp(options, "Plisky Tool");
                Console.WriteLine(s);
            }

          
           
        }

        private static bool PerformActionsFromCommandline() {
            Console.WriteLine("Performing Versioning Actions");

            switch (options.Command) {
                case "CreateVersion":
                    CreateNewVersionStore();
                    return true;

                case "Override":
                    CreateNewPendingIncrement();
                    return true;

                case "UpdateFiles":
                    ApplyVersionIncrement();
                    return true;

                default:
                    Console.WriteLine("Unrecognised Command: "+options.Command);
                    return false;
            }
        }

        private static void CreateNewPendingIncrement() {
            var per = new JsonVersionPersister(Program.options.VersionPersistanceValue);
            Versioning ver = new Versioning(per);
            versionerUsed = ver.Version;

            string verPendPattern = options.QuickValue;

            Console.WriteLine($"Apply Delayed Incremenet. [{ver.ToString()}] using [{verPendPattern}]");
            ver.Version.ApplyPendingVersion(verPendPattern);

            if (!options.TestMode) {
                per.Persist(ver.Version);
                ver.Increment();
                Console.WriteLine($"Saving Overriden Version [{ver.GetVersion()}]");
            } else {
                ver.Version.Increment();
                Console.WriteLine($"Would Save " + ver.Version.ToString());
            }
        }

        private static void ApplyVersionIncrement() {
            Console.WriteLine("Apply Version Increment");
            var per = new JsonVersionPersister(Program.options.VersionPersistanceValue);
            Versioning ver = new Versioning(per, options.TestMode);
            versionerUsed = ver.Version;

            ver.Logger = (v) => {
                Console.WriteLine(v);
            };

            if (options.PerformIncrement) {
                Console.WriteLine("Version Increment Requested - Currently " + ver.GetVersion());
                ver.Increment(options.Release);
            }

            Console.WriteLine("Version To Write: " + ver.GetVersion());

            // Increment done, now persist and then update the pages - first check if the command line ovverrides the minimatchers
            if ((options.VersionTargetMinMatch != null) && (options.VersionTargetMinMatch.Length > 0)) {                
                ver.LoadMiniMatches(options.VersionTargetMinMatch);
            }
            
            ver.SearchForAllFiles(options.Root);

            ver.UpdateAllRegisteredFiles();

            ver.SaveUpdatedVersion();
        }

        private static Action<string> GetActionToPerform(Versioning ver) {
            Action<string> ActionToPerform;
            if (!options.TestMode) {
                ActionToPerform = (fn) => {
                    if (fn.EndsWith(".cs")) {
                        ver.AddCSharpFile(fn);
                    }
                    if (fn.EndsWith(".nuspec")) {
                        ver.AddNugetFile(fn);
                    }
                };
            } else {
                Console.WriteLine("Dry Run Mode Active.");
                ActionToPerform = (fn) => {
                    Console.WriteLine("Would Update :" + fn);
                };
            }

            return ActionToPerform;
        }

        [Conditional("DEBUG")]
        private static void DebugLog(string l) {
#if DEBUG
            Console.WriteLine(l);
#endif
        }

        private static void CreateNewVersionStore() {
            string startVer = "0.0.0.0";
            if (!string.IsNullOrEmpty(options.QuickValue)) {
                Console.WriteLine($"Using Value From Command Line: {options.QuickValue}");
                startVer = options.QuickValue;
            }
            if (!string.IsNullOrEmpty(options.QuickValue)) {
                Console.WriteLine($"Setting Release From Command Line: {options.Release}");
            }
            Console.WriteLine($"Creating New Version Store: {startVer}");

            CompleteVersion cv = new CompleteVersion(startVer);
            versionerUsed = cv;

            cv.ReleaseName = options.Release;
            VersionStorage vs = new JsonVersionPersister(options.VersionPersistanceValue);

            Console.WriteLine($"Saving {cv.GetVersionString()}");
            vs.Persist(cv);
            
        }
    }
}