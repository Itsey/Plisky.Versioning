@echo off
:: %NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/vstore/versonify-version.store

goto %1
goto EndOfTimes

:Test1
:: Create new version with enough for pre-release
versonify  -Command=CreateVersion -VS=%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/vstore/versonify-version-pre.store -Q="1.0.0.0.0.0" -Release=Austen
goto EndOfTimes

:Test2
:: Update the fourth digit to be a release name
versonify -Command=Behaviour -VS=%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/vstore/versonify-version-pre.store -dg=3 -Q=ReleaseName
goto EndOfTimes

:Test3
:: Query current value
versonify -Command=Passive -VS=%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/vstore/versonify-version-pre.store
goto EndOfTimes

:TestX
::
versonify -Command=Behaviour -VS=C:\Users\itsey\Downloads\versonify-version-pre.store -dg=4 -Q=Fixed
versonify -Command=Behaviour -VS=C:\Users\itsey\Downloads\versonify-version-pre.store -dg=5 -Q=AutoIncrementWithReset
versonify -Command=PAssive -VS=C:\Users\itsey\Downloads\versonify-version-pre.store 
goto EndOfTimes




:EndOfTimes