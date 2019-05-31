


rem --------------------------------------------
SET version=1.0.0
rem --------------------------------------------

cd .\DomainBase

dotnet pack  --configuration release --output NuGetPackages 
REM --no-build --no-restore

rem --------------------------------------------

