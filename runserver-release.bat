@echo off
REM Goob-MalfAi: Release server - debug asserts (which crash the Debug build) are disabled.
dotnet run -c Release --project Content.Goobstation.Server
