using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Plisky.CodeCraft;
using Plisky.Nuke.Fusion;
using Serilog;

public partial class Build : NukeBuild {
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

          var mmPath = settings.DependenciesDirectory / "automation";
          mmPath /= "autoversion.txt";

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

          if (!PreRelease) {
              // Hack.  Curently the return from versonify is not set to be different display types, we need 3 digit for semver so hacking the last digit off.

              int periodCount = 0;
              string verNumberToUse = string.Empty;
              foreach (char c in vc.VersionLiteral) {
                  if (c == '.') {
                      if (periodCount == 2) {
                          break;
                      }
                      periodCount++;
                  }

                  verNumberToUse += c;
              }

              while (periodCount < 2) {
                  verNumberToUse += ".0";
                  periodCount++;
              }

              b.Assert.True(verNumberToUse.Count(x => x == '.') == 2, $"The version number ({verNumberToUse}) should be in the format N.N.N.");
              Log.Information($"[Versioning]{versioningType} Applying release version number to pre-release data. ({vc.VersionLiteral} > {verNumberToUse})");

              // NOTE - This is a project reference, this approach will not currently work anywhere except in this build project, if this is required then we need to make
              // versioning tasks a nuget package.  Its possible this is achievable via a quick value update to versonify.
              var cv = new CompleteVersion(verNumberToUse);
              cv.Digits[0].Behaviour = DigitIncrementBehaviour.Fixed;
              cv.Digits[1].Behaviour = DigitIncrementBehaviour.Fixed;
              cv.Digits[2].Behaviour = DigitIncrementBehaviour.ContinualIncrement;
              cv.Increment();
              verNumberToUse = cv.ToString();

              b.Info.Log($"POST Passive QUEUED >> {verNumberToUse}");
              vc.OverrideCommand(s => s
                  .SetVersionPersistanceValue(settings.VersioningPersistanceToken)
                  .SetOutputStyle("console-nf")
                  .AsDryRun(dryRunMode)
                  .SetRoot(Solution.Directory)
                  .SetQuickValue(verNumberToUse)
              );

              vc.OverrideCommand(s => s
                  .SetVersionPersistanceValue(settings.VersioningPersistanceTokenRelease)
                  .SetOutputStyle("console-nf")
                  .AsDryRun(dryRunMode)
                  .SetRoot(Solution.Directory)
                  .SetQuickValue(verNumberToUse)
              );

              b.Info.Log($"END >> {vc.VersionLiteral}");
          }

          settings.ActiveVersionNumber = vc.VersionLiteral;
          FullVersionNumber = vc.VersionLiteral;
          Log.Information($"[Versioning]Version applied:{vc.VersionLiteral}");

          // Set Azure DevOps variable for use in pipeline/release steps
          Console.WriteLine($"##vso[task.setvariable variable=FullVersionNumber;isOutput=true]{FullVersionNumber}");
      });

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