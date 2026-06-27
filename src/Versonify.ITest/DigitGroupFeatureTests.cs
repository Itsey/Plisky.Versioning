using System.Text.Json;
using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;

namespace Versonify.ITest;

public class DigitGroupFeatureTests : IDisposable {
    protected Bilge b = new Bilge("Versonify-ITest");
    protected UnitTestHelper uth;
    protected TestHelper sut;
    private readonly List<string> tempDirectories = new();

    public DigitGroupFeatureTests() {
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

    [Fact]
    public async Task Set_with_digit_group_assigns_group_and_passive_defaults_to_default_group() {
        string tempDir = CreateTemporaryDirectory();
        string store = await CreateVersionStore(tempDir, "1.2.3.4");

        string setOutput = await sut.ExecuteVersonify($"set -V={store} -D=2 -g=prerelease");
        setOutput.ShouldContain("Saving Updated Digit Values");
        sut.LastExecutionExitCode.ShouldBe(0);

        string? groupName = ReadDigitGroupName(store, 2);
        groupName.ShouldBe("prerelease");

        string passiveOutput = await sut.ExecuteVersonify($"passive -V={store}");
        passiveOutput.ShouldContain("Loaded [1.2.4]");
        sut.LastExecutionExitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Passive_supports_named_and_multiple_digit_group_filters() {
        string tempDir = CreateTemporaryDirectory();
        string store = await CreateVersionStore(tempDir, "1.2.3.4");
        _ = await sut.ExecuteVersonify($"set -V={store} -D=2 -g=prerelease");
        sut.LastExecutionExitCode.ShouldBe(0);

        string prereleaseOnlyOutput = await sut.ExecuteVersonify($"passive -V={store} -g=prerelease");
        prereleaseOnlyOutput.ShouldContain("Loaded [.3]");
        sut.LastExecutionExitCode.ShouldBe(0);

        string combinedGroupsOutput = await sut.ExecuteVersonify($"passive -V={store} --digit-group=default,prerelease");
        combinedGroupsOutput.ShouldContain("Loaded [1.2.3.4]");
        sut.LastExecutionExitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Increment_respects_default_named_and_wildcard_digit_groups() {
        string tempDir = CreateTemporaryDirectory();
        string store = await CreateVersionStore(tempDir, "1.2.3.4");
        _ = await sut.ExecuteVersonify($"behaviour -V={store} -D=* -Q=ContinualIncrement");
        sut.LastExecutionExitCode.ShouldBe(0);
        _ = await sut.ExecuteVersonify($"set -V={store} -D=2 -g=prerelease");
        sut.LastExecutionExitCode.ShouldBe(0);

        _ = await sut.ExecuteVersonify($"passive -V={store} -I");
        sut.LastExecutionExitCode.ShouldBe(0);
        string afterDefaultIncrementOutput = await sut.ExecuteVersonify($"passive -V={store} --digit-group=default,prerelease");
        afterDefaultIncrementOutput.ShouldContain("Loaded [2.3.3.5]");

        _ = await sut.ExecuteVersonify($"passive -V={store} -I -g=prerelease");
        sut.LastExecutionExitCode.ShouldBe(0);
        string afterNamedIncrementOutput = await sut.ExecuteVersonify($"passive -V={store} --digit-group=default,prerelease");
        afterNamedIncrementOutput.ShouldContain("Loaded [2.3.4.5]");

        _ = await sut.ExecuteVersonify($"passive -V={store} -I -g=*");
        sut.LastExecutionExitCode.ShouldBe(0);
        string afterWildcardIncrementOutput = await sut.ExecuteVersonify($"passive -V={store} --digit-group=default,prerelease");
        afterWildcardIncrementOutput.ShouldContain("Loaded [3.4.5.6]");
    }

    [Fact]
    public async Task Set_rejects_comma_and_wildcard_digit_group_values() {
        string tempDir = CreateTemporaryDirectory();
        string store = await CreateVersionStore(tempDir, "1.2.3.4");

        string commaOutput = await sut.ExecuteVersonify($"set -V={store} -D=2 -g=a,b");
        commaOutput.ShouldContain("Error >> The --digit-group option for set command cannot contain commas.");
        sut.LastExecutionExitCode.ShouldNotBe(0);

        string wildcardOutput = await sut.ExecuteVersonify($"set -V={store} -D=2 -g=*");
        wildcardOutput.ShouldContain("Error >> The --digit-group option for set command cannot be '*'.");
        sut.LastExecutionExitCode.ShouldNotBe(0);
    }

    [Fact]
    public async Task Pre_release_flag_applies_expected_group_shortcuts() {
        string tempDir = CreateTemporaryDirectory();
        string store = await CreateVersionStore(tempDir, "1.2.3.4");
        _ = await sut.ExecuteVersonify($"behaviour -V={store} -D=* -Q=ContinualIncrement");
        sut.LastExecutionExitCode.ShouldBe(0);

        _ = await sut.ExecuteVersonify($"set -V={store} -D=2 --pre-release");
        sut.LastExecutionExitCode.ShouldBe(0);
        ReadDigitGroupName(store, 2).ShouldBe("pre-release");

        string passiveOutput = await sut.ExecuteVersonify($"passive -V={store} --pre-release");
        passiveOutput.ShouldContain("Loaded [1.2.3.4]");
        sut.LastExecutionExitCode.ShouldBe(0);

        _ = await sut.ExecuteVersonify($"passive -V={store} -I --pre-release");
        sut.LastExecutionExitCode.ShouldBe(0);
        string incrementOutput = await sut.ExecuteVersonify($"passive -V={store} --digit-group=default,pre-release");
        incrementOutput.ShouldContain("Loaded [1.2.4.4]");
    }

    [Fact]
    public async Task Digit_group_and_pre_release_together_returns_validation_error() {
        string tempDir = CreateTemporaryDirectory();
        string store = await CreateVersionStore(tempDir, "1.2.3.4");

        string output = await sut.ExecuteVersonify($"passive -V={store} -g=prerelease --pre-release");
        output.ShouldContain("Error >> Both --digit-group and --pre-release cannot be specified together.");
        sut.LastExecutionExitCode.ShouldNotBe(0);
    }

    [Fact]
    public async Task Set_with_group_assignment_and_full_quick_value_requires_digits() {
        string tempDir = CreateTemporaryDirectory();
        string store = await CreateVersionStore(tempDir, "1.2.3.4");

        string digitGroupOutput = await sut.ExecuteVersonify($"set -V={store} -Q=9.8.7.6 -g=prerelease");
        digitGroupOutput.ShouldContain("Error >> The Set command requires at least one digit to update. Use -D=<digit> or -D=*.");
        sut.LastExecutionExitCode.ShouldNotBe(0);

        string preReleaseOutput = await sut.ExecuteVersonify($"set -V={store} -Q=9.8.7.6 --pre-release");
        preReleaseOutput.ShouldContain("Error >> The Set command requires at least one digit to update. Use -D=<digit> or -D=*.");
        sut.LastExecutionExitCode.ShouldNotBe(0);
    }

    [Fact]
    public async Task Set_digit_group_default_resets_to_unnamed_group() {
        string tempDir = CreateTemporaryDirectory();
        string store = await CreateVersionStore(tempDir, "1.2.3.4");

        _ = await sut.ExecuteVersonify($"set -V={store} -D=2 -g=prerelease");
        sut.LastExecutionExitCode.ShouldBe(0);
        ReadDigitGroupName(store, 2).ShouldBe("prerelease");

        _ = await sut.ExecuteVersonify($"set -V={store} -D=2 -g=default");
        sut.LastExecutionExitCode.ShouldBe(0);
        ReadDigitGroupName(store, 2).ShouldBe(string.Empty);

        string output = await sut.ExecuteVersonify($"passive -V={store}");
        output.ShouldContain("Loaded [1.2.3.4]");
    }

    [Fact]
    public async Task Set_digit_group_empty_string_resets_to_unnamed_group() {
        string tempDir = CreateTemporaryDirectory();
        string store = await CreateVersionStore(tempDir, "1.2.3.4");

        _ = await sut.ExecuteVersonify($"set -V={store} -D=2 -g=prerelease");
        sut.LastExecutionExitCode.ShouldBe(0);
        ReadDigitGroupName(store, 2).ShouldBe("prerelease");

        _ = await sut.ExecuteVersonify($"set -V={store} -D=2 --digit-group=");
        sut.LastExecutionExitCode.ShouldBe(0);
        ReadDigitGroupName(store, 2).ShouldBe(string.Empty);
    }

    [Fact]
    public async Task Passive_group_filter_applies_to_console_and_file_output_modes() {
        string tempDir = CreateTemporaryDirectory();
        string store = await CreateVersionStore(tempDir, "1.2.3.4");
        _ = await sut.ExecuteVersonify($"set -V={store} -D=2 -g=prerelease");
        sut.LastExecutionExitCode.ShouldBe(0);

        string consoleOutput = await sut.ExecuteVersonify($"passive -V={store} -g=prerelease -O=con");
        consoleOutput.ShouldContain(".3");
        sut.LastExecutionExitCode.ShouldBe(0);

        string outputFileName = "grouped.txt";
        string outputFilePath = Path.Combine(tempDir, outputFileName);
        _ = await sut.ExecuteVersonify($"passive -V={store} -g=prerelease -O=file:{outputFileName}", tempDir);
        sut.LastExecutionExitCode.ShouldBe(0);
        File.ReadAllText(outputFilePath).ShouldBe(".3");
    }

    private async Task<string> CreateVersionStore(string workingDirectory, string versionValue) {
        string result = Path.Combine(workingDirectory, "versionstore.vstore");
        string output = await sut.ExecuteVersonify($"createversion -V={result} -Q={versionValue}");

        output.ShouldContain("Creating New Version Store:");
        sut.LastExecutionExitCode.ShouldBe(0);

        return result;
    }

    private static string? ReadDigitGroupName(string versionStorePath, int digitIndex) {
        using var doc = JsonDocument.Parse(File.ReadAllText(versionStorePath));
        var digit = doc.RootElement.GetProperty("Digits")[digitIndex];
        if (digit.TryGetProperty("GroupName", out var groupNameProperty)) {
            return groupNameProperty.GetString();
        }

        return null;
    }

    private string CreateTemporaryDirectory() {
        string result = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        Directory.CreateDirectory(result);
        tempDirectories.Add(result);
        return result;
    }
}
