
using System;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;
using Serilog;

public partial class Build : NukeBuild {

    // Package Step - Well known step for bundling prior to the app release.   Arrange Construct Examine [Package] Release Test
    private Target PackageStep => _ => _
        .After(ExamineStep)
        .Before(ReleaseStep, Wrapup)
        .DependsOn(Initialise, ExamineStep)
        .Executes(() => {






            var project = Solution.GetProject("Versonify");
            if (project == null) { throw new InvalidOperationException("Project not found"); }

            var publishDirectory = ArtifactsDirectory + "\\publish\\versonify";
            var nugetStructure = ArtifactsDirectory + "\\nuget";
            var nugetTools = nugetStructure + "\\tools";

            Log.Information($"Publishing to {publishDirectory}");

            publishDirectory.CreateOrCleanDirectory();
            nugetStructure.CreateOrCleanDirectory();
            nugetTools.CreateOrCleanDirectory();

            DotNetTasks.DotNetPublish(s => s
                .SetProject(project)
                .SetConfiguration(Configuration)
                .SetOutput(publishDirectory)
                .SetNoRestore(true)
                .SetNoBuild(true)
                .SetVerbosity(DotNetVerbosity.normal)
                .SetSelfContained(false)
            );

            publishDirectory.CopyToDirectory(nugetTools, ExistsPolicy.MergeAndOverwrite);
            //FileSystemTasks.CopyDirectoryRecursively(publishDirectory, nugetTools, DirectoryExistsPolicy.Merge, FileExistsPolicy.Overwrite);

            AbsolutePath readmeFile = settings.DependenciesDirectory + "\\supportingFiles\\readme.md";
            var targetdir = nugetStructure + "\\readme.md";

            Log.Debug($"Copying file {readmeFile} > {targetdir}");
            readmeFile.Copy(targetdir, ExistsPolicy.FileOverwrite);
            //nugetStructure.Copy(readmeFile, ExistsPolicy.FileOverwrite);
            //FileSystemTasks.CopyFile(readmeFile, targetdir, FileExistsPolicy.Overwrite);

            var nugetPackageFile = Solution.GetProject("_Dependencies").Directory + "\\supportingFiles\\versonify.nuspec";
            var nugetPackageTarget = ArtifactsDirectory + "\\versonify.nuspec";
            Log.Debug($"Copying {nugetPackageFile} > {nugetPackageTarget}");
            nugetPackageFile.Copy(nugetPackageTarget, ExistsPolicy.FileOverwrite);

            //FileSystemTasks.CopyFile(nugetPackageFile, ArtifactsDirectory + "\\versonify.nuspec", FileExistsPolicy.Overwrite);





            /*

            var v = new VersonifyTasks();

            //VersonifyTasks.CommandPassive(s => s
            //  .SetRoot(Solution.Directory)
            //  .SetVersionPersistanceValue(@"c:\temp\vs.store")
            //  .SetDebug(true));

            v.PassiveExecute(s => s
              .SetRoot(Solution.Directory)
              .SetVersionPersistanceValue(@"c:\temp\vs.store")
              .SetDebug(true));


            v.PerformFileUpdate(v => v
              .SetRoot(ArtifactsDirectory)
              .AddMultimatchFile($"{Solution.Directory}\\_Dependencies\\Automation\\NuspecVersion.txt")
              .SetVersionPersistanceValue(@"c:\temp\vs.store")
              .SetDebug(true)
              .AsDryRun(true)
              .SetRelease(""));

            //VersonifyTasks.IncrementAndUpdateFiles(s => s
            // .SetRoot(ArtifactsDirectory)
            // .AddMultimatchFile($"{Solution.Directory}\\_Dependencies\\Automation\\NuspecVersion.txt")
            // .SetVersionPersistanceValue(@"c:\temp\vs.store")
            // .SetDebug(true)
            // .AsDryRun(true)
            // .SetRelease(""));

            */

            NuGetTasks.NuGetPack(s => s
              .SetTargetPath(ArtifactsDirectory + "\\versonify.nuspec")
              .SetOutputDirectory(ArtifactsDirectory));


        });
}

