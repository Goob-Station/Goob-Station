# SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

limited-charges-charges-remaining =
    Имеется { $charges } { $charges ->
        [one] заряд
        [few] заряда
       *[other] зарядов
    }.

limited-charges-max-charges = Имеет [color=green]максимум[/color] зарядов.
limited-charges-recharging =
    До нового заряда { $seconds ->
        [one] осталась [color=yellow]{ $seconds }[/color] секунда.
        [few] осталось [color=yellow]{ $seconds }[/color] секунды.
       *[other] осталось [color=yellow]{ $seconds }[/color] секунд.
    }
