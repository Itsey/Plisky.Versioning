using System;

namespace Plisky.CodeCraft {
    public class LogEventArgs : EventArgs {
        public string Severity { get; internal set; }
        public string Text { get; internal set; }
    }
}