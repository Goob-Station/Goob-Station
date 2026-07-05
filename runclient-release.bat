@echo off
REM Goob-MalfAi: Release client - debug asserts (which crash the Debug build) are disabled.
REM Output still goes to client_crash.log so crashes stay diagnosable.
dotnet run -c Release --project Content.Goobstation.Client > client_crash.log 2>&1
