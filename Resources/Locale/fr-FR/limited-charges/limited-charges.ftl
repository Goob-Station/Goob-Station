# SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

limited-charges-charges-remaining = {$charges ->
    [one] Ça a [color=fuchsia]{$charges}[/color] charges restantes.
    *[other] Ça a [color=fuchsia]{$charges}[/color] charges restantes.
}

limited-charges-max-charges = Les charges sont au [color=green]maximum[/color].
limited-charges-recharging = {$seconds ->
    [one] Il reste [color=yellow]{$seconds}[/color] seconde avant la prochaine charge.
    *[other] Il reste [color=yellow]{$seconds}[/color] secondes avant la prochaine charg.
}
