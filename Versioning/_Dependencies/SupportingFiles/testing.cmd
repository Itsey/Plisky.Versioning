pushd C:\Files\Code\git\PliskyVersioning\Versioning\PliskyTool\bin\Debug\netcoreapp3.1

:: Create a Version file
::dotnet PliskyTool.dll CreateVersion -VS=c:\temp\toolver.ver -Q=2.0.0.0

:: Test no version store
::dotnet PliskyTool.dll UpdateFiles -DryRun -Root=c:\files\code\git\pliskyversioning\versioning\testdata -VS=c:\temp\toolver.ver -Increment

::dotnet PliskyTool.dll Override -VS=c:\temp\toolver.ver -Q=.+.Alpha.0 
::-DryRun


::dotnet PliskyTool.dll CreateVersion -VS=\\unicorn\Files\BuildTools\VersionStore\pversioner.vstore -Q=1.0.0.0

popd



