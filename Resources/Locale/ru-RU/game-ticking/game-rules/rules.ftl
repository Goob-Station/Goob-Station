# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# General
rule-restarting-in-seconds =
    Перезапуск через { $seconds } { $seconds ->
        [one] секунду
        [few] секунды
       *[other] секунд
    }.
rule-time-has-run-out = Время вышло!

# Respawning
rule-respawn-in-seconds =
    Возрождение через { $second } { $second ->
        [one] секунду
        [few] секунды
       *[other] секунд
    }...
