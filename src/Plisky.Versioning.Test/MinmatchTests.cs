namespace Plisky.Versioning.Test;
using System;
using System.Collections.Generic;
using Minimatch;
using Plisky.CodeCraft.Test;
using Plisky.Diagnostics;
using Plisky.Test;
using Xunit;

public class MinmatchTests {
    private readonly Bilge b = new();
    private readonly UnitTestHelper uth;
    private readonly TestSupport ts;

    public MinmatchTests() {
        uth = new UnitTestHelper();
        ts = new TestSupport(uth);
    }

    private static void CheckTheseMatches(List<Tuple<string, bool>> mtchs, string againstThis) {
        var mm = new Minimatcher(againstThis, new Options { AllowWindowsPaths = true, IgnoreCase = true });
        int i = 0;
        foreach (var v in mtchs) {
            i++;
            bool isMatch = mm.IsMatch(v.Item1);
            Assert.Equal(v.Item2, isMatch);
        }
    }

    [Theory]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [InlineData(@"C:\temp\verworking\assemblyinfo.cs", @"**\assemblyinfo.cs", true)]
    [InlineData(@"C:\temp\verworking\testing.csproj", @"**/*.csproj", true)]
    [InlineData(@"C:\temp\verworking\testing.csproj", @"**\verworking\*.csproj", true)]
    [InlineData(@"C:\temp\verworking\AsUbBy\testing.csproj", @"**\asubby\**\*.csproj", true)]
    [InlineData(@"C:\temp\verworking\AsUbBy\commonassemblyinfo.cs", @"**\asubby\**\common*.cs", true)]
    public void MinimatchSyntax_Research(string filename, string minimatch, bool shouldPass) {
        var mtchs = new List<Tuple<string, bool>> {
            new Tuple<string, bool>(filename, shouldPass)
        };
        CheckTheseMatches(mtchs, minimatch);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void MiniMatchSyntax_FindAssemblyInfo() {
        var mtchs = new List<Tuple<string, bool>> {
            new Tuple<string, bool>(@"C:\temp\te st\properties\assemblyinfo.cs", true),
            new Tuple<string, bool>(@"C:\te mp\test\assemblyinfo.cs", false),
            new Tuple<string, bool>(@"C:\te mp\t e s t\properties\notassemblyinfo.cs", false),
            new Tuple<string, bool>(@"C:\temp\test\properties\assemblyinfo.cs.txt", false),
            new Tuple<string, bool>(@"C:\a\1\s\PliskyLibrary\PliskyLib\Properties\AssemblyInfo.cs", true)
        };
        string againstThis = @"**\properties\assemblyinfo.cs";
        CheckTheseMatches(mtchs, againstThis);

        var mm2 = new Minimatcher(@"C:\temp\test\testfile.tst", new Options { AllowWindowsPaths = true });
        Assert.True(mm2.IsMatch(@"C:\temp\test\testfile.tst"), "Cant match on full filename");
    }


    [Fact(DisplayName = nameof(GetDefaultMinimatchers_IsNotEmpty))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void GetDefaultMinimatchers_IsNotEmpty() {
        b.Info.Flow();

        var v = new MockVersioning(new MockVersionStorage(""));

        Assert.True(v.mock.ReturnMinMatchers().Length > 0, "There should be some default minmatchers loaded by versioning");
    }

    [Fact(DisplayName = nameof(SetMinMatchers_ReplacesAll))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void SetMinMatchers_ReplacesAll() {
        b.Info.Flow();

        var v = new MockVersioning(new MockVersionStorage(""));

        v.ClearMiniMatchers();

        Assert.True(v.mock.ReturnMinMatchers().Length == 0, "Clear should remove all minimatchers");
    }

    [Fact(DisplayName = nameof(Versioning_MMLoadedFromFile))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Versioning_MMLoadedFromFile() {
        b.Info.Flow();

        string reid = TestResources.GetIdentifiers(TestResourcesReferences.MMTypeData);
        string srcFile = uth.GetTestDataFile(reid);

        var mva = new MockVersionStorage("");
        var v = new MockVersioning(mva);

        v.LoadMiniMatches(srcFile);

        Assert.Equal(9, v.mock.ReturnMinMatchers().Length);
    }
}