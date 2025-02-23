namespace Plisky.CodeCraft;

/// <summary>
/// The DigitIncremementBehaviour enum is the determining factor for how each element is incremented within the VersionSupport class.  Each element
/// of the version when incrememented will do so in a different way depending on its revision behaviour.
/// </summary>
public enum DigitIncremementBehaviour {

    /// <summary>
    /// Fixed values do not change.  They remain constant throuought a version increment.
    /// </summary>
    Fixed = 0,

    /// <summary>
    /// MajorDeterminesVersionNumber is a special format used to support multiple paralell branches.  The version number does not change and remains
    /// constant during increments, however the Major digit is also used to determine which version number to load.  It is invalid to set any digit
    /// other than the Major digit to this number.
    /// </summary>
    MajorDeterminesVersionNumber = 1,

    /// <summary>
    /// DaysSinceDate will reflect the number of days that have elapsed since the BaseDate
    /// </summary>
    DaysSinceDate = 2,

    /// <summary>
    /// DailyAutoIncrement will increment each time that the increment is called for the current build date.  Therefore multiple builds
    /// on the same day will have incrementing versions but the next day the number resets to zero.
    /// </summary>
    DailyAutoIncrement = 3,

    /// <summary>
    /// AutoIncrementWithReset will increment continually unless the next digit up has changed.  It therefore behaves exactly like continual
    /// increment for the major version part.  For the minor version part it will increment until the major changes then it will reset to zero.
    /// For the build the build will increment until the minor version changes. Finally for the revision it will increment until the build version
    /// changes.
    /// </summary>
    AutoIncrementWithReset = 4,

    /// <summary>
    /// AutoIncrementWithResetAny will increment continually unless any of the higher order digits have changed.  It therefore continually
    /// increments untill a more significant digit changes then it resets to zero.  Major digits will continally incrememnt.  Minor digits
    /// will continue to incrememnt until the major changes.  Build will continue to increment until either Major or Minor changes and finally
    /// the revision digit wil continue to increment until any of Major / Minor or Build changes.
    /// </summary>
    AutoIncrementWithResetAny = 5,

    /// <summary>
    /// Continual increment will increment non stop until an overflow occurs. When an overflow occurs the digit is reset to 0.
    /// </summary>
    ContinualIncrement = 6,

    /// <summary>
    /// This will return the number of whole or partial weeks since the base date.
    /// </summary>
    WeeksSinceDate = 7,


    /// <summary>
    /// Will set this digit to be the release name as specified in the version.  Release names can change during an increment but are not
    /// incremented or decremented as such.  They are set to literal strings.
    /// </summary>
    ReleaseName


};