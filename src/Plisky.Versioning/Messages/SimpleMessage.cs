namespace Plisky.Versioning;

public class SimpleMessage {
    public OutputMessageType MessageType { get; set; }
    public SimpleMessage(string content) {
        MessageType = OutputMessageType.UserInfo;
        Content = content;
    }

    public string Content { get; set; }
}