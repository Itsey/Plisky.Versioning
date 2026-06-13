using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;

namespace Versonify.ManualTest {
    internal class Program {

        [STAThread]
        static void Main(string[] args) {
            VerifyablePrompt.LoadPrompts();
            if (VerifyablePrompt.Prompts.Count != 4) {
                throw new InvalidOperationException($"Expected exactly 4 prompts loaded, but found {VerifyablePrompt.Prompts.Count}.");
            }

            Bilge.AddHandler(new TCPHandler(new TCPHandlerOptions("127.0.0.1", 9060, true)));

            var b = new Bilge("ManualTest");

            b.ActiveTraceLevel = System.Diagnostics.SourceLevels.Verbose;
            Bilge.Alert.Online("Versonify.ManualTest");

            b.Info.Flow();

            string temporaryVstore = Path.GetTempFileName();
            bool fne;

            try {
                foreach (var bp in VerifyablePrompt.Prompts) {
                    fne = GetAndExecutePrompt(temporaryVstore, bp);
                    Fail(!fne, $"Failure Prompt {bp.Name}.");
                }
            } finally {
                File.Delete(temporaryVstore);
            }

        }

        private static bool GetAndExecutePrompt(string filenameToken, VerifyablePrompt bp) {
            bp.VersionStore = filenameToken;
            string prompt = bp.Prompt;

            Clipboard.SetText(prompt);
            MessageBox.Show($"Execute AI Prompt {bp.Name}");

            bool result = File.Exists(filenameToken);
            Console.WriteLine($"Expected - Vstore File Exists {result}");

            return result;
        }

        private static void Fail(bool doFail, string why) {
            if (doFail) {
                Console.WriteLine(why);
                throw new Exception(why);
            }
        }
    }
}
