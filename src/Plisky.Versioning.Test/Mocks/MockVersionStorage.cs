﻿namespace Plisky.CodeCraft.Test {

    internal class MockVersionStorage : VersionStorage {
        private string initialisationValue;
        private CompleteVersion loadedVersion;

        #region mocking implementation

        public Mocking mock;

        public class Mocking {
            private MockVersionStorage parent;

            public Mocking(MockVersionStorage p) {
                parent = p;
            }

            public void Mock_MockingBird() {
            }

            public void SetBehaviours(DigitIncrementBehaviour dig1, DigitIncrementBehaviour dig2, DigitIncrementBehaviour dig3, DigitIncrementBehaviour dig4) {
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

            mock = new Mocking(this);

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
        protected override bool ActualDoesVstoreExist(VersionStorageOptions? opts) {
            // For the mock, assume existence if the initialisation string is not null or empty and not 'invalid'.
            return opts != null && !string.IsNullOrWhiteSpace(opts.InitialisationString) && opts.InitialisationString != "invalid";
        }
    }
}