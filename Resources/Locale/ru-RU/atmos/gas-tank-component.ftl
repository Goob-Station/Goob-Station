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
comp-gas-tank-examine = Давление: [color=orange]{ PRESSURE($pressure) }[/color].

# Examine text when internals are active.
comp-gas-tank-connected = Он подключён к внешнему компоненту.

# Examine text when valve is open or closed.
comp-gas-tank-examine-open-valve = Клапан выпуска газа [color=red]открыт[/color].
comp-gas-tank-examine-closed-valve = Клапан выпуска газа [color=green]закрыт[/color].

## ControlVerb
control-verb-open-control-panel-text = Открыть панель управления

## UI
gas-tank-window-internals-toggle-button = Переключить
gas-tank-window-output-pressure-label = Выходное давление
gas-tank-window-tank-pressure-text = Давление: { $tankPressure } кПа
gas-tank-window-internal-text = Маска: { $status }
gas-tank-window-internal-connected = [color=green]Подключена[/color]
gas-tank-window-internal-disconnected = [color=red]Не подключена[/color]

## Valve
comp-gas-tank-open-valve = Открыть клапан
comp-gas-tank-close-valve = Закрыть клапан
