using Robust.Shared.Audio;

namespace Content.Server._Goobstation.Wizard.Components;

[RegisterComponent]
public sealed partial class UseDelayBlockKnockdownComponent : Component
{
    [DataField]
    public string Delay = "default";

    [DataField]
    public bool ResetDelayOnSuccess = true;

    [DataField]
    public SoundSpecifier? KnockdownSound = new SoundPathSpecifier("/Audio/Effects/Lightning/lightningbolt.ogg");

    [DataField]
    public bool DoSparks = true;
}
