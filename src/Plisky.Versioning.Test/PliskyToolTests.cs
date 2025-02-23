namespace Plisky.Versioning.Test;

using System;
using Plisky.CodeCraft;
using Plisky.CodeCraft.Test;
using Plisky.Diagnostics;
using Plisky.Test;
using Versonify;
using Xunit;

public class PliskyToolTests {
    private readonly Bilge b = new();
    private readonly UnitTestHelper uth;
    private readonly TestSupport ts;

    public PliskyToolTests() {
        uth = new UnitTestHelper();
        ts = new TestSupport(uth);
    }



    [Fact(DisplayName = nameof(ParseOptions_EnvSelected_Works))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void ParseOptions_EnvSelected_Works() {
        b.Info.Flow();

        var sut = new VersonifyCommandline {
            OutputOptions = "env"
        };

        Assert.True((sut.OutputsActive & OutputPossibilities.Environment) == OutputPossibilities.Environment);
    }

    [Fact(DisplayName = nameof(ParseOptions_FileSelected_Works))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void ParseOptions_FileSelected_Works() {
        b.Info.Flow();

        var sut = new VersonifyCommandline {
            OutputOptions = "file"
        };

        Assert.True((sut.OutputsActive & OutputPossibilities.File) == OutputPossibilities.File);
    }


    [Fact(DisplayName = nameof(ParseOptions_Invalid_ThrowsError))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void ParseOptions_Invalid_ThrowsError() {
        b.Info.Flow();

        _ = Assert.Throws<ArgumentOutOfRangeException>(() => {
            var sut = new VersonifyCommandline {
                OutputOptions = "MyIncrediblyWrongArgument"
            };
        });
    }


    [Fact(DisplayName = nameof(ParseOptions_DefaultsToNone))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void ParseOptions_DefaultsToNone() {
        b.Info.Flow();

        var sut = new VersonifyCommandline();
        Assert.Equal(OutputPossibilities.None, sut.OutputsActive);
    }


    [Fact(DisplayName = nameof(ParseOptions_NullIsNone))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void ParseOptions_NullIsNone() {
        b.Info.Flow();

        var sut = new VersonifyCommandline {
            OutputOptions = ""
        };
        Assert.Equal(OutputPossibilities.None, sut.OutputsActive);

        sut.OutputOptions = null;
        Assert.Equal(OutputPossibilities.None, sut.OutputsActive);
    }

}
