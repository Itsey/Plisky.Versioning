namespace Plisky.CodeCraft;

using System;

public class LogEventArgs : EventArgs {
    public string Severity { get; internal set; } = string.Empty;
    public string Text { get; internal set; } = string.Empty;
}