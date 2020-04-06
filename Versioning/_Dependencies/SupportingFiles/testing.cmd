pushd C:\Files\Code\git\PliskyVersioning\Versioning\PliskyTool\bin\Debug\netcoreapp3.1

:: Create a Version file
::dotnet PliskyTool.dll CreateVersion -VS=c:\temp\toolver.ver -Q=2.0.0.0

:: Test no version store
::dotnet PliskyTool.dll UpdateFiles -DryRun -Root=c:\files\code\git\pliskyversioning\versioning\testdata -VS=c:\temp\toolver.ver -Increment

dotnet PliskyTool.dll UpdateFiles -Root=D:\Temp\__Delworking\VersioingTestCodeDelete\ -VS=c:\temp\toolver.ver -Increment -mm=**\\properties\\assemblyinfo.cs,**\\versions\\CommonAssemblyInfo.cs,**\\*.nuspec,**\Plisky.DiagnosticsStd2\*.csproj

popd



