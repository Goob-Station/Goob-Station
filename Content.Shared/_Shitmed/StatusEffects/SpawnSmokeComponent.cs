using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.StatusEffects;

/// <summary>
///     For use as a status effect. Spawns a smoke cloud.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpawnSmokeComponent : SpawnEntityEffectComponent
{
    public override string EntityPrototype { get; set; } = "AdminInstantEffectSmoke10";
    public override bool AttachToParent { get; set; } = true;
}
