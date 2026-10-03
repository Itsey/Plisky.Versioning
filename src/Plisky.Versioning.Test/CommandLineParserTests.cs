namespace Plisky.CodeCraft.Test;

using Plisky.Test;
using Shouldly;
using Versonify;
using Xunit;

public class CommandLineParserTests {

    [Theory]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("passive", "--release")]
    [InlineData("--command=passive", "--release")]
    [InlineData("--command=passive", "-r")]

    public void Parse_when_passive_release_has_no_value_requests_release_output(string command, string releaseArg) {
        string[] args = [command, releaseArg, "-v=store.vstore", "--output=con"];

        var (success, options) = CommandLineParser.Parse(args);

        success.ShouldBeTrue();
        options.Release.ShouldBe(string.Empty);
        options.ReleaseValueMissing.ShouldBeTrue();
        options.VersionPersistanceValue.ShouldBe("store.vstore");
        options.OutputOptions.ShouldBe("con");
    }

    [Theory]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("passive")]
    [InlineData("set")]
    [InlineData("createversion")]
    public void Parse_when_release_is_absent_leaves_release_null(string command) {
        var (success, options) = CommandLineParser.Parse([command, "-v=store.vstore"]);

        success.ShouldBeTrue();
        options.Release.ShouldBeNull();
        options.ReleaseValueMissing.ShouldBeFalse();
    }

    [Theory]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("passive", "--release=Bronte")]
    [InlineData("set", "--release=Bronte")]
    [InlineData("createversion", "--release=Bronte")]
    [InlineData("set", "-r")]
    public void Parse_when_release_has_a_value_preserves_it(string command, string releaseArg) {
        string[] args = releaseArg == "-r"
            ? [command, "-v=store.vstore", releaseArg, "Bronte"]
            : [command, "-v=store.vstore", releaseArg];

        var (success, options) = CommandLineParser.Parse(args);

        success.ShouldBeTrue();
        options.Release.ShouldBe("Bronte");
        options.ReleaseValueMissing.ShouldBeFalse();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Parse_when_non_passive_release_has_no_value_records_missing_value() {
        var (success, options) = CommandLineParser.Parse(["set", "-v=store.vstore", "--release"]);

        success.ShouldBeTrue();
        options.Release.ShouldBe(string.Empty);
        options.ReleaseValueMissing.ShouldBeTrue();
        options.VersionPersistanceValue.ShouldBe("store.vstore");
    }

    [Theory]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("set")]
    [InlineData("passive")]
    [InlineData("createversion")]
    public void Parse_when_release_equals_has_no_value_records_missing_value(string command) {
        var (success, options) = CommandLineParser.Parse([command, "-v=store.vstore", "--release="]);

        success.ShouldBeTrue();
        options.Release.ShouldBe(string.Empty);
        options.ReleaseValueMissing.ShouldBeTrue();
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("--digit-group=")]
    [InlineData("-g=\"\"")]
    public void Parse_when_digit_group_is_empty_normalizes_to_default(string digitGroupArg) {
        string[] args = ["set", "--version-source=store.vstore", "--digits=2", digitGroupArg];

        var (success, options) = CommandLineParser.Parse(args);

        success.ShouldBeTrue();
        options.DigitGroup.ShouldBe("default");
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("--digit-group=prerelease")]
    [InlineData("-g=prerelease")]
    public void Parse_when_digit_group_option_is_used_works(string digitGroupArg) {
        string[] args = ["passive", "--version-source=store.vstore", digitGroupArg];

        var (success, options) = CommandLineParser.Parse(args);

        success.ShouldBeTrue();
        options.DigitGroup.ShouldBe("prerelease");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Parse_when_multiple_groups_are_specified_works() {
        string[] args = ["passive", "--version-source=store.vstore", "--digit-group=default,prerelease"];

        var (success, options) = CommandLineParser.Parse(args);

        success.ShouldBeTrue();
        options.DigitGroup.ShouldBe("default,prerelease");
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("--pre-release")]
    [InlineData("-p")]
    public void Parse_when_pre_release_option_is_used_works(string preReleaseArg) {
        string[] args = ["passive", "--version-source=store.vstore", preReleaseArg];

        var (success, options) = CommandLineParser.Parse(args);

        success.ShouldBeTrue();
        options.PreRelease.ShouldBeTrue();
    }
}