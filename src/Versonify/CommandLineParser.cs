using System;
using System.Collections.Generic;
using System.CommandLine;

namespace Versonify;

using static Versonify.Clargs;

public static class CommandLineParser {
    private static readonly IReadOnlyDictionary<string, string> deprecatedAliasMapLower = new Dictionary<string, string>(StringComparer.Ordinal) {
        ["-command"] = COMMAND_ARG,
        ["-debug"] = DEBUG_ARG,
        ["-dryrun"] = DRY_RUN_ARG,
        ["-digits"] = DIGITS_ARG,
        ["-noerror"] = NO_ERROR_ARG,
        ["-nooverride"] = NO_OVERRIDE_ARG,
        ["-output"] = OUTPUT_ARG,
        ["-increment"] = INCREMENT_ARG,
        ["-quickvalue"] = QUICK_VALUE_ARG,
        ["-release"] = RELEASE_ARG,
        ["-root"] = ROOT_ARG,
        ["-trace"] = TRACE_ARG,
        ["-versionsource"] = VERSION_SOURCE_ARG,
        ["-vs"] = VERSION_SOURCE_ARG,
        ["-minmatch"] = MIN_MATCH_ARG,
        ["-mm"] = MIN_MATCH_ARG,
    };

    private static readonly IReadOnlyDictionary<string, string> deprecatedAliasMapUpper = new Dictionary<string, string>(StringComparer.Ordinal) {
        ["--Command"] = COMMAND_ARG,
        ["--Debug"] = DEBUG_ARG,
        ["--DryRun"] = DRY_RUN_ARG,
        ["--Digits"] = DIGITS_ARG,
        ["--NoError"] = NO_ERROR_ARG,
        ["--NoOverride"] = NO_OVERRIDE_ARG,
        ["--Output"] = OUTPUT_ARG,
        ["--Increment"] = INCREMENT_ARG,
        ["--QuickValue"] = QUICK_VALUE_ARG,
        ["--Release"] = RELEASE_ARG,
        ["--Root"] = ROOT_ARG,
        ["--Trace"] = TRACE_ARG,
        ["--VersionSource"] = VERSION_SOURCE_ARG,
        ["--MinMatch"] = MIN_MATCH_ARG,
        ["-MM"] = MIN_MATCH_ARG,
        ["--MM"] = MIN_MATCH_ARG,
    };

    public static void DisplayHelp() {
        var helpCommand = BuildRootCommand(false);
        helpCommand.Parse([HELP_ARG]).Invoke(new InvocationConfiguration());
    }

