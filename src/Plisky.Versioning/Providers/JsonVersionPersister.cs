namespace Plisky.CodeCraft;

using System.IO;
using System.Text.Json;

public class JsonVersionPersister : VersionStorage {

    public JsonVersionPersister(string initialisationValue) {
        b.Info.Flow();
        InitValue = new VersionStorageOptions() {
            InitialisationString = initialisationValue
        };

        if (!IsValidFileName(InitValue.InitialisationString)) {
            StorageFailureMessage = $"Error >> The storage value passed as -v could not be resolved as a valid network or disk path.";
        }
    }

    public bool IsValidFileName(string fileName) {
        b.Info.Flow(fileName);

        fileName = Path.GetFileName(fileName);

        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
            return false;
        }

        return true;
    }

    protected override bool ActualDoesVstoreExist(VersionStorageOptions? opts) {
        if (opts == null || string.IsNullOrWhiteSpace(opts.InitialisationString)) {
            return false;
        }
        return File.Exists(opts.InitialisationString);
    }

    protected override CompleteVersion ActualLoad() {
        if (InitValue == null) {
            return CompleteVersion.GetDefault();
        }

        if (File.Exists(InitValue.InitialisationString)) {
            string txt = File.ReadAllText(InitValue.InitialisationString);
            var cv = JsonSerializer.Deserialize<CompleteVersion>(txt);
            if (cv != null) {
                return cv;
            }
        }
        return CompleteVersion.GetDefault();
    }

    protected override void ActualPersist(CompleteVersion cv) {
        string val = JsonSerializer.Serialize(cv);
        if (InitValue != null) {
            File.WriteAllText(InitValue.InitialisationString, val);
        }
    }
}