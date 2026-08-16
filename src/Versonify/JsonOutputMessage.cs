namespace Versonify;

using System.Collections.Generic;

/// <summary>
/// Simple Json formatting message for returning output to the caller in a more parsabl way.
/// </summary>
public record JsonOutputMessage {
    public string MessageLevel { get; set; } = "information";
    public string MessageContent { get; set; } = string.Empty;
    public Dictionary<string, string> Meta { get; set; } = [];

    public JsonOutputMessage() {
    }

    public JsonOutputMessage(string messageContent, string messageLevel = "information") {
        MessageContent = messageContent;
        MessageLevel = messageLevel;
    }
}
