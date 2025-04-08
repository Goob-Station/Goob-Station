using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.StatusEffects;

/// <summary>
///     For use as a status effect. Spawns a given entity prototype.
/// </summary>
public abstract partial class SpawnEntityEffectComponent : Component
{
    public virtual string EntityPrototype { get; set; }

    public virtual bool IsFriendly { get; set; }

    public virtual bool AttachToParent { get; set; }
}
