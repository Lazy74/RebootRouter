@echo off

set msbuild="C:\Program Files\MSBuild\14.0\Bin\MSBuild.exe"

%msbuild% RebootRouter.sln /p:Configuration=Release

pause
