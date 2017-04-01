@echo off

pushd ..\

SET msbuildPath=C:\Program Files (x86)\MSBuild\14.0\Bin
"%msbuildPath%\msbuild" TheBlueSky.SwiftOAuth.sln /p:Configuration=Release /p:Platform="Any CPU"

popd

pause
