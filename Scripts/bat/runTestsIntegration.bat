REM SPDX-FileCopyrightText: 2025 Goob Station Contributors
REM
REM SPDX-License-Identifier: MPL-2.0

cd ..\..\

mkdir Scripts\logs

del Scripts\logs\Content.IntegrationTests.log
dotnet test Content.IntegrationTests/Content.IntegrationTests.csproj -c DebugOpt -- NUnit.ConsoleOut=0 NUnit.MapWarningTo=Failed > Scripts\logs\Content.IntegrationTests.log

pause
