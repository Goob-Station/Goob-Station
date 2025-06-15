// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;

namespace Content.Server._Lavaland.Pressure;

[RegisterComponent]
public sealed partial class PressureArmorChangeComponent : Component
{
    [DataField]
    public float LowerBound = Atmospherics.OneAtmosphere * 0.2f;

    [DataField]
    public float UpperBound = Atmospherics.OneAtmosphere * 0.5f;

    [DataField]
    public bool ApplyWhenInRange;

    [DataField]
    public float ExtraPenetrationModifier = 0.5f;
}
