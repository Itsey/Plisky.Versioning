using System.Text.RegularExpressions;
using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;

namespace Versonify.ITest;

public class KebabCaseOptions : IDisposable {
    protected Bilge b = new Bilge("Versonify-ITest");
    protected UnitTestHelper uth;
    protected TestHelper sut;
    private readonly List<string> tempDirectories = new();

    public KebabCaseOptions() {
        b.Info.Flow();

        uth = new UnitTestHelper();
        sut = new TestHelper(uth);
    }

    public void Dispose() {
        foreach (string tempDirectory in tempDirectories) {
            if (Directory.Exists(tempDirectory)) {
                Directory.Delete(tempDirectory, true);
            }
        }
        uth.ClearUpTestFiles();
    }

    [Theory]
    [InlineData("--command=passive --version-source={VS}")]
    [InlineData("passive --version-source={VS} --debug")]
    [InlineData("set --version-source={VS} --digits=0 --quick-value=9 --dry-run")]
    [InlineData("behaviour --version-source={VS} --digits=*")]
    [InlineData("updatefiles --root={ROOT} --increment --version-source={VS} --min-match=*.zzz --output=con --no-error")]
    [InlineData("updatefiles --root={ROOT} --increment --version-source={VS} --min-match={MM}|StdFile --no-override")]
    [InlineData("passive --version-source={VS} --output=con")]
    [InlineData("updatefiles --root={ROOT} --increment --version-source={VS} --min-match={MM}|StdFile")]
    [InlineData("override --version-source={VS} --quick-value=9.9.9")]
    [InlineData("set --version-source={VS} --release=Beta")]
    [InlineData("updatefiles --root={ROOT} --version-source={VS} --min-match={MM}|StdFile")]
    [InlineData("passive --version-source={VS} --trace=info")]
    [InlineData("passive --version-source={VS}")]
    [InlineData("updatefiles --root={ROOT} --version-source={VS} --increment --min-match={MM}|StdFile")]
    [InlineData("passive --version-source={VS} --digit-group=default")]
    [InlineData("passive --version-source={VS} --pre-release")]
    public async Task Canonical_long_options_parse_without_warning(string argsTemplate) {
        string workingDirectory = CreateTemporaryDirectory();
        string versionStorePath = await CreateVersionStore(workingDirectory, "1.0.0");
        string projectFilePath = CopyResourceToDirectory(TestResourcesReferences.NetStdNone, workingDirectory, "Sample.csproj");
        string finalArgs = argsTemplate
            .Replace("{VS}", versionStorePath)
            .Replace("{ROOT}", workingDirectory)
            .Replace("{MM}", projectFilePath);

        if (finalArgs.Contains("--no-override", StringComparison.Ordinal)) {
            _ = await sut.ExecuteVersonify($"override --version-source={versionStorePath} --quick-value=9.9.9");
            sut.LastExecutionExitCode.ShouldBe(0);
        }

        var result = await sut.ExecuteVersonifyWithStreams(finalArgs, workingDirectory);

        result.ExitCode.ShouldBe(0);
        result.StdErr.ShouldNotContain("WARNING:");
    }

    [Theory]
    [InlineData("-Command", "--command")]
    [InlineData("-Debug", "--debug")]
    [InlineData("-DryRun", "--dry-run")]
    [InlineData("-Digits", "--digits")]
    [InlineData("-NoError", "--no-error")]
    [InlineData("-NoOverride", "--no-override")]
    [InlineData("-Output", "--output")]
    [InlineData("-Increment", "--increment")]
    [InlineData("-QuickValue", "--quick-value")]
    [InlineData("-Release", "--release")]
    [InlineData("-Root", "--root")]
    [InlineData("-Trace", "--trace")]
    [InlineData("-VersionSource", "--version-source")]
    [InlineData("-MinMatch", "--min-match")]
    [InlineData("-output", "--output")]
    public async Task Deprecated_long_aliases_emit_warning_and_remain_functional(string deprecatedAlias, string canonicalAlias) {
        string workingDirectory = CreateTemporaryDirectory();
        string versionStorePath = await CreateVersionStore(workingDirectory, "1.0.0");
        string projectFilePath = CopyResourceToDirectory(TestResourcesReferences.NetStdNone, workingDirectory, "Sample.csproj");
        string finalArgs = BuildArgsForAlias(deprecatedAlias, versionStorePath, workingDirectory, projectFilePath);

        if (deprecatedAlias.Equals("-NoOverride", StringComparison.Ordinal)) {
            _ = await sut.ExecuteVersonify($"override --version-source={versionStorePath} --quick-value=9.9.9");
            sut.LastExecutionExitCode.ShouldBe(0);
        }

        var result = await sut.ExecuteVersonifyWithStreams(finalArgs, workingDirectory);
        string expectedWarning = $"WARNING: '{deprecatedAlias}' is deprecated. Use '{canonicalAlias}' instead.";

        result.ExitCode.ShouldBe(0);
        GetWarningLineCount(result.StdErr, expectedWarning).ShouldBe(1);
    }

