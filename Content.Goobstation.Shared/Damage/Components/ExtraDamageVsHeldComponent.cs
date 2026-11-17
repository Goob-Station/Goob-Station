using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Damage;

/// <summary>
/// Deals extra damage to a target when it is holding an item with the component that passes the whitelist.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ExtraDamageVsHeldComponent : Component
{
    /// <summary>
    /// Self explanatory.
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();

    /// <summary>
    /// Extra damage dealt.
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// True damage? Just like Ekko from heckin arcane!!!
    /// </summary>
    [DataField]
    public bool IgnoreResistances;
}
