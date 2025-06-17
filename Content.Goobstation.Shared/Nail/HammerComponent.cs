using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Nail;

[RegisterComponent, NetworkedComponent]
public sealed partial class HammerComponent : Component
{
    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Weapons/smash.ogg");

    [DataField]
    public float DamageMultiplier = 1;
}
