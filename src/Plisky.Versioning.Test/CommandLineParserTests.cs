namespace Plisky.CodeCraft.Test;

using Plisky.Test;
using Shouldly;
using Versonify;
using Xunit;

public class CommandLineParserTests {
    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("--digit-group=prerelease")]
    [InlineData("-g=prerelease")]
    public void Parse_when_digit_group_option_is_used_works(string digitGroupArg) {
        string[] args = ["passive", "--version-source=store.vstore", digitGroupArg];

        var result = CommandLineParser.Parse(args);

        result.Success.ShouldBeTrue();
        result.Options.DigitGroup.ShouldBe("prerelease");
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("--pre-release")]
    [InlineData("-p")]
    public void Parse_when_pre_release_option_is_used_works(string preReleaseArg) {
        string[] args = ["passive", "--version-source=store.vstore", preReleaseArg];

        var result = CommandLineParser.Parse(args);

        result.Success.ShouldBeTrue();
        result.Options.PreRelease.ShouldBeTrue();
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("--digit-group=")]
    [InlineData("-g=\"\"")]
    public void Parse_when_digit_group_is_empty_normalizes_to_default(string digitGroupArg) {
        string[] args = ["set", "--version-source=store.vstore", "--digits=2", digitGroupArg];

        var result = CommandLineParser.Parse(args);

        result.Success.ShouldBeTrue();
        result.Options.DigitGroup.ShouldBe("default");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Parse_when_multiple_groups_are_specified_works() {
        string[] args = ["passive", "--version-source=store.vstore", "--digit-group=default,prerelease"];

        var result = CommandLineParser.Parse(args);

        result.Success.ShouldBeTrue();
        result.Options.DigitGroup.ShouldBe("default,prerelease");
    }
}
