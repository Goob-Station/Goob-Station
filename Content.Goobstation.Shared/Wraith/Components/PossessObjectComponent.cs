using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class PossessObjectComponent : Component
{
    [DataField]
    public SoundSpecifier? PossessSound = new SoundPathSpecifier("/Audio/_Goobstation/Wraith/wraithlivingobject.ogg");

    [DataField]
    public TimeSpan PossessDuration = TimeSpan.FromSeconds(30f);
}
