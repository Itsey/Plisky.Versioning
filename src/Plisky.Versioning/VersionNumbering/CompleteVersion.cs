namespace Plisky.CodeCraft;

using System;
using System.Collections.Generic;
using System.Text;
using Plisky.Diagnostics;

public class CompleteVersion {
    protected Bilge b = new Bilge("Plisky-Versioning");

    private string? actualReleaseName;
    private string? pendingReleaseName;
    private const string ALLDIGITSWILDCARD = "*";

    /// <summary>
    /// Returns the default, empty, version instance which is four digits and all fixed except the
    /// build digit which is set to autoincrementresetnany.
    /// </summary>
    /// <returns></returns>
    public static CompleteVersion GetDefault() {
        return new CompleteVersion(
            new VersionUnit("0"),
            new VersionUnit("0", "."),
            new VersionUnit("0", ".", DigitIncremementBehaviour.AutoIncrementWithResetAny),
            new VersionUnit("0", ".")
        ) {
            IsDefault = true
        };
    }

    public VersionUnit[] Digits { get; set; }

    public Dictionary<FileUpdateType, DisplayType> DisplayTypes { get; set; } = new Dictionary<FileUpdateType, DisplayType>();

    public CompleteVersion() {
        DisplayTypes.Add(FileUpdateType.NetAssembly, DisplayType.Short);
        DisplayTypes.Add(FileUpdateType.NetFile, DisplayType.Full);
        DisplayTypes.Add(FileUpdateType.NetInformational, DisplayType.Full);
        DisplayTypes.Add(FileUpdateType.Wix, DisplayType.Full);
        DisplayTypes.Add(FileUpdateType.Nuspec, DisplayType.ThreeDigit);
        DisplayTypes.Add(FileUpdateType.StdAssembly, DisplayType.Short);
        DisplayTypes.Add(FileUpdateType.StdFile, DisplayType.Full);
        DisplayTypes.Add(FileUpdateType.StdInformational, DisplayType.Full);
        DisplayTypes.Add(FileUpdateType.TextFile, DisplayType.Short);
    }



    public bool IsDefault { get; set; }
    public string? ReleaseName {
        get {
            return actualReleaseName;
        }
        set {
            actualReleaseName = value;
            foreach (var l in Digits) {
                if (l.Behaviour == DigitIncremementBehaviour.ReleaseName) {
                    l.Value = actualReleaseName;
                }
            }
        }
    }

    public void SetDisplayTypeForVersion(FileUpdateType fut, DisplayType dt) {
        DisplayTypes[fut] = dt;
    }

    public DisplayType GetDisplayType(FileUpdateType fut, DisplayType dt = DisplayType.Default) {
        if (dt != DisplayType.Default) { return dt; }
        return DisplayTypes[fut];
    }

    public CompleteVersion(params VersionUnit[] versionDigits) : this() {
        Digits = versionDigits;
    }

    public CompleteVersion(string initialValue) : this() {
        b.Verbose.Log($"Parsing Initialisation String [{initialValue}]");
        if (initialValue.Contains(".")) {
            string[] parse = initialValue.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var vd = new List<VersionUnit>();
            string prefix = "";
            foreach (string f in parse) {
                vd.Add(new VersionUnit(f, prefix, DigitIncremementBehaviour.Fixed));
                prefix = ".";
            }

            Digits = vd.ToArray();
        } else {
            Digits = new VersionUnit[1];
            Digits[0] = new VersionUnit(initialValue);
        }
    }

