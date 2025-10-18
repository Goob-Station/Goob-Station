using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wrestler.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class WrestlerKickComponent : Component
{
    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(2);

    [DataField]
    public SoundSpecifier? Sound = new SoundCollectionSpecifier("MachoRage");

    /// <summary>
    /// Throw speed
    /// </summary>
    [DataField]
    public float ThrowSpeed = 30f;

    /// <summary>
    /// Damage to deal to the target when colliding once thrown
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier? DamageWhenThrown = new()
    {
        DamageDict = new()
        {
            { "Blunt", 15}
        }
    };

    /// <summary>
    /// Damage dealt.
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "Blunt", 15}
        }
    };
}
