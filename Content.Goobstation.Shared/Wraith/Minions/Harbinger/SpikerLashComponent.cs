using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Minions.Harbinger;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpikerLashComponent : Component
{
    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(1.5);

    [DataField]
    public float BleedAmount = 25f;

    [DataField]
    public SoundSpecifier? LashSound = new SoundPathSpecifier("/Audio/_Goobstation/Wraith/Attack/Flesh_Stab_1.ogg");
}
