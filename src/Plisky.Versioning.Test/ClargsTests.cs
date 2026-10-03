namespace Plisky.CodeCraft.Test;

using Shouldly;
using Versonify;
using Xunit;

public class ClargsTests {

    [Fact]
    public void Build_with_no_pairings_returns_empty_string() {
        Clargs.Build().ShouldBe(string.Empty);
    }

    [Fact]
    public void Build_with_flag_without_value_returns_argument_name() {
        Clargs.Build(new Arg(ArgNames.Debug, string.Empty)).ShouldBe("--debug");
    }

    [Fact]
    public void Build_with_argument_value_joins_name_and_value() {
        Clargs.Build(
            new Arg(ArgNames.Command, "set"),
            new Arg(ArgNames.QuickValue, "1.2.3"),
            new Arg(ArgNames.DryRun, string.Empty))
            .ShouldBe("--command=set --quick-value=1.2.3 --dry-run");
    }

    [Fact]
    public void Build_with_unknown_argument_and_value_returns_value_as_positional_argument() {
        Clargs.Build(new Arg(ArgNames.Unknown, "positional")).ShouldBe("positional");
    }

    [Fact]
    public void Build_with_unknown_argument_and_empty_value_returns_empty_string() {
        Clargs.Build(new Arg(ArgNames.Unknown, string.Empty)).ShouldBe(string.Empty);
    }
}
