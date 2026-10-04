using System.Collections.Generic;

namespace Versonify;

internal class ExecutionResult {
    public string[] Errors {
        get { return [.. AllErrors]; }
    }

    public ExitCodes ExitCode { get; set; }
    public bool WasProcessedSuccessfully { get; internal set; }
    protected List<string> AllErrors { get; set; } = [];

    internal void AddError(string errorMessage) {
        AllErrors.Add(errorMessage);
    }

    internal void AddError(string errorMessage, ExitCodes exit) {
        AllErrors.Add(errorMessage);
        ExitCode = exit;
    }
}
