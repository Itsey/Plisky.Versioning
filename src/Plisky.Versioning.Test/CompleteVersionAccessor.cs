namespace Plisky.CodeCraft.Test;

public class CompleteVersionMock : CompleteVersion {

    #region mocking implementation

    public Mocking Mock { get; set; }

    public class Mocking(CompleteVersionMock p) {
        private readonly CompleteVersionMock parent = p;

        public string? ManipulateVersionBasedOnPattern(string pattern, string currentValue) {
            return parent.ManipulateValueBasedOnPattern(pattern, currentValue);
        }
    }

    #endregion mocking implementation

    public CompleteVersionMock() {
        Mock = new Mocking(this);
    }
}