using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.StatusEffects;

/// <summary>
///     For use as a status effect. Creates the same effect as a flash grenade.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpawnFlashComponent : SpawnEntityEffectComponent
{
    public override string EntityPrototype { get; set; } = "AdminInstantEffectFlash";
    public override bool AttachToParent { get; set; } = true;
}
