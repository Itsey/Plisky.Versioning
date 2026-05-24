# Plisky.Versioning

## Why

This is a versioning library and corresponding command line tool to apply versioning during CI-CD pipelines, its should be possible to set it up and then ignore it for versioning except to configure major releases.  



## Release Notes

#### 1.0.4 - Internal structure release (May Be Bronte)

Intent is to update internal structure so that its using MS command line parser not plisky and to restructure so that JSON output is viable, this should pave the way for a Versonify skill to be added to the solution.  Intent is to have that skill as a resource that can be output by the tool.

Also note much more heavy use of AI now, look for (ai) commits, these are entirely coded and reviewed by ai.  



#### 1.0.3  - Compatibility Release for PNF

 \- ✅ Feature - Implemented --QQpnf quick version return exit codes to tell PNF what compatibility version Versonify is running.
 \- ✅ Feature - Implemented -z to suppress non zero return exit codes.
 \- ✅ Feature - 💥Breaking Change💥 File Updates that do not update any files now default to returning non zero exit code.  Add -z for old functionality.

This was driven from the need for PNF to be able to determine what parameters the underlying tools supported, PNF didn't know which version of the tool they had installed therefore was calling with invalid parameters.   This change implements an exit code with the version compatibility level

| Exit Code | Notable Compat                                               | Release |
| --------- | ------------------------------------------------------------ | ------- |
| 200       | This is the first version that supports the exit code therefore everything that returns -1 for QQpnf has the original parameters that relate to versions <1.0.3.  This version allows for the new | 1.0.3   |
|           |                                                              |         |







#### 1.0.2

This was a duff release, unlisted it.  



#### 1.0.1 

This was the first release with full features ( Austen ) supported the display of behaviours along with the existing <1.0 functionality.
