namespace Plisky.CodeCraft.Test {

    public class CompleteVersionMock : CompleteVersion {

        #region mocking implementation

        public Mocking Mock;

        public class Mocking {
            private CompleteVersionMock parent;

            public Mocking(CompleteVersionMock p) {
                parent = p;
            }

            public string ManipulateVerisonBasedOnPattern(string pattern, string currentValue) {
                return parent.ManipulateValueBasedOnPattern(pattern, currentValue);
            }
        }

        #endregion mocking implementation

        public CompleteVersionMock() {
            Mock = new Mocking(this);
        }
    }
}