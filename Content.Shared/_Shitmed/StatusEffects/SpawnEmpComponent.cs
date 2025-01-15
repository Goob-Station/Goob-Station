using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.StatusEffects;

/// <summary>
///     For use as a status effect. Spawns EMPs.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpawnEmpComponent : SpawnEntityEffectComponent
{
    public override string EntityPrototype { get; set; } = "AdminInstantEffectEMP7";
    public override bool AttachToParent { get; set; } = true;
}
