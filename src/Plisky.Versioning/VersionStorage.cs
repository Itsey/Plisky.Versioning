using Plisky.Diagnostics;

namespace Plisky.CodeCraft {

    public abstract class VersionStorage {
        protected Bilge b = new Bilge("Plisky-Versioning");

        protected VersionStorageOptions InitValue = null;

        protected abstract void ActualPersist(CompleteVersion cv);

        protected abstract CompleteVersion ActualLoad();

        protected VersionStorage() {
            InitValue = null;    
        }

        /// <summary>
        /// Manages the storage of version numbers, allowing them to be saved and loaded.
        /// </summary>
        /// <param name="opts">Initialisation options for the underlying system.</param>
        public VersionStorage(VersionStorageOptions opts) : this() {
            InitValue = opts;
        }


        /// <summary>
        /// Called after the initialisation is set this can be used to validate whether the initialisation data was correct for the given version store, default
        /// implementation simply returns true.
        /// </summary>
        /// <returns></returns>
        public virtual bool ValidateInitialisation() {
            return true;
        }

        /// <summary>
        /// Saves the complete version to the underlying storage system.  Where the underlying storage system faults then this error will be passed
        /// up and it should be assumed that the save has not succeeded.
        /// </summary>
        /// <param name="cv">The CompleteVerison to save to the storage system.</param>
        public void Persist(CompleteVersion cv) {
            ActualPersist(cv);
        }

        /// <summary>
        /// Gets the version from storage, if the storage is not initialised then will return a default version.  If the underlying storage throws
        /// an error then this will be passed up to the caller.
        /// </summary>
        /// <returns>Version, or DefaultVersion where storage has not been used yet.</returns>
        public CompleteVersion GetVersion() {
            var loaded = ActualLoad();
            if (loaded == null) {
                loaded = CompleteVersion.GetDefault();
            }
            return loaded;
        }
    }
}