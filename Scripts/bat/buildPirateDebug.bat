REM SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
REM SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
REM
REM SPDX-License-Identifier: AGPL-3.0-or-later

@echo off
cd ../../

call git submodule update --init --recursive

cd Content.Pirate.Server
@echo on
call dotnet build -c Debug
@echo off
cd ..

cd Content.Pirate.Shared
@echo on
call dotnet build -c Debug
@echo off
cd ..

cd Content.Pirate.Client
@echo on
call dotnet build -c Debug
@echo off
cd ..

call dotnet build -c Debug
pause
