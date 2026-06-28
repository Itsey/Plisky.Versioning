namespace Versonify;

using System;
using System.Collections.Generic;

public enum ArgNames {
    Unknown,
    Command,
    Debug,
    DryRun,
    Digits,
    NoError,
    GetMdHelp,
    NoOverride,
    Output,
    Increment,
    QuickValue,
    Release,
    Root,
    Trace,
    VersionSource,
    MinMatch,
    DigitGroup,
    PreRelease,
    Help
}

public record Arg(ArgNames ArgName, string Value);

public static class Clargs {
    #region command line arguments
    public const string COMMAND_ARG = "--command";
    public const string DEBUG_ARG = "--debug";
    public const string DRY_RUN_ARG = "--dry-run";
    public const string DIGITS_ARG = "--digits";
    public const string NO_ERROR_ARG = "--no-error";
    public const string GET_MD_HELP_ARG = "--get-md-help";
    public const string NO_OVERRIDE_ARG = "--no-override";
    public const string OUTPUT_ARG = "--output";
    public const string INCREMENT_ARG = "--increment";
    public const string QUICK_VALUE_ARG = "--quick-value";
    public const string RELEASE_ARG = "--release";
    public const string ROOT_ARG = "--root";
    public const string TRACE_ARG = "--trace";
    public const string VERSION_SOURCE_ARG = "--version-source";
    public const string MIN_MATCH_ARG = "--min-match";
    public const string DIGIT_GROUP_ARG = "--digit-group";
    public const string PRE_RELEASE_ARG = "--pre-release";
    public const string HELP_ARG = "--help";
    #endregion

    public static string GetArgString(ArgNames argName) => argName switch {
        ArgNames.Command => COMMAND_ARG,
        ArgNames.Debug => DEBUG_ARG,
        ArgNames.DryRun => DRY_RUN_ARG,
        ArgNames.Digits => DIGITS_ARG,
        ArgNames.NoError => NO_ERROR_ARG,
        ArgNames.GetMdHelp => GET_MD_HELP_ARG,
        ArgNames.NoOverride => NO_OVERRIDE_ARG,
        ArgNames.Output => OUTPUT_ARG,
        ArgNames.Increment => INCREMENT_ARG,
        ArgNames.QuickValue => QUICK_VALUE_ARG,
        ArgNames.Release => RELEASE_ARG,
        ArgNames.Root => ROOT_ARG,
        ArgNames.Trace => TRACE_ARG,
        ArgNames.VersionSource => VERSION_SOURCE_ARG,
        ArgNames.MinMatch => MIN_MATCH_ARG,
        ArgNames.DigitGroup => DIGIT_GROUP_ARG,
        ArgNames.PreRelease => PRE_RELEASE_ARG,
        ArgNames.Help => HELP_ARG,
        ArgNames.Unknown => string.Empty,
        _ => throw new ArgumentOutOfRangeException(nameof(argName), argName, null)
    };

    public static IEnumerable<string> AllArguments(bool skipUnknown = true) {
        foreach (var arg in Enum.GetValues<ArgNames>()) {
            if (skipUnknown && arg == ArgNames.Unknown) {
                continue;
            }
            string argStr = GetArgString(arg);
            if (!string.IsNullOrEmpty(argStr)) {
                yield return argStr;
            }
        }
    }

    public static string Build(params Arg[] pairings) {
        var args = new List<string>();
        foreach (var pairing in pairings) {
            string argStr = GetArgString(pairing.ArgName);
            if (string.IsNullOrEmpty(pairing.Value)) {
                args.Add(argStr);
            } else {
                if (argStr.Length == 0) {
                    args.Add(pairing.Value);
                } else {
                    args.Add($"{argStr}={pairing.Value}");
                }
            }
        }
        return string.Join(" ", args);
    }
}
