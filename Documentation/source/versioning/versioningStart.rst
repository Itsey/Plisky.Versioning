
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

