using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class TerrorVentSmashComponent : Component
{
    [DataField]
    public SoundSpecifier SmashSound = new SoundPathSpecifier("/Audio/Effects/metal_crunch.ogg");
}
