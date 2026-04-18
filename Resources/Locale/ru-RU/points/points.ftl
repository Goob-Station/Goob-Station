# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

point-scoreboard-winner = Победитель — [color=lime]{ $player }![/color]
point-scoreboard-header = [bold]Таблица результатов[/bold]
point-scoreboard-list =
    { $place }. [bold][color=cyan]{ $name }[/color][/bold] набирает [color=yellow]{ $points ->
        [one] { $points } очко
       *[other] { $points } очков
    }.[/color]
