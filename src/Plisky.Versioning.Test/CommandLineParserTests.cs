namespace Plisky.CodeCraft.Test;

using System;
using System.IO;
using Plisky.Test;
using Shouldly;
using Versonify;
using Xunit;

public class CommandLineParserTests {

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("-VS", "WARNING: '-VS' is deprecated. Use '--version-source' instead.")]
    [InlineData("--VersionSource", "WARNING: '--VersionSource' is deprecated. Use '--version-source' instead.")]
    public void Parse_when_deprecated_alias_is_used_emits_warning_to_standard_error(string alias, string expectedWarning) {
        var (success, options, standardError) = ParseAndCaptureStandardError(["passive", $"{alias}=store.vstore"]);

        success.ShouldBeTrue();
        options.VersionPersistanceValue.ShouldBe("store.vstore");
        standardError.ShouldBe(expectedWarning + Environment.NewLine);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Parse_when_canonical_option_is_used_does_not_emit_deprecation_warning() {
        var (success, options, standardError) = ParseAndCaptureStandardError(["passive", "--version-source=store.vstore"]);

        success.ShouldBeTrue();
        options.VersionPersistanceValue.ShouldBe("store.vstore");
        standardError.ShouldBeEmpty();
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("-v")]
    [InlineData("-V")]
    public void Parse_when_version_source_short_alias_is_used_sets_version_source(string alias) {
        var (success, options) = CommandLineParser.Parse(["passive", $"{alias}=store.vstore"]);

        success.ShouldBeTrue();
        options.VersionPersistanceValue.ShouldBe("store.vstore");
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("-d")]
    [InlineData("-D")]
    public void Parse_when_digits_short_alias_is_used_sets_digit_manipulations(string alias) {
        var (success, options) = CommandLineParser.Parse(["passive", $"{alias}=2;3"]);

        success.ShouldBeTrue();
        options.DigitManipulations.ShouldBe(["2", "3"]);
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("-q")]
    [InlineData("-Q")]
    public void Parse_when_quick_value_short_alias_is_used_sets_quick_value(string alias) {
        var (success, options) = CommandLineParser.Parse(["set", $"{alias}=1.2.3"]);

        success.ShouldBeTrue();
        options.QuickValue.ShouldBe("1.2.3");
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("-o")]
    [InlineData("-O")]
    public void Parse_when_output_short_alias_is_used_sets_output_options(string alias) {
        var (success, options) = CommandLineParser.Parse(["passive", $"{alias}=env"]);

        success.ShouldBeTrue();
        options.RawOutputOptions.ShouldBe("env");
        options.OutputsActive.ShouldBe(OutputPossibilities.Environment);
    }

    [Theory]
    [Trait(Traits.Age, Traits.Fresh)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData("--output", "con-pf", OutputPossibilities.PliskyFusion)]
    [InlineData("-o", "con-pf", OutputPossibilities.PliskyFusion)]
    [InlineData("-O", "CON-PF", OutputPossibilities.PliskyFusion)]
    [InlineData("--output", "con-nf", OutputPossibilities.NukeFusion)]
    [InlineData("-o", "con-nf", OutputPossibilities.NukeFusion)]
    [InlineData("-O", "CON-NF", OutputPossibilities.NukeFusion)]
    public void Parse_when_fusion_output_is_selected_sets_console_and_fusion_flags(string alias, string mode, OutputPossibilities fusion) {
        var (success, options) = CommandLineParser.Parse(["passive", $"{alias}={mode}"]);

        success.ShouldBeTrue();
        options.OutputsActive.ShouldBe(OutputPossibilities.Console | fusion);
        options.ConsoleTemplate.ShouldBe("%VER%");

        options.OutputOptions = "con";
        options.OutputsActive.ShouldBe(OutputPossibilities.Console);
    }

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

    private static (bool Success, VersonifyOptions Options, string StandardError) ParseAndCaptureStandardError(string[] args) {
        var originalStandardError = Console.Error;
        using var capturedStandardError = new StringWriter();
        try {
            Console.SetError(capturedStandardError);
            var (success, options) = CommandLineParser.Parse(args);
            return (success, options, capturedStandardError.ToString());
        } finally {
            Console.SetError(originalStandardError);
        }
    }
}