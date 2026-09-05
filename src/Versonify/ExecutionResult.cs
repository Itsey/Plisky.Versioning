using System.Collections.Generic;

namespace Versonify;

internal class ExecutionResult {

    public string[] Errors {
        get {
            return [.. AllErrors];
        }
    }

    public int ExitCode { get; set; }
    public bool WasProcessedSuccessfully { get; internal set; }
    protected List<string> AllErrors { get; set; } = [];

    internal void AddError(string errorMessage) {
        AllErrors.Add(errorMessage);
    }

    internal void AddError(string errorMessage, int exit) {
        AllErrors.Add(errorMessage);
        ExitCode = exit;
    }
}