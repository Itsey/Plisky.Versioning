using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.PowerShell;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;
using Serilog;

[AzurePipelines(AzurePipelinesImage.UbuntuLatest, AutoGenerate = false, InvokedTargets = new[] { nameof(ArrangeStep), nameof(ConstructStep), nameof(ExamineStep), nameof(PackageStep), nameof(ReleaseStep) })]
public partial class Build : NukeBuild {
    public Bilge b = new("Nuke", tl: System.Diagnostics.SourceLevels.Verbose);

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository]
    private readonly GitRepository? GitRepository;

    [Solution]
    private readonly Solution? Solution;

    [Parameter("Uses a simpler logging approach that adds stability if required.", Name = "SimplifyLogging")]
    private readonly bool SingleThreadedTrace = false;

    [Parameter("Specifies a quick version command for the versioning quick step", Name = "QuickVersion")]
    readonly string QuickVersion = "";

    [Parameter("PreRelease will only release a pre-release version of the package.  Uses pre-release versioning.")]
    readonly bool PreRelease = true;

    [Parameter("Full version number")]
    private string FullVersionNumber = string.Empty;

    private AbsolutePath SourceDirectory => RootDirectory / "src";
    private AbsolutePath? ArtifactsDirectory;

    private LocalBuildConfig? settings;

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

    }

    public Target NexusLive => _ => _
      .After(Initialise)
      .DependsOn(Initialise)
      .Executes(() => {
          string? dotb = Environment.GetEnvironmentVariable("DOTB_BUILDTOOLS");
          if (!string.IsNullOrWhiteSpace(dotb)) {
              Log.Information($"Build> Ensure Nexus Is Live>  Build Tools Directory: {dotb}");

              string nexusInitScript = Path.Combine(dotb, "scripts", "nexusInit.ps1");
              if (File.Exists(nexusInitScript)) {
                  PowerShellTasks.PowerShell(x =>
                     x.SetFile(nexusInitScript)
                     .SetFileArguments("checkup")
                     .SetProcessToolPath("pwsh")
                  );
              } else {
                  Log.Error($"Build>Initialise>  Build Tools Directory: {nexusInitScript} - Nexus Init Script not found.");
              }

          } else {
              Log.Information("Build>Initialise>  Build Tools Directory: Not Set, no additional initialisation taking place.");
          }
      });

    public Target Initialise => _ => _
           .Before(ExamineStep, Wrapup)
           .Triggers(Wrapup)
           .Executes(() => {
               if (Solution == null) {
                   Log.Error("Build>Initialise>Solution is null.");
                   throw new InvalidOperationException("The solution must be set");
               }

               if (SingleThreadedTrace) {
                   // This is to work around a bug where trace was not being written.
                   Bilge.SimplifyRouter();
               }

               Bilge.AddHandler(new TCPHandler("127.0.0.1", 9060, true));

               Bilge.SetConfigurationResolver((a, b) => {
                   return System.Diagnostics.SourceLevels.Verbose;
               });

               b = new Bilge("Nuke", tl: System.Diagnostics.SourceLevels.Verbose);

               Bilge.Alert.Online("Versonify-Build");
               b.Info.Log("Versonify Build Process Initialised, preparing Initialisation section.");

               settings = new LocalBuildConfig {
                   NonDestructive = false,
                   MainProjectName = "Versonify",
                   MollyPrimaryToken = "%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/primaryfiles/XXVERSIONNAMEXX/",
                   MollyRulesToken = "%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/molly/XXVERSIONNAMEXX/defaultrules.mollyset",
                   MollyRulesVersion = "default",
                   VersioningPersistanceToken = @"%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/vstore/versonify-pre.vstore",
                   VersioningPersistanceTokenRelease = @"%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/vstore/versonify.vstore",
                   ArtifactsDirectory = Path.Combine(Path.GetTempPath(), "_build\\vsfbld\\"),
                   DependenciesDirectory = Solution.Projects.First(x => x.Name == "_Dependencies").Directory
               };

               if (settings.NonDestructive) {
                   Log.Information("Build > Initialise > Finish - In Non Destructive Mode.");
               } else {
                   Log.Information("Build>Initialise> Finish - In Destructive Mode.");
               }
           });
}