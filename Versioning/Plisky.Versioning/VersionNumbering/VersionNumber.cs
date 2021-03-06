﻿using System;

namespace Plisky.CodeCraft {

    /// <summary>
    /// Responsible for managing a series of four digits as a verison number.
    /// </summary>
    public class VersionNumber {
        private VersionableDigit[] digits = new VersionableDigit[4];

        public int Major {
            get {
                return digits[(int)DigitName.Major].DigitValue;
            }
            set {
                SetDigitValue(DigitName.Major, value);
            }
        }

        public int Minor {
            get {
                return digits[(int)DigitName.Minor].DigitValue;
            }
            set {
                SetDigitValue(DigitName.Minor, value);
            }
        }

        public int Build {
            get {
                return digits[(int)DigitName.Build].DigitValue;
            }
            set {
                SetDigitValue(DigitName.Build, value);
            }
        }

        public int Revision {
            get {
                return digits[(int)DigitName.Revision].DigitValue;
            }
            set {
                SetDigitValue(DigitName.Revision, value);
            }
        }

        private void SetDigitValue(DigitName digitPosition, int value) {
            if (value < 0) {
                throw new ArgumentOutOfRangeException(nameof(value), "The versioned digit can not be less than zero");
            }
            digits[(int)digitPosition].DigitValue = value;
        }

        public static VersionNumber Parse(string parseTxt) {
            if (string.IsNullOrEmpty(parseTxt)) {
                throw new ArgumentOutOfRangeException(nameof(parseTxt), "The text to parse for a version number must be present");
            }
            if (parseTxt.IndexOf('.') < 0) {
                throw new ArgumentOutOfRangeException(nameof(parseTxt), "The text for the verison number must be digits separated by periods");
            }
            string[] values = parseTxt.Split('.');

            if (values.Length != 4) {
                throw new ArgumentException("The current verison must be in the format n.n.n.n where n is a digit.", "current");
            }
            try {
                return new VersionNumber(int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]), int.Parse(values[3]));
            } catch (OverflowException ox) {
                throw new ArgumentOutOfRangeException(nameof(parseTxt), ox);
            } catch (FormatException fx) {
                throw new ArgumentException(nameof(parseTxt), fx);
            }
        }

        /// <summary>
        /// Creates a new VersionNumber class populated with the digits from the parameters.
        /// </summary>
        /// <param name="maj">The value for the Major Digit</param>
        /// <param name="min">The value for the Minor Digit</param>
        /// <param name="bui">The value for the Build Digit</param>
        /// <param name="rev">The value for the Revision Digit</param>
        public VersionNumber(int maj, int min, int bui, int rev) {
            SetDigitValue(DigitName.Major, maj);
            SetDigitValue(DigitName.Minor, min);
            SetDigitValue(DigitName.Build, bui);
            SetDigitValue(DigitName.Revision, rev);
        }

        public override string ToString() {
            return string.Format("{0}.{1}.{2}.{3}", this.Major, this.Minor, this.Build, this.Revision);
        }

        public override bool Equals(object obj) {
            var target = obj as VersionNumber;
            bool result = true;
            result &= this.Major == target.Major;
            result &= this.Minor == target.Minor;
            result &= this.Build == target.Build;
            result &= this.Revision == target.Revision;
            return result;
        }
    }
}