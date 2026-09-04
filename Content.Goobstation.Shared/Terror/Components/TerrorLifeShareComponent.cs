using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Sacrifice some health to heal another entity.
/// </summary>
[RegisterComponent]
public sealed partial class TerrorLifeShareComponent : Component
{
    /// <summary>
    /// Damage to deal as a cost for healing.
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier SelfCost = new();

    /// <summary>
    /// Amount to heal.
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier HealAmount = new();

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Voice/Slime/slime_schlorp.ogg");
}
