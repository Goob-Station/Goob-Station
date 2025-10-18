using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class FleshSurgeryComponent : Component, ITouchSpell
{
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(1);

    [DataField]
    public EntityUid? Action { get; set; }

    [DataField]
    public TimeSpan Cooldown { get; set; } = TimeSpan.FromSeconds(20);

    [DataField]
    public LocId Speech { get; set; } = "heretic-speech-flesh-surgery";

    [DataField]
    public SoundSpecifier? Sound { get; set; } = new SoundPathSpecifier("/Audio/Items/welder.ogg");
}
