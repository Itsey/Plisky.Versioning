
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


          var vc = new VersonifyTasks();
          vc.PassiveCommand(s => s
          .SetVersionPersistanceValue(settings.VersioningPersistanceToken)
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
          Log.Information($"Versioning Token : {settings.VersioningPersistanceToken}");

          var vc = new VersonifyTasks();
          vc.PassiveCommand(s => s
              .SetVersionPersistanceValue(settings.VersioningPersistanceToken)
              .SetOutputStyle("azdo")
              .SetRoot(Solution.Directory)
          );

          var mmPath = settings.DependenciesDirectory / "automation";
          mmPath /= "autoversion.txt";

          vc.FileUpdateCommand(s => s
              .SetVersionPersistanceValue(settings.VersioningPersistanceToken)
              .AddMultimatchFile(mmPath)
              .PerformIncrement(true)
              .SetOutputStyle("azdo-nf")
              .AsDryRun(dryRunMode)
              .SetRoot(Solution.Directory)
          );
          FullVersionNumber = vc.VersionLiteral;
          Log.Information($"Version applied:{vc.VersionLiteral}");

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

