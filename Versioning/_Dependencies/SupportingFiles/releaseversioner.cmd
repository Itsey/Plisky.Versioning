@echo off
set srcFolder=..\TaskPackage\
set dstFolder=D:\Temp\_DelWorking\PliskyVersioner\
set dstFolder2=D:\temp\_DelWorking\PliskyVerTask\
set binFolder=..\..\PliskyVer\bin\Debug\

xcopy %srcFolder%*.* %dstFolder%*.* /s /y
xcopy %binFolder%*.* %dstFolder%\scripts\*.* /s /y
xcopy %binFolder%*.* %dstFolder2%\*.* /s /y
xcopy %srcFolder%\buildtask\task.json %dstFolder2%\*.* /s /y
xcopy %srcFolder%\scripts\*.* %dstFolder2%\*.* /s /y
pushd %dstfolder%
cd..
tfx build tasks upload --task.path ./pliskyvertask
::tfx extension create --manifest-globs vss-extension.jss
popd
