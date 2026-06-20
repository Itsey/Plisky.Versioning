# Plisky.Versioning

## Why

This is a versioning library and corresponding command line tool to apply versioning during CI-CD pipelines, its should be possible to set it up and then ignore it for versioning except to configure major releases.  



## Release Notes

#### 1.0.4 - Internal structure release (May Be Bronte)

➕ The manual tester has highlighted that there are a lot of tokens being spent trying to work out what the command line for versonify is, and this involves reading the web, reading the help and a bit of trial and error.  Added a local embedded version of the help and a specific instruction to ai agents to extract this to get help about the tool.  Requested a refactor of program.cs too to simplify it as it was getting heavy.   docs\docs.md now contains a document embedded within versonify to spit out better quality help instructions.

➕ Adding in manual tester to support the use of the prompt so that its easier to identify if something has broken, using a series of prompts and expected values so that you can run the prompt then test whether or not the outcome was the expected one.

Also moved all code to nullable enabled, interestingly the ai decided to generate a md spec file and run a ralph loop all on its own for this one - all I said was fix the nullable warnings.  

Intent is to update internal structure so that its using MS command line parser not Plisky and to restructure so that JSON output is viable, this should pave the way for a Versonify skill to be added to the solution.  Intent is to have that skill as a resource that can be output by the tool.

Also note much more heavy use of AI now, look for (ai) commits, these are entirely coded and reviewed by ai.  

Commit to add kebab-case double hyphen arguments threw up an inconsistency during secondary review.  The secondary review pointed out that there were -Debug=v-** statements in the test cases - which would make sense for debug levels of tracing.  However at that time the implementation had switched to a Boolean Debug flag.  Its not clear at the current time if this was an error in the test cases ( mis understanding / mismatch / bad copy paste) or if the argument refactoring had switched it from a string parameter to a Boolean.  If regression testing highlights this in command line options then this is what has happened.  It was not clear from the code that the trace identifier there was actually used.

⚠️ Note documentation update will be required for this release.  

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
