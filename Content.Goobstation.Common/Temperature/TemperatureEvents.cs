// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Temperature;

[ByRefEvent]
public record struct GetTemperatureThresholdsEvent(
    float HeatDamageThreshold,
    float ColdDamageThreshold,
    Dictionary<float, float>? SpeedThresholds);

[ByRefEvent]
public record struct GetCurrentTemperatureEvent(
    float? CurrentTemperature
    );
