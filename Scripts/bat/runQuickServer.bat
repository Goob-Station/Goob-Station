REM SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
REM SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
REM
REM SPDX-License-Identifier: AGPL-3.0-or-later

@echo off
cd /d "%~dp0..\.."

REM Goobstation entry + local config (status bind, lobby, lavaland).
REM Drop --no-build so Lavaland/Oskarrr changes actually compile in.
call dotnet run --project Content.Goobstation.Server -- --config-file "%CD%\server_config_local.toml" %*

pause
