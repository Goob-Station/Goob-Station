using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wrestler.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class WrestlerThrowComponent : Component
{
    [DataField]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(2);

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
}
