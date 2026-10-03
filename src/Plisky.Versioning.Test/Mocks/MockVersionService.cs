using System;
using System.Collections.Generic;

namespace Plisky.CodeCraft.Test;

public class MockVersionService : IKnowHowToVersion {
    private readonly Dictionary<string, Dictionary<string, VersionNumber>> store = [];

    public MockVersionService() {
        var dct = new Dictionary<string, VersionNumber> {
            { "default", new VersionNumber(900, 900, 900, 900) }
        };
        store.Add("default", dct);
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

    public void RegisterVersion(Guid g, string branch, VersionNumber? v) {

        #region entry code

        if (string.IsNullOrEmpty(branch)) {
            throw new ArgumentOutOfRangeException(nameof(branch), "branch must be specified, use 'default' if not known");
        }
        if (v is null) {
            throw new ArgumentException("Version number must be specified", nameof(v));
        }

        #endregion entry code

        string verIdent = g.ToString();

        if (!store.ContainsKey(verIdent)) {
            store.Add(verIdent, []);
        }
        if (!store[verIdent].ContainsKey(branch)) {
            store[verIdent].Add(branch, v);
        } else {
            store[verIdent][branch] = v;
        }
    }
}