using System;

namespace Plisky.CodeCraft {
    public class PliskyApiVersionServices : VersionStorage {
        protected override void ActualPersist(CompleteVersion cv) {

            // Name of the version to update
            // Branch Name

            
        }

        protected override CompleteVersion ActualLoad() {
            return new CompleteVersion();
        }
    }
}
