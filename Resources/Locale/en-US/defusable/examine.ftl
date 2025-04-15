# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2023 eclips_e <67359748+Just-a-Unity-Dev@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

defusable-examine-defused = {CAPITALIZE(THE($name))} is [color=lime]defused[/color].
defusable-examine-live = {CAPITALIZE(THE($name))} is [color=red]ticking[/color] and has [color=red]{$time}[/color] seconds remaining.
defusable-examine-live-display-off = {CAPITALIZE(THE($name))} is [color=red]ticking[/color], and the timer appears to be off.
defusable-examine-inactive = {CAPITALIZE(THE($name))} is [color=lime]inactive[/color], but can still be armed.
defusable-examine-bolts = The bolts are {$down ->
[true] [color=red]down[/color]
*[false] [color=green]up[/color]
}.
