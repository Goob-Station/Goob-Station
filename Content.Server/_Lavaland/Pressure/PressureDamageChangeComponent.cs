// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Atmos;
namespace Content.Server._Lavaland.Pressure;

[RegisterComponent]
public sealed partial class PressureDamageChangeComponent : Component
{
    [DataField]
    public float LowerBound = 0;

    [DataField]
    public float UpperBound = Atmospherics.OneAtmosphere * 0.5f;

    [DataField]
    public bool ApplyWhenInRange = false;

    [DataField]
    public float AppliedModifier = 0.25f;

    [DataField]
    public bool ApplyToMelee = true;

    [DataField]
    public bool ApplyToProjectiles = true;
}