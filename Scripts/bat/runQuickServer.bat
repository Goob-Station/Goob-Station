REM SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
REM SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
REM
REM SPDX-License-Identifier: AGPL-3.0-or-later

@echo off
cd ../../

REM AltHub Space -> start
call dotnet run --project Content.Goobstation.Server --no-build %*
REM AltHub Space -> end

pause
