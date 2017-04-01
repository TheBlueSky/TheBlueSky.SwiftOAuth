@echo off

pushd ..\

SET msbuildPath=C:\Program Files (x86)\MSBuild\14.0\Bin
"%msbuildPath%\msbuild" TheBlueSky.SwiftOAuth.sln /p:Configuration=Release /p:Platform="Any CPU"

popd

mkdir artifacts
copy ..\src\TheBlueSky.SwiftOAuth\bin\release\*.dll .\artifacts
nuget pack TheBlueSky.SwiftOAuth.nuspec -o .\artifacts

pause
