@echo off
git submodule init

cd .\Submodules\SharpNEAT\src\
dotnet build --configuration Release
cd ..\..\..

cd .\Submodules\BizHawk\Dist\
call QuickTestBuildAndPackage.bat
cd ..\..\..

dotnet build Application\Retro_ML.Application.csproj -c Release -o bin