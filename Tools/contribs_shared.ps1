# SPDX-FileCopyrightText: 2022 ike709 <ike709@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

$replacements = @{
    "moonheart08" = "moony"
    "Elijahrane" = "Rane"
    "ZeroDayDaemon" = "Daemon"
    "ElectroJr" = "ElectroSR"
    "Partmedia" = "notafet"
    "Just-a-Unity-Dev" = "eclips_e"
}

$ignore = @{
    "PJBot" = $true
    "github-actions[bot]" = $true
    "GoobBot" = $true
    "GoobBot[bot]" = $true
    "GoobBot [bot]" = $true
    "ZDDM" = $true
    "TYoung86" = $true
    "paul" = $true # erroneously included -- presumably from PaulRitter, somehow, who is already credited
    "08a" = $true # erroneously included -- valid github account, but not an actual contributor, probably an alias of a contributor who does not own this github account and is already credited somewhere.
    "UristMcContributor" = $true # this was an account used to demonstrate how to create a valid PR, and is in actuality Willhelm53, who is already credited.
}

$add = @("RamZ")
