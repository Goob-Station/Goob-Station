using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CurseOfRotComponent : Component
{
    [DataField]
    public SoundSpecifier? CurseSound = new SoundPathSpecifier("/Audio/_Goobstation/Wraith/wraithstaminadrain.ogg");
}
