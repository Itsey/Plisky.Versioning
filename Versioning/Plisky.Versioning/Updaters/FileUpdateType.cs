namespace Plisky.CodeCraft {

    // If you add here also check the methods called:  CompleteVersion 

    public enum FileUpdateType {
        Assembly=0x0000,
        AssemblyInformational=0x0001,
        AssemblyFile=0x0002,
        Wix=0x0003,
        Nuspec=0x0004,
        NetStdAssembly = 0x0005,
        NetStdInformational = 0x0006,
        NetStdFile = 0x007
    }
}