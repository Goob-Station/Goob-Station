// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.StatusEffects;

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
