REM SPDX-FileCopyrightText: 2025 Goob Station Contributors
REM
REM SPDX-License-Identifier: MPL-2.0

@echo off
cd ../../

call dotnet run --project Content.Server --no-build %*

pause
