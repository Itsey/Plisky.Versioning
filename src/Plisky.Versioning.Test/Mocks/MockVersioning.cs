using System.Collections.Generic;
using System.Linq;

namespace Plisky.CodeCraft.Test {

    public class MockVersioning : Versioning {

        private List<string> filenamesToFind = new List<string>();

        protected override IEnumerable<string> ActualGetFiles(string root) {
            return filenamesToFind;
        }

        #region mocking implementation

        public Mocking mock;

        public class Mocking {
            private MockVersioning parent;

            public Mocking(MockVersioning p) {
                parent = p;
            }

            public void Mock_MockingBird() {
            }

            public string[] ReturnNuspecEntries() {
                return parent.filenamesRegistered.Where(x => (x.Item2 & FileUpdateType.Nuspec) == FileUpdateType.Nuspec)
                                                 .Select(f => f.Item1).ToArray();
            }

            internal string[] ReturnNetStdEntries() {
                return parent.filenamesRegistered.Where(x => (x.Item2 & FileUpdateType.StdAssembly) == FileUpdateType.StdAssembly ||
                                                             (x.Item2 & FileUpdateType.StdFile) == FileUpdateType.StdFile ||
                                                             (x.Item2 & FileUpdateType.StdInformational) == FileUpdateType.StdInformational)
                                                 .Select(f => f.Item1).ToArray();
            }

            internal string[] ReturnTextEntries() {
                return parent.filenamesRegistered.Where(x => (x.Item2 & FileUpdateType.TextFile) == FileUpdateType.TextFile)
                                                 .Select(f => f.Item1).ToArray();
            }

            internal string[] ReturnNetEntries() {
                return parent.filenamesRegistered.Where(x => (x.Item2 & FileUpdateType.NetAssembly) == FileUpdateType.NetAssembly ||
                                                             (x.Item2 & FileUpdateType.NetFile) == FileUpdateType.NetFile ||
                                                             (x.Item2 & FileUpdateType.NetInformational) == FileUpdateType.NetInformational)
                                                 .Select(f => f.Item1).ToArray();
            }

            internal string[] ReturnMinMatchers() {
                var result = new List<string>();
                foreach (var l in parent.fileUpdateMinmatchers.Keys) {
                    result.AddRange(parent.fileUpdateMinmatchers[l]);
                }

                return result.ToArray();
            }

            public void AddFilenameToFind(params string[] filenames) {
                parent.filenamesToFind.AddRange(filenames);
            }
        }

        #endregion mocking implementation



        public MockVersioning(VersionStorage vs) : base(vs) {
            mock = new Mocking(this);
        }
    }
}