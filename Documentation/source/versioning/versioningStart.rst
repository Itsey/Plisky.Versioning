
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
-DryRun

Commands
========

-Command CreateVersion
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


-Command UpdateFiles
Requires -VersionStore or -QuickValue
Requires -Root
Optional -Increment
Optional -DryRun
Optional -MinMatch 

'pliskytool.exe -Command=Override -VersionSource=C:\temp\aversion.vstore -Root=C:\Build\Code\MyApp

Will optionally increment the version number specified by the source and then run through the directory specified by root and update any files that are matched by the
minmatchers for the specified file types.  There are a default set of minmatches in effect but they can be overriden.

To override a minmatch specify it using the -MM or -MinMatch command.  This is a series of one or more strings separated by ;.  If a single string is passed with no ;
and if this refers to a file that exists on disk then this file will be parsed for MinMatches instead.  The file format is as follows.

' <minmatch to the file>|<FileTypeToMatch>
'**/MyApp/commonAssemblyInfo.cs|NetAssembly
' **/MyApp/_Dependencies/CDSupport/readme.txt|TextFile
' **/MyApp/AppDir/App.csproj|NetInformational
' **/MyApp/AppDir/App.csproj|NetFile
' **/MyApp/AppDir/App.csproj|Wix
' **/_Dependencies/versioning.nuspec|Nuspec
' **/MyApp/AppDir/AssemblyInfo.cs|StdAssembly
' **/MyApp/AppDir/AssemblyInfo.cs|StdInformational
' **/MyApp/AppDir/AssemblyInfo.cs|StdFile

The pipe separator separates the minmatch from the type of file that it is updating.  Multiple file types can reside in the same file and therefore use the same minmatch.

Each file type has a rule to determine how to match versions.

NetAssembly, NetFile, NetInfomrational

Net framework based cs files looking for the corresponding attribute in the file to apply the version.

StdAssembly, StdInformational, StdFile

Net standard (csproj file) looking for properties in a property group to apply the version.

TextFile

Any file, looking for XXX-VERSION-XXX to replace with the version number.

Wix 

Wix Setup file, looking for the version attribute.  To update the version in the name too, use the text file version as well.

Nuspec

Nuget Package File format.

