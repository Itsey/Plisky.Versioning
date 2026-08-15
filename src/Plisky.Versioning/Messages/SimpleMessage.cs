namespace Plisky.Versioning;

public class SimpleMessage {

    public SimpleMessage(string content) {
        Content = content;
    }

    public string Content { get; set; }
}