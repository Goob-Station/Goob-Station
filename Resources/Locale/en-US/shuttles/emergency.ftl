# SPDX-FileCopyrightText: 2022 LittleBuilderJane <63973502+LittleBuilderJane@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Myctai <108953437+Myctai@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
# SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
# SPDX-FileCopyrightText: 2024 strO0pwafel <153459934+strO0pwafel@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

# Commands
## Delay shuttle round end
emergency-shuttle-command-round-desc = Stops the timer that ends the round when the emergency shuttle exits hyperspace.
emergency-shuttle-command-round-yes = Round delayed.
emergency-shuttle-command-round-no = Unable to delay round end.

## Dock emergency shuttle
emergency-shuttle-command-dock-desc = Calls the emergency shuttle and docks it to the station... if it can.

## Launch emergency shuttle
emergency-shuttle-command-launch-desc = Early launches the emergency shuttle if possible.

# Emergency shuttle
emergency-shuttle-left = The Emergency Shuttle has left the station. Estimate {$transitTime} seconds until the shuttle arrives at CentComm.
emergency-shuttle-launch-time = The emergency shuttle will launch in {$consoleAccumulator} seconds.
emergency-shuttle-docked = The Emergency Shuttle has docked {$direction} of the station, {$location}. It will leave in {$time} seconds.{$extended}
emergency-shuttle-good-luck = The Emergency Shuttle is unable to find a station. Good luck.
emergency-shuttle-nearby = The Emergency Shuttle is unable to find a valid docking port. It has warped in {$direction} of the station, {$location}. It will leave in {$time} seconds.{$extended}
emergency-shuttle-extended = {" "}Launch time has been extended due to inconvenient circumstances.

# Emergency shuttle console popup / announcement
emergency-shuttle-console-no-early-launches = Early launch is disabled
emergency-shuttle-console-auth-left = {$remaining} authorizations needed until shuttle is launched early.
emergency-shuttle-console-auth-revoked = Early launch authorization revoked, {$remaining} authorizations needed.
emergency-shuttle-console-denied = Access denied

# UI
emergency-shuttle-console-window-title = Emergency Shuttle Console
emergency-shuttle-ui-engines = ENGINES:
emergency-shuttle-ui-idle = Idle
emergency-shuttle-ui-repeal-all = Repeal All
emergency-shuttle-ui-early-authorize = Early Launch Authorization
emergency-shuttle-ui-authorize = AUTHORIZE
emergency-shuttle-ui-repeal = REPEAL
emergency-shuttle-ui-authorizations = Authorizations
emergency-shuttle-ui-remaining = Remaining: {$remaining}

# Map Misc.
map-name-centcomm = Central Command
map-name-terminal = Arrivals Terminal