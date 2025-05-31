using System;
using Plisky.Diagnostics;
using Plisky.Test;
using Xunit;

namespace Plisky.CodeCraft.Test;

public class BuildTaskTests {
    private readonly Bilge b = new("BuildTaskTests");
    private readonly UnitTestHelper uth;
    private readonly TestSupport ts;

    public BuildTaskTests() {
        uth = new UnitTestHelper();
        ts = new TestSupport(uth);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Set_invalid_ruletype_throws() {
        _ = Assert.Throws<InvalidOperationException>(() => {
            var sut = new TestableVersioningTask();
            string verItemsSimple = "**/assemblyinfo.cs;ASSXXEMBLY";
            sut.SetAllVersioningItems(verItemsSimple);
        });
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Set_valid_ruletype_works() {
        var v = ts.GetDefaultVersion();
        var sut = new TestableVersioningTask();
        sut.SetVersionNumber(v);
        string verItemsSimple = "**/assemblyinfo.cs!ASSEMBLY";
        sut.SetAllVersioningItems(verItemsSimple);

        Assert.True(sut.IsThisMinimatchIncluded("**/assemblyinfo.cs"), "The minimatch was not included");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Set_multiple_valid_ruletypes_works() {
        var v = ts.GetDefaultVersion();
        var sut = new TestableVersioningTask();
        sut.SetVersionNumber(v);
        string verItemsSimple =
            $"**/assemblyinfo.cs!ASSEMBLY{Environment.NewLine}xxMonkey!FILE{Environment.NewLine}yyzzxxbannana!WIX{Environment.NewLine}";
        sut.SetAllVersioningItems(verItemsSimple);

        Assert.True(sut.IsThisMinimatchIncluded("**/assemblyinfo.cs"), "The minimatch was not included");
        Assert.True(sut.IsThisMinimatchIncluded("xxMonkey"), "The second minimatch was not included");
        Assert.True(sut.IsThisMinimatchIncluded("yyzzxxbannana"), "The third minimatch was not included");
    }

}
