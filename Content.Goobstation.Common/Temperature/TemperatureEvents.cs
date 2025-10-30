// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Temperature;

[ByRefEvent]
public record struct GetTemperatureThresholdsEvent(
    float HeatDamageThreshold,
    float ColdDamageThreshold,
    Dictionary<float, float>? SpeedThresholds);
