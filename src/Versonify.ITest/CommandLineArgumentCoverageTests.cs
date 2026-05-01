using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;

namespace Versonify.ITest;

public class CommandLineArgumentCoverageTests {
    protected Bilge b = new Bilge("Versonify-ITest");
    protected UnitTestHelper uth;
    protected TestHelper th;

    public CommandLineArgumentCoverageTests() {
        b.Info.Flow();

        uth = new UnitTestHelper();
        th = new TestHelper(uth);
    }

    ~CommandLineArgumentCoverageTests() {
        uth.ClearUpTestFiles();
    }

    [Theory]
    [InlineData("-Digits", false)]
    [InlineData("-DG", true)]
    public async Task Digits_argument_aliases_load_behaviour_output(string digitsArgument, bool shouldWarn) {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.OneEachBehaviourStore);
        string versionStorePath = uth.GetTestDataFile(resourceName);

        string output = await th.ExecuteVersonify($"behaviour -VersionSource={versionStorePath} {digitsArgument}=*");

        output.ShouldContain("[0]:Fixed(0)");
        output.ShouldContain("[7]:ReleaseName(8)");
        if (shouldWarn) {
            output.ShouldContain($"Warning >> Argument '{digitsArgument}' is now deprecated.");
        }
        th.LastExecutionExitCode.ShouldBe(0);
    }

    [Theory]
    [InlineData("-VersionSource", false)]
    [InlineData("-VS", true)]
    public async Task VersionSource_argument_aliases_load_passive_output(string versionSourceArgument, bool shouldWarn) {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore);
        string versionStorePath = uth.GetTestDataFile(resourceName);

        string output = await th.ExecuteVersonify($"passive {versionSourceArgument}={versionStorePath}");

        output.ShouldContain("Loaded [");
        if (shouldWarn) {
            output.ShouldContain($"Warning >> Argument '{versionSourceArgument}' is now deprecated.");
        }
        th.LastExecutionExitCode.ShouldBe(0);
    }

    [Theory]
    [InlineData("-MinMatch", false)]
    [InlineData("-MM", true)]
    public async Task MinMatch_argument_aliases_update_files(string minMatchArgument, bool shouldWarn) {
        b.Info.Flow();
        string workingDirectory = CreateTemporaryDirectory();

        try {
            string versionStorePath = await CreateVersionStore(workingDirectory, "1.0.0");
            string projectFilePath = CopyResourceToDirectory(TestResourcesReferences.NetStdNone, workingDirectory, "Sample.csproj");

            string output = await th.ExecuteVersonify($"updatefiles -Root={workingDirectory} -I -VersionSource={versionStorePath} {minMatchArgument}={projectFilePath}|StdFile");

            output.ShouldContain("Version Increment Requested - Currently");
            output.ShouldContain("Version To Write:");
            if (shouldWarn) {
                output.ShouldContain($"Warning >> Argument '{minMatchArgument}' is now deprecated.");
            }
            th.LastExecutionExitCode.ShouldBe(0);
        } finally {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Theory]
    [InlineData("-NoOverride", false)]
    [InlineData("-NO", true)]
    public async Task NoOverride_argument_aliases_disable_pending_override(string noOverrideArgument, bool shouldWarn) {
        b.Info.Flow();
        string workingDirectory = CreateTemporaryDirectory();

        try {
            string versionStorePath = await CreateVersionStore(workingDirectory, "1.0.0");
            string projectFilePath = CopyResourceToDirectory(TestResourcesReferences.NetStdNone, workingDirectory, "Sample.csproj");

            _ = await th.ExecuteVersonify($"override -VersionSource={versionStorePath} -QuickValue=9.9.9");
            string output = await th.ExecuteVersonify($"updatefiles -Root={workingDirectory} -I -VersionSource={versionStorePath} -MinMatch={projectFilePath}|StdFile {noOverrideArgument}");

            output.ShouldContain("Version Increment Override, Disabled");
            if (shouldWarn) {
                output.ShouldContain($"Warning >> Argument '{noOverrideArgument}' is now deprecated.");
            }
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
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore);
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
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.OneEachBehaviourStore);
        string versionStorePath = uth.GetTestDataFile(resourceName);

        string output = await th.ExecuteVersonify($"behaviour -VersionSource={versionStorePath} -D=1 -QuickValue=Fixed");

        output.ShouldContain("Setting Behaviour for Digit[1] to Fixed(0)");
        th.LastExecutionExitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Release_short_argument_sets_release_name() {
        b.Info.Flow();
        string resourceName = TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore);
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
        string resourceName = TestResources.GetIdentifiers(resourceReference);
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
