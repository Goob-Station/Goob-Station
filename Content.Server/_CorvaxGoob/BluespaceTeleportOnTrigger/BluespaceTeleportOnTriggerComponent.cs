using Robust.Shared.Audio;

namespace Content.Server._CorvaxGoob.BluespaceTeleportOnTriggerOnTrigger;

[RegisterComponent]
public sealed partial class BluespaceTeleportOnTriggerComponent : Component
{
    [DataField]
    public int Range = 6;

    [DataField]
    public float Probability = 0.7f;

    [DataField]
    public SoundSpecifier TeleportSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");
}
