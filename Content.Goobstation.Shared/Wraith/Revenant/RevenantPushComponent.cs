using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Revenant;

[RegisterComponent, NetworkedComponent]
public sealed partial class RevenantPushComponent : Component
{
    [DataField]
    public SoundSpecifier? RevPushSound = new SoundCollectionSpecifier("Rev_Push");

    /// <summary>
    /// Throw speed
    /// </summary>
    [DataField]
    public float ThrowSpeed = 30f;

    /// <summary>
    /// Damage to deal to the target when colliding once thrown
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier? DamageWhenThrown = new();

}
