using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Hastur.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HasturLashComponent : Component
{
    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(2);

    [DataField]
    public float BleedAmount = 50f;

    [DataField]
    public SoundSpecifier? LashSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/Hastur/tentacle_hit.ogg");
}
