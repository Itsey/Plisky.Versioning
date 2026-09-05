using System.Collections.Generic;
using System.Linq;

namespace Plisky.CodeCraft.Test;

public class MockVersioning : Versioning {
    private readonly List<string> filenamesToFind = new List<string>();

    public MockVersioning(VersionStorage vs) : base(vs) {
        mock = new Mocking(this);
    }

    protected override IEnumerable<string> ActualGetFiles(string root) {
        return filenamesToFind;
    }

    #region mocking implementation

    public Mocking mock;

    public class Mocking(MockVersioning p) {
        private readonly MockVersioning parent = p;

        public void AddFilenameToFind(params string[] filenames) {
            parent.filenamesToFind.AddRange(filenames);
        }

        public static void Mock_MockingBird() {
        }

        public string[] ReturnNuspecEntries() {
            return [.. parent.filenamesRegistered.Where(x => (x.Item2 & FileUpdateType.Nuspec) == FileUpdateType.Nuspec).Select(f => f.Item1)];
        }

        internal string[] ReturnMinMatchers() {
            var result = new List<string>();
            foreach (var l in parent.fileUpdateMinmatchers.Keys) {
                result.AddRange(parent.fileUpdateMinmatchers[l]);
            }

            return [.. result];
        }

        internal string[] ReturnNetEntries() {
            return [.. parent.filenamesRegistered.Where(x => (x.Item2 & FileUpdateType.NetAssembly) == FileUpdateType.NetAssembly ||
                                                         (x.Item2 & FileUpdateType.NetFile) == FileUpdateType.NetFile ||
                                                         (x.Item2 & FileUpdateType.NetInformational) == FileUpdateType.NetInformational)
                                             .Select(f => f.Item1)];
        }

        internal string[] ReturnNetStdEntries() {
            return [.. parent.filenamesRegistered.Where(x => (x.Item2 & FileUpdateType.StdAssembly) == FileUpdateType.StdAssembly ||
                                                         (x.Item2 & FileUpdateType.StdFile) == FileUpdateType.StdFile ||
                                                         (x.Item2 & FileUpdateType.StdInformational) == FileUpdateType.StdInformational)
                                             .Select(f => f.Item1)];
        }

        internal string[] ReturnTextEntries() {
            return [.. parent.filenamesRegistered.Where(x => (x.Item2 & FileUpdateType.TextFile) == FileUpdateType.TextFile).Select(f => f.Item1)];
        }
    }

    #endregion mocking implementation
}