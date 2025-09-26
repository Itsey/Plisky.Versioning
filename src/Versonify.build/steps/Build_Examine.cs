using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;

public partial class Build : NukeBuild {

    [NuGetPackage(packageId: "dotnet-stryker", packageExecutable: "Stryker.CLI.dll", Framework = "net8.0")]
    protected Tool? StrykerNet { get; set; }

    // Examine is the well known step for post compilation, pre package and deploy. Arrange Construct [Examine] Package Release Test
    public Target ExamineStep => _ => _
        .After(ConstructStep)
        .Before(PackageStep, Wrapup)
        .DependsOn(Initialise, ConstructStep)
        .Triggers(ConfigureAnalysisMode, UnitTest, MutationAnalysis)
        .Executes(() => {
            Log.Information("--> Examine Step <-- ");
        });
    private Target ConfigureAnalysisMode => _ => _
    .After(Initialise)
    .Before(UnitTest, MutationAnalysis)
    .Executes(() => {
        if (!string.IsNullOrWhiteSpace(analysisModeOverride) && Enum.TryParse<AnalysisMode>(analysisModeOverride, true, out var parsedMode)) {
            analysisMode = parsedMode;
            Log.Information($"Build>ConfigureAnalysisMode> Analysis Mode override parameter specified: {analysisMode}");
        } else {
            analysisMode = IsLocalBuild
                ? AnalysisMode.Lite
                : BuildReason switch {
                    AzurePipelinesBuildReason.Manual => AnalysisMode.Deep,
                    AzurePipelinesBuildReason.Schedule => AnalysisMode.Deep,
                    AzurePipelinesBuildReason.IndividualCI => AnalysisMode.Lite,
                    _ => AnalysisMode.Lite
                };
            Log.Information($"Build>ConfigureAnalysisMode> Analysis Mode derived from Build Reason {(BuildReason.HasValue ? BuildReason.Value.ToString() : "LocalBuild")} and set to: '{analysisMode}'");
        }
    });

    private Target MutationAnalysis => _ => _
        .After(UnitTest)
        .Before(PackageStep)
        .DependsOn(Initialise)
        .OnlyWhenDynamic(() => analysisMode > AnalysisMode.Lite)
        .Executes(() => {
            if (Solution == null) {
                Log.Error("Build>MutationAnalysis>Solution is null.");
                throw new InvalidOperationException("The solution must be set");
            }
            var testProjects = Solution.GetAllProjects("*.Test");
            if (testProjects.Any()) {
                string potentialStrykerConfig = Path.Combine(testProjects.First().Directory, "stryker-config.json");
                string args = string.Empty;
                if (File.Exists(potentialStrykerConfig)) {
                    args = $"--config-file {potentialStrykerConfig}";
                }

                void StrykerLogger(OutputType outputType, string message) {
                    if (outputType == OutputType.Std) {
                        if (reporting != null) {
                            string scoreMarker = "The final mutation score is ";
                            if (message.Contains(scoreMarker)) {
                                string score = message.Substring(message.IndexOf(scoreMarker) + scoreMarker.Length);
                                reporting.MutationScore = score.Trim();
                            }
                        }
                        Log.Information(message);
                    }
                }

                StrykerNet(arguments: $"{args}", workingDirectory: testProjects.First().Directory, logger: StrykerLogger);
            }
        });

    private Target UnitTest => _ => _
      .After(ExamineStep)
      .Before(PackageStep)
      .Executes(() => {
          if (Solution == null) {
              Log.Error("Build>UnitTest>Solution is null.");
              throw new InvalidOperationException("The solution must be set");
          }

          var testProjects = Solution.GetAllProjects("*.Test");
          if (testProjects.Any()) {
              DotNetTasks.DotNetTest(s => s
                  .EnableNoRestore()
                  .EnableNoBuild()
                  .SetConfiguration(Configuration)
                  .SetProjectFile(testProjects.First().Directory));
          }
      });
}