using System.Diagnostics;
using Plisky.Test;
using Shouldly;

namespace Versonify.ITest;

internal sealed class VersonifyExecutionResult {
    public string StdOut { get; init; } = string.Empty;
    public string StdErr { get; init; } = string.Empty;
    public int ExitCode { get; init; }
}

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

    internal async Task<string> ExecuteVersonify(string v, string? workingDirectory = null) {
        var result = await ExecuteVersonifyWithStreams(v, workingDirectory);
        return result.StdOut;
    }

    internal async Task<VersonifyExecutionResult> ExecuteVersonifyWithStreams(string v, string? workingDirectory = null) {
        var psi = new ProcessStartInfo();
        psi.FileName = GetVersonifyPath();
        psi.Arguments = v;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        if (!string.IsNullOrEmpty(workingDirectory)) {
            psi.WorkingDirectory = workingDirectory;
        }

        var p = Process.Start(psi);

        p.ShouldNotBeNull();

        var stdOutReadTask = p.StandardOutput.ReadToEndAsync();
        var stdErrReadTask = p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();
        string stdOut = await stdOutReadTask;
        string stdErr = await stdErrReadTask;
        LastExecutionExitCode = p.ExitCode;

        var result = new VersonifyExecutionResult {
            StdOut = stdOut,
            StdErr = stdErr,
            ExitCode = p.ExitCode,
        };
        return result;
    }
}