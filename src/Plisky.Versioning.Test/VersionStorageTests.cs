namespace Plisky.CodeCraft.Test;

using System.IO;
using Plisky.Diagnostics;
using Plisky.Test;
using Xunit;

public class VersionStorageTests {
    private readonly Bilge b = new();
    private readonly UnitTestHelper uth;
    private readonly TestSupport ts;

    public VersionStorageTests() {
        uth = new UnitTestHelper();
        ts = new TestSupport(uth);
    }




    [Fact]
    public void VersionStorage_Json_Loads() {
        string fn = uth.NewTemporaryFileName(true);
        var sut = new JsonVersionPersister(fn);
        var cv = new CompleteVersion(new VersionUnit("1", ".", DigitIncrementBehaviour.ContinualIncrement), new VersionUnit("Alpha", "-"), new VersionUnit("1"), new VersionUnit("1", "", DigitIncrementBehaviour.ContinualIncrement));
        sut.Persist(cv);

        var cv2 = sut.GetVersion();

        Assert.Equal(cv.GetVersionString(), cv2.GetVersionString());
        cv.Increment();
        cv2.Increment();
        Assert.Equal(cv.GetVersionString(), cv2.GetVersionString());
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Integration)]
    public void VersionStoreAndLoad_StoresUpdatedValues() {
        string fn = uth.NewTemporaryFileName(true);
        var sut = new JsonVersionPersister(fn);
        var cv = new CompleteVersion(new VersionUnit("1", "", DigitIncrementBehaviour.ContinualIncrement),
            new VersionUnit("1", ".", DigitIncrementBehaviour.ContinualIncrement),
            new VersionUnit("1", ".", DigitIncrementBehaviour.ContinualIncrement),
            new VersionUnit("1", ".", DigitIncrementBehaviour.ContinualIncrement));

        cv.Increment();
        _ = cv.GetVersionString();
        sut.Persist(cv);
        var cv2 = sut.GetVersion();

        Assert.Equal(cv.GetVersionString(), cv2.GetVersionString()); //, "The two version strings should match");
        Assert.Equal("2.2.2.2", cv2.GetVersionString()); //, "The loaded version string should keep the increment");

        cv.Increment(); cv2.Increment();
        Assert.Equal(cv.GetVersionString(), cv2.GetVersionString()); //, "The two version strings should match");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Integration)]
    public void VersionStoreAndLoad_StoresDisplayTypes() {
        string fn = uth.NewTemporaryFileName(true);
        var sut = new JsonVersionPersister(fn);
        var cv = new CompleteVersion(new VersionUnit("1", "", DigitIncrementBehaviour.ContinualIncrement),
            new VersionUnit("1", ".", DigitIncrementBehaviour.ContinualIncrement),
            new VersionUnit("1", ".", DigitIncrementBehaviour.ContinualIncrement),
            new VersionUnit("1", ".", DigitIncrementBehaviour.ContinualIncrement));

        // None of the defaults are no display, therefore this should set all to a new value
        cv.SetDisplayTypeForVersion(FileUpdateType.NetAssembly, DisplayType.NoDisplay);
        cv.SetDisplayTypeForVersion(FileUpdateType.NetFile, DisplayType.NoDisplay);
        cv.SetDisplayTypeForVersion(FileUpdateType.NetInformational, DisplayType.NoDisplay);
        cv.SetDisplayTypeForVersion(FileUpdateType.Wix, DisplayType.NoDisplay);
        cv.SetDisplayTypeForVersion(FileUpdateType.StdAssembly, DisplayType.NoDisplay);
        cv.SetDisplayTypeForVersion(FileUpdateType.StdFile, DisplayType.NoDisplay);
        cv.SetDisplayTypeForVersion(FileUpdateType.StdInformational, DisplayType.NoDisplay);
        cv.SetDisplayTypeForVersion(FileUpdateType.Nuspec, DisplayType.NoDisplay);

        sut.Persist(cv);
        var cv2 = sut.GetVersion();

        // Check that all of the display types come back as epxected
        Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.NetAssembly));
        Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.NetFile));
        Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.NetInformational));
        Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.Wix));
        Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.StdAssembly));
        Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.StdFile));
        Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.StdInformational));
        Assert.Equal(DisplayType.NoDisplay, cv2.GetDisplayType(FileUpdateType.Nuspec));
    }

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

    [Fact(DisplayName = nameof(Storage_DefaultValidationIsTrue))]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Storage_DefaultValidationIsTrue() {
        b.Info.Flow();
        VersionStorage sut = new MockVersionStorage("itsamock");
        Assert.True(sut.ValidateInitialisation());
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Integration)]
    public void VersionStorage_Json_Saves() {
        string fn = uth.NewTemporaryFileName(true);
        var sut = new JsonVersionPersister(fn);
        var cv = new CompleteVersion(new VersionUnit("1"), new VersionUnit("1"), new VersionUnit("1"), new VersionUnit("1"));
        sut.Persist(cv);
        Assert.True(File.Exists(fn), "The file must be created");
    }
}