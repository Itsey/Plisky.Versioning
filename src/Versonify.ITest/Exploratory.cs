using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;

namespace Versonify.ITest;

public class Exploratory {
    protected Bilge b = new Bilge("Versonify-ITest");
    protected UnitTestHelper uth;
    protected TestHelper th;

    public Exploratory() {
        b.Info.Flow();

        uth = new UnitTestHelper();
        th = new TestHelper(uth);
    }

    [Fact]
    public async Task No_arguments_presents_default_help() {
        b.Info.Flow();
        string output = await th.ExecuteVersonify("");

        output.ShouldContain("Parameter help for Versonify.");
        th.LastExecutionExitCode.ShouldNotBe(0, "No Parameters is an error condition.");
    }

    [Fact(Skip = "This looks like it could be a bug in current implementation while evidencing LFY-10")]
    public async Task Pre_and_release_versioning_use_case_works() {
        // Usecase where pre-release is incremented, then a release version takes over, then pre-release is incremented again.
        b.Info.Flow();
        string preReleaseVersionStore = uth.NewTemporaryFileName(true);
        string releaseVersionStore = uth.NewTemporaryFileName(true);
        string tDirectory = Path.Combine(Path.GetTempPath(), "tmp-ver-empty");

        Assert.False(Directory.Exists(tDirectory));
        Directory.CreateDirectory(tDirectory);

        try {
            string output;

            output = await th.ExecuteVersonify($"-Command=CreateVersion -VS={releaseVersionStore} -Q=\"2.0.0\" -Release=Austen");
            output = await th.ExecuteVersonify($"-Command=Behaviour -VS={releaseVersionStore} -dg=2 -Q=AutoIncrementWithResetAny");

            output = await th.ExecuteVersonify($"-Command=CreateVersion -VS={preReleaseVersionStore} -Q=\"1.0.0.0.0.0\" -Release=Austen");
            output = await th.ExecuteVersonify($"-Command=Prefix -VS={preReleaseVersionStore} -dg=3 -Q=-");
            output = await th.ExecuteVersonify($"-Command=Behaviour -VS={preReleaseVersionStore} -dg=5 -Q=AutoIncrementWithResetAny");
            output = await th.ExecuteVersonify($"-Command=Set -VS={preReleaseVersionStore} -dg=3 -Q=Austen");

            FileShouldContain(preReleaseVersionStore, "\"-\"", "\"Austen\"");

            output = await th.ExecuteVersonify($"-Command=Passive -VS={preReleaseVersionStore}");
            output.ShouldContain("1.0.0-Austen.0.0");

            // Increment PreRelease versions 1.0.0-Austen.1.0 > 1.0.0-Austen.1.1 > 1.0.0-Austen.1.2 etc.

            for (int i = 1; i <= 5; i++) {
                output = await th.ExecuteVersonify($"-Command=UpdateFiles -Root=D:\\Scratch\\xx -Increment -VS={preReleaseVersionStore}");
                output.ShouldContain($"1.0.0-Austen.0.{i}");
            }

            // Now create a release version.
            output = await th.ExecuteVersonify($"-Command=UpdateFiles -Root={tDirectory} -Increment -VS={releaseVersionStore} -output=con-nf");
            output.ShouldContain("2.0.1");
            output = await th.ExecuteVersonify($"-Command=Override -Root={tDirectory} -VS={preReleaseVersionStore} -Q=1.0.0 -output=con-nf");

            // Apply overriden pre-release version.
            output = await th.ExecuteVersonify($"-Command=UpdateFiles -Root={tDirectory} -Increment -VS={preReleaseVersionStore}");
            output = await th.ExecuteVersonify($"-Command=Passive -VS={preReleaseVersionStore}");
            output.ShouldContain("2.0.1-Austen.0.0");

        } finally {
            uth.ClearUpTestFiles();
        }
    }

    private static void FileShouldContain(string preReleaseVersionStore, params string[] contains) {
        string currentFileContents = File.ReadAllText(preReleaseVersionStore);
        foreach (string c in contains) {
            currentFileContents.ShouldContain(c);
        }
    }

