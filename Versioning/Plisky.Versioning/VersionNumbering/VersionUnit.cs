using Plisky.Plumbing;
using System;
using Plisky.Diagnostics;
using System.Collections.Generic;

namespace Plisky.CodeCraft {
    public class VersionUnit {
    

        public DigitIncremementBehaviour Behaviour { get; set; }
        private Bilge b = new Bilge("Plisky-VersionUnit");
        const int DAYS_IN_A_WEEK = 7;

        private string actualValue = null;

        public string IncrementOverride { get; set; }
        public string Value {
            get { return actualValue; }
            set { actualValue = value; ValidateForBehaviour(); }
        }
        public string PreFix { get; set; }



        public VersionUnit() : this(string.Empty, string.Empty, DigitIncremementBehaviour.Fixed) {
        }

        public VersionUnit(string v) : this(v, String.Empty) {
        }

        public VersionUnit(string versionValue, string versionPrefix, DigitIncremementBehaviour beh = DigitIncremementBehaviour.Fixed) {
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
            #endregion

            b.Verbose.Log($"VersioningSupport, Applying version change to {Value} using {Behaviour.ToString()}");

            if (!string.IsNullOrEmpty(IncrementOverride)) {
                b.Verbose.Log($"Override Value Present {IncrementOverride} - All Other considerations ignored.");
                // An override overrules anything else - even fixed.
                if (IncrementOverride != actualValue) {
                    actualValue = IncrementOverride;
                    return true;
                } else {
                    return false;
                }
            }

            if (Behaviour == DigitIncremementBehaviour.Fixed) {
                b.Verbose.Log("Behaviour Set to Fixed, not doing anything.");
                return false;
            }

            TimeSpan ts;
            int versionPriorToIncrement = int.Parse(Value);
            b.Verbose.Log("No override, moving to perform increment");

            //unchecked to make it explicit that an overflow wraps around.  
            unchecked {

                switch (Behaviour) {

                    case DigitIncremementBehaviour.DailyAutoIncrement:
                        if (DateTime.Today == lastBuildDate) {
                            versionPriorToIncrement++;
                        } else {
                            versionPriorToIncrement = 0;
                        }
                        break;

                    case DigitIncremementBehaviour.DaysSinceDate:
                        ts = DateTime.Now - baseDate;
                        versionPriorToIncrement = (int)ts.TotalDays;
                        break;

                    case DigitIncremementBehaviour.WeeksSinceDate:
                        ts = DateTime.Now - baseDate;
                        versionPriorToIncrement = (int)(ts.TotalDays / DAYS_IN_A_WEEK);
                        break;



                    case DigitIncremementBehaviour.AutoIncrementWithReset:
                        if (higherDigitChanged) {
                            versionPriorToIncrement = 0;
                        } else {
                            versionPriorToIncrement++;
                        }
                        break;

                    case DigitIncremementBehaviour.AutoIncrementWithResetAny:
                        if (anyHigherDigitChanged) {
                            versionPriorToIncrement = 0;
                        } else {
                            versionPriorToIncrement++;
                        }
                        break;
                    case DigitIncremementBehaviour.ContinualIncrement:
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

        public void SetBehaviour(DigitIncremementBehaviour newBehaviour) {
            b.Verbose.Log($"New behaviour being set {newBehaviour}");
            Behaviour = newBehaviour;
            ValidateForBehaviour();
        }

        private void ValidateForBehaviour() {
            int i;
            if (Behaviour != DigitIncremementBehaviour.Fixed) {
                try {
                    int.Parse(Value);
                } catch (Exception inr) {
                    throw new InvalidOperationException("Increment behaviour can not be set on non integer value", inr);
                }
            }

        }

      

       


        public override string ToString() {
            return PreFix + Value;
        }
    }
}