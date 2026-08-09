using System.Diagnostics;
using Plisky.Diagnostics;
using Plisky.Test;
using Shouldly;

namespace Versonify.ITest;

internal sealed class VersonifyExecutionOutput {
    public string Item1 { get; init; } = string.Empty;
    public int Item2 { get; init; }

    public static implicit operator string(VersonifyExecutionOutput output) {
        return output.Item1;
    }
}

internal sealed class VersonifyExecutionResult {
    public int ExitCode { get; init; }
    public string StdErr { get; init; } = string.Empty;
    public string StdOut { get; init; } = string.Empty;
}

public class TestHelper {
    protected Bilge b;
    private readonly UnitTestHelper uth;

    public TestHelper(UnitTestHelper unitTestHelper) {
        b = new Bilge("Versonify.TestHelper");
        uth = unitTestHelper;
    }

    public int LastExecutionExitCode { get; set; } = 0;
    protected static string? SolutionPathCache { get; set; } = null;
    protected static string? VersonifyPathCache { get; set; } = null;

    public static string? GetSolutionPath() {
        if (SolutionPathCache != null) {
            return SolutionPathCache;
        }
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && dir.GetFiles("PliskyVersioning.sln").Length == 0) {
            dir = dir.Parent;
        }
        SolutionPathCache = dir?.FullName ?? null;
        return SolutionPathCache;
    }

    public string GetVersonifyPath() => VersonifyPathCache ??= ResolveVersonifyPath();

    internal async Task<VersonifyExecutionOutput> ExecuteVersonify(string v, string? workingDirectory = null, bool appendDebug = true) {
        b.Info.Flow();
        var result = await ExecuteVersonifyWithStreams(v, workingDirectory, appendDebug);

        return new VersonifyExecutionOutput {
            Item1 = result.StdOut,
            Item2 = result.ExitCode,
        };
    }

    internal async Task<VersonifyExecutionResult> ExecuteVersonifyWithStreams(string v, string? workingDirectory = null, bool appendDebug = true) {
        b.Info.Flow();
        var psi = new ProcessStartInfo {
            FileName = GetVersonifyPath(),
            Arguments = appendDebug ? $"{v} --debug --trace=Verbose" : v,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        if (!string.IsNullOrEmpty(workingDirectory)) {
            psi.WorkingDirectory = workingDirectory;
        }

        b.Verbose.Log($"Starting versonfiy - {psi.Arguments}");
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

        b.Verbose.Log($"Complete - {p.ExitCode}", stdOut);
        return result;
    }

    private string ResolveVersonifyPath() {
        string? solutionPath = GetSolutionPath() ?? throw new FileNotFoundException($"Versonify executable not found Loc:{Directory.GetCurrentDirectory()}.");

        var testOutputDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        string currentTargetFramework = testOutputDirectory.Name;
        string? currentConfiguration = testOutputDirectory.Parent?.Name;
        if (string.IsNullOrWhiteSpace(currentConfiguration)) {
            throw new FileNotFoundException("Unable to resolve build configuration from test output path.");
        }

        string locatedPathToVersonify = Path.Combine(solutionPath, "Versonify", "bin", currentConfiguration, currentTargetFramework, "versonify.exe");
        b.Info.Log($"Versonify Path {locatedPathToVersonify}");
        if (!File.Exists(locatedPathToVersonify)) {
            throw new FileNotFoundException($"Executable not found. {locatedPathToVersonify}", locatedPathToVersonify);
        }

        return locatedPathToVersonify;
    }
}