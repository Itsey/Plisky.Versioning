namespace Plisky.CodeCraft;

using System.IO;
using Newtonsoft.Json;

public class JsonVersionPersister : VersionStorage {

    public static bool IsValidFileName(string fileName) {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        fileName = Path.GetFileName(fileName);
        foreach (char c in invalidChars) {
            if (fileName.Contains(c)) {
                return false;
            }
        }
        return true;
    }

    public JsonVersionPersister(string initialisationValue) {
        this.InitValue = new VersionStorageOptions() {
            InitialisationString = initialisationValue
        };

        if (!IsValidFileName(InitValue.InitialisationString)) {
            StorageFailureMessage = $"Error >> The storage value passed as -VS could not be resolved as a valid network or disk path.";
        }
    }

    protected override CompleteVersion ActualLoad() {
        if (InitValue == null) {
            return CompleteVersion.GetDefault();
        }

        if (File.Exists(InitValue.InitialisationString)) {
            string txt = File.ReadAllText(InitValue.InitialisationString);
            var cv = JsonConvert.DeserializeObject<CompleteVersion>(txt);
            if (cv != null) {
                return cv;
            }
        }
        return CompleteVersion.GetDefault();
    }

    protected override void ActualPersist(CompleteVersion cv) {
        string val = JsonConvert.SerializeObject(cv);
        File.WriteAllText(InitValue.InitialisationString, val);
    }
}