# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2023 eclips_e <67359748+Just-a-Unity-Dev@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

defusable-examine-defused = { CAPITALIZE($name) } [color=lime]обезврежена[/color].
defusable-examine-live =
    { CAPITALIZE($name) } тикает [color=red][/color] и осталось [color=red]{ $time } { $time ->
        [one] секунда
        [few] секунды
       *[other] секунд
    }.
defusable-examine-live-display-off = { CAPITALIZE($name) } [color=red]тикает[/color] и таймер, похоже, выключен.
defusable-examine-inactive = { CAPITALIZE($name) } [color=lime]неактивна[/color], но всё ещё может взорваться.
defusable-examine-bolts =
    Болты { $down ->
        [true] [color=red]опущены[/color]
       *[false] [color=green]подняты[/color]
    }.
