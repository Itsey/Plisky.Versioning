pushd C:\Files\Code\git\PliskyVersioning\Versioning\PliskyTool\bin\Debug\netcoreapp3.1

:: Create a Version file
::dotnet PliskyTool.dll CreateVersion -VS=c:\temp\toolver.ver -Q=2.0.0.0

:: Test no version store
::dotnet PliskyTool.dll UpdateFiles -DryRun -Root=c:\files\code\git\pliskyversioning\versioning\testdata -VS=c:\temp\toolver.ver -Increment

::dotnet PliskyTool.dll Override -VS=c:\temp\toolver.ver -Q=.+.Alpha.0 
::-DryRun


::dotnet PliskyTool.dll CreateVersion -VS=\\unicorn\Files\BuildTools\VersionStore\pversioner.vstore -Q=1.0.0.0

::Release name Test
::pliskytool.exe CreateVersion -VS=\\unicorn\Files\BuildTools\VersionStore\jsbsite.vstore -Q=1.1.0.0 -R=Sunflower


PliskyTool.exe UpdateFiles -Root=C:\Temp\votalot -VS=\\unicorn\Files\BuildTools\VersionStore\jsbsite.vstore -Increment -R=Daffodil -MM="**/temp.txt|TextFile" -Debug
popd



