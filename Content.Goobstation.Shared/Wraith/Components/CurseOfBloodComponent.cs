using Robust.Shared.Audio;
using Robust.Shared.GameStates;

[RegisterComponent, NetworkedComponent]
public sealed partial class CurseOfBloodComponent : Component
{
    [DataField]
    public SoundSpecifier? CurseSound = new SoundPathSpecifier("/Audio/_Goobstation/Wraith/wraithstaminadrain.ogg");
}
