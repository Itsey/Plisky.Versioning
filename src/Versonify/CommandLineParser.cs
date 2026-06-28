namespace Versonify;

using System;
using System.Collections.Generic;
using System.CommandLine;
using static Versonify.Clargs;

public static class CommandLineParser {

    private static readonly IReadOnlyDictionary<string, string> deprecatedAliasMap = new Dictionary<string, string>(StringComparer.Ordinal) {
        ["-Command"] = COMMAND_ARG,
        ["-Debug"] = DEBUG_ARG,
        ["-DryRun"] = DRY_RUN_ARG,
        ["-Digits"] = DIGITS_ARG,
        ["-NoError"] = NO_ERROR_ARG,
        ["-NoOverride"] = NO_OVERRIDE_ARG,
        ["-Output"] = OUTPUT_ARG,
        ["-Increment"] = INCREMENT_ARG,
        ["-QuickValue"] = QUICK_VALUE_ARG,
        ["-Release"] = RELEASE_ARG,
        ["-Root"] = ROOT_ARG,
        ["-Trace"] = TRACE_ARG,
        ["-VersionSource"] = VERSION_SOURCE_ARG,
        ["-MinMatch"] = MIN_MATCH_ARG,
        ["-output"] = OUTPUT_ARG,
    };

    public static bool IsHelpRequested(string[] args) {
        foreach (string arg in args) {
            if (arg.Equals(HELP_ARG, StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("-h", StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }

        return false;
    }

    public static void DisplayHelp() {
        var helpCommand = BuildRootCommand(false);
        helpCommand.Parse(new[] { HELP_ARG }).Invoke(new System.CommandLine.InvocationConfiguration());
    }

    public static (bool Success, VersonifyOptions Options) Parse(string[] args) {
        var options = new VersonifyOptions();
        var rootCommand = BuildRootCommand();
        string[] normalizedArgs = NormalizeDigitGroupArguments(args);
        var parseResult = rootCommand.Parse(normalizedArgs);

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
        var commandOpt = FindOption<string>(rootCommand, COMMAND_ARG);
        var debugOpt = FindOption<bool>(rootCommand, DEBUG_ARG);
        var dryRunOpt = FindOption<bool>(rootCommand, DRY_RUN_ARG);
        var digitsOpt = FindOption<string>(rootCommand, DIGITS_ARG);
        var noErrorOpt = FindOption<bool>(rootCommand, NO_ERROR_ARG);
        var getMdHelpOpt = FindOption<bool>(rootCommand, GET_MD_HELP_ARG);
        var noOverrideOpt = FindOption<bool>(rootCommand, NO_OVERRIDE_ARG);
        var outputOpt = FindOption<string>(rootCommand, OUTPUT_ARG);
        var incrementOpt = FindOption<bool>(rootCommand, INCREMENT_ARG);
        var quickValueOpt = FindOption<string>(rootCommand, QUICK_VALUE_ARG);
        var releaseOpt = FindOption<string>(rootCommand, RELEASE_ARG);
        var rootPathOpt = FindOption<string>(rootCommand, ROOT_ARG);
        var traceOpt = FindOption<string>(rootCommand, TRACE_ARG);
        var versionSourceOpt = FindOption<string>(rootCommand, VERSION_SOURCE_ARG);
        var minMatchOpt = FindOption<string>(rootCommand, MIN_MATCH_ARG);
        var digitGroupOpt = FindOption<string>(rootCommand, DIGIT_GROUP_ARG);
        var preReleaseOpt = FindOption<bool>(rootCommand, PRE_RELEASE_ARG);

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

        options.DigitGroup = parseResult.GetValue(digitGroupOpt!);
        options.PreRelease = parseResult.GetValue(preReleaseOpt!);

        options.RawOutputOptions = parseResult.GetValue(outputOpt!);
        options.OutputOptions = options.RawOutputOptions ?? "";

        return (true, options);
    }

    private static string[] NormalizeDigitGroupArguments(string[] args) {
        string[] result = new string[args.Length];
        const string LONGDIGITGROUP = DIGIT_GROUP_ARG + "=";
        const string SHORTDIGITGROUP = "-g=";

        for (int i = 0; i < args.Length; i++) {
            string argument = args[i];
            if (argument.StartsWith(LONGDIGITGROUP, StringComparison.Ordinal)) {
                string value = argument.Substring(LONGDIGITGROUP.Length);
                if (string.IsNullOrEmpty(value) || value == "\"\"") {
                    result[i] = $"{LONGDIGITGROUP}default";
                } else {
                    result[i] = argument;
                }
            } else if (argument.StartsWith(SHORTDIGITGROUP, StringComparison.Ordinal)) {
                string value = argument.Substring(SHORTDIGITGROUP.Length);
                if (string.IsNullOrEmpty(value) || value == "\"\"") {
                    result[i] = $"{SHORTDIGITGROUP}default";
                } else {
                    result[i] = argument;
                }
            } else {
                result[i] = argument;
            }
        }

        return result;
    }

    private static Option<T>? FindOption<T>(RootCommand rootCommand, string alias) {
        foreach (var symbol in rootCommand.Options) {
            if (symbol is Option<T> opt) {
                if (symbol.Name.Equals(alias, StringComparison.Ordinal)) {
                    return opt;
                }
                foreach (string a in opt.Aliases) {
                    if (a.Equals(alias, StringComparison.Ordinal)) {
                        return opt;
                    }
                }
            }
        }
        return null;
    }

    private static RootCommand BuildRootCommand(bool includeDeprecatedAliases = true) {
        var rc = new RootCommand($"Parameter help for Versonify. AI/automation: run {GET_MD_HELP_ARG} to write the embedded docs.md file into the current directory.");
        rc.TreatUnmatchedTokensAsErrors = true;

        var commandArg = new Argument<string>("command");
        commandArg.Description = "Command to execute: createversion|override|updatefiles|passive|behaviour|set|prefix";
        commandArg.Arity = ArgumentArity.ZeroOrOne;
        commandArg.DefaultValueFactory = _ => null!;
        rc.Add(commandArg);

        string[] commandAliases = includeDeprecatedAliases ? new[] { "-Command" } : Array.Empty<string>();
        var commandOpt = new Option<string>(COMMAND_ARG, commandAliases);
        commandOpt.Description = "Command name";
        rc.Add(commandOpt);

        string[] debugAliases = includeDeprecatedAliases ? new[] { "-Debug" } : Array.Empty<string>();
        var debugOpt = new Option<bool>(DEBUG_ARG, debugAliases);
        debugOpt.Description = "Enables debug logging and echoes command-line arguments";
        rc.Add(debugOpt);

        string[] dryRunAliases = includeDeprecatedAliases ? new[] { "-DryRun" } : Array.Empty<string>();
        var dryRunOpt = new Option<bool>(DRY_RUN_ARG, dryRunAliases);
        dryRunOpt.Description = "Runs in output-only mode; no changes are persisted";
        rc.Add(dryRunOpt);

        string[] digitsAliases = includeDeprecatedAliases ? new[] { "-D", "-d", "-Digits" } : new[] { "-D", "-d" };
        var digitsOpt = new Option<string>(DIGITS_ARG, digitsAliases);
        digitsOpt.Description = "Semicolon-separated digit indices or * for all";
        rc.Add(digitsOpt);

        string[] noErrorAliases = includeDeprecatedAliases ? new[] { "-z", "-NoError" } : new[] { "-z" };
        var noErrorOpt = new Option<bool>(NO_ERROR_ARG, noErrorAliases);
        noErrorOpt.Description = "Forces zero exit code on otherwise failing executions";
        rc.Add(noErrorOpt);

        var getMdHelpOpt = new Option<bool>(GET_MD_HELP_ARG);
        getMdHelpOpt.Description = "Writes the embedded docs.md file to the current working directory";
        rc.Add(getMdHelpOpt);

        string[] noOverrideAliases = includeDeprecatedAliases ? new[] { "-NoOverride" } : Array.Empty<string>();
        var noOverrideOpt = new Option<bool>(NO_OVERRIDE_ARG, noOverrideAliases);
        noOverrideOpt.Description = "Ignores any saved pending-increment override";
        rc.Add(noOverrideOpt);

        string[] outputAliases = includeDeprecatedAliases ? new[] { "-O", "-o", "-Output", "-output" } : new[] { "-O", "-o" };
        var outputOpt = new Option<string>(OUTPUT_ARG, outputAliases);
        outputOpt.Description = "Output mode: env|con|azdo[:VarName]|file[:FileName]|con-nf";
        rc.Add(outputOpt);

        string[] incrementAliases = includeDeprecatedAliases ? new[] { "-I", "-i", "-Increment" } : new[] { "-I", "-i" };
        var incrementOpt = new Option<bool>(INCREMENT_ARG, incrementAliases);
        incrementOpt.Description = "Performs a version increment before other operations";
        rc.Add(incrementOpt);

        string[] quickValueAliases = includeDeprecatedAliases ? new[] { "-Q", "-QuickValue" } : new[] { "-Q" };
        var quickValueOpt = new Option<string>(QUICK_VALUE_ARG, quickValueAliases);
        quickValueOpt.Description = "Quick value parameter used by set/override/behaviour/prefix commands";
        rc.Add(quickValueOpt);

        string[] releaseAliases = includeDeprecatedAliases ? new[] { "-R", "-Release" } : new[] { "-R" };
        var releaseOpt = new Option<string>(RELEASE_ARG, releaseAliases);
        releaseOpt.Description = "Release name associated with this version";
        rc.Add(releaseOpt);

        string[] rootPathAliases = includeDeprecatedAliases ? new[] { "-Root" } : Array.Empty<string>();
        var rootPathOpt = new Option<string>(ROOT_ARG, rootPathAliases);
        rootPathOpt.Description = "Root directory from which to search for versionable files";
        rc.Add(rootPathOpt);

        string[] traceAliases = includeDeprecatedAliases ? new[] { "-Trace" } : Array.Empty<string>();
        var traceOpt = new Option<string>(TRACE_ARG, traceAliases);
        traceOpt.Description = "Trace level: info|verbose|off";
        rc.Add(traceOpt);

        string[] versionSourceAliases = includeDeprecatedAliases ? new[] { "-V", "-v", "-VersionSource" } : new[] { "-V", "-v" };
        var versionSourceOpt = new Option<string>(VERSION_SOURCE_ARG, versionSourceAliases);
        versionSourceOpt.Description = "Version store initialisation string";
        rc.Add(versionSourceOpt);

        string[] minMatchAliases = includeDeprecatedAliases ? new[] { "-M", "-m", "-MinMatch" } : new[] { "-M", "-m" };
        var minMatchOpt = new Option<string>(MIN_MATCH_ARG, minMatchAliases);
        minMatchOpt.Description = "Semicolon-separated minmatch patterns for file update";
        rc.Add(minMatchOpt);

        string[] digitGroupAliases = new[] { "-g" };
        var digitGroupOpt = new Option<string>(DIGIT_GROUP_ARG, digitGroupAliases);
        digitGroupOpt.Description = "Named digit group to target (e.g., 'prerelease') or * for all; comma-separated for passive mode";
        rc.Add(digitGroupOpt);

        string[] preReleaseAliases = new[] { "-p" };
        var preReleaseOpt = new Option<bool>(PRE_RELEASE_ARG, preReleaseAliases);
        preReleaseOpt.Description = "Shortcut for pre-release workflows (targets pre-release digit group)";
        rc.Add(preReleaseOpt);

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
