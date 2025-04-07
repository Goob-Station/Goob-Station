// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared._Lavaland.Weather;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Weather;

[RegisterComponent]
public sealed partial class LavalandStormedMapComponent : Component
{
    [DataField]
    public float Accumulator;

    [DataField]
    public ProtoId<LavalandWeatherPrototype> CurrentWeather;

    [DataField]
    public float Duration;

    [DataField]
    public float NextDamage = 10f;

    [DataField]
    public float DamageAccumulator;
}