using System.Collections.Generic;

namespace Versonify;

/// <summary>
/// Simple Json formatting message for returning output to the caller in a more parsabl way.
/// </summary>
public record JsonOutputMessage {
    public JsonOutputMessage() {
    }

    public JsonOutputMessage(string messageContent, string messageLevel = "information") {
        MessageContent = messageContent;
        MessageCategory = messageLevel;
    }

    public string MessageCategory { get; set; } = "information";
    public string MessageContent { get; set; } = string.Empty;
    public Dictionary<string, string> Meta { get; set; } = [];
}