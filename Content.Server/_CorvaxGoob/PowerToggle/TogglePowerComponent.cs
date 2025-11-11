using Content.Shared._CorvaxGoob.PowerToggle;
using Robust.Shared.Audio;

namespace Content.Server._CorvaxGoob.PowerToggle;

[RegisterComponent]
public sealed partial class TogglePowerComponent : SharedTogglePowerComponent
{
    [DataField("turnOnSound")]
    public SoundSpecifier? TurnOnSound = new SoundPathSpecifier("/Audio/_CorvaxGoob/Machine/terminal_on.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.05f)
    };

    [DataField("turnOffSound")]
    public SoundSpecifier? TurnOffSound = new SoundPathSpecifier("/Audio/_CorvaxGoob/Machine/terminal_off.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.05f)
    };

    [DataField("applyPowerOnSpawn")]
    public bool ApplyPowerOnSpawn = false;
}
