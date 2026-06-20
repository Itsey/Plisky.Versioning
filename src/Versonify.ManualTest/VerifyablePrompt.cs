using Plisky.Diagnostics;

namespace Versonify.ManualTest {
    public class VerifyablePrompt {
        protected Bilge b = new Bilge("VerifyablePrompt");
        public string Name { get; set; } = string.Empty;
        private string promptText = string.Empty;
        public string Prompt {
            get {
                string baseText = promptText;
                if (!string.IsNullOrEmpty(VersionStore) && promptText.Contains("{0}")) {
                    baseText = string.Format(promptText, VersionStore);
                }
                if (PrefixWithVersonify) {
                    return "/versonify " + baseText;
                }
                return baseText;
            }
            set {
                promptText = value;
            }
        }
        public Func<string, bool> Verify { get; set; } = (_) => false;
        public string VersionStore { get; set; } = string.Empty;
        public bool PrefixWithVersonify { get; set; }

        public List<VerifyablePrompt> Prompts { get; } = new List<VerifyablePrompt>();

        public void AddPrompt(VerifyablePrompt prompt) {
            Prompts.Add(prompt);
        }

        public void LoadPrompts() {
            AddPrompt(new VerifyablePrompt {
                Name = "1-CreateVstore",
                Prompt = "Create a Versonify VStore with the following details.  Version: 1.0.0.0, Name: testVstore Version Store Location: {0}",
                PrefixWithVersonify = true,
                Verify = (vstorePath) => {
                    b.Info.Log($"Prompt1 - VstoreExists check for {vstorePath}");
                    if (string.IsNullOrEmpty(vstorePath) || !File.Exists(vstorePath)) {
                        return false;
                    }
                    return true;
                }
            });
            AddPrompt(new VerifyablePrompt {
                Name = "2-QueryVstore",
                Prompt = "What is the next version number using the vstore {0}.  Copy the version number to the clipboard",
                PrefixWithVersonify = true,
                Verify = (vstorePath) => {
                    b.Info.Log($"Prompt2 - VstoreExists check for 1.0.0.0");

                    return true;
                }

            });
            AddPrompt(new VerifyablePrompt {
                Name = "3-QueueVersion",
                Prompt = "Queue up a new version for the vstore {0} with the following details.  Version:9.9.9.9",
                PrefixWithVersonify = true
            });
            AddPrompt(new VerifyablePrompt {
                Name = "4-IncrementMajor",
                Prompt = "Make the next increment update the Major digit.",
                PrefixWithVersonify = true
            });
        }
    }
}
