using System;

namespace Plisky.CodeCraft {
    // If you add here also check the methods called:  CompleteVersion

    [Flags]
    public enum FileUpdateType {
        NetAssembly = 0x0001,
        NetInformational = 0x0002,
        NetFile = 0x0004,
        Wix = 0x0008,
        Nuspec = 0x0010,
        StdAssembly = 0x0020,
        StdInformational = 0x0040,
        StdFile = 0x0080,
        TextFile = 0x0100,

        AllNetFramework = NetAssembly | NetInformational | NetFile,
        AllNetStd = StdAssembly | StdInformational | StdFile,
    }
}