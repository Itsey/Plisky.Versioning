using System.Collections.Generic;

namespace Versonify {
    internal class ExeuctionResult {
        protected List<string> AllErrors { get; set; } = new List<string>();
        public bool WasProcessedSuccessfully { get; internal set; }
        public string[] Errors {
            get {
                return AllErrors.ToArray();
            }
        }

        public int ExitCode { get; set; }

        internal void AddError(string errorMessage) {
            AllErrors.Add(errorMessage);
        }
        internal void AddError(string errorMessage, int exit) {
            AllErrors.Add(errorMessage);
            ExitCode = exit;
        }
    }
}