using System;
using Plisky.Plumbing;
using Plisky.CodeCraft;
using Minimatch;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;

namespace PliskyTool {
    class Program {
        public static CommandLineArguments Options = new CommandLineArguments();

        static void Main(string[] args) {
            Console.WriteLine("Plisky Tool - Online");
            Bilge b = new Bilge(tl: SourceLevels.Verbose);
            b.AddHandler(new TCPHandler("127.0.0.1", 9060, true));
            b.Info.Log("Online");
            CommandArgumentSupport clas = new CommandArgumentSupport();

            clas.ArgumentPostfix = "=";
            clas.ProcessArguments(Options, args);

            b.Verbose.Log("Perform Action");
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
                case "UpdateFiles":
                    ApplyVersionIncrement();
                    return true;
                default:
                    return false;
            }

        }

        private static void ApplyVersionIncrement() {
            Console.WriteLine("Apply Version Increment");
            var per = new JsonVersionPersister(Program.Options.VersionPersistanceValue);
            Versioning ver = new Versioning(per);

            if (Options.PerformIncrement) {
                Console.WriteLine("Version Increment Requested - Currently "+ver.GetVersion());
                ver.Increment();
            }

            Console.WriteLine("Version To Write: "+ver.GetVersion());

            // Increment done, now persist and then update the pages.
            //Options.VersionTargetMinMatch 
            //Options.Root = @"C:\Files\Code\git\PliskyVersioning";

            var allFiles = SearchForAllFiles(Options.Root, Options.VersionTargetMinMatch);


            Action<string> ActionToPerform = GetActionToPerform(ver);

            foreach (var l in allFiles) {
                ActionToPerform(l);
            }

           
            if (!Options.TestMode) {
                ver.UpdateAllRegisteredFiles();
                Console.WriteLine($"Saving {ver.GetVersion()}");
                per.Persist(ver.Version);
            }
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

        private static List<string> SearchForAllFiles(string root, string[] versionTargetMinMatch) {

            Console.WriteLine($"Searching in [{root}]");

            List<string> result = new List<string>();
            Minimatcher[] mm = new Minimatcher[versionTargetMinMatch.Length];
            for (int i = 0; i < versionTargetMinMatch.Length; i++) {
                Console.WriteLine("Matching >"+versionTargetMinMatch[i]);
                mm[i] = new Minimatcher(versionTargetMinMatch[i], new Options { AllowWindowsPaths = true });
            }

            var fls = Directory.EnumerateFiles(root, "*.*", new EnumerationOptions() { IgnoreInaccessible = true, RecurseSubdirectories = true, MatchCasing = MatchCasing.PlatformDefault, ReturnSpecialDirectories = false });

            foreach (var l in fls) {

                for (int j = 0; j < mm.Length; j++) {
                    if (mm[j].IsMatch(l)) {
                        Console.WriteLine("Match :" + l);
                        result.Add(l);
                        break;
                    }
                }

            }

            return result;


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
