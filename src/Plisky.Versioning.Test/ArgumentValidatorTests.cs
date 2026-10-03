namespace Plisky.CodeCraft.Test;

using Plisky.Test;
using Shouldly;
using Versonify;
using Xunit;

public class ArgumentValidatorTests {

    [Theory]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("passive", true)]
    [InlineData("set", false)]
    [InlineData("createversion", false)]
    [InlineData("updatefiles", false)]
    [InlineData("override", false)]
    [InlineData("behaviour", false)]
    [InlineData("get", false)]
    [InlineData("prefix", false)]
    public void Validate_when_release_value_is_missing_only_passive_is_allowed(string command, bool expected) {
        var sut = new VersonifyOptions {
            Command = command,
            VersionPersistanceValue = "store.vstore",
            Release = string.Empty,
            DigitManipulations = ["0"],
            QuickValue = command is "set" or "behaviour" ? null : "1",
            VersionTargetMinMatch = ["matches.txt"],
        };
        ArgumentValidator.ValidateArgumentSettings(sut).ShouldBeTrue();

        sut.ReleaseValueMissing = true;

        ArgumentValidator.ValidateArgumentSettings(sut).ShouldBe(expected);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Validate_when_digit_group_and_pre_release_are_both_set_fails() {
        var sut = CreateSetOptions();
        sut.DigitManipulations = ["2"];
        sut.DigitGroup = "prerelease";
        sut.PreRelease = true;

        bool result = ArgumentValidator.ValidateArgumentSettings(sut);

        result.ShouldBeFalse();
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("a,b")]
    [InlineData("*")]
    public void Validate_when_set_digit_group_is_invalid_fails(string digitGroup) {
        var sut = CreateSetOptions();
        sut.DigitManipulations = ["2"];
        sut.DigitGroup = digitGroup;

        bool result = ArgumentValidator.ValidateArgumentSettings(sut);

        result.ShouldBeFalse();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Validate_when_set_has_digit_group_and_no_digits_fails() {
        var sut = CreateSetOptions();
        sut.QuickValue = "9.8.7.6";
        sut.DigitGroup = "prerelease";

        bool result = ArgumentValidator.ValidateArgumentSettings(sut);

        result.ShouldBeFalse();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Validate_when_set_has_full_version_and_no_group_assignment_works() {
        var sut = CreateSetOptions();
        sut.QuickValue = "9.8.7.6";

        bool result = ArgumentValidator.ValidateArgumentSettings(sut);

        result.ShouldBeTrue();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Validate_when_set_has_pre_release_and_no_digits_fails() {
        var sut = CreateSetOptions();
        sut.QuickValue = "9.8.7.6";
        sut.PreRelease = true;

        bool result = ArgumentValidator.ValidateArgumentSettings(sut);

        result.ShouldBeFalse();
    }

    private static VersonifyOptions CreateSetOptions() {
        return new VersonifyOptions {
            Command = "set",
            VersionPersistanceValue = "store.vstore",
        };
    }
}