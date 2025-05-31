namespace Plisky.CodeCraft.Test;
using Plisky.Diagnostics;
using Plisky.Test;
using Xunit;

public class VersionStorageTests {
    private readonly Bilge b = new();

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Basic_save_works() {
        var msut = new MockVersionStorage("itsamock");
        VersionStorage sut = msut;

        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1"), new VersionUnit("1"), new VersionUnit("1"));
        sut.Persist(cv);
        Assert.True(msut.PersistWasCalled, "The persist method was not called");
        Assert.Equal("1111", msut.VersionStringPersisted);
    }


    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [Fact]
    public void When_created_valid_version_marked_as_default() {
        b.Info.Flow();

        VersionStorage vs = new MockVersionStorage("default");
        var ver = vs.GetVersion();

        Assert.True(ver.IsDefault);
        Assert.Equal(4, ver.Digits.Length);
        Assert.Equal("0.0.0.0", ver.ToString());
    }


    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    [Fact]
    public void When_created_not_default_if_value_passed() {
        b.Info.Flow();

        VersionStorage vs = new MockVersionStorage("0.0.0.0");
        var ver = vs.GetVersion();

        Assert.False(ver.IsDefault);
        Assert.Equal(4, ver.Digits.Length);
        Assert.Equal("0.0.0.0", ver.ToString());
    }
}