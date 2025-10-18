using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wrestler.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class WrestlerSlamComponent : Component
{
    [DataField]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(3);

    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(3);

    [DataField]
    public SoundSpecifier? Sound = new SoundCollectionSpecifier("MachoRage");

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

    /// <summary>
    /// Damage dealt if target is being choked.
    /// </summary>
    [DataField]
    public DamageSpecifier DamageIfChokehold = new()
    {
        DamageDict = new()
        {
            { "Blunt", 25},
            { "Heat", 15}
        }
    };
}
