namespace Plisky.CodeCraft;

using System.IO;
using System.Text;
using System.Text.Json;


public class NexusVersionPersister : VersionStorage {
    protected NexusSupport remote;
    protected NexusConfig config;

    public NexusVersionPersister(string initialisationValue) {
        this.InitValue = new VersionStorageOptions() {
            InitialisationString = initialisationValue
        };

        remote = new NexusSupport();
        config = remote.GetNexusSettings(initialisationValue);

        if (config == null) {
            StorageFailureMessage = $"Error >> The storage value passed as -VS for a Nexus repository could not be parsed as a valid Nexus repository.";
        }

    }

    protected override CompleteVersion ActualLoad() {
        CompleteVersion result = null;


        var tsk = remote.DownloadFileAsync(config.Url, (byteArray, fileName) => {

            string json = Encoding.UTF8.GetString(byteArray);
            result = JsonSerializer.Deserialize<CompleteVersion>(json);

        }, InitValue.InitialisationString, config.Username, config.Password);

        tsk.Wait();

        return result;
    }

    protected override void ActualPersist(CompleteVersion cv) {
        if (!IsValid) { return; }

        byte[] val = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cv));
        var t = remote.UploadFileAsync(new MemoryStream(val), config.Url, config.Username, config.Password);
        t.Wait();

    }
}