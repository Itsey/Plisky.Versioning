
using Plisky.Plumbing;
using System;
using System.Collections.Generic;

namespace Plisky.CodeCraft {
    public class CompleteVersion {

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


        public VersionUnit[] Digits;
        public DisplayType[] displayTypes;

        public CompleteVersion() {
            
            

            displayTypes = new DisplayType[8];
            displayTypes[(int)FileUpdateType.Assembly] = DisplayType.Short;
            displayTypes[(int)FileUpdateType.AssemblyFile] = DisplayType.Full;
            displayTypes[(int)FileUpdateType.AssemblyInformational] = DisplayType.Full;
            displayTypes[(int)FileUpdateType.Wix] = DisplayType.Full;
            displayTypes[(int)FileUpdateType.Nuspec] = DisplayType.ThreeDigit;
            displayTypes[(int)FileUpdateType.NetStdAssembly] = DisplayType.Short;
            displayTypes[(int)FileUpdateType.NetStdFile] = DisplayType.Full;
            displayTypes[(int)FileUpdateType.NetStdInformational] = DisplayType.Full;
        }

        public bool IsDefault { get; set; }

        public void SetDisplayTypeForVersion(FileUpdateType fut, DisplayType dt) {
            displayTypes[(int)fut] = dt;
        }

        public DisplayType GetDisplayType(FileUpdateType fut, DisplayType dt= DisplayType.Default) {
            if (dt != DisplayType.Default) { return dt;  }
            return displayTypes[(int)fut];
        }

        public CompleteVersion(params VersionUnit[] versionDigits) : this() {
            Digits = versionDigits;
        }

 

        public CompleteVersion(string initialValue) : this() {
            if (initialValue.Contains(".")) {
                string[] parse = initialValue.Split(new char[] { '.'},StringSplitOptions.RemoveEmptyEntries);
                var vd = new List<VersionUnit>();
                string prefix = "";
                foreach (var f in parse) {
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
            for(int i=0; i<changes.Length; i++) {

                if (Digits.Length > i) {
                    string cval = Digits[i].Value;
                    var nv = ManipulateValueBasedOnPattern(changes[i], cval);
                    if (nv!=null) {
                        Digits[i].IncrementOverride = nv;
                    }                    
                }
            }

            
        }

        protected string ManipulateValueBasedOnPattern(string pattern, string currentValue) {
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
            if ((dt == DisplayType.ThreeDigit) && (Digits.Length> 3)) {
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

        public void Increment() {
            bool lastChanged = false;
            bool anyChanged = false;
            DateTime t1 = DateTime.Now;
            
            foreach (var un in Digits) {
                lastChanged = un.PerformIncrement(lastChanged, anyChanged, t1, t1);
                if (lastChanged) { anyChanged = true; }
            }
        }



    }
}