    [Fact]
    public async Task Behaviour_command_shows_all_digits() {
        b.Info.Flow();
        string resName = TestResources.GetIdentifiers(TestResourcesReferences.OneEachBehaviourStore);
        string vStoreFilePath = uth.GetTestDataFile(resName);

        string output = await th.ExecuteVersonify($"behaviour -vs={vStoreFilePath} -DG=*");

        output.ShouldContain("[0]:Fixed(0)");
        output.ShouldContain("[1]:DaysSinceDate(2)");
        output.ShouldContain("[2]:DailyAutoIncrement(3)");
        output.ShouldContain("[3]:AutoIncrementWithReset(4)");
        output.ShouldContain("[4]:AutoIncrementWithResetAny(5)");
        output.ShouldContain("[5]:ContinualIncrement(6)");
        output.ShouldContain("[6]:WeeksSinceDate(7)");
        output.ShouldContain("[7]:ReleaseName(8)");
        th.LastExecutionExitCode.ShouldBe(0);
    }

    [Theory]
    [InlineData("[0]:Fixed", "Increment", 0)]
    [InlineData("[1]:DaysSinceDate", "Increment", 1)]
    [InlineData("[4]:AutoIncrementWithResetAny", "Fixed", 4)]
    public async Task Behaviour_command_shows_correct_digits(string outputData, string noOutputData, int digitPosition) {
        b.Info.Flow();
        string resName = TestResources.GetIdentifiers(TestResourcesReferences.OneEachBehaviourStore);
        string vStoreFilePath = uth.GetTestDataFile(resName);

        string output = await th.ExecuteVersonify($"behaviour -vs={vStoreFilePath} -DG={digitPosition}");

        output.ShouldContain(outputData);
        output.ShouldNotContain(noOutputData);
        th.LastExecutionExitCode.ShouldBe(0);
    }

    [Theory]
    [InlineData(1, "Fixed", DigitIncrementBehaviour.Fixed)]
    [InlineData(2, "autoincrementwithreset", DigitIncrementBehaviour.AutoIncrementWithReset)]
    [InlineData(3, "Weekssincedate", DigitIncrementBehaviour.WeeksSinceDate)]
    [InlineData(4, "6", DigitIncrementBehaviour.ContinualIncrement)]
    public async Task Behaviour_command_sets_correct_digits(int digitPosition, string quickValue, DigitIncrementBehaviour outputdata) {
        b.Info.Flow();
        string resName = TestResources.GetIdentifiers(TestResourcesReferences.OneEachBehaviourStore);
        string vStoreFilePath = uth.GetTestDataFile(resName);
        string expectedOutput = $"Setting Behaviour for Digit[{digitPosition}] to {outputdata}({(int)outputdata})";

        string output = await th.ExecuteVersonify($"behaviour -vs={vStoreFilePath} -DG={digitPosition} -Q={quickValue}");

        output.ShouldContain(expectedOutput);
        th.LastExecutionExitCode.ShouldBe(0);
    }

    [Fact]
    public async Task Console_with_nuke_has_markers() {
        b.Info.Flow();
        string resName = TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore);
        string vStoreFilePath = uth.GetTestDataFile(resName);

        string args = $"passive -vs={vStoreFilePath} -O=con-nf -Debug=v-** -Q=1.9.4.3";
        string s = await th.ExecuteVersonify(args);

        s.ShouldContain("PNFV]", customMessage: "Nuke Marker not found in output");
    }

    [Fact]
    public async Task Console_does_not_have_nuke_markers() {
        b.Info.Flow();

        string resName = TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore);
        string vStoreFilePath = uth.GetTestDataFile(resName);

        string args = $"passive -vs={vStoreFilePath} -O=con -Debug=v-** -Q=1.9.4.3";
        string s = await th.ExecuteVersonify(args);

        s.ShouldNotContain("PNFV]");
        th.LastExecutionExitCode.ShouldBe(0);
    }

    [Theory]
    [InlineData("passive", false, true)]
    [InlineData("passive", true, false)]
    [InlineData("createversion", true, true)]
    [InlineData("createversion", false, false)]
    [Trait("Cause", "Bug:LFY-25")]
    public async Task VersionStorage_DoesVstoreExist_ValidatesFileExistence(string command, bool isVstoreValid, bool shouldError) {
        b.Info.Flow();
        string resName = TestResources.GetIdentifiers(TestResourcesReferences.DefaultVersionStore);
        string vStoreFilePath = uth.GetTestDataFile(resName);
        if (!isVstoreValid) {
            vStoreFilePath = vStoreFilePath + ".invalid";
        }

        string output = await th.ExecuteVersonify($"{command} -vs={vStoreFilePath}");

        if (shouldError) {
            output.ShouldContain("Error >>");
        } else {
            output.ShouldNotContain("Error >>");
        }
    }
}