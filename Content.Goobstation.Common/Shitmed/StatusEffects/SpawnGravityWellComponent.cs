// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

/*
    2026-05-26
    All other shitmed status effect components have been moved into Content.Goobstation.Shared. This one
    remains here, since it is relied on by Content.Shared/Nutrition/EntitySystems/FoodSequenceSystem.cs
*/

using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Shitmed.StatusEffects;

/// <summary>
///     For use as a status effect. Spawns a gravity well.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpawnGravityWellComponent : SpawnEntityEffectComponent
{
    public override string EntityPrototype { get; set; } = "AdminInstantEffectGravityWell";
    public override bool AttachToParent { get; set; } = true;

    // Taken from GravityWellComponent
    [DataField]
    public float MaxRange;

    [DataField]
    public float MinRange = 0f;

    [DataField]
    public float BaseRadialAcceleration = 0.0f;

    [DataField]
    public float BaseTangentialAcceleration = 0.0f;
}
