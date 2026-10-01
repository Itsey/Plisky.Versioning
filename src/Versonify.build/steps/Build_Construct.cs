using System;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Plisky.Nuke.Fusion;
using Serilog;

public partial class Build : NukeBuild {
    private const string MAJOR_QUICK_VALUE = "+.0.0";
    private const string MINOR_QUICK_VALUE = ".+.0";
    private const string PATCH_QUICK_VALUE = "..+";

    // Standard entrypoint for compiling the app.  Arrange [Construct] Examine Package Release Test

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

          b.Info.Log($"Build Local [{IsLocalBuild}] Dry Run [{dryRunMode}] VersionType [{versioningType}]");
          Log.Information($"[Versioning]{versioningType} versioning displaying curent version number.");

          var vc = new VersonifyTasks();
          vc.PassiveCommand(s => s
              .SetVersionPersistanceValue(vtFile)
              .SetOutputStyle("console-nf")
              .SetRoot(Solution.Directory)
          );

          var mmPathBase = settings.DependenciesDirectory / "automation";
          var mmPath = mmPathBase / "autoversion.txt";

          if (IsMajor) {
              Log.Information("[Versioning] Major version increment requested.");
              vc.OverrideCommand(s => s
                  .SetVersionPersistanceValue(vtFile)
                  .SetOutputStyle("console-nf")
                  .AsDryRun(dryRunMode)
                  .SetRoot(Solution.Directory)
                  .SetQuickValue(MAJOR_QUICK_VALUE)
              );
          } else if (IsMinor) {
              Log.Information("[Versioning] Minor version increment requested.");
              vc.OverrideCommand(s => s
                  .SetVersionPersistanceValue(vtFile)
                  .SetOutputStyle("console-nf")
                  .AsDryRun(dryRunMode)
                  .SetRoot(Solution.Directory)
                  .SetQuickValue(MINOR_QUICK_VALUE)
              );
          }

          b.Info.Log($"PRE File Update >> {vc.VersionLiteral}");
          vc.FileUpdateCommand(s => s
              .SetVersionPersistanceValue(vtFile)
              .AddMultimatchFile(mmPath)
              .PerformIncrement(true)
              .SetOutputStyle("console-nf")
              .AsDryRun(dryRunMode)
              .SetRoot(Solution.Directory)
          );

          Log.Information($"[Versioning]{versioningType} Increment and Update Existing Files.({vc.VersionLiteral})");

          // Capture the version stamped into the files now; the pre-release store update below changes vc.VersionLiteral.
          string appliedVersion = vc.VersionLiteral;

          if (!PreRelease) {
              UpdatePreReleaseVersionNumber(dryRunMode, versioningType, vc, mmPathBase);
          }

          settings.ActiveVersionNumber = appliedVersion;
          FullVersionNumber = appliedVersion;
          Log.Information($"[Versioning]Version applied:{appliedVersion}");

          // Set Azure DevOps variable for use in pipeline/release steps
          Console.WriteLine($"##vso[task.setvariable variable=FullVersionNumber;isOutput=true]{FullVersionNumber}");
      });

    private void UpdatePreReleaseVersionNumber(bool dryRunMode, string versioningType, VersonifyTasks vc, AbsolutePath mmPathBase) {
        Log.Information($"[Versioning]{versioningType} Applying release version number to pre-release data. ({vc.VersionLiteral})");

        // Once the release version changes the pre-release version numbers must move to match, otherwise they stay on the old version base.  The file
        // update targets a non existent file so that only the version store is updated.
        string quickVal = PATCH_QUICK_VALUE;
        if (IsMajor) {
            quickVal = MAJOR_QUICK_VALUE;
        } else if (IsMinor) {
            quickVal = MINOR_QUICK_VALUE;
        }

        vc.OverrideCommand(s => s
            .SetVersionPersistanceValue(settings!.VersioningPersistanceToken)
            .SetOutputStyle("console-nf")
            .AsDryRun(dryRunMode)
            .SetRoot(Solution!.Directory)
            .SetQuickValue(quickVal)
        );

        var nmPath = mmPathBase / "noversion.txt";
        vc.FileUpdateCommand(s => s
            .SetVersionPersistanceValue(settings!.VersioningPersistanceToken)
            .SetOutputStyle("console-nf")
            .AddMultimatchFile(nmPath)
            .PerformIncrement(true)
            .AsDryRun(dryRunMode)
            .SetZeroReturnCode(true)
            .SetRoot(mmPathBase)
        );
    }

    public Target ConstructStep => _ => _
            .Before(ExamineStep, Wrapup)
        .After(ArrangeStep)
        .Triggers(Compile, ApplyVersion)
        .DependsOn(Initialise, ArrangeStep)
        .Executes(() => {
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