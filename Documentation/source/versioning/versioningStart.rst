
Versioning
===========================================


Versioning provides the capability to add version numbers to your build cycles and have them update automatically using a notation that you prefer and it offers a great
deal of flexibility in the way that those version numbers are incremented and applied to your source code.

Quick Start Guide
======================

Versioning can be done through your own code or through using PliskyTool. The quickstart uses plisky tool as a command line way of manipulating version numbers.


Command Line Options

Command line options are prefixed with -.  They are postfixed with =.   e.g. -Command=CreateVersion 

-Command  (-C)
-VersionSource  (-VS)
-Increment
-Digits
-QuickValue  (-Q)
-MinMatch
-Root


Commands
========

-Command CreateVersionStore
Requires -VersionSource

'pliskytool.exe -Command=CreateVersion -VersionSource=C:\temp\aversion.vstore

Will create a new version at 1.0.0.0 in the source specified by version source.

-Command Override
Requires -VersionSource
Requires -QuickValue

'pliskytool.exe -Command=Override -VersionSource=C:\temp\aversion.vstore -Q=.+.0.0

Will create a pending version that will be applied on the next increment.  This will override changes that the default increments will perform
applying a pattern.  This is normally used for release versions, where versions do not follow the same pattern as build versions.  

To alter the behaviour specify a new pattern separated by . therefore +.+.+.+ increments each of a four digit version number on the next increment.

+ = Increment this digit.
- = Decrement this digit
nnnn = Any number of digits use this as the version number for this digit
abc  = Any number of letters replaces the digit with this version (for named digits)

E.g. 
1.0.0.0  =>  +...  => 2.0.0.0
1.1.1.1  =>  +.+.+.+ => 2.2.2.2
1.1.1.1 => +.-+.-  => 2.0.2.0
1.0.0.0 => +.0.alpha.0  => 2.0.alpha.0
