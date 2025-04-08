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
}
