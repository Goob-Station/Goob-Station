# SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2023 eclips_e <67359748+Just-a-Unity-Dev@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

defusable-examine-defused = {CAPITALIZE(THE($name))} est [color=lime]désamorcé[/color].
defusable-examine-live = {CAPITALIZE(THE($name))} [color=red]tique[/color] et il reste [color=red]{$time}[/color] secondes.
defusable-examine-live-display-off = {CAPITALIZE(THE($name))} [color=red]tique[/color], et le minuteur semble éteint.
defusable-examine-inactive = {CAPITALIZE(THE($name))} est [color=lime]inactif[/color], mais peut toujours être armé.
defusable-examine-bolts = The bolts are {$down ->
[true] [color=red]down[/color]
*[false] [color=green]up[/color]
}.
