
using System;
using System.IO;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;
using Serilog;

public partial class Build : NukeBuild {
    // Package Step - Well known step for bundling prior to the app release.   Arrange Construct Examine [Package] Release Test
    private static readonly Array TargetFrameworks = new[] { "net8.0", "net9.0" };
    private Target PackageStep => _ => _
        .After(ExamineStep)
        .Before(ReleaseStep, Wrapup)
        .DependsOn(Initialise, ExamineStep)
        .Executes(() => {
            if (Solution == null) {
                Log.Error("Build>PackageStep>Solution is null.");
                throw new InvalidOperationException("The solution must be set");
            }

            if (settings == null) {
                Log.Error("Build>PackageStep>Settings is null.");
                throw new InvalidOperationException("The settings must be set");
            }

            var project = Solution.GetProject("Versonify");
            if (project == null) { throw new InvalidOperationException("Project not found"); }

            var publishDirectory = settings.ArtifactsDirectory + "\\publish\\";
            var nugetStructure = settings.ArtifactsDirectory + "\\nuget";
            var nugetTools = nugetStructure + "\\tools";

            Log.Information($"Publishing to {publishDirectory}");

            publishDirectory.CreateOrCleanDirectory();
            nugetStructure.CreateOrCleanDirectory();
            nugetTools.CreateOrCleanDirectory();

            foreach (string l in TargetFrameworks) {
                DotNetTasks.DotNetPublish(s => s
                  .SetProject(project)
                  .SetConfiguration(Configuration)
                  .SetOutput(Path.Combine(publishDirectory, l))
                  .SetFramework(l)
                  .EnableNoRestore()
                  .EnableNoBuild()
                );
            }
            publishDirectory.CopyToDirectory(nugetTools, ExistsPolicy.MergeAndOverwrite);

            var readmeFile = settings.DependenciesDirectory + "\\supportingFiles\\readme.md";
            var targetdir = nugetStructure + "\\readme.md";

            Log.Debug($"Copying file {readmeFile} > {targetdir}");
            readmeFile.Copy(targetdir, ExistsPolicy.FileOverwrite);

            var nugetPackageFile = Solution.GetProject("_Dependencies").Directory + "\\supportingFiles\\versonify.nuspec";
            var nugetPackageTarget = settings.ArtifactsDirectory + "\\versonify.nuspec";
            Log.Debug($"Copying {nugetPackageFile} > {nugetPackageTarget}");
            nugetPackageFile.Copy(nugetPackageTarget, ExistsPolicy.FileOverwrite);


            NuGetTasks.NuGetPack(s => s
              .SetTargetPath(settings.ArtifactsDirectory + "\\versonify.nuspec")
              .SetOutputDirectory(settings.ArtifactsDirectory));
        });
}
