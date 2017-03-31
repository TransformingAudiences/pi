echo off

dotnet build tapi.sln
dotnet test src/tapi.unittest/tapi.unittest.csproj --no-build -o ../../out/bin/debug

rem reset baseline files
rem dotnet out/bin/Debug/tapi.dll -conf:src/tapi.unittest/reports/config.xml -data:sample/data -templates:sample/templates/ -out:src/tapi.unittest/reports/ -log:Silent