# SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

limited-charges-charges-remaining = {$charges ->
    [one] It has [color=fuchsia]{$charges}[/color] charge remaining.
    *[other] It has [color=fuchsia]{$charges}[/color] charges remaining.
}

limited-charges-max-charges = It's at [color=green]maximum[/color] charges.
limited-charges-recharging = {$seconds ->
    [one] There is [color=yellow]{$seconds}[/color] second left until the next charge.
    *[other] There are [color=yellow]{$seconds}[/color] seconds left until the next charge.
}