    public static bool IsHelpRequested(string[] args) {
        if (args.Length == 0) {
            return true;
        }

        foreach (string arg in args) {
            if (arg.Equals(HELP_ARG, StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("-h", StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }

        return false;
    }

    public static (bool Success, VersonifyOptions Options) Parse(string[] args) {
        var result = new VersonifyOptions();
        var rootCommand = BuildRootCommand();
        string[] normalizedArgs = NormalizeDigitGroupArguments(args);
        var parseResult = rootCommand.Parse(normalizedArgs);

        if (parseResult.Errors.Count > 0) {
            Console.WriteLine("Fatal: Invalid Arguments Passed to Versonify.");
            foreach (var error in parseResult.Errors) {
                Console.WriteLine(error.Message);
            }
            return (false, result);
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
        var flushOpt = FindOption<bool>(rootCommand, FLUSH_ARG);

        string? cmdFromPositional = parseResult.GetValue(commandArg!);
        string? cmdFromOption = parseResult.GetValue(commandOpt!);
        result.Command = cmdFromPositional ?? cmdFromOption;

        result.Debug = parseResult.GetValue(debugOpt!);
        result.DryRunOnly = parseResult.GetValue(dryRunOpt!);
        result.Flush = parseResult.GetValue(flushOpt!);
        result.ReturnZero = parseResult.GetValue(noErrorOpt!);
        result.GetMdHelp = parseResult.GetValue(getMdHelpOpt!);
        result.NoOverride = parseResult.GetValue(noOverrideOpt!);
        result.PerformIncrement = parseResult.GetValue(incrementOpt!);
        result.QuickValue = parseResult.GetValue(quickValueOpt!);
        result.Release = parseResult.GetValue(releaseOpt!);
        result.Root = parseResult.GetValue(rootPathOpt!);
        result.Trace = parseResult.GetValue(traceOpt!);
        result.VersionPersistanceValue = parseResult.GetValue(versionSourceOpt!);

        string? rawDigits = parseResult.GetValue(digitsOpt!);
        result.DigitManipulations = rawDigits?.Split(';', StringSplitOptions.RemoveEmptyEntries);

        string? rawMinMatch = parseResult.GetValue(minMatchOpt!);
        result.VersionTargetMinMatch = rawMinMatch?.Split(';', StringSplitOptions.RemoveEmptyEntries);

        result.DigitGroup = parseResult.GetValue(digitGroupOpt!);
        result.PreRelease = parseResult.GetValue(preReleaseOpt!);

        result.RawOutputOptions = parseResult.GetValue(outputOpt!);
        result.OutputOptions = result.RawOutputOptions ?? "con";

        return (true, result);
    }

    private static RootCommand BuildRootCommand(bool includeDeprecatedAliases = true) {
        var rc = new RootCommand($"Parameter help for Versonify. AI/automation: run {GET_MD_HELP_ARG} to write the embedded docs.md file into the current directory.") {
            TreatUnmatchedTokensAsErrors = true
        };

        var commandArg = new Argument<string>("command") {
            Description = "Command to execute: createversion|override|updatefiles|passive|behaviour|get|set|prefix",
            Arity = ArgumentArity.ZeroOrOne,
            DefaultValueFactory = _ => null!
        };
        rc.Add(commandArg);

        string[] commandAliases = includeDeprecatedAliases ? ["-Command", "--Command"] : [];
        var commandOpt = new Option<string>(COMMAND_ARG, commandAliases) {
            Description = "Command name"
        };
        rc.Add(commandOpt);

        string[] debugAliases = includeDeprecatedAliases ? ["-Debug", "--Debug"] : [];
        var debugOpt = new Option<bool>(DEBUG_ARG, debugAliases) {
            Description = "Enables debug logging and echoes command-line arguments"
        };
        rc.Add(debugOpt);

        string[] digitGroupAliases = includeDeprecatedAliases ? ["-g"] : ["-g"];
        var digitGroupOpt = new Option<string>(DIGIT_GROUP_ARG, digitGroupAliases) {
            Description = "Named digit group to target (e.g., 'prerelease') or * for all; comma-separated for passive mode"
        };
        rc.Add(digitGroupOpt);

        string[] digitsAliases = includeDeprecatedAliases ? ["-D", "-d", "-Digits", "--Digits"] : ["-d"];
        var digitsOpt = new Option<string>(DIGITS_ARG, digitsAliases) {
            Description = "Semicolon-separated digit indices or * for all"
        };
        rc.Add(digitsOpt);

        string[] dryRunAliases = includeDeprecatedAliases ? ["-DryRun", "--DryRun"] : [];
        var dryRunOpt = new Option<bool>(DRY_RUN_ARG, dryRunAliases) {
            Description = "Runs in output-only mode; no changes are persisted"
        };
        rc.Add(dryRunOpt);

        var flushOpt = new Option<bool>(FLUSH_ARG) {
            Description = "Forces a flush of trace listeners before exit. Do not use unless diagnosing faults."
        };
        rc.Add(flushOpt);

        var getMdHelpOpt = new Option<bool>(GET_MD_HELP_ARG) {
            Description = "Writes the embedded docs.md file to the current working directory"
        };
        rc.Add(getMdHelpOpt);

        string[] incrementAliases = includeDeprecatedAliases ? ["-I", "-i", "-Increment"] : ["-i"];
        var incrementOpt = new Option<bool>(INCREMENT_ARG, incrementAliases) {
            Description = "Performs a version increment before other operations"
        };
        rc.Add(incrementOpt);

        string[] minMatchAliases = includeDeprecatedAliases ? ["-M", "-m", "-MinMatch", "--MinMatch", "-MM", "-mm"] : ["-m"];
        var minMatchOpt = new Option<string>(MIN_MATCH_ARG, minMatchAliases) {
            Description = "Semicolon-separated minmatch patterns for file update"
        };
        rc.Add(minMatchOpt);

        string[] noErrorAliases = includeDeprecatedAliases ? ["-z", "-Z", "-NoError", "--NoError"] : ["-z"];
        var noErrorOpt = new Option<bool>(NO_ERROR_ARG, noErrorAliases) {
            Description = "Forces zero exit code on otherwise failing executions"
        };
        rc.Add(noErrorOpt);

        string[] noOverrideAliases = includeDeprecatedAliases ? ["-NoOverride", "--NoOverride", "-NO", "-no"] : [];
        var noOverrideOpt = new Option<bool>(NO_OVERRIDE_ARG, noOverrideAliases) {
            Description = "Ignores any saved pending-increment override"
        };
        rc.Add(noOverrideOpt);

        string[] outputAliases = includeDeprecatedAliases ? ["-O", "-o", "-Output", "-output"] : ["-o"];
        var outputOpt = new Option<string>(OUTPUT_ARG, outputAliases) {
            Description = "Output mode: env|con|jcon|azdo[:VarName]|file[:FileName]|con-nf"
        };
        rc.Add(outputOpt);

        string[] preReleaseAliases = includeDeprecatedAliases ? ["-p"] : ["-p"];
        var preReleaseOpt = new Option<bool>(PRE_RELEASE_ARG, preReleaseAliases) {
            Description = "Shortcut for pre-release workflows (targets pre-release digit group)"
        };
        rc.Add(preReleaseOpt);

        string[] quickValueAliases = includeDeprecatedAliases ? ["-Q", "-QuickValue", "-q"] : ["-q"];
        var quickValueOpt = new Option<string>(QUICK_VALUE_ARG, quickValueAliases) {
            Description = "Quick value parameter used by set/override/behaviour/prefix commands"
        };
        rc.Add(quickValueOpt);

        string[] releaseAliases = includeDeprecatedAliases ? ["-R", "-r", "-Release", "--Release"] : ["-r"];
        var releaseOpt = new Option<string>(RELEASE_ARG, releaseAliases) {
            Description = "Release name associated with this version"
        };
        rc.Add(releaseOpt);

        string[] rootPathAliases = includeDeprecatedAliases ? ["-Root", "--Root"] : [];
        var rootPathOpt = new Option<string>(ROOT_ARG, rootPathAliases) {
            Description = "Root directory from which to search for versionable files"
        };
        rc.Add(rootPathOpt);

        string[] traceAliases = includeDeprecatedAliases ? ["-Trace", "--Trace"] : [];
        var traceOpt = new Option<string>(TRACE_ARG, traceAliases) {
            Description = "Trace level: info|verbose|off"
        };
        rc.Add(traceOpt);

        string[] versionSourceAliases = includeDeprecatedAliases ? ["-V", "-v", "-VS", "-vs", "-VersionSource", "--VersionSource"] : ["-v"];
        var versionSourceOpt = new Option<string>(VERSION_SOURCE_ARG, versionSourceAliases) {
            Description = "Version store initialisation string"
        };
        rc.Add(versionSourceOpt);

        return rc;
    }

    private static void EmitDeprecatedAliasWarnings(string[] args) {
        var seenAliases = new HashSet<string>(StringComparer.Ordinal);
        foreach (string arg in args) {
            string extractedToken = ExtractOptionToken(arg);

            if (!deprecatedAliasMapUpper.TryGetValue(extractedToken, out string? canonicalAlias) &&
                !deprecatedAliasMapLower.TryGetValue(extractedToken.ToLowerInvariant(), out canonicalAlias)) {
                continue;
            }

            if (seenAliases.Add(extractedToken)) {
                Console.Error.WriteLine(FormatDeprecationWarning(extractedToken, canonicalAlias));
            }
        }
    }

    private static string ExtractOptionToken(string rawArg) {
        if (string.IsNullOrWhiteSpace(rawArg) || !rawArg.StartsWith('-')) {
            return string.Empty;
        }

        int equalsIndex = rawArg.IndexOf('=');
        if (equalsIndex >= 0) {
            return rawArg[..equalsIndex];
        }

        return rawArg;
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

    private static string FormatDeprecationWarning(string deprecatedAlias, string canonicalAlias) {
        return $"WARNING: '{deprecatedAlias}' is deprecated. Use '{canonicalAlias}' instead.";
    }

    private static string[] NormalizeDigitGroupArguments(string[] args) {
        string[] result = new string[args.Length];
        const string LONGDIGITGROUP = DIGIT_GROUP_ARG + "=";
        const string SHORTDIGITGROUP = "-g=";

        for (int i = 0; i < args.Length; i++) {
            string argument = args[i];
            if (argument.StartsWith(LONGDIGITGROUP, StringComparison.Ordinal)) {
                string value = argument[LONGDIGITGROUP.Length..];
                if (string.IsNullOrEmpty(value) || value == "\"\"") {
                    result[i] = $"{LONGDIGITGROUP}default";
                } else {
                    result[i] = argument;
                }
            } else if (argument.StartsWith(SHORTDIGITGROUP, StringComparison.Ordinal)) {
                string value = argument[SHORTDIGITGROUP.Length..];
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
}