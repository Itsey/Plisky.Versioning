using Plisky.Diagnostics;
using Plisky.Test;
using Xunit;

namespace Plisky.CodeCraft.Test;

public class VersionNumberTests {
    private readonly Bilge b = new();

    [Trait(Traits.Age, Traits.Regression)]
    [Theory]
    [InlineData("1.2.3.4", "1.2.3.4", true, true)]
    [InlineData("0.0.0.0", "0.0.0.0", true, true)]
    [InlineData("0.0", "0.0", true, true)]
    [InlineData("1.0", "1.0.0.0", true, true)]
    [InlineData("2.2.3.4", "1.2.3.4", true, false)]
    [InlineData("1.0.3.4", "1.2.3.4", false, false)]
    public void Greaterthan_equal_is_correct(string v1, string v2, bool v1IsGreaterOrEqual, bool isEqual) {
        b.Info.Flow();
        var vn1 = VersionNumber.Parse(v1);
        var vn2 = VersionNumber.Parse(v2);

        bool isGreater = vn1 >= vn2;
        bool isActuallyEqual = vn1 == vn2;

        Assert.Equal(isEqual, isActuallyEqual);
        Assert.Equal(v1IsGreaterOrEqual, isGreater);
    }

    [Trait(Traits.Age, Traits.Regression)]
    [Theory]
    [InlineData("1.2.3.4", "1.2.3.4", false)]
    [InlineData("2.2.3.4", "1.2.3.4", true)]
    [InlineData("0.2.3.4", "1.2.3.4", false)]
    [InlineData("0.2.4.0", "0.2.3.400", true)]
    [InlineData("1.0.0.0", "0.999.999.999", true)]
    [InlineData("0.0.1.0", "0.0.0.400", true)]
    [InlineData("1.0", "0.0.0.400", true)]
    [InlineData("7.1.0.0", "8.0.0.0", false)]
    [InlineData("7.0.1.0", "8.0.0.0", false)]
    [InlineData("7.1.0.1", "8.1.0.0", false)]
    public void Greaterthan_is_correct(string v1, string v2, bool v1IsGreater) {
        b.Info.Flow();
        var vn1 = VersionNumber.Parse(v1);
        var vn2 = VersionNumber.Parse(v2);

        bool isGreater = vn1 > vn2;
        Assert.Equal(v1IsGreater, isGreater);
    }

    [Trait(Traits.Age, Traits.Regression)]
    [Theory]
    [InlineData("1.2.3.4", "1.2.3.4", false)]
    [InlineData("2.2.3.4", "1.2.3.4", false)]
    [InlineData("0.2.3.4", "1.2.3.4", true)]
    [InlineData("0.2.4.0", "0.2.3.400", false)]
    [InlineData("1.0.0.0", "0.999.999.999", false)]
    [InlineData("0.0.1.0", "0.0.0.400", false)]
    [InlineData("1.0", "0.0.0.400", false)]
    [InlineData("7.1.0.0", "8.0.0.0", true)]
    [InlineData("7.0.1.0", "8.0.0.0", true)]
    [InlineData("7.1.0.1", "8.1.0.0", true)]
    public void Lessthan_is_correct(string v1, string v2, bool v1IsGreater) {
        b.Info.Flow();
        var vn1 = VersionNumber.Parse(v1);
        var vn2 = VersionNumber.Parse(v2);

        bool isGreater = vn1 < vn2;
        Assert.Equal(v1IsGreater, isGreater);
    }

    [Trait(Traits.Age, Traits.Regression)]
    [Theory]
    [InlineData("0.0.0.0", "0.0.0.0")]
    [InlineData("1.2.3.4", "1.2.3.4")]
    [InlineData("1234.1234.1234.1234", "1234.1234.1234.1234")]
    [InlineData("0", "0.0.0.0")]
    [InlineData("1.2.3", "1.2.3.0")]
    public void Parse_string_works(string parseString, string expected) {
        b.Info.Flow();
        var vnd = VersionNumber.Parse(parseString);
        Assert.Equal(expected, vnd.ToString());
    }
}