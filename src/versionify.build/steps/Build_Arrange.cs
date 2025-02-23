using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Plisky.Nuke.Fusion;
using Serilog;

public partial class Build : NukeBuild {

    // ArrangeStep = Well Known Initial Step for correctness and Linting. [Arrange] Construct Examine Package Release Test
    public Target ArrangeStep => _ => _
        .Before(ConstructStep, Wrapup)
        .DependsOn(Initialise)
        .Triggers(Clean, MollyCheck)
        .Executes(() => {
            Log.Information("--> Arrange <-- ");
        });

    private Target Clean => _ => _
        .DependsOn(Initialise)
        .After(ArrangeStep, Initialise)
        .Before(ConstructStep)
        .Executes(() => {
            b.Info.Log("Clean Step in Arrange Starts");

            DotNetTasks.DotNetClean(s => s.SetProject(Solution));

            b.Verbose.Log("Clean completed, cleaning artefact directory");

            settings.ArtifactsDirectory.CreateOrCleanDirectory();
        });

    private Target MollyCheck => _ => _
       .After(Clean, ArrangeStep)
       .DependsOn(Initialise)
       .Before(ConstructStep)
       .Executes(() => {
           Log.Information("Mollycoddle Structure Linting Starts.");

           if (settings == null) {
               throw new InvalidOperationException("The settings are not configured, Mollycoddle cannot run.");
           }

           var mcOk = ValidateMollySettings(settings?.MollyRulesToken, GitRepository.LocalDirectory.Exists());
           if (mcOk != ValidationResult.Success) {
               Log.Error("Mollycoddle Structure Linting Skipped - Validation Failed.");
               foreach (string item in mcOk.MemberNames) {
                   Log.Error(item);
               }
               return;
           }

           Log.Verbose($"MC ({settings?.MollyRulesToken}) ({settings?.MollyPrimaryToken}) ({GitRepository.LocalDirectory})");
           var mc = new MollycoddleTasks();

           string formatter = IsLocalBuild ? "plain" : "azdo";
           mc.PerformScan(s => s
               .AddRuleHelp(true)
               .AddRulesetVersion(settings.MollyRulesVersion)
               .SetRulesFile(settings.MollyRulesToken)
               .SetPrimaryRoot(settings.MollyPrimaryToken)
               .SetFormatter(formatter)
               .SetDirectory(GitRepository.LocalDirectory));


           Log.Information("Mollycoddle Structure Linting Completes.");
       });

    [Pure]
    private ValidationResult ValidateMollySettings(string? mollyRulesToken, bool localDirectoryExists) {
        var errors = new List<string>();

        if (!localDirectoryExists) {
            errors.Add("Mollycoddle: Local Working Directory Error.  Directory Does Not Exist.");
        }
        if (string.IsNullOrWhiteSpace(mollyRulesToken)) {
            errors.Add("Mollycoddle: Ruleset Initialisation Token Not Set.");
        }

        if (errors.Count > 0) {
            return new ValidationResult("Mollycoddle: Parameter Validation Failed.", errors);
        }
        return ValidationResult.Success;

    }
}

