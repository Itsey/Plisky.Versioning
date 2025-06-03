namespace Plisky.CodeCraft.Test;
using Plisky.CodeCraft;
using Plisky.Diagnostics;
using Plisky.Test;
using Versonify;
using Xunit;

public class VersionOutputterTests {

    private readonly Bilge b = new();
    private readonly UnitTestHelper uth;
    private readonly TestSupport ts;

    public VersionOutputterTests() {
        uth = new UnitTestHelper();
        ts = new TestSupport(uth);
    }


    [Fact(DisplayName = nameof(Args_OutputterParseConsole_Works))]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Args_OutputterParseConsole_Works() {
        b.Info.Flow();

        var cla = new VersonifyCommandline {
            OutputOptions = "con"
        };

        Assert.True((cla.OutputsActive & OutputPossibilities.Console) == OutputPossibilities.Console);
    }


    [Theory(DisplayName = nameof(Args_OutputterParseSetsConsoleString))]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("con:variable", "%VER%")]
    [InlineData("vsts:variable", "variable")]
    [InlineData("con", "%VER%")]
    [InlineData("vsts:variable", "%VER%")]
    [InlineData("vsts", "%VER%")]
    [InlineData("vsts:bone", "bone")]
    public void Args_OutputterParseSetsConsoleString(string argument, string contains) {
        b.Info.Flow();

        var cla = new VersonifyCommandline {
            OutputOptions = argument
        };

        Assert.Contains(contains, cla.ConsoleTemplate);
    }
    [Theory(DisplayName = nameof(Args_OutputterParseSetsFileName))]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("file:myfile.txt", "myfile.txt")]
    [InlineData("file", "pver-latest.txt")]
    public void Args_OutputterParseSetsFileName(string argument, string contains) {
        b.Info.Flow();

        var cla = new VersonifyCommandline {
            OutputOptions = argument
        };

        Assert.Equal(contains, cla.PverFileName);
    }

    [Fact(DisplayName = nameof(Outputter_Environment_WritesToEnvironment))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Outputter_Environment_WritesToEnvironment() {
        b.Info.Flow();


        var mvs = new MockVersionStorage("0.0.0.1");
        var sut = new Versioning(mvs);
        var v = sut.Version;

        var op = new MockVersioningOutputter(v);
        op.DoOutput(OutputPossibilities.File);

        Assert.True(op.FileWasWritten);
        Assert.False(op.EnvWasSet);
    }

}
