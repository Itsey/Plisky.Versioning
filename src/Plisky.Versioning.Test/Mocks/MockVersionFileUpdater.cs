namespace Plisky.CodeCraft.Test;

using System.Collections.Generic;
using Plisky.CodeCraft;

public class MockVersionFileUpdater : VersionFileUpdater {
    private readonly List<string> allFileSystemFiles = new();

    #region mocking implementation

    public Mocking mock;

    public class Mocking {
        private readonly MockVersionFileUpdater parent;

        public Mocking(MockVersionFileUpdater p) {
            parent = p;
        }

        public void Mock_MockingBird() {
        }

        public void AddFilesystemFile(string fname) {
            parent.allFileSystemFiles.Add(fname);
        }
        public bool ContainsFilesystemFile(string fname) {
            return parent.allFileSystemFiles.Contains(fname);
        }
    }

    #endregion mocking implementation

    public MockVersionFileUpdater() {
        mock = new Mocking(this);
    }
}