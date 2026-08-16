using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;

namespace Versonify.ITest;

public class CommandLineArgumentCoverageTests : IDisposable {
    protected Bilge b = new Bilge("Versonify-ITest");
    protected TestHelper th;
    protected UnitTestHelper uth;

    public CommandLineArgumentCoverageTests() {
        b.Info.Flow();

        uth = new UnitTestHelper();
        th = new TestHelper(uth);
    }

    [Fact(Skip = "Deprecated alias support kept till vnext - LFY-70.")]
    public async Task Deprecated_DG_alias_is_not_accepted() {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.OneEachBehaviourStore)!;
        string versionStorePath = uth.GetTestDataFile(resourceName);
        var output = await th.ExecuteVersonify($"behaviour -VersionSource={versionStorePath} -DG=*");
        output.Item2.ShouldNotBe(0, "Deprecated alias -DG must not be accepted.");
    }

    [Fact(Skip = "Deprecated alias support kept till vnext - LFY-70.")]
    public async Task Deprecated_MM_alias_is_not_accepted() {
        b.Info.Flow();
        string workingDirectory = CreateTemporaryDirectory();
        try {
            string versionStorePath = await CreateVersionStore(workingDirectory, "1.0.0");
            string projectFilePath = CopyResourceToDirectory(TestResourcesReferences.NetStdNone, workingDirectory, "Sample.csproj");
            var output = await th.ExecuteVersonify($"updatefiles -Root={workingDirectory} -I -VersionSource={versionStorePath} -MM={projectFilePath}|StdFile");
            output.Item2.ShouldNotBe(0, "Deprecated alias -MM must not be accepted.");
        } finally {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact(Skip = "Deprecated alias support kept till vnext - LFY-70.")]
    public async Task Deprecated_NO_alias_is_not_accepted() {
        b.Info.Flow();
        string workingDirectory = CreateTemporaryDirectory();
        try {
            string versionStorePath = await CreateVersionStore(workingDirectory, "1.0.0");
            string projectFilePath = CopyResourceToDirectory(TestResourcesReferences.NetStdNone, workingDirectory, "Sample.csproj");
            _ = await th.ExecuteVersonify($"override -VersionSource={versionStorePath} -QuickValue=9.9.9");
            var output = await th.ExecuteVersonify($"updatefiles -Root={workingDirectory} -I -VersionSource={versionStorePath} -MinMatch={projectFilePath}|StdFile -NO");
            output.Item2.ShouldNotBe(0, "Deprecated alias -NO must not be accepted.");
        } finally {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact(Skip = "Deprecated alias support kept till vnext - LFY-70.")]
    public async Task Deprecated_VS_alias_is_not_accepted() {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!;
        string versionStorePath = uth.GetTestDataFile(resourceName);
        var output = await th.ExecuteVersonify($"passive -VS={versionStorePath}");

        output.Item2.ShouldNotBe(0, "Deprecated alias -VS must not be accepted.");
    }

    [Fact]
    public async Task Digits_argument_loads_behaviour_output() {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.OneEachBehaviourStore)!;
        string versionStorePath = uth.GetTestDataFile(resourceName);
        var output = await th.ExecuteVersonify($"behaviour -VersionSource={versionStorePath} -Digits=*");
        output.Item1.ShouldContain("[0]:Fixed(0)");
        output.Item1.ShouldContain("[7]:ReleaseName(8)");
        output.Item2.ShouldBe(0);
    }

    public void Dispose() {
        uth.ClearUpTestFiles();
    }

    [Fact]
    public async Task DryRun_argument_does_not_persist_set_command_changes() {
        b.Info.Flow();
        string workingDirectory = CreateTemporaryDirectory();

        try {
            string versionStorePath = await CreateVersionStore(workingDirectory, "1.0.0");
            string before = File.ReadAllText(versionStorePath);

            var output = await th.ExecuteVersonify($"set -VersionSource={versionStorePath} -D=0 -QuickValue=9 -DryRun");
            string after = File.ReadAllText(versionStorePath);

            output.Item1.ShouldContain("DryRun - Would Save:");
            after.ShouldBe(before);
            output.Item2.ShouldBe(0);
        } finally {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    public async Task GetMdHelp_argument_writes_docs_file_to_current_directory() {
        b.Info.Flow();
        string workingDirectory = CreateTemporaryDirectory();

        try {
            var result = await th.ExecuteVersonifyWithStreams("--get-md-help", workingDirectory);
            string docsPath = Path.Combine(workingDirectory, "docs.md");

            result.ExitCode.ShouldBe(0);
            result.StdOut.ShouldContain("Wrote markdown help to");
            File.Exists(docsPath).ShouldBeTrue();
            File.ReadAllText(docsPath).ShouldContain("Versonify CLI arguments");
        } finally {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    public async Task Invalid_argument_prints_error_and_exits_nonzero() {
        b.Info.Flow();
        var output = await th.ExecuteVersonify("--totally-unknown-option");
        output.Item1.ShouldContain("Fatal:");
        output.Item1.ShouldContain("Use '--help' to display available options");
        output.Item1.ShouldNotContain("Parameter help for Versonify.");
        output.Item2.ShouldNotBe(0);
    }

    [Fact]
    public async Task MinMatch_argument_updates_files() {
        b.Info.Flow();
        string workingDirectory = CreateTemporaryDirectory();
        try {
            string versionStorePath = await CreateVersionStore(workingDirectory, "1.0.0");
            string projectFilePath = CopyResourceToDirectory(TestResourcesReferences.NetStdNone, workingDirectory, "Sample.csproj");
            var output = await th.ExecuteVersonify($"updatefiles -Root={workingDirectory} -I -VersionSource={versionStorePath} -MinMatch={projectFilePath}|StdFile");
            output.Item1.ShouldContain("Version Increment Requested - Currently");
            output.Item1.ShouldContain("Version To Write:");
            output.Item2.ShouldBe(0);
        } finally {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    public async Task No_arguments_prints_how_to_get_help_and_exits_nonzero() {
        b.Info.Flow();
        var output = await th.ExecuteVersonify("");
        output.Item1.ShouldContain("Use '--help' to display available options");
        output.Item1.ShouldNotContain("Parameter help for Versonify.");
        output.Item2.ShouldNotBe(0);
    }

    [Fact]
    public async Task NoError_long_argument_forces_zero_exit_code() {
        b.Info.Flow();
        string workingDirectory = CreateTemporaryDirectory();

        try {
            string versionStorePath = await CreateVersionStore(workingDirectory, "2.0.0");

            var output = await th.ExecuteVersonify($"updatefiles -Root={workingDirectory} -Increment -VersionSource={versionStorePath} -MinMatch=*.zzz -Output=con -NoError");

            output.Item1.ShouldContain("WARNING - No files found to update.");
            output.Item2.ShouldBe(0);
        } finally {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    public async Task NoError_suppresses_exit_code_via_dash_z_alias() {
        b.Info.Flow();
        string workingDirectory = CreateTemporaryDirectory();
        try {
            string versionStorePath = await CreateVersionStore(workingDirectory, "2.0.0");
            var output = await th.ExecuteVersonify($"updatefiles -Root={workingDirectory} -Increment -VersionSource={versionStorePath} -MinMatch=*.zzz -Output=con -z");
            output.Item1.ShouldContain("WARNING - No files found to update.");
            output.Item2.ShouldBe(0);
        } finally {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    public async Task NoOverride_argument_disables_pending_override() {
        b.Info.Flow();
        string workingDirectory = CreateTemporaryDirectory();
        try {
            string versionStorePath = await CreateVersionStore(workingDirectory, "1.0.0");
            string projectFilePath = CopyResourceToDirectory(TestResourcesReferences.NetStdNone, workingDirectory, "Sample.csproj");
            _ = await th.ExecuteVersonify($"override -VersionSource={versionStorePath} -QuickValue=9.9.9");
            var output = await th.ExecuteVersonify($"updatefiles -Root={workingDirectory} -I -VersionSource={versionStorePath} -MinMatch={projectFilePath}|StdFile -NoOverride");
            output.Item1.ShouldContain("Version Increment Override, Disabled");
            output.Item2.ShouldBe(0);
        } finally {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    public async Task QuickValue_long_argument_updates_behaviour() {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.OneEachBehaviourStore)!;
        string versionStorePath = uth.GetTestDataFile(resourceName);

        var output = await th.ExecuteVersonify($"behaviour -VersionSource={versionStorePath} -D=1 -QuickValue=Fixed");

        output.Item1.ShouldContain("Setting Behaviour for Digit[1] to Fixed(0)");
        output.Item2.ShouldBe(0);
    }

    [Fact]
    public async Task Release_short_argument_sets_release_name() {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!;
        string versionStorePath = uth.GetTestDataFile(resourceName);
        string releaseName = "ShortRelease";

        var output = await th.ExecuteVersonify($"set -VersionSource={versionStorePath} -R={releaseName}");
        output.Item1.ShouldContain($"Saving new Release Name as: {releaseName}");
        output.Item2.ShouldBe(0);

        output = await th.ExecuteVersonify($"passive -VersionSource={versionStorePath} -R=LookupRelease");

        output.Item1.ShouldContain($"Loaded Release Name: {releaseName}");
        output.Item2.ShouldBe(0);
    }

    [Fact]
    public async Task Single_dash_long_MinMatch_option_is_accepted() {
        b.Info.Flow();
        string workingDirectory = CreateTemporaryDirectory();
        try {
            string versionStorePath = await CreateVersionStore(workingDirectory, "1.0.0");
            string projectFilePath = CopyResourceToDirectory(TestResourcesReferences.NetStdNone, workingDirectory, "Sample.csproj");
            var output = await th.ExecuteVersonify($"updatefiles -Root={workingDirectory} -I -VersionSource={versionStorePath} -MinMatch={projectFilePath}|StdFile");
            output.Item1.ShouldContain("Version To Write:");
            output.Item2.ShouldBe(0);
        } finally {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    public async Task Single_dash_long_VersionSource_option_is_accepted() {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!;
        string versionStorePath = uth.GetTestDataFile(resourceName);
        var output = await th.ExecuteVersonify($"passive -VersionSource={versionStorePath}");
        output.Item1.ShouldContain("Loaded [");
        output.Item2.ShouldBe(0);
    }

    [Fact]
    public async Task Trace_argument_enables_trace_handler_output() {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!;
        string versionStorePath = uth.GetTestDataFile(resourceName);

        var output = await th.ExecuteVersonify($"passive -VersionSource={versionStorePath} -Trace=info", appendDebug: false);

        output.Item1.ShouldContain("Debug Mode, Adding Trace Handler");
        output.Item2.ShouldBe(0);
    }

    [Fact]
    public async Task Version_argument_prints_version_and_skips_normal_actions() {
        b.Info.Flow();
        var version = typeof(VersonifyOptions).Assembly.GetName().Version;
        string expectedVersion = $"{version?.Major}.{version?.Minor}.{version?.Build}.{version?.Revision}";

        var output = await th.ExecuteVersonify("--version");

        output.Item1.ShouldContain(expectedVersion);
        output.Item1.ShouldNotContain("Performing Versioning Actions");
        output.Item1.ShouldNotContain("Versioning By Versonify");
        output.Item2.ShouldBe(0);
    }

    [Fact]
    public async Task VersionSource_argument_loads_passive_output() {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!;
        string versionStorePath = uth.GetTestDataFile(resourceName);
        var output = await th.ExecuteVersonify($"passive -VersionSource={versionStorePath}");
        output.Item1.ShouldContain("Loaded [");
        output.Item2.ShouldBe(0);
    }

    private static string CreateTemporaryDirectory() {
        string result = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        Directory.CreateDirectory(result);
        return result;
    }

    private string CopyResourceToDirectory(TestResourcesReferences resourceReference, string workingDirectory, string destinationFileName) {
        string resourceName = TestResources.GetIdentifiers(resourceReference)!;
        string sourcePath = uth.GetTestDataFile(resourceName);
        string result = Path.Combine(workingDirectory, destinationFileName);

        File.Copy(sourcePath, result, true);

        return result;
    }

    private async Task<string> CreateVersionStore(string workingDirectory, string versionValue) {
        string result = Path.Combine(workingDirectory, "versionstore.vstore");
        var output = await th.ExecuteVersonify($"createversion -V={result} -Q={versionValue}");

        output.Item1.ShouldContain("Creating New Version Store:");
        output.Item2.ShouldBe(0);

        return result;
    }
}