using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.StatusEffects;

/// <summary>
///     For use as a status effect. Spawns slimes.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpawnSlimesComponent : SpawnEntityEffectComponent
{
    public override string EntityPrototype { get; set; } = "MobAdultSlimesBlueAngry";

    public override bool IsFriendly { get; set; } = true;
}
