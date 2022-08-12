using Newtonsoft.Json;
using System.IO;

namespace Plisky.CodeCraft {

    public class JsonVersionPersister : VersionStorage {

        public JsonVersionPersister(string initialisationValue)   {
            this.InitValue = new VersionStorageOptions() {
                InitialisationString = initialisationValue
            };
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
}