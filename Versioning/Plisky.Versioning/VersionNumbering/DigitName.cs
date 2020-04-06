namespace Plisky.CodeCraft {
    /// <summary>
    /// The names given to each of the digits that make up an assembly version number.
    /// </summary>
    public enum DigitName {
        /// <summary>
        /// The major digit is the first of the four.  X.0.0.0
        /// </summary>
        Major = 0x0000,
        /// <summary>
        /// The minor digit is the second of the four.  0.X.0.0
        /// </summary>
        Minor = 0x0001,
        /// <summary>
        /// The build digit is the third of the four.  0.0.X.0
        /// </summary>
        Build = 0x0002,
        /// <summary>
        /// The revision digit is the final one.  0.0.0.X
        /// </summary>
        Revision = 0x003
    }
}
