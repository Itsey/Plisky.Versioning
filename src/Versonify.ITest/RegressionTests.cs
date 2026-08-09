using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;

namespace Versonify.ITest;

public class RegressionTests {
    protected Bilge b = new("Versonify-ITest");
    protected TestHelper th;
    protected UnitTestHelper uth;

    public RegressionTests() {
        b.Info.Flow();

        uth = new UnitTestHelper();
        th = new TestHelper(uth);
    }

    [Fact(DisplayName = "Compatibility Level is 201")]
    public async Task Call_with_qq_returns_expected_compat_code() {
        b.Info.Flow();

        var output = await th.ExecuteVersonify("--qqpnf", appendDebug: false);
        output.Item2.ShouldBe(201, "Current compat version is 201.");
    }
}