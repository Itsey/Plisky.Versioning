using System;
using System.IO;
using System.Linq;

public partial class Build : NukeBuild {

    // Examine is the well known step for post compilation, pre package and deploy. Arrange Construct [Examine] Package Release Test
    public Target ExamineStep => _ => _
        .After(ConstructStep)
        .Before(PackageStep, Wrapup)
        .DependsOn(Initialise, ConstructStep)
        .Triggers(UnitTest, MutationAnalysis)
        .Executes(() => {
            Log.Information("--> Examine Step <-- ");
        });

    [NuGetPackage(packageId: "dotnet-stryker", packageExecutable: "Stryker.CLI.dll", Framework = "net8.0")]
    protected Tool? StrykerNet { get; set; }

    private Target MutationAnalysis => _ => _
        .After(UnitTest)
        .Before(PackageStep)
        .DependsOn(Initialise)
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