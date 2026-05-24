using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;

namespace Versonify.ITest;

public class OutputModeAndValidationTests {
    protected Bilge b = new Bilge("Versonify-ITest");
    protected UnitTestHelper uth;
    protected TestHelper th;

    public OutputModeAndValidationTests() {
        b.Info.Flow();

        uth = new UnitTestHelper();
        th = new TestHelper(uth);
    }

    ~OutputModeAndValidationTests() {
        uth.ClearUpTestFiles();
    }

    private async Task<string> CreateVersionStore(string workingDirectory, string versionValue) {
        string result = Path.Combine(workingDirectory, "versionstore.vstore");
        string output = await th.ExecuteVersonify($"createversion -V={result} -Q={versionValue}");
        output.ShouldContain("Creating New Version Store:");
        th.LastExecutionExitCode.ShouldBe(0);
        return result;
    }

    private static string CreateTemporaryDirectory() {
        string result = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        Directory.CreateDirectory(result);
        return result;
    }

    // Group A — Output mode ITests


    // Note output type env has been deleted, this creates an environment variable and therefore leaves
    // state on the machine where it runs.

    [Fact]
    public async Task Output_file_mode_creates_output_file_in_working_directory() {
        b.Info.Flow();
        string tempDir = CreateTemporaryDirectory();
        string store = await CreateVersionStore(tempDir, "1.0.0");

        try {
            string output = await th.ExecuteVersonify($"passive -V={store} -Output=file", tempDir);
            File.Exists(Path.Combine(tempDir, "pver-latest.txt")).ShouldBeTrue();
            th.LastExecutionExitCode.ShouldBe(0);
        } finally {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task Output_file_with_custom_name_creates_named_file() {
        b.Info.Flow();
        string tempDir = CreateTemporaryDirectory();
        string store = await CreateVersionStore(tempDir, "1.0.0");

        // Production code validates PverFileName against Path.GetInvalidFileNameChars() (which
        // includes ':' and '\' on Windows), so only a simple filename — not a full path — is
        // accepted.  We pass a relative name and supply tempDir as the working directory so that
        // the file lands in a known, clean location.
        const string CUSTOMFILENAME = "myver.txt";
        string customFile = Path.Combine(tempDir, CUSTOMFILENAME);

        try {
            string output = await th.ExecuteVersonify($"passive -V={store} -Output=file:{CUSTOMFILENAME}", tempDir);
            File.Exists(customFile).ShouldBeTrue();
            th.LastExecutionExitCode.ShouldBe(0);
        } finally {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task Output_azdo_default_writes_default_vso_pipeline_variable() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore));
        string output = await th.ExecuteVersonify($"passive -V={store} -Output=azdo");
        output.ShouldContain("##vso[task.setvariable variable=CodeVersionNumber");
        th.LastExecutionExitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Output_azdo_custom_variable_writes_named_pipeline_variable() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore));
        string output = await th.ExecuteVersonify($"passive -V={store} -Output=azdo:MyVar");
        output.ShouldContain("##vso[task.setvariable variable=MyVar");
        th.LastExecutionExitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Output_vsts_alias_writes_default_vso_pipeline_variable() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore));
        string output = await th.ExecuteVersonify($"passive -V={store} -Output=vsts");
        output.ShouldContain("##vso[task.setvariable variable=CodeVersionNumber");
        th.LastExecutionExitCode.ShouldBe(0);
    }

    // Group B — Debug flag

    [Fact]
    public async Task Debug_flag_echoes_command_line_arguments_to_stdout() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore));
        string output = await th.ExecuteVersonify($"passive -V={store} -Debug");
        output.ShouldContain("Command Line:");
        th.LastExecutionExitCode.ShouldBe(0);
    }

    // Group C — Semicolon array

    [Fact]
    public async Task Behaviour_semicolon_separated_digits_processes_multiple_digits() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.OneEachBehaviourStore));
        string output = await th.ExecuteVersonify($"behaviour -V={store} -D=0;1");
        output.ShouldContain("[0]:");
        output.ShouldContain("[1]:");
        th.LastExecutionExitCode.ShouldBe(0);
    }

    // Group D — Prefix command

    [Fact]
    public async Task Prefix_command_stores_and_applies_prefix_to_version_output() {
        b.Info.Flow();
        string tempDir = CreateTemporaryDirectory();
        string store = await CreateVersionStore(tempDir, "1.0.0.0");

        try {
            _ = await th.ExecuteVersonify($"prefix -V={store} -D=2 -Q=-");
            th.LastExecutionExitCode.ShouldBe(0);
            string output = await th.ExecuteVersonify($"passive -V={store}");
            output.ShouldContain("-");
            th.LastExecutionExitCode.ShouldBe(0);
        } finally {
            Directory.Delete(tempDir, true);
        }
    }

    // Group E — Validation errors

    [Fact]
    public async Task Behaviour_command_missing_digit_argument_returns_error() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore));
        string output = await th.ExecuteVersonify($"behaviour -V={store}");
        output.ShouldContain("Error >>");
        th.LastExecutionExitCode.ShouldNotBe(0);
    }

    [Fact]
    public async Task Override_command_missing_value_argument_returns_error() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore));
        string output = await th.ExecuteVersonify($"override -V={store}");
        output.ShouldContain("Error >>");
        th.LastExecutionExitCode.ShouldNotBe(0);
    }

    [Fact]
    public async Task Set_command_missing_value_argument_returns_error() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore));
        string output = await th.ExecuteVersonify($"set -V={store}");
        output.ShouldContain("Error >>");
        th.LastExecutionExitCode.ShouldNotBe(0);
    }

    [Fact]
    public async Task Prefix_command_missing_value_argument_returns_error() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore));
        string output = await th.ExecuteVersonify($"prefix -V={store} -D=0");
        output.ShouldContain("Error >>");
        th.LastExecutionExitCode.ShouldNotBe(0);
    }

    [Fact]
    public async Task Prefix_command_missing_digit_argument_returns_error() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore));
        string output = await th.ExecuteVersonify($"prefix -V={store} -Q=-");
        output.ShouldContain("Error >>");
        th.LastExecutionExitCode.ShouldNotBe(0);
    }

    [Fact]
    public async Task Set_command_with_conflicting_Q_and_R_arguments_returns_error() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore));
        string output = await th.ExecuteVersonify($"set -V={store} -Q=9 -R=MyRelease");
        output.ShouldContain("Error >>");
        th.LastExecutionExitCode.ShouldNotBe(0);
    }

    [Fact]
    public async Task Command_with_invalid_root_directory_returns_error() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore));
        string nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string output = await th.ExecuteVersonify($"passive -V={store} -Root={nonExistentPath}");
        output.ShouldContain("Error >> Invalid Directory");
        th.LastExecutionExitCode.ShouldNotBe(0);
    }
}
