using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MakeRevenantComponent : Component
{
    [DataField]
    public SoundSpecifier? PossessSound = new SoundPathSpecifier("/Audio/_Goobstation/Wraith/wraithlivingobject.ogg");

    [DataField]
    public SoundSpecifier? PossessEndSound = new SoundPathSpecifier("/Audio/_Goobstation/Wraith/wraithleaveobject.ogg");

    /// <summary>
    /// How long the Haunted component lasts until it deletes itself.
    /// </summary>
    [DataField]
    public TimeSpan PossessTimer = TimeSpan.FromSeconds(60);
}
