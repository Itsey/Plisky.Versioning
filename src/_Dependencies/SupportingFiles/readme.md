# 💖 Versioning By Versonify 💖


## About

This is the nuget tool package for Versonify, a versioning tool built to replace Plisky.Versioning.  See the full documentation on the gitub pages https://itsey.github.io/version-index.html

## Key Features

Command line tool used to provide version management and updating of various types of source code file with the version number, deisgned to be applied during your CI / CD builds.   The package is a dotnet tool and can be used from the command line or from the Nuke build engine using the Plisky.Nuke.Fusion package.

Current version supports disk based file stores including shared paths and Nexus based stores.

## Usage.


 See the full documentation on the gitub pages https://itsey.github.io/version-index.html
 
 ```Code
versonify.exe -Command=CreateVersion -VersionSource=C:\temp\aversion.vstore
versonify.exe -Command=Passive -VersionSource=C:\temp\aversion.vstore -O=File
 ```
 
 ## Additional Documentation
 
 * Main Documentation: https://itsey.github.io/
 * Versioning Documentation: https://itsey.github.io/version-index.html
 * Project Repository: https://github.com/Itsey/Plisky.Versioning
 
 
 
 