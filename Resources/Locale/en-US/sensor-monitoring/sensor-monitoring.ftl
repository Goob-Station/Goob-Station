# SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

sensor-monitoring-window-title = Sensor Monitoring Console

sensor-monitoring-value-display = {$unit ->
    [PressureKpa] { PRESSURE($value) }
    [PowerW] { POWERWATTS($value) }
    [EnergyJ] { POWERJOULES($value) }
    [TemperatureK] { TOSTRING($value, "N3") } K
    [Ratio] { NATURALPERCENT($value) }
    [Moles] { TOSTRING($value, "N3") } mol
    *[Other] { $value }
}

# ({ TOSTRING(SUB($value, 273.15), "N3") } °C)
