
Using The Versioning In Powershell
===========================================


Sometimes you will want to use the version number that is generated in a script, rather than replace in a predefined file.  There are several output methods
that are used to do this.



Walkthrough - Using Versioning in a Docker Tag
========

This assumes that you have already created a version store and are using it to version elements such as the code.  For simplicity none of that will be included
in this guide and it will also be assumed that the version store will be incremented elsewhere ( for example during the code build process).  Therefore this
walkthrough shows how to tag a docker image with the same version number that was just applied to the code.

Take this powershell script:

'docker build -t papi-api .
'docker tag papi-api itseyreg.azurecr.io/papi-api:latest

one way to tag the version is by doing a replacement on the file and marking it like this:

'docker tag papi-api itseyreg.azurecr.io/papi-api:XXX-VERSION-XXX

this works but requires that you update your script file each time, and that is not the most convenient if running by hand, therefore we will look at an alternative
approach that can be run inline to the file to take the correct version.

'pliskytool.exe -Command=Passive -VersionSource=C:\temp\aversion.vstore -O=File
'$verval = Get-Content plisky-version.txt
''docker tag papi-api itseyreg.azurecr.io/papi-api:$verval


