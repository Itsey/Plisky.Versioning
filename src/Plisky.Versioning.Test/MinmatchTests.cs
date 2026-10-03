namespace Plisky.CodeCraft.Test;

using System;
using System.Collections.Generic;
using GlobExpressions;
using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;
using Xunit;

public class MinmatchTests {
    private readonly Bilge b = new();
    private readonly TestSupport ts;
    private readonly UnitTestHelper uth;

    public MinmatchTests() {
        uth = new UnitTestHelper();
        ts = new TestSupport(uth);
    }

    [Fact(DisplayName = nameof(GetDefaultMinimatchers_IsEmpty))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void GetDefaultMinimatchers_IsEmpty() {
        b.Info.Flow();

        var v = new MockVersioning(new MockVersionStorage(""));

        v.mock.ReturnMinMatchers().Length.ShouldBe(0, "There should be no default minmatchers loaded by versioning");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void MiniMatchSyntax_FindAssemblyInfo() {
        var mtchs = new List<Tuple<string, bool>> {
            new(@"C:\temp\te st\properties\assemblyinfo.cs", true),
            new(@"C:\te mp\test\assemblyinfo.cs", false),
            new(@"C:\te mp\t e s t\properties\notassemblyinfo.cs", false),
            new(@"C:\temp\test\properties\assemblyinfo.cs.txt", false),
            new(@"C:\a\1\s\PliskyLibrary\PliskyLib\Properties\AssemblyInfo.cs", true)
        };
        const string AGAINSTTHIS = @"**\properties\assemblyinfo.cs";
        CheckTheseMatches(mtchs, AGAINSTTHIS);

        var mm2 = new Glob(@"C:\temp\test\testfile.tst".Replace('\\', '/'));
        mm2.IsMatch(@"C:\temp\test\testfile.tst").ShouldBeTrue("Cant match on full filename");
    }

    [Theory]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData(@"C:\temp\verworking\assemblyinfo.cs", @"**\assemblyinfo.cs", true)]
    [InlineData(@"C:\temp\verworking\testing.csproj", "**/*.csproj", true)]
    [InlineData(@"C:\temp\verworking\testing.csproj", @"**\verworking\*.csproj", true)]
    [InlineData(@"C:\temp\verworking\AsUbBy\testing.csproj", @"**\asubby\**\*.csproj", true)]
    [InlineData(@"C:\temp\verworking\AsUbBy\commonassemblyinfo.cs", @"**\asubby\**\common*.cs", true)]
    public void MinimatchSyntax_Research(string filename, string minimatch, bool shouldPass) {
        var mtchs = new List<Tuple<string, bool>> {
            new(filename, shouldPass)
        };
        CheckTheseMatches(mtchs, minimatch);
    }

    [Fact(DisplayName = nameof(SetMinMatchers_ReplacesAll))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void SetMinMatchers_ReplacesAll() {
        b.Info.Flow();

        var v = new MockVersioning(new MockVersionStorage(""));

        v.ClearMiniMatchers();
        v.mock.ReturnMinMatchers().Length.ShouldBe(0, "Clear should remove all minimatchers");
    }

    [Fact(DisplayName = nameof(Versioning_MMLoadedFromFile))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Versioning_MMLoadedFromFile() {
        b.Info.Flow();

        string reid = TestResources.GetIdentifiers(TestResourcesReferences.MMTypeData)!;
        string srcFile = uth.GetTestDataFile(reid);

        var mva = new MockVersionStorage("");
        var v = new MockVersioning(mva);

        v.LoadMiniMatches(srcFile);
        v.mock.ReturnMinMatchers().Length.ShouldBe(9);
    }

    private static void CheckTheseMatches(List<Tuple<string, bool>> mtchs, string againstThis) {
        var mm = new Glob(againstThis.Replace('\\', '/'), GlobOptions.CaseInsensitive);
        int i = 0;
        foreach (var v in mtchs) {
            i++;
            bool isMatch = mm.IsMatch(v.Item1);
            v.Item2.ShouldBe(isMatch, $"Test {i} failed. Expected {v.Item2} but got {isMatch} for {v.Item1} against {againstThis}");
        }
    }
}