    /// <summary>
    /// Apply a version change that is to take effect next time the increment is called . Pending versions are based on a pattern where
    /// + indicates an integer increment, - indicates an integer decrement, nothing indicates no change and any combination of numbers
    /// represent a fixed new number and any combination of letters represents a fixed new phrase.
    /// </summary>
    /// <param name="pendingPattern">Pattern [val].[val].[val].[val]</param>
    public void ApplyPendingVersion(string pendingPattern) {
        string[] changes = pendingPattern.Split('.');

        for (int i = 0; i < changes.Length; i++) {
            if (Digits.Length > i) {
                string? currentDigitValue = Digits[i].Value;
                string? newDigitValue = ManipulateValueBasedOnPattern(changes[i], currentDigitValue);

                // Only set this if it is not null, this allows for stacking patterns to work.  
                if (newDigitValue != null) {
                    Digits[i].IncrementOverride = newDigitValue;
                }
            }
        }
    }

    protected string? ManipulateValueBasedOnPattern(string pattern, string? currentValue) {
        if (string.IsNullOrEmpty(pattern)) { return null; }

        if (int.TryParse(currentValue, out int currentInteger)) {
            if (pattern == "+") {
                return (++currentInteger).ToString();
            } else if (pattern == "-") {
                return (--currentInteger).ToString();
            }
        } else {
            if (int.TryParse(pattern, out int patternAsInt)) {
                return patternAsInt.ToString();
            }
        }

        // Fallthrough - pattern is a version name.
        return pattern;
    }

    public string GetVersionString(DisplayType dt = DisplayType.Full) {
        string result = string.Empty;
        int stopPoint = Digits.Length;
        if ((dt == DisplayType.Short) && (Digits.Length > 2)) {
            stopPoint = 2;
        }
        if ((dt == DisplayType.ThreeDigit) && (Digits.Length > 3)) {
            stopPoint = 3;
        }

        for (int i = 0; i < stopPoint; i++) {
            result += Digits[i].ToString();
        }
        return result;
    }

    public override string ToString() {
        return GetVersionString(DisplayType.Full);
    }

    public string GetBehaviourString(string digitRequested) {
        string result;
        if (digitRequested == ALLDIGITSWILDCARD) {
            var sb = new StringBuilder();
            for (int i = 0; i < Digits.Length; i++) {
                sb.AppendLine($"[{i}]:{Digits[i].Behaviour}({(int)Digits[i].Behaviour})");
            }
            result = sb.ToString().Trim();
        } else {
            int digitIndex = int.Parse(digitRequested);
            result = $"[{digitIndex}]:{Digits[digitIndex].Behaviour}({(int)Digits[digitIndex].Behaviour})";
        }
        return result;
    }

    public void Increment() {

        bool lastChanged = false;
        bool anyChanged = false;
        var t1 = DateTime.Now;


        if (pendingReleaseName != null) {
            ReleaseName = pendingReleaseName;
            pendingReleaseName = null;
        }

        b.Verbose.Log("Incrementing Version.", ReleaseName);

        foreach (var un in Digits) {
            string tmp = un.Value;
            lastChanged = un.PerformIncrement(lastChanged, anyChanged, t1, t1);
            b.Verbose.Log($"{tmp}>{un.Value} using {un.Behaviour}");
            if (lastChanged) { anyChanged = true; }
        }
    }

    public void ApplyPendingRelease(string newReleaseName) {
        pendingReleaseName = newReleaseName;
    }
    public bool ValidateDigitOptions(string[] digitsRequested) {
        bool isValid = false;

        if (digitsRequested is null || digitsRequested.Length == 0) {
            Console.WriteLine("Error >> No digit specified, please use the -DG option.");
            return isValid;
        }
        b.Verbose.Log($"Validating Digit Options [{string.Join(",", digitsRequested)}]");

        foreach (string d in digitsRequested) {
            if (string.IsNullOrWhiteSpace(d)) {
                continue;
            }
            if (d.Equals(ALLDIGITSWILDCARD) || (int.TryParse(d, out int result) && result >= 0 && result <= Digits.Length)) {
                b.Verbose.Log($"Digit [{d}] is valid.");
                isValid = true;
            } else {
                throw new ArgumentOutOfRangeException(nameof(digitsRequested), $"The digit [{d}] is not a valid digit. It must be a positive integer or '*' (all digits).");
            }
        }
        return isValid;
    }
}