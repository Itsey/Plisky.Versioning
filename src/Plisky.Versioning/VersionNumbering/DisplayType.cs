namespace Plisky.CodeCraft;

/// <summary>
/// Allows the version number to display in different formats, short version format, full version format or with no display.
/// </summary>
public enum DisplayType {
    Default = 0x0000,
    Short = 0x0001,
    Full = 0x0002,
    NoDisplay = 0x0003,
    ThreeDigit = 0x0004,
    Release = 0x5,
    FourDigitNumeric = 0x6,  // Added to support FileVersion and AssemblyVersion as this is recommended by Microsoft
    ThreeDigitNumeric = 0x7,
    QueuedFull = 0x8,
}