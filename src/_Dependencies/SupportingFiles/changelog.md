## Versonify Change Log.

⬆️ XXX-VERSION3-XXX

* ✅ Feature - 



⬆️ **1.0.3** - Austen Compatibility Release.
  - ✅ Feature - Implemented --QQpnf quick version return exit codes to tell PNF what compatibility version Versonify is running. 
  - ✅ Feature - Implemented -z to suppress non zero return exit codes.
  - ✅ Feature - 💥Breaking Change💥 File Updates that do not update any files now default to returning non zero exit code.  Add -z for old functionality.

Note that the QQpnf feature is not really aimed at end users but purely at the Pliksy.Nuke.Fusion library to ensure that it understands what parameters are available to different versions of Versonify.  File updates that update no files now default to an error, this was because it was more common that it was a mistake rather than intentional.

Changes to command line support are coming, with a view to standardising the command line in the way that is now more common.  These warnings are added to this version:

⚠️ -MM is now deprecated.  Use -M instead.
⚠️ -VS is now deprecated. Use -V instead.
⚠️ -NO is now deprecated. Use -NoOverride instead.
⚠️ -DG is now deprecated. Use -D instead. 



⬆️ **1.0.1** - Austen.
  - First Release ( Austen Release ).
  - Behaviour Update Added.

⬆️ 0.2.0 - Austen Pre-Release.
  - Moved to being a dotnet tool.
  - Now supports display of behaviours.  ( -Command=behaviour)

⬆️ 0.1.3 - Initial.
  - 🐞 Critical Bug - Typo in exe name in package corrected.  0.1.2. Superceeded.

⬆️  0.1.2 - Initial.
  - Added Nexus Support.      

⬆️  0.1.1 - Initial.
  - Updated documentation to remove references to PliskyTool.
  - 🐞 Fix - DryRun no longer updates the files on disk.

