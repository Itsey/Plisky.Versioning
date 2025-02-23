namespace Plisky.CodeCraft.Test {

    internal class MockVersionStorage : VersionStorage {
        private string initialisationValue;
        private CompleteVersion loadedVersion;

        #region mocking implementation

        public Mocking Mock;

        public class Mocking {
            private MockVersionStorage parent;

            public Mocking(MockVersionStorage p) {
                parent = p;
            }

            public void Mock_MockingBird() {
            }

            public void SetBehaviours(DigitIncremementBehaviour dig1, DigitIncremementBehaviour dig2, DigitIncremementBehaviour dig3, DigitIncremementBehaviour dig4) {
                parent.loadedVersion.Digits[0].SetBehaviour(dig1);
                parent.loadedVersion.Digits[1].SetBehaviour(dig2);
                parent.loadedVersion.Digits[2].SetBehaviour(dig3);
                parent.loadedVersion.Digits[3].SetBehaviour(dig4);
            }
        }

        #endregion mocking implementation

        public MockVersionStorage(string initValue) {
            this.InitValue = new VersionStorageOptions() {
                InitialisationString = initValue
            };

            Mock = new Mocking(this);

            initialisationValue = initValue;
        }

        public bool PersistWasCalled { get; private set; }
        public string VersionStringPersisted { get; private set; }

        protected override CompleteVersion ActualLoad() {
            loadedVersion = initialisationValue == "default" ? null : new CompleteVersion(initialisationValue);

            return loadedVersion;
        }

        protected override void ActualPersist(CompleteVersion cv) {
            PersistWasCalled = true;
            VersionStringPersisted = cv.GetVersionString();
        }
    }
}