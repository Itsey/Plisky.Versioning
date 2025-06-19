namespace Plisky.CodeCraft;

using System;
using Plisky.Diagnostics;

public class VersionUnit {
    public DigitIncrementBehaviour Behaviour { get; set; }
    private Bilge b = new Bilge("Plisky-VersionUnit");
    private const int DAYS_IN_A_WEEK = 7;

    private string? actualValue = null;

    public string? IncrementOverride { get; set; }

    public string? Value {
        get { return actualValue; }
        set { actualValue = value; ValidateForBehaviour(); }
    }

    public string PreFix { get; set; }

    public VersionUnit() : this(string.Empty, string.Empty, DigitIncrementBehaviour.Fixed) {
    }

    public VersionUnit(string v) : this(v, string.Empty) {
    }

    public VersionUnit(string versionValue, string versionPrefix, DigitIncrementBehaviour beh = DigitIncrementBehaviour.Fixed) {
        this.Value = versionValue;
        this.PreFix = versionPrefix;
        SetBehaviour(beh);
    }

    /// <summary>
    /// Modifies the versioning digit using the behaviours rule and information as to whether the next most significant digit
    /// has changed.
    /// </summary>
    /// <param name="higherDigitChanged">Wether or not the next significant digit is changed (required for some behaviours)</param>
    /// <param name="anyHigherDigitChanged">Whether any of the more significant digits have changed</param>
    /// <param name="baseDate">A date to work from when date based version digits are used</param>
    /// <param name="lastBuildDate">The date of the last build, for when digits reset on day rollovers</param>
    /// <returns>Returns true if the digit changed during the increment</returns>
    internal bool PerformIncrement(bool higherDigitChanged, bool anyHigherDigitChanged, DateTime lastBuildDate, DateTime baseDate) {

        #region entry code

        if (higherDigitChanged) { b.Assert.True(anyHigherDigitChanged, "Logic error on changed digits"); }

        #endregion entry code

        b.Verbose.Log($"VersioningSupport, Applying version change to {Value ?? "null"} using {Behaviour.ToString()}");

        if (!string.IsNullOrEmpty(IncrementOverride)) {
            b.Verbose.Log($"Override Value Present {IncrementOverride} - All Other considerations ignored.");
            // An override overrules anything else - even fixed.

            if (IncrementOverride != actualValue) {
                actualValue = IncrementOverride;
                IncrementOverride = null;
                return true;
            } else {
                IncrementOverride = null;
                return false;
            }
        }

        if ((Behaviour == DigitIncrementBehaviour.Fixed) || (Behaviour == DigitIncrementBehaviour.ReleaseName)) {
            b.Verbose.Log($"Behaviour Set to {Behaviour}, not doing anything.");
            return false;
        }

        TimeSpan ts;
        int versionPriorToIncrement = int.Parse(Value);
        b.Verbose.Log("No override, moving to perform increment");

        //unchecked to make it explicit that an overflow wraps around.
        unchecked {
            switch (Behaviour) {
                case DigitIncrementBehaviour.DailyAutoIncrement:
                    if (DateTime.Today == lastBuildDate) {
                        versionPriorToIncrement++;
                    } else {
                        versionPriorToIncrement = 0;
                    }
                    break;

                case DigitIncrementBehaviour.DaysSinceDate:
                    ts = DateTime.Now - baseDate;
                    versionPriorToIncrement = (int)ts.TotalDays;
                    break;

                case DigitIncrementBehaviour.WeeksSinceDate:
                    ts = DateTime.Now - baseDate;
                    versionPriorToIncrement = (int)(ts.TotalDays / DAYS_IN_A_WEEK);
                    break;

                case DigitIncrementBehaviour.AutoIncrementWithReset:
                    if (higherDigitChanged) {
                        versionPriorToIncrement = 0;
                    } else {
                        versionPriorToIncrement++;
                    }
                    break;

                case DigitIncrementBehaviour.AutoIncrementWithResetAny:
                    if (anyHigherDigitChanged) {
                        versionPriorToIncrement = 0;
                    } else {
                        versionPriorToIncrement++;
                    }
                    break;

                case DigitIncrementBehaviour.ContinualIncrement:
                    versionPriorToIncrement++;
                    break;
            }
        }

        // Code change to move from uints to ints means that an overflow can create a negative version.  This code resets
        // the value back to zero if an overflow has caused a negative number.
        if (versionPriorToIncrement < 0) { versionPriorToIncrement = 0; }

        string tstr = versionPriorToIncrement.ToString();
        if (Value != tstr) {
            Value = tstr;
            return true;
        }
        return false;
    }

    public void SetBehaviour(DigitIncrementBehaviour newBehaviour) {
        b.Verbose.Log($"New behaviour being set {newBehaviour}");
        Behaviour = newBehaviour;
        ValidateForBehaviour();
    }

    private void ValidateForBehaviour() {
        if ((Behaviour != DigitIncrementBehaviour.Fixed) && (Behaviour != DigitIncrementBehaviour.ReleaseName)) {
            try {
                int.Parse(Value);
            } catch (Exception inr) {
                throw new InvalidOperationException($"Behaviour set to {Behaviour}.  This requires an integer value for the digit. Only Fixed and ReleaseName behaviours can be strings", inr);
            }
        }
    }

    public override string ToString() {
        return PreFix + Value;
    }
}