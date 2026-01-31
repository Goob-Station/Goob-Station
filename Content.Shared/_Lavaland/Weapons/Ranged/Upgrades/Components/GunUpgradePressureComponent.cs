// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;

namespace Content.Shared._Lavaland.Weapons.Ranged.Upgrades.Components;

/// <summary>
/// Changes pressure modifiers on a weapon that supports them.
/// </summary>
[RegisterComponent]
public sealed partial class GunUpgradePressureComponent : Component
{
    [DataField]
    public float? NewLowerBound;

    [DataField]
    public float? NewUpperBound;

    [DataField]
    public bool? NewApplyWhenInRange = true;

    [DataField]
    public float? NewAppliedModifier = 2f;

    [ViewVariables]
    public float SavedLowerBound = Atmospherics.OneAtmosphere * 0.2f;

    [ViewVariables]
    public float SavedUpperBound = Atmospherics.OneAtmosphere * 0.5f;

    [ViewVariables]
    public bool SavedApplyWhenInRange = true;

    [ViewVariables]
    public float SavedAppliedModifier = 2f;
}
