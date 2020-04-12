pushd C:\Files\Code\git\PliskyVersioning\Versioning\PliskyTool\bin\Debug\netcoreapp3.1

:: Create a Version file
::dotnet PliskyTool.dll CreateVersion -VS=\\blackpearl\DevShare\Versions\PliskyDiagnostics.version -Q=2.8.0.0

:: Test no version store
dotnet PliskyTool.dll UpdateFiles -Root=C:\Files\Code\git\PliskyDiagnostics\ -VS=\\blackpearl\DevShare\Versions\PliskyDiagnostics.version -Increment -mm=**\\properties\\assemblyinfo.cs,**\\versions\\CommonAssemblyInfo.cs,**\\*.nuspec,**\Plisky.DiagnosticsStd2\*.csproj

::dotnet PliskyTool.dll UpdateFiles -Root=D:\Temp\__Delworking\VersioingTestCodeDelete\ -VS=c:\temp\toolver.ver -Increment -mm=**\\properties\\assemblyinfo.cs,**\\versions\\CommonAssemblyInfo.cs,**\\*.nuspec,**\Plisky.DiagnosticsStd2\*.csproj

popd



