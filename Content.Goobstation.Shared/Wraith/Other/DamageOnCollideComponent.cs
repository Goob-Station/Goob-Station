using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Other;

/// <summary>
/// Will damage the entity with DamageOnCollideComponent upon colliding with any entity from the whitelist (if empty will damage to any collision)
/// </summary>
// LUMMINAL IM GOING TO FUCKING CRUCIFY YOU MAKE A NORMAL SUMMARIES YOU CHUD -lucifer
[RegisterComponent, NetworkedComponent]
public sealed partial class DamageOnCollideComponent : Component
{
    [DataField(required: true)]
    public DamageSpecifier Damage;

    [DataField]
    public EntityWhitelist? IgnoreWhitelist = new();

    [DataField]
    public EntityWhitelist? Whitelist = new();

    /// <summary>
    /// If true, the damage is applied to the entity colliding with us (args.OtherEntity)
    /// If false (default), the damage is applied to us (ent.Owner)
    /// </summary>
    [DataField]
    public bool Inverted = false;
}