    [Theory]
    [InlineData("-D")]
    [InlineData("-d")]
    [InlineData("-O")]
    [InlineData("-o")]
    [InlineData("-I")]
    [InlineData("-i")]
    [InlineData("-Q")]
    [InlineData("-R")]
    [InlineData("-V")]
    [InlineData("-v")]
    [InlineData("-M")]
    [InlineData("-m")]
    [InlineData("-z")]
    [InlineData("-g")]
    [InlineData("-p")]
    public async Task Short_aliases_are_silent_and_functional(string shortAlias) {
        string workingDirectory = CreateTemporaryDirectory();
        string versionStorePath = await CreateVersionStore(workingDirectory, "1.0.0");
        string projectFilePath = CopyResourceToDirectory(TestResourcesReferences.NetStdNone, workingDirectory, "Sample.csproj");
        string finalArgs = BuildArgsForAlias(shortAlias, versionStorePath, workingDirectory, projectFilePath);

        var result = await sut.ExecuteVersonifyWithStreams(finalArgs, workingDirectory);

        result.ExitCode.ShouldBe(0);
        result.StdErr.ShouldNotContain("WARNING:");
    }

    [Fact]
    public async Task Repeated_deprecated_alias_emits_single_warning() {
        string workingDirectory = CreateTemporaryDirectory();
        string versionStorePath = await CreateVersionStore(workingDirectory, "1.0.0");
        string finalArgs = $"passive -VersionSource={versionStorePath} -Debug -Debug";

        var result = await sut.ExecuteVersonifyWithStreams(finalArgs, workingDirectory);
        string expectedWarning = "WARNING: '-Debug' is deprecated. Use '--debug' instead.";

        result.ExitCode.ShouldBe(0);
        GetWarningLineCount(result.StdErr, expectedWarning).ShouldBe(1);
    }

    [Fact]
    public async Task Help_lists_canonical_and_short_aliases_only() {
        var result = await sut.ExecuteVersonifyWithStreams("--help");

        result.ExitCode.ShouldBe(0);
        result.StdOut.ShouldContain("--command");
        result.StdOut.ShouldContain("--debug");
        result.StdOut.ShouldContain("--dry-run");
        result.StdOut.ShouldContain("--digits");
        result.StdOut.ShouldContain("--no-error");
        result.StdOut.ShouldContain("--get-md-help");
        result.StdOut.ShouldContain("--no-override");
        result.StdOut.ShouldContain("--output");
        result.StdOut.ShouldContain("--increment");
        result.StdOut.ShouldContain("--quick-value");
        result.StdOut.ShouldContain("--release");
        result.StdOut.ShouldContain("--root");
        result.StdOut.ShouldContain("--trace");
        result.StdOut.ShouldContain("--version-source");
        result.StdOut.ShouldContain("--min-match");
        result.StdOut.ShouldContain("--digit-group");
        result.StdOut.ShouldContain("--pre-release");
        result.StdOut.ShouldContain("-D");
        result.StdOut.ShouldContain("-d");
        result.StdOut.ShouldContain("-g");
        result.StdOut.ShouldContain("-p");
        result.StdOut.ShouldContain("-O");
        result.StdOut.ShouldContain("-o");
        result.StdOut.ShouldContain("-I");
        result.StdOut.ShouldContain("-i");
        result.StdOut.ShouldContain("-Q");
        result.StdOut.ShouldContain("-R");
        result.StdOut.ShouldContain("-V");
        result.StdOut.ShouldContain("-v");
        result.StdOut.ShouldContain("-M");
        result.StdOut.ShouldContain("-m");
        result.StdOut.ShouldContain("-z");

        var deprecatedLongTokens = GetSingleDashLongTokens(result.StdOut);

        deprecatedLongTokens.ShouldNotContain("-Command");
        deprecatedLongTokens.ShouldNotContain("-Debug");
        deprecatedLongTokens.ShouldNotContain("-DryRun");
        deprecatedLongTokens.ShouldNotContain("-Digits");
        deprecatedLongTokens.ShouldNotContain("-NoError");
        deprecatedLongTokens.ShouldNotContain("-NoOverride");
        deprecatedLongTokens.ShouldNotContain("-Output");
        deprecatedLongTokens.ShouldNotContain("-Increment");
        deprecatedLongTokens.ShouldNotContain("-QuickValue");
        deprecatedLongTokens.ShouldNotContain("-Release");
        deprecatedLongTokens.ShouldNotContain("-Root");
        deprecatedLongTokens.ShouldNotContain("-Trace");
        deprecatedLongTokens.ShouldNotContain("-VersionSource");
        deprecatedLongTokens.ShouldNotContain("-MinMatch");
        deprecatedLongTokens.ShouldNotContain("-output");
    }

