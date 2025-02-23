namespace Plisky.CodeCraft;

using System.IO;
using Newtonsoft.Json;

public class JsonVersionPersister : VersionStorage {

    public JsonVersionPersister(string initialisationValue) {
        this.InitValue = new VersionStorageOptions() {
            InitialisationString = initialisationValue
        };

        if (!File.Exists(InitValue.InitialisationString)) {
            StorageFailureMessage = $"Error >> The storage value passed as -VS could not be resolved as a valid network or disk path.";
        }
    }

    protected override CompleteVersion ActualLoad() {
        if (File.Exists(InitValue.InitialisationString)) {
            string txt = File.ReadAllText(InitValue.InitialisationString);
            return JsonConvert.DeserializeObject<CompleteVersion>(txt);
        }
        return null;
    }

    protected override void ActualPersist(CompleteVersion cv) {
        string val = JsonConvert.SerializeObject(cv);
        File.WriteAllText(InitValue.InitialisationString, val);
    }
}