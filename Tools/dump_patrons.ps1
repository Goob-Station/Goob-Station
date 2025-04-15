# SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
#
# SPDX-License-Identifier: MIT

#!/usr/bin/env pwsh

param([string]$csvPath)

# Dumps Patreon's CSV download into a YAML file the game reads.

# Have to trim patron names because apparently Patreon doesn't which is quite ridiculous.
Get-content $csvPath | ConvertFrom-Csv -Delimiter "," | select @{l="Name";e={$_.Name.Trim()}},Tier | where-object Tier -ne "" | ConvertTo-Yaml
