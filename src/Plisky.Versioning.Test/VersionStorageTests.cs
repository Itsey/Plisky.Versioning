namespace Plisky.CodeCraft.Test;
using Plisky.Diagnostics;
using Plisky.Test;
using Xunit;

public class VersionStorageTests {
    private readonly Bilge b = new();

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void ManualExportVersionFile() {
        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("0", "."),
            new VersionUnit("Unicorn", "-"),
            new VersionUnit("0", ".", DigitIncremementBehaviour.AutoIncrementWithResetAny));
        VersionStorage vs = new JsonVersionPersister(@"c:\temp\output.json");
        vs.Persist(cv);
    }

    [Fact(DisplayName = nameof(VersionStorage_CreatesDefaultIfNotPresent))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void VersionStorage_CreatesDefaultIfNotPresent() {
        b.Info.Flow();

        VersionStorage vs = new MockVersionStorage("default");
        var ver = vs.GetVersion();

        Assert.True(ver.IsDefault);
        Assert.Equal(4, ver.Digits.Length);
        Assert.Equal("0.0.0.0", ver.ToString());
    }

    [Fact(DisplayName = nameof(VersionStorage_NotDefaultWhenPresent))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void VersionStorage_NotDefaultWhenPresent() {
        b.Info.Flow();

        VersionStorage vs = new MockVersionStorage("0.0.0.0");
        var ver = vs.GetVersion();

        Assert.False(ver.IsDefault);
        Assert.Equal(4, ver.Digits.Length);
        Assert.Equal("0.0.0.0", ver.ToString());
    }
}