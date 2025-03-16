namespace Plisky.CodeCraft;

using System;

[Flags]
public enum OutputPossibilities {
    None = 0x00000000,
    Environment = 0x00000001,
    File = 0x00000002,
    Console = 0x00000004,
    NukeFusion = 0x00000008
}