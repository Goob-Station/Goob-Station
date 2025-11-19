REM SPDX-FileCopyrightText: 2025 Goob Station Contributors
REM
REM SPDX-License-Identifier: MPL-2.0

cd ..\..\

mkdir Scripts\logs

del Scripts\logs\Content.Tests.log
dotnet test Content.Tests/Content.Tests.csproj -c DebugOpt -- NUnit.ConsoleOut=0 > Scripts\logs\Content.Tests.log

pause
