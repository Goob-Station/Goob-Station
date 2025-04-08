using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.ReverseBearTrap;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ReverseBearTrapComponent : Component
{
    [DataField, AutoNetworkedField]
    public float CountdownDuration; //Seconds

    [DataField, AutoNetworkedField]
    public EntityUid? Wearer;

    [DataField, AutoNetworkedField]
    public bool Ticking;

    [DataField, AutoNetworkedField]
    public TimeSpan ActivateTime;

    [DataField, AutoNetworkedField]
    public float CurrentEscapeChance;

    [DataField, AutoNetworkedField]
    public bool Struggling;

    [DataField, AutoNetworkedField]
    public EntityUid? LoopSoundStream { get; set; }

    [DataField("soundPath")]
    public SoundSpecifier LoopSound { get; set; } = new SoundPathSpecifier("/Audio/_Goobstation/Machines/clock_tick.ogg");

    [DataField("beepSoundPath")]
    public SoundSpecifier BeepSound { get; set; } = new SoundPathSpecifier("/Audio/_Goobstation/Machines/beep.ogg");

    [DataField("snapSoundPath")]
    public SoundSpecifier SnapSound { get; set; } = new SoundPathSpecifier("/Audio/_Goobstation/Effects/snap.ogg");

    [DataField]
    public SoundSpecifier StartCuffSound = new SoundPathSpecifier("/Audio/Items/Handcuffs/cuff_start.ogg");

    [DataField]
    public List<float>? DelayOptions = null;

    [DataField]
    public float BaseEscapeChance;
}
