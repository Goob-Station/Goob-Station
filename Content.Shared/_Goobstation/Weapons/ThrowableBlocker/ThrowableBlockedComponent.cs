using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Weapons.ThrowableBlocker;

/// <summary>
/// Added to objects that can be blocked by ThrowableBlockerComponent
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ThrowableBlockedComponent : Component
{
    [DataField]
    public BlockBehavior Behavior = BlockBehavior.KnockOff;

    /// <summary>
    /// How much damage will the entity take on block if Behavior is Damage
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new();
}

public enum BlockBehavior : byte
{
    KnockOff = 0,
    Damage,
    Destroy,
}
