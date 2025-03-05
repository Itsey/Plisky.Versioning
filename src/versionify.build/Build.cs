using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Plisky.Diagnostics;
using Serilog;

partial class Build : NukeBuild {
    protected Bilge b = new Bilge("Versonify-Build");

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository]
    readonly GitRepository? GitRepository;

    [Solution]
    readonly Solution? Solution;

    [Parameter("Specifies a quick version command for the versioning quick step")]
    readonly string QuickVersion = "";


    AbsolutePath SourceDirectory => RootDirectory / "src";
    //AbsolutePath ArtifactsDirectory => Path.GetTempPath() + "\\artifacts";
    AbsolutePath ArtifactsDirectory => @"D:\Scratch\_build\vsfbld\";

    LocalBuildConfig? settings;


    public Target Wrapup => _ => _
        .DependsOn(Initialise)
        .After(Initialise)
        .Executes(() => {
            b.Info.Log("Build >> Wrapup >> All Done.");
            Log.Information("Build>Wrapup>  Finish - Build Process Completed.");
            b.Flush().Wait();
            System.Threading.Thread.Sleep(10);
        });

    protected override void OnBuildFinished() {
        //string discordHook = settings.Config.BuildSection.DiscordHookUrl;
        //if (!string.IsNullOrWhiteSpace(discordHook)) {

        //    string buildTypeMessage = Build.IsLocalBuild ? $"Local [{settings.Config.ExecutingMachineName}]" : $"Server [{settings.Config.ExecutingMachineName}]";

        //    string buildSuccessMessage = string.Empty;
        //    if (IsSucceeding) {
        //        buildSuccessMessage = "Succeeded";
        //        if (NoSuccessNotify) {
        //            return;
        //        }
        //    } else {
        //        buildSuccessMessage = "Failed (";
        //        FailedTargets.ForEach(x => {
        //            buildSuccessMessage += x.Name + ", ";
        //        });
        //        buildSuccessMessage += ")";
        //    }
        //    var ressy = discordHook.PostJsonAsync(new {
        //        content = $"{buildTypeMessage} Listify Build {buildSuccessMessage} for {EnvironmentId} @ {DateTime.Now.Hour}:{DateTime.Now.Minute}"
        //    });
        //    ressy.Wait();
        //} else {
        //    Log.Information("Build>Wrapup>  Discord Hook URL is not set, skipping notification.");
        //}
    }

    public Target Initialise => _ => _
           .Before(ExamineStep, Wrapup)
           .Triggers(Wrapup)
           .Executes(() => {
               if (Solution == null) {
                   Log.Error("Build>Initialise>Solution is null.");
                   throw new InvalidOperationException("The solution must be set");
               }


               //Bilge.AddHandler(new TCPHandler("127.0.0.1", 9060, true));

               Bilge.SetConfigurationResolver((a, b) => {
                   return System.Diagnostics.SourceLevels.Verbose;
               });

               b = new Bilge("Nuke", tl: System.Diagnostics.SourceLevels.Verbose);

               Bilge.Alert.Online("Versonify-Build");
               b.Info.Log("Versionify Build Process Initialised, preparing Initialisation section.");


               settings = new LocalBuildConfig() {
                   DependenciesDirectory = Solution.Projects.First(x => x.Name == "_Dependencies").Directory,
                   ArtifactsDirectory = @"D:\Scratch\_build\vsfbld\",
                   NonDestructive = false,
                   MainProjectName = "Versonify",
                   MollyPrimaryToken = "%NEXUSCONFIG%[R::plisky[L::http://51.141.43.222:8081/repository/plisky/primaryfiles/XXVERSIONNAMEXX/",
                   MollyRulesToken = "%NEXUSCONFIG%[R::plisky[L::http://51.141.43.222:8081/repository/plisky/molly/XXVERSIONNAMEXX/defaultrules.mollyset",
                   MollyRulesVersion = "default",
                   VersioningPersistanceToken = @"%NEXUSCONFIG%[R::plisky[L::http://51.141.43.222:8081/repository/plisky/vstore/versonify-version.store"
               };

               if (settings.NonDestructive) {
                   Log.Information("Build > Initialise > Finish - In Non Destructive Mode.");
               } else {
                   Log.Information("Build>Initialise> Finish - In Destructive Mode.");
               }

           });




    public Target VersionSource => _ => _
        .Executes(() => {

            //var v = new VersonifyTasks();
            //v.PassiveCommand(s => s
            //  .SetRoot(Solution.Directory)
            //  .SetVersionPersistanceValue(@"c:\temp\vs.store")
            //  .SetDebug(true));


            //VersonifyTasks.CommandPassive(s => s
            //  .SetRoot(Solution.Directory)
            //  .SetVersionPersistanceValue(@"c:\temp\vs.store")
            //  .SetDebug(true));

            //v.PerformFileUpdate(s => s
            //  .SetRoot(Solution.Directory)
            //  .AddMultimatchFile($"{Solution.Directory}\\_Dependencies\\Automation\\AutoVersion.txt")
            //  .PerformIncrement(true)
            //  .SetVersionPersistanceValue(@"c:\temp\vs.store")
            //  .SetDebug(true)
            //  .AsDryRun(true)
            //  .SetRelease(""));

            //VersonifyTasks.IncrementAndUpdateFiles(s => s
            // .SetRoot(Solution.Directory)
            // .AddMultimatchFile($"{Solution.Directory}\\_Dependencies\\Automation\\AutoVersion.txt")
            // .PerformIncrement(true)
            // .SetVersionPersistanceValue(@"c:\temp\vs.store")
            // .SetDebug(true)
            // .AsDryRun(true)
            // .SetRelease(""));

            // Logger.Info($"Version is {version}");
        });




    [Obsolete]
    Target Publish => _ => _
        .DependsOn(Compile)
        .Executes(() => {


        });





}
