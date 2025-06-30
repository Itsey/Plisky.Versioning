

using System;
using Nuke.Common;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.NuGet;
using Serilog;

public partial class Build : NukeBuild {

    public Target ApplyGitTag => _ => _
      .After(ReleaseStep)
      .DependsOn(Initialise)
      .Before(Wrapup)
      .Executes(() => {

          if (IsSucceeding) {
              if (string.IsNullOrEmpty(FullVersionNumber)) {
                  Log.Information("No version number, skipping Tag");
              } else {
                  Log.Information("Applying Git Tag");
                  GitTasks.Git($"tag -a {FullVersionNumber} -m \"Release {FullVersionNumber}\"");
              }
          }
      });


    // Well known step for releasing into the selected environment.  Arrange Construct Examine Package [Release] Test
    public Target ReleaseStep => _ => _
      .DependsOn(Initialise, PackageStep)
      .Before(TestStep, Wrapup)
      .Triggers(ApplyGitTag)
      .After(PackageStep)
      .Executes(() => {

          NuGetTasks.NuGetPush(s => s
           .SetTargetPath(settings!.ArtifactsDirectory + "\\nuget\\Plisky.Versonify*.nupkg")
           .SetSource("https://api.nuget.org/v3/index.json")
           .SetApiKey(Environment.GetEnvironmentVariable("PLISKY_PUBLISH_KEY")));
      });


}

