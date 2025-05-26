using System.Diagnostics;
using FluentAssertions;
using Plisky.Diagnostics;

namespace Versonify.ITest;
public class Exploratory {
    protected Bilge b = new Bilge("Versonify-ITest");

    [Fact]
    public async Task Console_with_nuke_has_markers() {
        b.Info.Flow();

        var psi = new ProcessStartInfo();
        psi.FileName = @"X:\Code\ghub\Plisky.Versioning\src\Versonify\bin\Debug\net8.0\versonify.exe";
        psi.Arguments = "passive -vs=D:\\Scratch\\_build\\vstore\\mollycoddle-version.vstore -O=con-nf -Debug=v-** -Q=1.9.4.3";
        psi.RedirectStandardOutput = true;

        var p = Process.Start(psi);

        Assert.NotNull(p);

        string s = await p.StandardOutput.ReadToEndAsync();

        s.Should().Contain("PNFV]");
        await p.WaitForExitAsync();
        b.Info.Log("Std" + s);
        p.ExitCode.Should().Be(0);

    }


    [Fact]
    public async Task Console_does_not_have_nuke_markers() {
        b.Info.Flow();

        var psi = new ProcessStartInfo();
        psi.FileName = @"X:\Code\ghub\Plisky.Versioning\src\Versonify\bin\Debug\net8.0\versonify.exe";
        psi.Arguments = "passive -vs=D:\\Scratch\\_build\\vstore\\mollycoddle-version.vstore -O=con -Debug=v-** -Q=1.9.4.3";
        psi.RedirectStandardOutput = true;

        var p = Process.Start(psi);
        Assert.NotNull(p);

        string s = await p.StandardOutput.ReadToEndAsync();


        s.Should().NotContain("PNFV]");
        await p.WaitForExitAsync();
        b.Info.Log("Std" + s);
        p.ExitCode.Should().Be(0);

    }

    [Fact]
    public void Get_from_nexus_returns_release_name() {
    }
}
