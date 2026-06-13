namespace Plisky.CodeCraft;

using System.IO;
using System.Text;
using System.Text.Json;


public class NexusVersionPersister : VersionStorage {
    protected NexusSupport remote;
    protected NexusConfig? config;

    public NexusVersionPersister(string initialisationValue) {
        InitValue = new VersionStorageOptions() {
            InitialisationString = initialisationValue
        };

        remote = new NexusSupport();
        config = remote.GetNexusSettings(initialisationValue);

        if (config == null) {
            StorageFailureMessage = $"Error >> The storage value passed as -VS for a Nexus repository could not be parsed as a valid Nexus repository.";
        }

    }

    protected override CompleteVersion ActualLoad() {
        var result = CompleteVersion.GetDefault();

        if (config != null && InitValue != null) {
            CompleteVersion? downloadedResult = null;
            var tsk = remote.DownloadFileAsync(config.Url, (byteArray, fileName) => {

                string json = Encoding.UTF8.GetString(byteArray);
                downloadedResult = JsonSerializer.Deserialize<CompleteVersion>(json);

            }, InitValue.InitialisationString, config.Username, config.Password);

            tsk.Wait();
            
            if (downloadedResult != null) {
                result = downloadedResult;
            }
        }

        return result;
    }

    protected override void ActualPersist(CompleteVersion cv) {
        if (!IsValid || config == null || InitValue == null) { return; }

        byte[] val = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cv));
        var t = remote.UploadFileAsync(new MemoryStream(val), config.Url, config.Username, config.Password);
        t.Wait();

    }
    protected override bool ActualDoesVstoreExist(VersionStorageOptions? opts) {
        if (opts == null || string.IsNullOrWhiteSpace(opts.InitialisationString) || config == null) {
            return false;
        }
        var existsTask = remote.FileExistsAsync(config.Url, config.Username, config.Password);
        existsTask.Wait();
        return existsTask.Result;
    }
}