using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

/// <summary>
/// Component used for the vent smash action from terror spiders, which busts open a welded vent.
/// </summary>

[RegisterComponent, NetworkedComponent]
public sealed partial class TerrorVentSmashComponent : Component
{
    [DataField]
    public SoundSpecifier SmashSound = new SoundPathSpecifier("/Audio/Effects/metal_crunch.ogg");
}
