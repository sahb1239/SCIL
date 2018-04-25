@echo off
dotnet build SCIL.sln --configuration Release

mkdir Output
cd Output

dotnet ../SCIL/bin/Release/netcoreapp2.0/SCIL.dll --InputFile "../SimpleCILProgram/bin/Release/netcoreapp2.0/SimpleCILProgram.dll" --OutputPath ./

pause