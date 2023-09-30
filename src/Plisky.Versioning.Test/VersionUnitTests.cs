namespace Plisky.CodeCraft.Test;

using System;
using Diagnostics;
using Plisky.Test;
using Xunit;

public class VersionUnitTests {
    private readonly Bilge b = new();

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void DefaultBehaviour_IsFixed() {
        var vu = new VersionUnit("2", ".");
        Assert.Equal(DigitIncremementBehaviour.Fixed, vu.Behaviour);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void SetBehaviour_OnConstructor_Works() {
        var vu = new VersionUnit("2", ".", DigitIncremementBehaviour.AutoIncrementWithReset);
        Assert.Equal(DigitIncremementBehaviour.AutoIncrementWithReset, vu.Behaviour);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void VersionUnit_ChangeBehaviour_ExceptionIfNotIncrimentable() {
        b.Info.Flow();

        _ = Assert.Throws<InvalidOperationException>(() => {
            var sut = new VersionUnit("monkey");
            sut.SetBehaviour(DigitIncremementBehaviour.AutoIncrementWithReset);
        });
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void ChangeBehaviour_OnceSet_Exception() {
        b.Info.Flow();

        _ = Assert.Throws<InvalidOperationException>(() => {
            var sut = new VersionUnit("1");
            sut.SetBehaviour(DigitIncremementBehaviour.AutoIncrementWithReset);
            sut.Value = "Bannana";
        });
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void VersionUnit_FirstParamIsTheDigit() {
        var sut = new VersionUnit("1");
        Assert.Equal("1", sut.Value);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void VersionUnit_DefaultPostfix_IsCorrect() {
        var sut = new VersionUnit("1");
        Assert.Equal("", sut.PreFix);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void VersionUnit_SecondParameterChangesPostfix() {
        var sut = new VersionUnit("1", "X");
        Assert.Equal("X", sut.PreFix); //, "The prefix needs to be set by the constructor");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void Prefix_IsPrefixWhenSpecified() {
        var sut = new VersionUnit("5", "Monkey");
        Assert.Equal("Monkey5", sut.ToString()); //, "The prefix was not correctly specified in the ToSTring method");
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void VersionUnit_DefaultsToIncrementWithNumber() {
        var sut = new VersionUnit("1");
        Assert.Equal("1", sut.Value); //, "The value should default correctly");
        Assert.Equal(DigitIncremementBehaviour.Fixed, sut.Behaviour);
    }

    [Fact]
    [Trait(Traits.Age, Traits.Regression)]
    [Trait(Traits.Style, Traits.Unit)]
    public void VersionUnit_NonDigit_WorksFine() {
        var sut = new VersionUnit("monkey");
        Assert.Equal("monkey", sut.Value); //, "The value was not set correctly");
    }
}