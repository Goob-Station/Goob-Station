# SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

robotics-console-window-title = Robotics Console
robotics-console-no-cyborgs = No Cyborgs!

robotics-console-select-cyborg = Select a cyborg above.
robotics-console-model = [color=gray]Model:[/color] {$name}
# name is not formatted to prevent players trolling
robotics-console-designation = [color=gray]Designation:[/color]
robotics-console-battery = [color=gray]Battery charge:[/color] [color={$color}]{$charge}[/color]%
robotics-console-modules = [color=gray]Modules installed:[/color] {$count}
robotics-console-brain = [color=gray]Brain installed:[/color] [color={$brain ->
    [true] green]Yes
    *[false] red]No
}[/color]

robotics-console-locked-message = Controls locked, swipe ID.
robotics-console-disable = Disable
robotics-console-destroy = Destroy

robotics-console-cyborg-destroying = {$name} is being remotely detonated!
