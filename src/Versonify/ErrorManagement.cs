namespace Versonify;

/// <summary>
/// Application exit code values, used as return values during error conditions.
/// </summary>
public enum ExitCodes {
    Ok = 0,
    ValidationFailVersionStorage = 1,
    ValidationInvalidDigitGroup = 2,
    ValidationDigitNotInDigitGroup = 3,
    NoRootPathPresent = 5,
    UpdateFileUpdatedNoFiles = 6,
    InvalidOrMissingMinMatch = 7,
    InvalidOrUnrecognisedCommand = 8,
    FailedToParseArguments = 11,
    FailedToValidateArguments = 12,
    UnknownFatalError = 100,
    VersionCheckInitialVersion = 200, // Austen release version check, prior to Austen an error was returned.
    BronteCompatibilityVersion = 201, // Bronte release version compatibility check.
}
