namespace Plisky.CodeCraft;

using System;
using System.Globalization;
using Plisky.Diagnostics;

internal class VersionableDigit {
    private Bilge b = new Bilge();
    private int? overrideValue;
    private int currentValue;

    private DigitIncremementBehaviour behaviour = DigitIncremementBehaviour.Fixed;
    private DigitName position;

    internal int DigitValue {
        get { return currentValue; }
        set {
            if (value >= 0) {
                currentValue = value;
            } else {
                currentValue = 0;
            }
        }
    }

    internal DigitName DigitPosition {
        get { return position; }
        set { position = value; }
    }

    internal DigitIncremementBehaviour Behaviour {
        get { return behaviour; }
        set { behaviour = value; }
    }

    /// <summary>
    /// Gets or Sets the override value.  This value will be used in place of any other during the execute phase
    /// effectivly setting the specified digit to this value.
    /// </summary>
    /// <remarks>Set to -1 to perform no pending update</remarks>
    internal int? OverrideValueDuringIncrement {
        get { return this.overrideValue; }
        set {
            if (value >= 0) {
                this.overrideValue = value;
            } else {
                this.overrideValue = null;
            }
        }
    }

    /// <summary>
    /// Gets or sets the prompt value.  Used
    /// when prompts are used for version numbers to reflect the prompt request without popping the dialig up
    /// more than once.
    /// </summary>
    internal string PromptValueDuringIncrement {
        get { return overrideValue.ToString(); }
        set {
            // b.Assert.True(this.Behaviour == DigitIncremementBehaviour.Prompt, "Invalid operation when the behaviour is not prompt");

            this.overrideValue = int.Parse(value, CultureInfo.CurrentUICulture);

            if (this.overrideValue <= 0) {
                this.overrideValue = -1;
            }
        }
    }

    public override string ToString() {
        return this.currentValue.ToString(CultureInfo.CurrentUICulture);
    }

    internal void SetFromString(string value) {
        // Let the cast explode if wrong values are tried.
        DigitValue = int.Parse(value, CultureInfo.CurrentUICulture);
    }

    internal void Initialise(DigitName currentPosition, DigitIncremementBehaviour beh) {
        this.position = currentPosition;
        this.behaviour = beh;
    }

    internal VersionableDigit() {
    }

    internal VersionableDigit(DigitName currentPosition, DigitIncremementBehaviour beh) {
        Initialise(currentPosition, beh);
        this.overrideValue = null;
    }

    internal VersionableDigit(DigitName currentPosition, DigitIncremementBehaviour beh, int startValue)
        : this(currentPosition, beh) {
        this.DigitValue = startValue;
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

        const int DAYS_IN_A_WEEK = 7;

        b.Verbose.Log("VersioningSupport, Applying version change to " + position.ToString() + " using " + behaviour.ToString());

        int verStash = currentValue;  // remember initial value to determine whether a change was made.
        TimeSpan ts;              // Used for those that need to know the time since the base date has elaspsed

        if (overrideValue >= 0) {
            //Tex.FurtherInfo("A version override was detected, replacing the value");
            this.currentValue = overrideValue.Value;
            overrideValue = -1;
        } else {
            // Else there was no override.

            // Removed try/catch as the code is not checked then no exception is thrown when these methods overflow.  Added unchecked
            // keyword so that this is now explicit.
            unchecked {
                switch (behaviour) {
                    // This will incremement by one each time its run on a specific day.
                    case DigitIncremementBehaviour.DailyAutoIncrement:
                        if (DateTime.Today == lastBuildDate) {
                            currentValue++;
                        } else {
                            currentValue = 0;
                        }
                        break;

                    case DigitIncremementBehaviour.DaysSinceDate:
                        // This will take the number of days since a specified date
                        ts = DateTime.Now - baseDate;
                        currentValue = (int)ts.TotalDays;
                        break;

                    case DigitIncremementBehaviour.WeeksSinceDate:
                        ts = DateTime.Now - baseDate;
                        currentValue = (int)(ts.TotalDays / DAYS_IN_A_WEEK);
                        break;

                    case DigitIncremementBehaviour.AutoIncrementWithReset:
                        if (higherDigitChanged) {
                            currentValue = 0;
                        } else {
                            currentValue++;
                        }
                        break;

                    case DigitIncremementBehaviour.AutoIncrementWithResetAny:
                        if (anyHigherDigitChanged) {
                            currentValue = 0;
                        } else {
                            currentValue++;
                        }
                        break;

                    case DigitIncremementBehaviour.ContinualIncrement:
                        currentValue++;
                        break;
                }
            }
        }
        // Since we changed from Uints an overflow now creates a -ve version. Changed this to force it back
        // to zero.  As the code is unchecked we end up with -ve not a thrown exception.
        if (currentValue < 0) { currentValue = 0; }
        return (currentValue != verStash);  // Returns true if the value has changed.
    }
}