# SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

logic-gate-examine = It is currently {INDEFINITE($gate)} {$gate} gate.

logic-gate-cycle = Switched to {INDEFINITE($gate)} {$gate} gate

power-sensor-examine = It is currently checking the network's {$output ->
    [true] output
    *[false] input
} battery.
power-sensor-voltage-examine = It is checking the {$voltage} power network.

power-sensor-switch = Switched to checking the network's {$output ->
    [true] output
    *[false] input
} battery.
power-sensor-voltage-switch = Switched network to {$voltage}!