    private static HashSet<string> GetSingleDashLongTokens(string output) {
        var result = new HashSet<string>(StringComparer.Ordinal);
        var matches = Regex.Matches(output, @"(?<!-)-[A-Za-z][A-Za-z]+");
        foreach (Match match in matches) {
            result.Add(match.Value);
        }

        return result;
    }

    private static int GetWarningLineCount(string stderr, string expectedWarning) {
        int result = 0;
        string[] lines = stderr.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines) {
            if (line.Equals(expectedWarning, StringComparison.Ordinal)) {
                result++;
            }
        }
        return result;
    }

    private string BuildArgsForAlias(string alias, string versionStorePath, string workingDirectory, string projectFilePath) {
        return alias switch {
            "-Command" => $"{alias}=passive --version-source={versionStorePath}",
            "-Debug" => $"passive --version-source={versionStorePath} {alias}",
            "-DryRun" => $"set --version-source={versionStorePath} --digits=0 --quick-value=9 {alias}",
            "-Digits" => $"behaviour --version-source={versionStorePath} {alias}=*",
            "-NoError" => $"updatefiles --root={workingDirectory} --increment --version-source={versionStorePath} --min-match=*.zzz --output=con {alias}",
            "-NoOverride" => $"updatefiles --root={workingDirectory} --increment --version-source={versionStorePath} --min-match={projectFilePath}|StdFile {alias}",
            "-Output" => $"passive --version-source={versionStorePath} {alias}=con",
            "-Increment" => $"updatefiles --root={workingDirectory} {alias} --version-source={versionStorePath} --min-match={projectFilePath}|StdFile",
            "-QuickValue" => $"override --version-source={versionStorePath} {alias}=9.9.9",
            "-Release" => $"set --version-source={versionStorePath} {alias}=Beta",
            "-Root" => $"updatefiles {alias}={workingDirectory} --increment --version-source={versionStorePath} --min-match={projectFilePath}|StdFile",
            "-Trace" => $"passive --version-source={versionStorePath} {alias}=info",
            "-VersionSource" => $"passive {alias}={versionStorePath}",
            "-MinMatch" => $"updatefiles --root={workingDirectory} --increment --version-source={versionStorePath} {alias}={projectFilePath}|StdFile",
            "-output" => $"passive --version-source={versionStorePath} {alias}=con",
            "-D" => $"behaviour --version-source={versionStorePath} {alias}=*",
            "-d" => $"behaviour --version-source={versionStorePath} {alias}=*",
            "-O" => $"passive --version-source={versionStorePath} {alias}=con",
            "-o" => $"passive --version-source={versionStorePath} {alias}=con",
            "-I" => $"updatefiles --root={workingDirectory} {alias} --version-source={versionStorePath} --min-match={projectFilePath}|StdFile",
            "-i" => $"updatefiles --root={workingDirectory} {alias} --version-source={versionStorePath} --min-match={projectFilePath}|StdFile",
            "-Q" => $"override --version-source={versionStorePath} {alias}=9.9.9",
            "-R" => $"set --version-source={versionStorePath} {alias}=Beta",
            "-V" => $"passive {alias}={versionStorePath}",
            "-v" => $"passive {alias}={versionStorePath}",
            "-M" => $"updatefiles --root={workingDirectory} --increment --version-source={versionStorePath} {alias}={projectFilePath}|StdFile",
            "-m" => $"updatefiles --root={workingDirectory} --increment --version-source={versionStorePath} {alias}={projectFilePath}|StdFile",
            "-z" => $"updatefiles --root={workingDirectory} --increment --version-source={versionStorePath} --min-match=*.zzz --output=con {alias}",
            "-g" => $"passive --version-source={versionStorePath} {alias}=default",
            "-p" => $"passive --version-source={versionStorePath} {alias}",
            _ => throw new InvalidOperationException($"Unsupported alias: {alias}"),
        };
    }

    private async Task<string> CreateVersionStore(string workingDirectory, string versionValue) {
        string result = Path.Combine(workingDirectory, "versionstore.vstore");
        var executionResult = await sut.ExecuteVersonifyWithStreams($"createversion --version-source={result} --quick-value={versionValue}", workingDirectory);

        executionResult.ExitCode.ShouldBe(0);
        executionResult.StdOut.ShouldContain("Creating New Version Store:");
        return result;
    }

    private string CopyResourceToDirectory(TestResourcesReferences resourceReference, string workingDirectory, string destinationFileName) {
        string resourceName = TestResources.GetIdentifiers(resourceReference)!;
        string sourcePath = uth.GetTestDataFile(resourceName);
        string result = Path.Combine(workingDirectory, destinationFileName);

        File.Copy(sourcePath, result, true);
        return result;
    }

    private string CreateTemporaryDirectory() {
        string result = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        Directory.CreateDirectory(result);
        tempDirectories.Add(result);
        return result;
    }
}
