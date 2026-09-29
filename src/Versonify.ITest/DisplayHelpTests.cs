using System.Text;
using System.Text.RegularExpressions;
using Shouldly;

namespace Versonify.ITest;

/// <summary>
/// Exercises CommandLineParser.DisplayHelp in process so the help rendering path itself is covered,
/// rather than only the subprocess behaviour asserted by the CLI integration tests.
/// </summary>
public class DisplayHelpTests {

    [Fact]
    public void DisplayHelp_writes_help_text_to_the_console() {
        string output = CaptureDisplayHelp();

        output.ShouldNotBeNullOrWhiteSpace();
        output.ShouldContain(Clargs.HELP_ARG);
    }

    [Fact]
    public void DisplayHelp_lists_every_canonical_argument() {
        string output = CaptureDisplayHelp();

        foreach (string canonicalArg in Clargs.AllArguments()) {
            output.ShouldContain(canonicalArg, customMessage: $"Canonical argument '{canonicalArg}' should appear in the help output.");
        }
    }

    [Fact]
    public void DisplayHelp_lists_supported_short_aliases() {
        string output = CaptureDisplayHelp();

        foreach (string shortAlias in new[] { "-d", "-g", "-p", "-o", "-i", "-q", "-r", "-v", "-m", "-z" }) {
            output.ShouldContain(shortAlias, customMessage: $"Short alias '{shortAlias}' should appear in the help output.");
        }
    }

    [Fact]
    public void DisplayHelp_excludes_deprecated_long_aliases() {
        string output = CaptureDisplayHelp();
        var singleDashLongTokens = GetSingleDashLongTokens(output);

        foreach (string deprecatedAlias in new[] {
            "-Command", "-Debug", "-DryRun", "-Digits", "-NoError", "-NoOverride", "-Output",
            "-Increment", "-QuickValue", "-Release", "-Root", "-Trace", "-VersionSource", "-MinMatch",
        }) {
            bool isPresent = singleDashLongTokens.Contains(deprecatedAlias);

            isPresent.ShouldBeFalse($"Deprecated alias '{deprecatedAlias}' must not be advertised in help output.");
        }
    }

    [Fact]
    public void DisplayHelp_does_not_emit_deprecation_warnings() {
        var originalError = Console.Error;
        var errorBuffer = new StringWriter();

        try {
            Console.SetError(errorBuffer);
            _ = CaptureDisplayHelp();
        } finally {
            Console.SetError(originalError);
        }

        errorBuffer.ToString().ShouldNotContain("WARNING:");
    }

    [Fact]
    public void DisplayHelp_can_be_called_repeatedly() {
        string firstOutput = CaptureDisplayHelp();
        string secondOutput = CaptureDisplayHelp();

        secondOutput.ShouldBe(firstOutput);
    }

    private static string CaptureDisplayHelp() {
        var originalOutput = Console.Out;
        var buffer = new StringWriter(new StringBuilder());

        try {
            Console.SetOut(buffer);
            CommandLineParser.DisplayHelp();
        } finally {
            Console.SetOut(originalOutput);
        }

        return buffer.ToString();
    }

    private static HashSet<string> GetSingleDashLongTokens(string output) {
        var result = new HashSet<string>(StringComparer.Ordinal);

        foreach (Match match in Regex.Matches(output, @"(?<!-)-[A-Za-z][A-Za-z]+")) {
            result.Add(match.Value);
        }

        return result;
    }
}
