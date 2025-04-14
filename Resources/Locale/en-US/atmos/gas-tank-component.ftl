# SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
# SPDX-FileCopyrightText: 2021 Kara D <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Morb <14136326+Morb0@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
# SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

### GasTankComponent stuff.

# Examine text showing pressure in tank.
comp-gas-tank-examine = Pressure: [color=orange]{PRESSURE($pressure)}[/color].

# Examine text when internals are active.
comp-gas-tank-connected = It's connected to an external component.

# Examine text when valve is open or closed.
comp-gas-tank-examine-open-valve = Gas release valve is [color=red]open[/color].
comp-gas-tank-examine-closed-valve = Gas release valve is [color=green]closed[/color].

## ControlVerb
control-verb-open-control-panel-text = Open Control Panel

## UI
gas-tank-window-internals-toggle-button = Toggle
gas-tank-window-output-pressure-label = Output Pressure
gas-tank-window-tank-pressure-text = Pressure: {$tankPressure} kPA
gas-tank-window-internal-text = Internals: {$status}
gas-tank-window-internal-connected = [color=green]Connected[/color]
gas-tank-window-internal-disconnected = [color=red]Disconnected[/color]

## Valve
comp-gas-tank-open-valve = Open Valve
comp-gas-tank-close-valve = Close Valve
