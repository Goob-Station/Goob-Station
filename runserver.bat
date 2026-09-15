REM SPDX-FileCopyrightText: 2018 DamianX <DamianX@users.noreply.github.com>
REM SPDX-FileCopyrightText: 2019 Silver <Silvertorch5@gmail.com>
REM SPDX-FileCopyrightText: 2019 clusterfack <8516830+clusterfack@users.noreply.github.com>
REM SPDX-FileCopyrightText: 2021 SweptWasTaken <sweptwastaken@protonmail.com>
REM SPDX-FileCopyrightText: 2024 Vasilis <vasilis@pikachu.systems>
REM SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
REM
REM SPDX-License-Identifier: AGPL-3.0-or-later

@echo off
REM Always use repo-root local config (bin/server_config.toml is overwritten on build).
dotnet run --project Content.Goobstation.Server -- --config-file "%~dp0server_config_local.toml"
pause
