namespace Plisky.CodeCraft;
using System.IO;
using Plisky.Diagnostics;

public abstract class VersionStorage {
    protected Bilge b = new Bilge("Plisky-Versioning");

    protected VersionStorageOptions? InitValue { get; set; } = null;

    protected abstract void ActualPersist(CompleteVersion cv);

    protected abstract CompleteVersion ActualLoad();

    public bool IsValid {
        get {
            return string.IsNullOrEmpty(StorageFailureMessage);
        }
    }

    public string? StorageFailureMessage { get; set; }
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
        if (!IsValid) { return; }
        ActualPersist(cv);
    }

    /// <summary>
    /// Gets the version from storage, if the storage is not initialised then will return a default version.  If the underlying storage throws
    /// an error then this will be passed up to the caller.
    /// </summary>
    /// <returns>Version, or DefaultVersion where storage has not been used yet.</returns>
    public CompleteVersion GetVersion() {
        var result = CompleteVersion.GetDefault();
        if (!IsValid) { return result; }

        result = ActualLoad();
        if (result == null) {
            result = CompleteVersion.GetDefault();
        }
        return result;
    }

    /// <summary>
    /// Checks if the InitValue is a file path that already exists on disk.
    /// </summary>
    /// <returns>True if InitValue is a valid, existing file path; otherwise, false.</returns>
    public bool DoesVstoreExist() {
        if (InitValue == null || string.IsNullOrWhiteSpace(InitValue.InitialisationString)) {
            return false;
        }
        string path = InitValue.InitialisationString;
        return File.Exists(path);
    }



    public static VersionStorage CreateFromInitialisation(string vpv) {
        VersionStorage result;

        if (vpv.Length > 7 && vpv.Substring(0, 7).ToUpperInvariant().StartsWith("[NEXUS]")) {
            result = new NexusVersionPersister(vpv);
        } else {
            result = new JsonVersionPersister(vpv);
        }

        return result;
    }
}