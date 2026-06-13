using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;

namespace Versonify.ITest;

public class CommandLineArgumentCoverageTests : IDisposable {
    protected Bilge b = new Bilge("Versonify-ITest");
    protected UnitTestHelper uth;
    protected TestHelper th;

    public CommandLineArgumentCoverageTests() {
        b.Info.Flow();

        uth = new UnitTestHelper();
        th = new TestHelper(uth);
    }

    public void Dispose() {
        uth.ClearUpTestFiles();
    }

    [Fact]
    public async Task Digits_argument_loads_behaviour_output() {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.OneEachBehaviourStore)!;
        string versionStorePath = uth.GetTestDataFile(resourceName);
        string output = await th.ExecuteVersonify($"behaviour -VersionSource={versionStorePath} -Digits=*");
        output.ShouldContain("[0]:Fixed(0)");
        output.ShouldContain("[7]:ReleaseName(8)");
        th.LastExecutionExitCode.ShouldBe(0);
    }

    [Fact]
    public async Task VersionSource_argument_loads_passive_output() {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!;
        string versionStorePath = uth.GetTestDataFile(resourceName);
        string output = await th.ExecuteVersonify($"passive -VersionSource={versionStorePath}");
        output.ShouldContain("Loaded [");
        th.LastExecutionExitCode.ShouldBe(0);
    }

    [Fact]
    public async Task MinMatch_argument_updates_files() {
        b.Info.Flow();
        string workingDirectory = CreateTemporaryDirectory();
        try {
            string versionStorePath = await CreateVersionStore(workingDirectory, "1.0.0");
            string projectFilePath = CopyResourceToDirectory(TestResourcesReferences.NetStdNone, workingDirectory, "Sample.csproj");
            string output = await th.ExecuteVersonify($"updatefiles -Root={workingDirectory} -I -VersionSource={versionStorePath} -MinMatch={projectFilePath}|StdFile");
            output.ShouldContain("Version Increment Requested - Currently");
            output.ShouldContain("Version To Write:");
            th.LastExecutionExitCode.ShouldBe(0);
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
            string output = await th.ExecuteVersonify($"updatefiles -Root={workingDirectory} -I -VersionSource={versionStorePath} -MinMatch={projectFilePath}|StdFile -NoOverride");
            output.ShouldContain("Version Increment Override, Disabled");
            th.LastExecutionExitCode.ShouldBe(0);
        } finally {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    public async Task Deprecated_DG_alias_is_not_accepted() {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.OneEachBehaviourStore)!;
        string versionStorePath = uth.GetTestDataFile(resourceName);
        _ = await th.ExecuteVersonify($"behaviour -VersionSource={versionStorePath} -DG=*");
        th.LastExecutionExitCode.ShouldNotBe(0, "Deprecated alias -DG must not be accepted.");
    }

    [Fact]
    public async Task Deprecated_VS_alias_is_not_accepted() {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!;
        string versionStorePath = uth.GetTestDataFile(resourceName);
        _ = await th.ExecuteVersonify($"passive -VS={versionStorePath}");
        th.LastExecutionExitCode.ShouldNotBe(0, "Deprecated alias -VS must not be accepted.");
    }

    [Fact]
    public async Task Deprecated_MM_alias_is_not_accepted() {
        b.Info.Flow();
        string workingDirectory = CreateTemporaryDirectory();
        try {
            string versionStorePath = await CreateVersionStore(workingDirectory, "1.0.0");
            string projectFilePath = CopyResourceToDirectory(TestResourcesReferences.NetStdNone, workingDirectory, "Sample.csproj");
            _ = await th.ExecuteVersonify($"updatefiles -Root={workingDirectory} -I -VersionSource={versionStorePath} -MM={projectFilePath}|StdFile");
            th.LastExecutionExitCode.ShouldNotBe(0, "Deprecated alias -MM must not be accepted.");
        } finally {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    public async Task Deprecated_NO_alias_is_not_accepted() {
        b.Info.Flow();
        string workingDirectory = CreateTemporaryDirectory();
        try {
            string versionStorePath = await CreateVersionStore(workingDirectory, "1.0.0");
            string projectFilePath = CopyResourceToDirectory(TestResourcesReferences.NetStdNone, workingDirectory, "Sample.csproj");
            _ = await th.ExecuteVersonify($"override -VersionSource={versionStorePath} -QuickValue=9.9.9");
            _ = await th.ExecuteVersonify($"updatefiles -Root={workingDirectory} -I -VersionSource={versionStorePath} -MinMatch={projectFilePath}|StdFile -NO");
            th.LastExecutionExitCode.ShouldNotBe(0, "Deprecated alias -NO must not be accepted.");
        } finally {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    public async Task QQpnf_compatibility_probe_returns_200() {
        b.Info.Flow();
        _ = await th.ExecuteVersonify("--QQpnf");
        th.LastExecutionExitCode.ShouldBe(200);
    }

    [Fact]
    public async Task No_arguments_prints_help_and_exits_nonzero() {
        b.Info.Flow();
        string output = await th.ExecuteVersonify("");
        output.ShouldContain("Parameter help for Versonify.");
        th.LastExecutionExitCode.ShouldNotBe(0);
    }

    [Fact]
    public async Task Invalid_argument_prints_error_and_exits_nonzero() {
        b.Info.Flow();
        string output = await th.ExecuteVersonify("--totally-unknown-option");
        output.ShouldContain("Fatal:");
        th.LastExecutionExitCode.ShouldNotBe(0);
    }

    [Fact]
    public async Task Single_dash_long_VersionSource_option_is_accepted() {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!;
        string versionStorePath = uth.GetTestDataFile(resourceName);
        string output = await th.ExecuteVersonify($"passive -VersionSource={versionStorePath}");
        output.ShouldContain("Loaded [");
        th.LastExecutionExitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Single_dash_long_MinMatch_option_is_accepted() {
        b.Info.Flow();
        string workingDirectory = CreateTemporaryDirectory();
        try {
            string versionStorePath = await CreateVersionStore(workingDirectory, "1.0.0");
            string projectFilePath = CopyResourceToDirectory(TestResourcesReferences.NetStdNone, workingDirectory, "Sample.csproj");
            string output = await th.ExecuteVersonify($"updatefiles -Root={workingDirectory} -I -VersionSource={versionStorePath} -MinMatch={projectFilePath}|StdFile");
            output.ShouldContain("Version To Write:");
            th.LastExecutionExitCode.ShouldBe(0);
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
            string output = await th.ExecuteVersonify($"updatefiles -Root={workingDirectory} -Increment -VersionSource={versionStorePath} -MinMatch=*.zzz -Output=con -z");
            output.ShouldContain("WARNING - No files found to update.");
            th.LastExecutionExitCode.ShouldBe(0);
        } finally {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    public async Task DryRun_argument_does_not_persist_set_command_changes() {
        b.Info.Flow();
        string workingDirectory = CreateTemporaryDirectory();

        try {
            string versionStorePath = await CreateVersionStore(workingDirectory, "1.0.0");
            string before = File.ReadAllText(versionStorePath);

            string output = await th.ExecuteVersonify($"set -VersionSource={versionStorePath} -D=0 -QuickValue=9 -DryRun");
            string after = File.ReadAllText(versionStorePath);

            output.ShouldContain("DryRun - Would Save:");
            after.ShouldBe(before);
            th.LastExecutionExitCode.ShouldBe(0);
        } finally {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    public async Task Trace_argument_enables_trace_handler_output() {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!;
        string versionStorePath = uth.GetTestDataFile(resourceName);

        string output = await th.ExecuteVersonify($"passive -VersionSource={versionStorePath} -Trace=info");

        output.ShouldContain("Debug Mode, Adding Trace Handler");
        th.LastExecutionExitCode.ShouldBe(0);
    }

    [Fact]
    public async Task NoError_long_argument_forces_zero_exit_code() {
        b.Info.Flow();
        string workingDirectory = CreateTemporaryDirectory();

        try {
            string versionStorePath = await CreateVersionStore(workingDirectory, "2.0.0");

            string output = await th.ExecuteVersonify($"updatefiles -Root={workingDirectory} -Increment -VersionSource={versionStorePath} -MinMatch=*.zzz -Output=con -NoError");

            output.ShouldContain("WARNING - No files found to update.");
            th.LastExecutionExitCode.ShouldBe(0);
        } finally {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    public async Task QuickValue_long_argument_updates_behaviour() {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.OneEachBehaviourStore)!;
        string versionStorePath = uth.GetTestDataFile(resourceName);

        string output = await th.ExecuteVersonify($"behaviour -VersionSource={versionStorePath} -D=1 -QuickValue=Fixed");

        output.ShouldContain("Setting Behaviour for Digit[1] to Fixed(0)");
        th.LastExecutionExitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Release_short_argument_sets_release_name() {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!;
        string versionStorePath = uth.GetTestDataFile(resourceName);
        string releaseName = "ShortRelease";

        string output = await th.ExecuteVersonify($"set -VersionSource={versionStorePath} -R={releaseName}");
        output.ShouldContain($"Saving new Release Name as: {releaseName}");
        th.LastExecutionExitCode.ShouldBe(0);

        output = await th.ExecuteVersonify($"passive -VersionSource={versionStorePath} -R=LookupRelease");

        output.ShouldContain($"Loaded Release Name: {releaseName}");
        th.LastExecutionExitCode.ShouldBe(0);
    }

    private async Task<string> CreateVersionStore(string workingDirectory, string versionValue) {
        string result = Path.Combine(workingDirectory, "versionstore.vstore");
        string output = await th.ExecuteVersonify($"createversion -V={result} -Q={versionValue}");

        output.ShouldContain("Creating New Version Store:");
        th.LastExecutionExitCode.ShouldBe(0);

        return result;
    }

    private string CopyResourceToDirectory(TestResourcesReferences resourceReference, string workingDirectory, string destinationFileName) {
        string resourceName = TestResources.GetIdentifiers(resourceReference)!;
        string sourcePath = uth.GetTestDataFile(resourceName);
        string result = Path.Combine(workingDirectory, destinationFileName);

        File.Copy(sourcePath, result, true);

        return result;
    }

    private static string CreateTemporaryDirectory() {
        string result = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        Directory.CreateDirectory(result);
        return result;
    }
}
