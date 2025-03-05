
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Plisky.Nuke.Fusion;
using Serilog;

public partial class Build : NukeBuild {

    // Standard entrypoint for compiling the app.  Arrange [Construct] Examine Package Release Test
    public Target ConstructStep => _ => _
        .Before(ExamineStep, Wrapup)
        .After(ArrangeStep)
        .Triggers(Compile)
        .DependsOn(Initialise, ArrangeStep)
        .Executes(() => {
        });

    public Target VersionQuickStep => _ => _
      .After(ConstructStep)
      .DependsOn(Initialise)
      .Before(Compile)
      .Executes(() => {
          Log.Information($"Manual Quick Step QV:{QuickVersion}");

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


    public Target ApplyVersion => _ => _
      .After(ConstructStep)
      .DependsOn(Initialise)
      .Before(Compile)
      .Executes(() => {

          if (IsLocalBuild) {
              // Passive Get Current Version
              Log.Information("Local Build - Skipping Versioning");
              //return;
          }


          Log.Information($"Versioning Token : {settings.VersioningPersistanceToken}");

          var vc = new VersonifyTasks();
          vc.PassiveCommand(s => s
              .SetVersionPersistanceValue(settings.VersioningPersistanceToken)
              .SetDebug(true)
              .SetRoot(Solution.Directory)
          );

          var mmPath = settings.DependenciesDirectory / "automation";
          mmPath /= "autoversion.txt";

          vc.FileUpdateCommand(s => s
              .SetVersionPersistanceValue(settings.VersioningPersistanceToken)
              .AddMultimatchFile(mmPath)
              .PerformIncrement(true)
              .SetDebug(true)
              .SetRoot(Solution.Directory)

          );



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

