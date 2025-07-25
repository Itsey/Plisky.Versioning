using System;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Plisky.Nuke.Fusion;
using Serilog;

public partial class Build : NukeBuild {
    // Standard entrypoint for compiling the app.  Arrange [Construct] Examine Package Release Test

    public string FullVersionNumber { get; set; } = string.Empty;
    public Target ConstructStep => _ => _
        .Before(ExamineStep, Wrapup)
        .After(ArrangeStep)
        .Triggers(Compile, ApplyVersion)
        .DependsOn(Initialise, ArrangeStep)
        .Executes(() => {
        });

    public Target VersionQuickStep => _ => _
      .After(ConstructStep)
      .DependsOn(Initialise)
      .Before(Compile)
      .Executes(() => {
          Log.Information($"Manual Quick Step QV:{QuickVersion}");

          if (settings == null) {
              Log.Error("Build>ApplyVersion>Settings is null.");
              throw new InvalidOperationException("The settings must be set");
          }

          if (Solution == null) {
              Log.Error("Build>ApplyVersion>Solution is null.");
              throw new InvalidOperationException("The solution must be set");
          }


          if (!string.IsNullOrEmpty(QuickVersion)) {
              var vc = new VersonifyTasks();

              vc.OverrideCommand(s => s
                .SetVersionPersistanceValue(settings.VersioningPersistanceToken)
                .SetDebug(true)
                .SetRoot(Solution.Directory)
                .SetQuickValue(QuickVersion)
              );
          }
      });

    public Target QueryNextVersion => _ => _
      .After(ConstructStep)
      .DependsOn(Initialise)
      .Before(Compile)
      .Executes(() => {

          if (settings == null) {
              Log.Error("Build>ApplyVersion>Settings is null.");
              throw new InvalidOperationException("The settings must be set");
          }

          if (Solution == null) {
              Log.Error("Build>ApplyVersion>Solution is null.");
              throw new InvalidOperationException("The solution must be set");
          }

          string versioningToken = settings.VersioningPersistanceTokenRelease;
          if (PreRelease) {
              Log.Information("Build>QueryNextVersion>PreRelease is set, using non release Token.");
              versioningToken = settings.VersioningPersistanceToken;
          }
          var vc = new VersonifyTasks();
          vc.PassiveCommand(s => s
          .SetVersionPersistanceValue(versioningToken)
          .SetOutputStyle("con-nf")
          .SetRoot(Solution.Directory));

          Log.Information($"Version Is:{vc.VersionLiteral}");
      });



    public Target ApplyVersion => _ => _
      .After(ConstructStep)
      .DependsOn(Initialise)
      .Before(Compile)
      .Executes(() => {

          if (settings == null) {
              Log.Error("Build>ApplyVersion>Settings is null.");
              throw new InvalidOperationException("The settings must be set");
          }

          if (Solution == null) {
              Log.Error("Build>ApplyVersion>Solution is null.");
              throw new InvalidOperationException("The solution must be set");
          }

          bool dryRunMode = false;

          if (IsLocalBuild) {
              // Passive Get Current Version
              Log.Information("Local Build - Versioning Set To Dry Run");
              dryRunMode = true;
          }

          string versioningType = "Pre-Release";
          string vtFile = settings.VersioningPersistanceToken;
          if (!PreRelease) {
              vtFile = settings.VersioningPersistanceTokenRelease;
              versioningType = "Release";
          }

          Log.Information($"[Versioning]{versioningType} versioning displaying curent version number.");

          var vc = new VersonifyTasks();
          vc.PassiveCommand(s => s
              .SetVersionPersistanceValue(vtFile)
              .SetOutputStyle("azdo")
              .SetRoot(Solution.Directory)
          );

          var mmPath = settings.DependenciesDirectory / "automation";
          mmPath /= "autoversion.txt";

          Log.Information($"[Versioning]{versioningType} Increment and Update Exsiting Files.({vc.VersionLiteral})");

          vc.FileUpdateCommand(s => s
              .SetVersionPersistanceValue(vtFile)
              .AddMultimatchFile(mmPath)
              .PerformIncrement(true)
              .SetOutputStyle("azdo-nf")
              .AsDryRun(dryRunMode)
              .SetRoot(Solution.Directory)
          );

          if (!PreRelease) {
              Log.Information($"[Versioning]{versioningType} Applying release version number to pre-release data. ({vc.VersionLiteral})");

              vc.OverrideCommand(s => s
                  .SetVersionPersistanceValue(settings.VersioningPersistanceToken)
                  .SetOutputStyle("azdo-nf")
                  .AsDryRun(dryRunMode)
                  .SetRoot(Solution.Directory)
                  .SetQuickValue(vc.VersionLiteral)
              );
          }

          FullVersionNumber = vc.VersionLiteral;
          Log.Information($"[Versioning]Version applied:{vc.VersionLiteral}");

      });

    private Target Compile => _ => _
        .Before(ExamineStep)
        .Executes(() => {
            DotNetTasks.DotNetBuild(s => s
              .SetProjectFile(Solution)
              .SetConfiguration(Configuration)
              .SetDeterministic(IsServerBuild)
              .EnableNoRestore()
              .SetContinuousIntegrationBuild(IsServerBuild)
          );
        });

}
