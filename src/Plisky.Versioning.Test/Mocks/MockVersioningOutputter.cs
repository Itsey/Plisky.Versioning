using System.Collections.Generic;
using Plisky.Plumbing;

namespace Plisky.CodeCraft.Test;

public class MockVersioningOutputter(CompleteVersion v, Hub outey) : VersioningOutputter(v, outey) {
    protected List<string> outputReceived = [];

    public bool EnvWasSet { get; set; } = false;
    public bool FileWasWritten { get; set; } = false;

    public string[] OutputLines {
        get { return [.. outputReceived]; }
    }

    public string? WrittenToConsole { get; set; } = null;

    public string GetTheValueRequestedToWrite() {
        return ValToWrite;
    }

    protected override void SetEnvironmentWithValue() {
        EnvWasSet = true;
    }

    protected override void SetFileValue(string outputString) {
        FileWasWritten = true;
        RecordOutputReceived(outputString);
    }

    protected override void WriteToConsole(string outputString) {
        WrittenToConsole = outputString;
        RecordOutputReceived(outputString);
    }

    private void RecordOutputReceived(string outputString) {
        if (!string.IsNullOrEmpty(outputString)) {
            string[] lines = outputString.Split("\r\n");
            outputReceived.AddRange(lines);
        }
    }
}