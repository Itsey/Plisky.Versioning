namespace Plisky.CodeCraft.Test;

using System;
using Plisky.Diagnostics;
using Plisky.Test;
using Versonify;
using Xunit;

public class VersonifyCommandLineTests {
    private readonly Bilge b = new();
    private readonly UnitTestHelper uth;
    private readonly TestSupport ts;

    public VersonifyCommandLineTests() {
        uth = new UnitTestHelper();
        ts = new TestSupport(uth);
    }



    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Output_environment_selected_works() {
        b.Info.Flow();

        var sut = new VersonifyCommandline {
            OutputOptions = "env"
        };

        Assert.True((sut.OutputsActive & OutputPossibilities.Environment) == OutputPossibilities.Environment);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Output_file_selected_works() {
        b.Info.Flow();

        var sut = new VersonifyCommandline {
            OutputOptions = "file"
        };

        Assert.True((sut.OutputsActive & OutputPossibilities.File) == OutputPossibilities.File);
    }


    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Output_incorrect_value_throws() {
        b.Info.Flow();

        _ = Assert.Throws<ArgumentOutOfRangeException>(() => {
            var sut = new VersonifyCommandline {
                OutputOptions = "MyIncrediblyWrongArgument"
            };
        });
    }


    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Output_defaults_to_none() {
        b.Info.Flow();

        var sut = new VersonifyCommandline();
        Assert.Equal(OutputPossibilities.None, sut.OutputsActive);
    }


    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Output_null_is_same_as_none() {
        b.Info.Flow();

        var sut = new VersonifyCommandline {
            OutputOptions = ""
        };
        Assert.Equal(OutputPossibilities.None, sut.OutputsActive);

        sut.OutputOptions = null;
        Assert.Equal(OutputPossibilities.None, sut.OutputsActive);
    }

}
