using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;

namespace Versonify.ITest;

public class RegressionTests {
    protected Bilge b = new("Versonify-ITest");
    protected TestHelper th;
    protected UnitTestHelper uth;

    public RegressionTests() {
        b.Info.Flow();

        uth = new UnitTestHelper();
        th = new TestHelper(uth);
    }

    [Fact(DisplayName = "Compatibility Level is 201")]
    public async Task Call_with_qq_returns_expected_compat_code() {
        b.Info.Flow();

        var output = await th.ExecuteVersonify("--qqpnf", appendDebug: false);
        output.ReturnCode.ShouldBe(201, "Current compat version is 201.");
    }

    [Theory]
    [InlineData("-Command", "-mm", "-v")]
    [InlineData(Clargs.COMMAND_ARG, Clargs.MIN_MATCH_ARG, Clargs.VERSION_SOURCE_ARG)]
    public async Task Legacy_aliases_still_work_as_expected(string commandCmd, string minMatchCmd, string versionStoreCmd) {
        b.Info.Flow();

        string pth = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        Directory.CreateDirectory(pth);

        try {
            string versionStore = Path.Combine(pth, "vstore.delme");
            string fileToUpdate = Path.Combine(pth, "test.txt");
            string fileToUpdate2 = Path.Combine(pth, "test.wxs");
            string mmFile = Path.Combine(pth, "mm.txt");

            void ResetFiles() {
                File.WriteAllText(fileToUpdate, "This is a test file AXXX-VERSION-XXX, BXXX-VERSION2-XXX, XXX-VERSION3-XXX, XXX-VERSION4-XXX");
                File.WriteAllText(fileToUpdate2, "<Wix xmlns=\"http://schemas.microsoft.com/wix/2006/wi\"><Product Version=\"9.9\"/></Wix>");
            }

            void AssertFilesContain(string expectedTxt, string expectedWxs) {
                File.ReadAllText(fileToUpdate).ShouldContain(expectedTxt);
                File.ReadAllText(fileToUpdate2).ShouldContain(expectedWxs);
            }

            async Task<VersonifyExecutionOutput> RunUpdateFiles(string mmPatternOrFile, string extraArgs = "") {
                string args = string.IsNullOrEmpty(extraArgs)
                    ? $"{commandCmd}=UpdateFiles -Root={pth} -Increment {versionStoreCmd}={versionStore} {minMatchCmd}={mmPatternOrFile} -output=con"
                    : $"{commandCmd}=UpdateFiles -Root={pth} -Increment {versionStoreCmd}={versionStore} {minMatchCmd}={mmPatternOrFile} {extraArgs} -output=con";
                var result = await th.ExecuteVersonify(args);
                result.ReturnCode.ShouldBe(0);
                return result;
            }

            File.WriteAllLines(mmFile, ["*.txt|TextFile", "*.wxs|Wix"]);

            var output = await th.ExecuteVersonify($"{commandCmd}=CreateVersion {versionStoreCmd}={versionStore} -Q=\"1.0.0.0\"");
            output.ReturnCode.ShouldBe(0, "Warning Test setup failed");

            // 1. Semicolon-separated minmatch patterns
            ResetFiles();
            _ = await RunUpdateFiles("*.txt|TextFile;*.wxs|Wix");
            AssertFilesContain("A1.0.0.0,", "Version=\"1.0.0.0\"");

            // 2. File-based minmatch patterns
            ResetFiles();
            _ = await RunUpdateFiles(mmFile);
            AssertFilesContain("A1.0.0.0,", "Version=\"1.0.0.0\"");

            // 3. Pending override disabled by -NO
            ResetFiles();
            output = await th.ExecuteVersonify($"{commandCmd}=Override {versionStoreCmd}={versionStore} -Q=9.9.9.9");
            output.ReturnCode.ShouldBe(0);

            output = await RunUpdateFiles(mmFile, "-NO");
            output.StdOut.ShouldContain("Version Increment Override, Disabled");
            AssertFilesContain("A1.0.0.0,", "Version=\"1.0.0.0\"");
            File.ReadAllText(fileToUpdate).ShouldNotContain("9.9.9.9");
            File.ReadAllText(fileToUpdate2).ShouldNotContain("9.9.9.9");

            // 4. Pending override applied without -NO
            ResetFiles();
            output = await th.ExecuteVersonify($"{commandCmd}=Override {versionStoreCmd}={versionStore} -Q=9.9.9.9");
            output.ReturnCode.ShouldBe(0);

            output = await RunUpdateFiles(mmFile, "");
            AssertFilesContain("A9.9.9.9,", "Version=\"9.9.9.9\"");
        } finally {
            if (Directory.Exists(pth)) {
                Directory.Delete(pth, true);
            }
        }
    }
}