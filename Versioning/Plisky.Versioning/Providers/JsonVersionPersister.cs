using Newtonsoft.Json;
using System.IO;

namespace Plisky.CodeCraft {

    public class JsonVersionPersister : VersionStorage {

        public JsonVersionPersister(string initialisationValue) : base(initialisationValue) {
        }

        protected override CompleteVersion ActualLoad() {
            if (File.Exists(InitValue)) {
                string txt = File.ReadAllText(InitValue);
                return JsonConvert.DeserializeObject<CompleteVersion>(txt);
            }
            return null;
        }

        protected override void ActualPersist(CompleteVersion cv) {
            string val = JsonConvert.SerializeObject(cv);
            File.WriteAllText(InitValue, val);
        }
    }
}