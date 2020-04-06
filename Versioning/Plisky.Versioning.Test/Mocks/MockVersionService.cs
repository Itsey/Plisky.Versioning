using System;
using System.Collections.Generic;
using Plisky.CodeCraft;

namespace Plisky.CodeCraft.Test {
    public class MockVersionService : IKnowHowToVersion {
        private Dictionary<string, Dictionary<string, VersionNumber>> store = new Dictionary<string, Dictionary<string, VersionNumber>>();

        public MockVersionService() {
            var dct = new Dictionary<string, VersionNumber>();
            dct.Add("default", new VersionNumber(900, 900, 900, 900));
            store.Add("default",dct );
        }

        public VersionNumber GetBuildVersionNumberAfterIncrement(string g, string branchIndicator) {
            return store[g][branchIndicator];
        }

        public VersionNumber GetBuildVersionNumberWithoutIncrement(string g, string branchIndicator) {
            return store[g][branchIndicator];
        }

        public VersionNumber GetCompatibilityVersionNumberAfterIncrement(string g, string branchIndicator) {
            return store[g][branchIndicator];
        }

        public VersionNumber GetCompatibilityVersionNumberWithoutIncrement(string g, string branchIndicator) {
            return store[g][branchIndicator];
        }

        

        public void RegisterVersion(Guid g,string branch, VersionNumber v) {
            #region entry code
            if (string.IsNullOrEmpty(branch)) {
                throw new ArgumentOutOfRangeException(nameof(branch), "branch must be specified, use 'default' if not known");
            }
            if (v == null) {
                throw new ArgumentNullException(nameof(v));
            }
            #endregion

            var verIdent = g.ToString();

            if (!store.ContainsKey(verIdent)) {
                store.Add(verIdent, new Dictionary<string, VersionNumber>());                
            }
            if (!store[verIdent].ContainsKey(branch)) {
                store[verIdent].Add(branch, v);
            } else {
                store[verIdent][branch] = v;
            }
        }
    }
}