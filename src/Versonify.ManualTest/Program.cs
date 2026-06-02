using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;

namespace Versonify.ManualTest {
    internal class Program {

        [STAThread]
        static void Main(string[] args) {
            const int PROMPTMAX = 2;

            Bilge.AddHandler(new TCPHandler(new TCPHandlerOptions("127.0.0.1", 9060, true)));

            var b = new Bilge("ManualTest");

            b.ActiveTraceLevel = System.Diagnostics.SourceLevels.Verbose;
            Bilge.Alert.Online("Versonify.ManualTest");

            b.Info.Flow();

            string temporaryVstore = Path.GetTempFileName();


            int promptNo;
            bool fne;

            try {

                for (int i = 1; i <= PROMPTMAX; i++) {
                    promptNo = i;
                    fne = GetAndExecutePrompt(temporaryVstore, promptNo);
                    Fail(!fne, $"Failure Prompt {promptNo}.");
                }

            } finally {
                File.Delete(temporaryVstore);
            }

        }

        private static bool GetAndExecutePrompt(string filenameToken, int promptNumber) {
            string prompt = GetPrompt(promptNumber);

            prompt = string.Format(prompt, filenameToken, promptNumber);
            Clipboard.SetText(prompt);
            MessageBox.Show($"Execute AI Prompt {promptNumber}");

            bool result = File.Exists(filenameToken);
            Console.WriteLine($"Expected - Vstore File Exists {result}");


            return result;

        }

        private static string GetPrompt(int number) {
            string allText = GetPromptText();

            string[] lines = allText.Split('\n');
            string startMarker = $"#{number}";
            bool capturing = false;
            var result = new System.Text.StringBuilder();

            foreach (string line in lines) {
                string trimmed = line.TrimEnd('\r');
                if (trimmed == startMarker) {
                    capturing = true;
                    continue;
                }
                if (capturing) {
                    if (trimmed.StartsWith('#')) {
                        break;
                    }
                    result.AppendLine(trimmed);
                }
            }

            return result.ToString().Trim();
        }

        private static string GetPromptText() {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                .Single(n => n.EndsWith("Prompts.txt", StringComparison.OrdinalIgnoreCase));
            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            string allText = reader.ReadToEnd();
            return allText;
        }

        private static void Fail(bool doFail, string why) {
            if (doFail) {
                Console.WriteLine(why);
                throw new Exception(why);
            }
        }
    }
}
