using System.Diagnostics;
using Plisky.Test;

namespace Versonify.ITest;

public class TestHelper {
    private UnitTestHelper uth;

    public TestHelper(UnitTestHelper uth) {
        this.uth = uth;
    }

    protected static string? SolutionPathCache { get; set; } = null;
    protected static string? VersonifyPathCache { get; set; } = null;

    public string? GetSolutionPath() {
        if (SolutionPathCache != null) {
            return SolutionPathCache;
        }
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !dir.GetFiles("PliskyVersioning.sln").Any()) {
            dir = dir.Parent;
        }
        SolutionPathCache = dir?.FullName ?? null;
        return SolutionPathCache;
    }

    public string GetVersonifyPath() {
        if (VersonifyPathCache != null) {
            return VersonifyPathCache;
        }

        string? solutionPath = GetSolutionPath();
        if (solutionPath == null) {
            throw new FileNotFoundException($"Versonify executable not found Loc:{Directory.GetCurrentDirectory()}.");
        }
        string locatedPathToVersonify = Path.Combine(solutionPath, @"Versonify\bin\Debug\net9.0\versonify.exe");
        if (!File.Exists(locatedPathToVersonify)) {
            throw new FileNotFoundException($"Executable not found. {locatedPathToVersonify}", locatedPathToVersonify);
        }
        VersonifyPathCache = locatedPathToVersonify;
        return locatedPathToVersonify;
    }

    public int LastExecutionExitCode { get; set; } = 0;

    internal async Task<string> ExecuteVersonify(string v) {
        var psi = new ProcessStartInfo();
        psi.FileName = GetVersonifyPath();
        psi.Arguments = v;
        psi.RedirectStandardOutput = true;

        var p = Process.Start(psi);

        Assert.NotNull(p);

        string s = await p.StandardOutput.ReadToEndAsync();
        await p.WaitForExitAsync();
        LastExecutionExitCode = p.ExitCode;
        return s;
    }
}