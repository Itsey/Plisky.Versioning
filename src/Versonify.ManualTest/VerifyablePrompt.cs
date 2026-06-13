using System;
using System.Collections.Generic;

namespace Versonify.ManualTest {
    public class VerifyablePrompt {
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
        public Func<bool> Verify { get; set; } = () => false;
        public string VersionStore { get; set; } = string.Empty;
        public bool PrefixWithVersonify { get; set; }

        public static List<VerifyablePrompt> Prompts { get; } = new List<VerifyablePrompt>();

        public static void AddPrompt(VerifyablePrompt prompt) {
            Prompts.Add(prompt);
        }

        public static void LoadPrompts() {
            AddPrompt(new VerifyablePrompt {
                Name = "1-CreateVstore",
                Prompt = "Create a Versonify VStore with the following details.  Version: 1.0.0.0, Name: testVstore Version Store Location: {0}",
                PrefixWithVersonify = true
            });
            AddPrompt(new VerifyablePrompt {
                Name = "2-QueryVstore",
                Prompt = "What is the next version number using the vstore {0}",
                PrefixWithVersonify = true
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
