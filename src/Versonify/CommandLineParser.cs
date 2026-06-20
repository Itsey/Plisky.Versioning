namespace Versonify;

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;

public static class CommandLineParser {
    private static readonly IReadOnlyDictionary<string, string> deprecatedAliasMap = new Dictionary<string, string>(StringComparer.Ordinal) {
        ["-Command"] = "--command",
        ["-Debug"] = "--debug",
        ["-DryRun"] = "--dry-run",
        ["-Digits"] = "--digits",
        ["-NoError"] = "--no-error",
        ["-NoOverride"] = "--no-override",
        ["-Output"] = "--output",
        ["-Increment"] = "--increment",
        ["-QuickValue"] = "--quick-value",
        ["-Release"] = "--release",
        ["-Root"] = "--root",
        ["-Trace"] = "--trace",
        ["-VersionSource"] = "--version-source",
        ["-MinMatch"] = "--min-match",
        ["-output"] = "--output",
    };

    public static bool IsHelpRequested(string[] args) {
        foreach (string arg in args) {
            if (arg.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("-h", StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }

        return false;
    }

    public static void DisplayHelp() {
        var helpCommand = BuildRootCommand(false);
        helpCommand.Parse(new[] { "--help" }).Invoke(new System.CommandLine.InvocationConfiguration());
    }

    public static (bool Success, VersonifyOptions Options) Parse(string[] args) {
        var options = new VersonifyOptions();
        var rootCommand = BuildRootCommand();
        var parseResult = rootCommand.Parse(args);

        if (parseResult.Errors.Count > 0) {
            Console.WriteLine("Fatal: Invalid Arguments Passed to Versonify.");
            foreach (var error in parseResult.Errors) {
                Console.WriteLine(error.Message);
            }
            return (false, options);
        }

        EmitDeprecatedAliasWarnings(args);

        // Find the argument and options we need to query
        var commandArg = rootCommand.Arguments[0] as Argument<string>;
        var commandOpt = FindOption<string>(rootCommand, "--command");
        var debugOpt = FindOption<bool>(rootCommand, "--debug");
        var dryRunOpt = FindOption<bool>(rootCommand, "--dry-run");
        var digitsOpt = FindOption<string>(rootCommand, "--digits");
        var noErrorOpt = FindOption<bool>(rootCommand, "--no-error");
        var getMdHelpOpt = FindOption<bool>(rootCommand, "--get-md-help");
        var noOverrideOpt = FindOption<bool>(rootCommand, "--no-override");
        var outputOpt = FindOption<string>(rootCommand, "--output");
        var incrementOpt = FindOption<bool>(rootCommand, "--increment");
        var quickValueOpt = FindOption<string>(rootCommand, "--quick-value");
        var releaseOpt = FindOption<string>(rootCommand, "--release");
        var rootPathOpt = FindOption<string>(rootCommand, "--root");
        var traceOpt = FindOption<string>(rootCommand, "--trace");
        var versionSourceOpt = FindOption<string>(rootCommand, "--version-source");
        var minMatchOpt = FindOption<string>(rootCommand, "--min-match");

        string? cmdFromPositional = parseResult.GetValue(commandArg!);
        string? cmdFromOption = parseResult.GetValue(commandOpt!);
        options.Command = cmdFromPositional ?? cmdFromOption;

        options.Debug = parseResult.GetValue(debugOpt!);
        options.DryRunOnly = parseResult.GetValue(dryRunOpt!);
        options.ReturnZero = parseResult.GetValue(noErrorOpt!);
        options.GetMdHelp = parseResult.GetValue(getMdHelpOpt!);
        options.NoOverride = parseResult.GetValue(noOverrideOpt!);
        options.PerformIncrement = parseResult.GetValue(incrementOpt!);
        options.QuickValue = parseResult.GetValue(quickValueOpt!);
        options.Release = parseResult.GetValue(releaseOpt!);
        options.Root = parseResult.GetValue(rootPathOpt!);
        options.Trace = parseResult.GetValue(traceOpt!);
        options.VersionPersistanceValue = parseResult.GetValue(versionSourceOpt!);

        string? rawDigits = parseResult.GetValue(digitsOpt!);
        options.DigitManipulations = rawDigits != null
            ? rawDigits.Split(';', StringSplitOptions.RemoveEmptyEntries)
            : null;

        string? rawMinMatch = parseResult.GetValue(minMatchOpt!);
        options.VersionTargetMinMatch = rawMinMatch != null
            ? rawMinMatch.Split(';', StringSplitOptions.RemoveEmptyEntries)
            : null;

        options.RawOutputOptions = parseResult.GetValue(outputOpt!);
        options.OutputOptions = options.RawOutputOptions ?? "";

        return (true, options);
    }

    private static Option<T>? FindOption<T>(RootCommand rootCommand, string alias) {
        foreach (var symbol in rootCommand.Options) {
            if (symbol is Option<T> opt) {
                if (symbol.Name.Equals(alias, StringComparison.Ordinal)) {
                    return opt;
                }
                foreach (var a in opt.Aliases) {
                    if (a.Equals(alias, StringComparison.Ordinal)) {
                        return opt;
                    }
                }
            }
        }
        return null;
    }

    private static RootCommand BuildRootCommand(bool includeDeprecatedAliases = true) {
        var rc = new RootCommand("Parameter help for Versonify. AI/automation: run --get-md-help to write the embedded docs.md file into the current directory.");
        rc.TreatUnmatchedTokensAsErrors = true;

        var commandArg = new Argument<string>("command");
        commandArg.Description = "Command to execute: createversion|override|updatefiles|passive|behaviour|set|prefix";
        commandArg.Arity = ArgumentArity.ZeroOrOne;
        commandArg.DefaultValueFactory = _ => null!;
        rc.Add(commandArg);

        string[] commandAliases = includeDeprecatedAliases ? new[] { "-Command" } : Array.Empty<string>();
        var commandOpt = new Option<string>("--command", commandAliases);
        commandOpt.Description = "Command name";
        rc.Add(commandOpt);

        string[] debugAliases = includeDeprecatedAliases ? new[] { "-Debug" } : Array.Empty<string>();
        var debugOpt = new Option<bool>("--debug", debugAliases);
        debugOpt.Description = "Enables debug logging and echoes command-line arguments";
        rc.Add(debugOpt);

        string[] dryRunAliases = includeDeprecatedAliases ? new[] { "-DryRun" } : Array.Empty<string>();
        var dryRunOpt = new Option<bool>("--dry-run", dryRunAliases);
        dryRunOpt.Description = "Runs in output-only mode; no changes are persisted";
        rc.Add(dryRunOpt);

        string[] digitsAliases = includeDeprecatedAliases ? new[] { "-D", "-d", "-Digits" } : new[] { "-D", "-d" };
        var digitsOpt = new Option<string>("--digits", digitsAliases);
        digitsOpt.Description = "Semicolon-separated digit indices or * for all";
        rc.Add(digitsOpt);

        string[] noErrorAliases = includeDeprecatedAliases ? new[] { "-z", "-NoError" } : new[] { "-z" };
        var noErrorOpt = new Option<bool>("--no-error", noErrorAliases);
        noErrorOpt.Description = "Forces zero exit code on otherwise failing executions";
        rc.Add(noErrorOpt);

        var getMdHelpOpt = new Option<bool>("--get-md-help");
        getMdHelpOpt.Description = "Writes the embedded docs.md file to the current working directory";
        rc.Add(getMdHelpOpt);

        string[] noOverrideAliases = includeDeprecatedAliases ? new[] { "-NoOverride" } : Array.Empty<string>();
        var noOverrideOpt = new Option<bool>("--no-override", noOverrideAliases);
        noOverrideOpt.Description = "Ignores any saved pending-increment override";
        rc.Add(noOverrideOpt);

        string[] outputAliases = includeDeprecatedAliases ? new[] { "-O", "-o", "-Output", "-output" } : new[] { "-O", "-o" };
        var outputOpt = new Option<string>("--output", outputAliases);
        outputOpt.Description = "Output mode: env|con|azdo[:VarName]|file[:FileName]|con-nf";
        rc.Add(outputOpt);

        string[] incrementAliases = includeDeprecatedAliases ? new[] { "-I", "-i", "-Increment" } : new[] { "-I", "-i" };
        var incrementOpt = new Option<bool>("--increment", incrementAliases);
        incrementOpt.Description = "Performs a version increment before other operations";
        rc.Add(incrementOpt);

        string[] quickValueAliases = includeDeprecatedAliases ? new[] { "-Q", "-QuickValue" } : new[] { "-Q" };
        var quickValueOpt = new Option<string>("--quick-value", quickValueAliases);
        quickValueOpt.Description = "Quick value parameter used by set/override/behaviour/prefix commands";
        rc.Add(quickValueOpt);

        string[] releaseAliases = includeDeprecatedAliases ? new[] { "-R", "-Release" } : new[] { "-R" };
        var releaseOpt = new Option<string>("--release", releaseAliases);
        releaseOpt.Description = "Release name associated with this version";
        rc.Add(releaseOpt);

        string[] rootPathAliases = includeDeprecatedAliases ? new[] { "-Root" } : Array.Empty<string>();
        var rootPathOpt = new Option<string>("--root", rootPathAliases);
        rootPathOpt.Description = "Root directory from which to search for versionable files";
        rc.Add(rootPathOpt);

        string[] traceAliases = includeDeprecatedAliases ? new[] { "-Trace" } : Array.Empty<string>();
        var traceOpt = new Option<string>("--trace", traceAliases);
        traceOpt.Description = "Trace level: info|verbose|off";
        rc.Add(traceOpt);

        string[] versionSourceAliases = includeDeprecatedAliases ? new[] { "-V", "-v", "-VersionSource" } : new[] { "-V", "-v" };
        var versionSourceOpt = new Option<string>("--version-source", versionSourceAliases);
        versionSourceOpt.Description = "Version store initialisation string";
        rc.Add(versionSourceOpt);

        string[] minMatchAliases = includeDeprecatedAliases ? new[] { "-M", "-m", "-MinMatch" } : new[] { "-M", "-m" };
        var minMatchOpt = new Option<string>("--min-match", minMatchAliases);
        minMatchOpt.Description = "Semicolon-separated minmatch patterns for file update";
        rc.Add(minMatchOpt);

        return rc;
    }

    private static void EmitDeprecatedAliasWarnings(string[] args) {
        var seenAliases = new HashSet<string>(StringComparer.Ordinal);
        foreach (string arg in args) {
            string extractedToken = ExtractOptionToken(arg);
            if (!deprecatedAliasMap.TryGetValue(extractedToken, out string? canonicalAlias)) {
                continue;
            }

            if (seenAliases.Add(extractedToken)) {
                Console.Error.WriteLine(FormatDeprecationWarning(extractedToken, canonicalAlias));
            }
        }
    }

    private static string ExtractOptionToken(string rawArg) {
        if (string.IsNullOrWhiteSpace(rawArg) || !rawArg.StartsWith("-", StringComparison.Ordinal)) {
            return string.Empty;
        }

        int equalsIndex = rawArg.IndexOf('=');
        if (equalsIndex >= 0) {
            return rawArg.Substring(0, equalsIndex);
        }

        return rawArg;
    }

    private static string FormatDeprecationWarning(string deprecatedAlias, string canonicalAlias) {
        return $"WARNING: '{deprecatedAlias}' is deprecated. Use '{canonicalAlias}' instead.";
    }
}
