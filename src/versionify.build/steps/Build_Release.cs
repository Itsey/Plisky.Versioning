

using System;
using Nuke.Common;
using Nuke.Common.Tools.NuGet;

public partial class Build : NukeBuild {

    // Well known step for releasing into the selected environment.  Arrange Construct Examine Package [Release] Test
    public Target ReleaseStep => _ => _
      .DependsOn(Initialise, PackageStep)
      .Before(TestStep, Wrapup)
      .After(PackageStep)
      .Executes(() => {



          NuGetTasks.NuGetPush(s => s
           .SetTargetPath(ArtifactsDirectory + "\\Plisky.Versonify*.nupkg")
           .SetSource("https://api.nuget.org/v3/index.json")
           .SetApiKey(Environment.GetEnvironmentVariable("PLISKY_PUBLISH_KEY")));
      });


}

