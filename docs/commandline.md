## Plisky Tool Command Line reference

### Command Line Options

Command line options are prefixed with -.  They are postfixed with =.  
e.g. -Command=CreateVersion 

```plaintext
-Command  (-C)              Specify the Command that is to be run
-VersionSource  (-VS)       Specify an inititialisation string to a supported version source
-Increment                  Increment the version number during the command operation.
-Digits                     
-QuickValue  (-Q)           Provide a value for the versioning command.
-MinMatch                   Provide a file or list of minimatches to identify files to update.
-Root                       The root folder to recursivly search for files to update.
-DryRun                     If specified then no updates are made, but output is written to the logs.
-Debug                      Enables trace handling for debugging and additional logging.

Full Example Commandline:

PliskyTool.exe UpdateFiles -Root=c:\src\ -VS=c:\store\pversioner.vstore -Increment -MM="**/*.csproj|StdFile,**/*.csproj|StdAssembly,**/*.csproj|StdInformational"

```

### Commands

#### Create Version

Creates a new default version number 

```plaintext
-Command=CreateVersion

Requires:
-VersionSource  (-VS)
```

```dos
pliskytool.exe -Command=CreateVersion -VersionSource=C:\temp\aversion.vstore
```

Will create a new version at 1.0.0.0 in the source specified by version source, this will be persisted with the default values of Fixed versioning. Therefore your version number will default to 1.0.0.0.

#### Passive

Passively reads the version number for use in scripts.

```plaintext
-Command=Passive

Requires:
-VersionSource  (-VS)  or -QuickValue (-Q)
```

```dos
pliskytool.exe -Command=Passive -VersionSource=C:\temp\aversion.vstore
pliskytool.exe -Command=Passive -VersionSource=C:\temp\aversion.vstore -O=File
```

Will load the version number into the tool then perform no action.  This is only really used in conjuction with the -O output option to ensure that the version number is made available to a calling or alternative process.

#### Override
Overrides the values of version numbers at the point of next increment

```plaintext
-Command=Override

Requires:
-VersionSource  (-VS)  or -QuickValue (-Q)
```

```dos
pliskytool.exe -Command=Override -VersionSource=C:\temp\aversion.vstore -Q=.+.0.0
```

Will create a pending version that will be applied on the next increment.  This will override changes that the default increments will perform applying a pattern.  This is normally used for release versions, where versions do not follow the same pattern as build versions.  

To alter the behaviour specify a new pattern separated by . therefore +.+.+.+ increments each of a four digit version number on the next increment.

\+ = Increment this digit.  
\- = Decrement this digit.  
nnnn = Any number of digits use this as the version number for this digit  
abc  = Any number of letters replaces the digit with this version (for named digits)  

**examples**  
1.0.0.0  =>  +...  => 2.0.0.0  
1.1.1.1  =>  +.+.+.+ => 2.2.2.2  
1.1.1.1 => +.-+.-  => 2.0.2.0  
1.0.0.0 => +.0.alpha.0  => 2.0.alpha.0  

#### Update Files
Overrides the values of version numbers at the point of next increment

```plaintext
-Command=UpdateFiles

Requires:
-VersionSource  (-VS)  or -QuickValue (-Q)
-Root

Optional:
-Increment
-DryRun
-MinMatch 
```

```dos
pliskytool.exe -Command=Override -VersionSource=C:\temp\aversion.vstore -Root=C:\Build\Code\MyApp
```
Will optionally increment the version number specified by the source and then run through the directory specified by root and update any files that are matched by the minmatchers for the specified file types.  There are a default set of minmatches in effect but they can be overriden.

To override a minmatch specify it using the -MM or -MinMatch command.  This is a series of one or more strings separated by ;.  If a single string is passed with no ; and if this refers to a file that exists on disk then this file will be parsed for MinMatches instead.  The file format is as follows.

It is generally more convenient to specify the file and store it in your source repository than to configure all of the minmatches on the command line using the ; syntax.

```plaintext
<minmatch to the file>|<FileTypeToMatch>
```
Each line in the file adds a new minmatch. 

```plaintext
**/MyApp/commonAssemblyInfo.cs|NetAssembly
**/MyApp/_Dependencies/CDSupport/readme.txt|TextFile
**/MyApp/AppDir/App.csproj|NetInformational
**/MyApp/AppDir/App.csproj|NetFile
**/MyApp/AppDir/App.csproj|Wix
**/_Dependencies/versioning.nuspec|Nuspec
**/MyApp/AppDir/AssemblyInfo.cs|StdAssembly
**/MyApp/AppDir/AssemblyInfo.cs|StdInformational
**/MyApp/AppDir/AssemblyInfo.cs|StdFile
```

The pipe separator separates the minmatch from the type of file that it is updating.  Multiple file types can reside in the same file and therefore use the same minmatch.

Each file type has a rule to determine how to match versions, see (version matching reference)[vermatchref.md].



Full Eample Command Line:
```dos
'PliskyTool.exe UpdateFiles -Root=c:\src\ -VS=c:\store\pversioner.vstore -Increment -MM="**/*.csproj|StdFile,**/*.csproj|StdAssembly,**/*.csproj|StdInformational"
```
This will search the folder c:\src for all .csproj files and attempt to add the .net standard versioninng for the three different file types to any csproj files that are
found.  Note that for the std file type it will look inside the file and see whether it looks like a net std file or a framework one.  Framework ones
will not be udpated.  