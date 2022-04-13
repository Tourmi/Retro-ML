@echo off
git submodule init

cd .\Submodules\SharpNEAT\src\
dotnet build --configuration Release
cd ..\..\..

cd .\Submodules\BizHawk\Dist\
call QuickTestBuildAndPackage.bat
cd ..\..\..

dotnet build SMW-ML\SMW_ML.csproj -c Release -o bin