using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;

namespace Versonify.ITest;

public class OutputModeAndValidationTests {
    protected Bilge b = new("Versonify-ITest");
    protected TestHelper th;
    protected UnitTestHelper uth;

    public OutputModeAndValidationTests() {
        b.Info.Flow();

        uth = new UnitTestHelper();
        th = new TestHelper(uth);
    }

    ~OutputModeAndValidationTests() {
        uth.ClearUpTestFiles();
    }

    [Fact]
    public async Task Behaviour_command_missing_digit_argument_returns_error() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!);
        var output = await th.ExecuteVersonify($"behaviour -V={store}");
        output.Item1.ShouldContain("Error >>");
        output.Item2.ShouldNotBe(0);
    }

    [Fact]
    public async Task Behaviour_semicolon_separated_digits_processes_multiple_digits() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.OneEachBehaviourStore)!);
        var output = await th.ExecuteVersonify($"behaviour -V={store} -D=0;1");
        output.Item1.ShouldContain("[0]:");
        output.Item1.ShouldContain("[1]:");
        output.Item2.ShouldBe(0);
    }

    [Fact]
    public async Task Command_with_invalid_root_directory_returns_error() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!);
        string nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var output = await th.ExecuteVersonify($"passive -V={store} -Root={nonExistentPath}");
        output.Item1.ShouldContain("Error >> Invalid Directory");
        output.Item2.ShouldNotBe(0);
    }

    [Fact]
    public async Task Debug_flag_echoes_command_line_arguments_to_stdout() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!);
        var output = await th.ExecuteVersonify($"passive -v={store} --debug", appendDebug: false);
        output.Item1.ShouldContain("Command Line Arguments:");
        output.Item2.ShouldBe(0);
    }

    [Fact]
    public async Task Output_azdo_custom_variable_writes_named_pipeline_variable() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!);
        var output = await th.ExecuteVersonify($"passive -V={store} -Output=azdo:MyVar");
        output.Item1.ShouldContain("##vso[task.setvariable variable=MyVar");
        output.Item2.ShouldBe(0);
    }

    [Fact]
    public async Task Output_azdo_default_writes_default_vso_pipeline_variable() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!);
        var output = await th.ExecuteVersonify($"passive -V={store} -Output=azdo");
        output.Item1.ShouldContain("##vso[task.setvariable variable=CodeVersionNumber");
        output.Item2.ShouldBe(0);
    }

    [Fact]
    public async Task Output_file_mode_creates_output_file_in_working_directory() {
        b.Info.Flow();
        string tempDir = CreateTemporaryDirectory();
        string store = await CreateVersionStore(tempDir, "1.0.0");

        try {
            var output = await th.ExecuteVersonify($"passive -V={store} -Output=file", tempDir);
            File.Exists(Path.Combine(tempDir, "pver-latest.txt")).ShouldBeTrue();
            output.Item2.ShouldBe(0);
        } finally {
            Directory.Delete(tempDir, true);
        }
    }

    // Note output type env has been deleted, this creates an environment variable and therefore leaves
    // state on the machine where it runs.
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
            var output = await th.ExecuteVersonify($"passive -V={store} -Output=file:{CUSTOMFILENAME}", tempDir);
            File.Exists(customFile).ShouldBeTrue();
            output.Item2.ShouldBe(0);
        } finally {
            Directory.Delete(tempDir, true);
        }
    }

    // Group A — Output mode ITests
    [Fact]
    public async Task Output_vsts_alias_writes_default_vso_pipeline_variable() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!);
        var output = await th.ExecuteVersonify($"passive -V={store} -Output=vsts");
        output.Item1.ShouldContain("##vso[task.setvariable variable=CodeVersionNumber");
        output.Item2.ShouldBe(0);
    }

    // Group E — Validation errors
    [Fact]
    public async Task Override_command_missing_value_argument_returns_error() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!);
        var output = await th.ExecuteVersonify($"override -V={store}");
        output.Item1.ShouldContain("Error >>");
        output.Item2.ShouldNotBe(0);
    }

    [Fact]
    public async Task Prefix_command_missing_digit_argument_returns_error() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!);
        var output = await th.ExecuteVersonify($"prefix -V={store} -Q=-");
        output.Item1.ShouldContain("Error >>");
        output.Item2.ShouldNotBe(0);
    }

    [Fact]
    public async Task Prefix_command_missing_value_argument_returns_error() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!);
        var output = await th.ExecuteVersonify($"prefix -V={store} -D=0");
        output.Item1.ShouldContain("Error >>");
        output.Item2.ShouldNotBe(0);
    }

    [Fact]
    public async Task Prefix_command_stores_and_applies_prefix_to_version_output() {
        b.Info.Flow();
        string tempDir = CreateTemporaryDirectory();
        string store = await CreateVersionStore(tempDir, "1.0.0.0");

        try {
            var output = await th.ExecuteVersonify($"prefix -V={store} -D=2 -Q=-");
            output.Item2.ShouldBe(0);
            output = await th.ExecuteVersonify($"passive -V={store}");
            output.Item1.ShouldContain("-");
            output.Item2.ShouldBe(0);
        } finally {
            Directory.Delete(tempDir, true);
        }
    }

    // Group B — Debug flag
    // Group C — Semicolon array
    // Group D — Prefix command
    [Fact]
    public async Task Set_command_missing_value_argument_returns_error() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!);
        var output = await th.ExecuteVersonify($"set -V={store}");
        output.Item1.ShouldContain("Error >>");
        output.Item2.ShouldNotBe(0);
    }

    [Fact]
    public async Task Set_command_with_conflicting_Q_and_R_arguments_returns_error() {
        b.Info.Flow();
        string store = uth.GetTestDataFile(TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore)!);
        var output = await th.ExecuteVersonify($"set -V={store} -Q=9 -R=MyRelease");
        output.Item1.ShouldContain("Error >>");
        output.Item2.ShouldNotBe(0);
    }

    private static string CreateTemporaryDirectory() {
        string result = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        Directory.CreateDirectory(result);
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