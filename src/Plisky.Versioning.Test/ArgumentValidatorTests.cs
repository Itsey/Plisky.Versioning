namespace Plisky.CodeCraft.Test;

using System;
using System.IO;
using Plisky.Test;
using Shouldly;
using Versonify;
using Xunit;

public class ArgumentValidatorTests {

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData(new string[] { }, "1.2.3", true)]
    [InlineData(new string[] { "1" }, "1.2.3", false)]
    [InlineData(new string[] { }, "123", false)]
    [InlineData(new string[] { }, "", false)]
    [InlineData(new string[] { }, "  ", false)]
    [InlineData(new string[] { }, null, false)]
    public void ShouldSetCompleteVersionFromString_returns_expected_result(string[] digitsToUpdate, string? valueToSet, bool expected) {
        ArgumentValidator.ShouldSetCompleteVersionFromString(digitsToUpdate, valueToSet).ShouldBe(expected);
    }

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

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Validate_when_root_directory_does_not_exist_fails() {
        var sut = CreateValidOptions("passive");
        sut.Root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        ArgumentValidator.ValidateArgumentSettings(sut).ShouldBeFalse();
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_when_version_store_is_missing_fails(string? versionStore) {
        var sut = CreateValidOptions("passive");
        sut.VersionPersistanceValue = versionStore;

        ArgumentValidator.ValidateArgumentSettings(sut).ShouldBeFalse();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Validate_when_output_file_name_contains_invalid_character_fails() {
        var sut = CreateValidOptions("passive");
        sut.PverFileName = $"invalid{Path.GetInvalidFileNameChars()[0]}name.txt";

        ArgumentValidator.ValidateArgumentSettings(sut).ShouldBeFalse();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Validate_when_command_is_invalid_fails() {
        var sut = CreateValidOptions("unknown");

        ArgumentValidator.ValidateArgumentSettings(sut).ShouldBeFalse();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Validate_when_behaviour_has_no_digits_fails() {
        var sut = CreateValidOptions("behaviour");
        sut.QuickValue = null;
        sut.DigitManipulations = [];

        ArgumentValidator.ValidateArgumentSettings(sut).ShouldBeFalse();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Validate_when_prefix_has_no_digits_fails() {
        var sut = CreateValidOptions("prefix");
        sut.DigitManipulations = [];

        ArgumentValidator.ValidateArgumentSettings(sut).ShouldBeFalse();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Validate_when_prefix_value_is_missing_fails() {
        var sut = CreateValidOptions("prefix");
        sut.QuickValue = null;

        ArgumentValidator.ValidateArgumentSettings(sut).ShouldBeFalse();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Validate_when_set_has_no_value_or_group_fails() {
        var sut = CreateValidOptions("set");
        sut.QuickValue = null;

        ArgumentValidator.ValidateArgumentSettings(sut).ShouldBeFalse();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Validate_when_set_individual_value_has_no_digits_fails() {
        var sut = CreateValidOptions("set");
        sut.QuickValue = "9";
        sut.DigitManipulations = [];

        ArgumentValidator.ValidateArgumentSettings(sut).ShouldBeFalse();
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_when_override_pattern_is_missing_fails(string? pattern) {
        var sut = CreateValidOptions("override");
        sut.QuickValue = pattern;

        ArgumentValidator.ValidateArgumentSettings(sut).ShouldBeFalse();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Validate_when_set_release_name_has_quick_value_fails() {
        var sut = CreateValidOptions("set");
        sut.Release = "alpha";

        ArgumentValidator.ValidateArgumentSettings(sut).ShouldBeFalse();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Validate_when_update_files_has_no_minmatch_files_fails() {
        var sut = CreateValidOptions("updatefiles");
        sut.VersionTargetMinMatch = [];

        ArgumentValidator.ValidateArgumentSettings(sut).ShouldBeFalse();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void ValidateVersionStorage_when_storage_is_null_fails() {
        ArgumentValidator.ValidateVersionStorage(null, CreateValidOptions("passive")).ShouldBeFalse();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void ValidateVersionStorage_when_storage_is_invalid_fails() {
        var storage = new MockVersionStorage("store.vstore") {
            StorageFailureMessage = "Invalid storage",
        };

        ArgumentValidator.ValidateVersionStorage(storage, CreateValidOptions("passive")).ShouldBeFalse();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void ValidateVersionStorage_when_storage_does_not_exist_for_non_create_command_fails() {
        var storage = new MockVersionStorage("invalid");

        ArgumentValidator.ValidateVersionStorage(storage, CreateValidOptions("passive")).ShouldBeFalse();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void ValidateVersionStorage_when_storage_exists_for_create_command_fails() {
        var storage = new MockVersionStorage("store.vstore");

        ArgumentValidator.ValidateVersionStorage(storage, CreateValidOptions("createversion")).ShouldBeFalse();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void ValidateVersionStorage_when_storage_does_not_exist_for_create_command_succeeds() {
        var storage = new MockVersionStorage("invalid");

        ArgumentValidator.ValidateVersionStorage(storage, CreateValidOptions("createversion")).ShouldBeTrue();
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void ValidateVersionStorage_when_storage_exists_for_non_create_command_succeeds() {
        var storage = new MockVersionStorage("store.vstore");

        ArgumentValidator.ValidateVersionStorage(storage, CreateValidOptions("passive")).ShouldBeTrue();
    }

    private static VersonifyOptions CreateValidOptions(string command) {
        return new VersonifyOptions {
            Command = command,
            VersionPersistanceValue = "store.vstore",
            DigitManipulations = ["0"],
            QuickValue = "1",
            VersionTargetMinMatch = ["matches.txt"],
        };
    }

    private static VersonifyOptions CreateSetOptions() {
        return new VersonifyOptions {
            Command = "set",
            VersionPersistanceValue = "store.vstore",
        };
    }
}