using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Plisky.Plumbing;
using System;
using System.Diagnostics;

namespace PliskyTool {

    internal class Program {
        public static CommandLineARguments Options = new CommandLineARguments();

        private static void Main(string[] args) {
            Console.WriteLine("Plisky Tool - Online");
            
            
            
            CommandArgumentSupport clas = new CommandArgumentSupport();

            clas.ArgumentPostfix = "=";
            clas.ProcessArguments(Options, args);

            if (Options.Debug) {
                Console.WriteLine("Debug Mode, Adding Trace Handler");
                Bilge.AddMessageHandler(new TCPHandler("192.168.1.15", 9060));
                Bilge.SetConfigurationResolver((name, inLevel) => {

                    if (name == "Plisky-Versioning") {
                        return SourceLevels.Verbose;
                    }
                    return inLevel;
                });
            }

            Bilge b = new Bilge("Plisky-Versioning");

            b.Info.Log("Online");            
            b.Verbose.Dump(Options, "App Options");

            Console.WriteLine(Options.QuickValue);

            if (PerformActionsFromCommandline()) {
                b.Info.Log("Complete.");
            } else {
                string s = clas.GenerateShortHelp(Options, "Plisky Tool");
                Console.WriteLine(s);
            }
           
        }

        private static bool PerformActionsFromCommandline() {
            Console.WriteLine("Performing Versioning Actions");

            switch (Options.Command) {
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
                    return false;
            }
        }

        private static void CreateNewPendingIncrement() {
            var per = new JsonVersionPersister(Program.Options.VersionPersistanceValue);
            Versioning ver = new Versioning(per);

            string verPendPattern = Options.QuickValue;

            Console.WriteLine($"Apply Delayed Incremenet. [{ver.ToString()}] using [{verPendPattern}]");
            ver.Version.ApplyPendingVersion(verPendPattern);

            if (!Options.TestMode) {
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
            var per = new JsonVersionPersister(Program.Options.VersionPersistanceValue);
            Versioning ver = new Versioning(per, Options.TestMode);
            ver.Logger = (v) => {
                Console.WriteLine(v);
            };

            if (Options.PerformIncrement) {
                Console.WriteLine("Version Increment Requested - Currently " + ver.GetVersion());
                ver.Increment();
            }

            Console.WriteLine("Version To Write: " + ver.GetVersion());

            // Increment done, now persist and then update the pages - first check if the command line ovverrides the minimatchers
            if ((Options.VersionTargetMinMatch != null) && (Options.VersionTargetMinMatch.Length > 0)) {                
                ver.LoadMiniMatches(Options.VersionTargetMinMatch);
            }

            ver.SearchForAllFiles(Options.Root);

            ver.UpdateAllRegisteredFiles();

            ver.SaveUpdatedVersion();
        }

        private static Action<string> GetActionToPerform(Versioning ver) {
            Action<string> ActionToPerform;
            if (!Options.TestMode) {
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
            if (!string.IsNullOrEmpty(Options.QuickValue)) {
                Console.WriteLine($"Using Value From Command Line: {Options.QuickValue}");
                startVer = Options.QuickValue;
            }
            Console.WriteLine($"Creating New Version Store: {startVer}");

            CompleteVersion cv = new CompleteVersion(startVer);
            VersionStorage vs = new JsonVersionPersister(Options.VersionPersistanceValue);

            Console.WriteLine($"Saving {cv.GetVersionString()}");
            vs.Persist(cv);
        }
    }
}