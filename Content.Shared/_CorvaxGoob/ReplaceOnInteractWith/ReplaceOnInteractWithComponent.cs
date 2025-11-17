using Content.Shared.DoAfter;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob.ReplaceOnInteractWith;

[RegisterComponent]
public sealed partial class ReplaceOnInteractWithComponent : Component
{
    [DataField("sound")]
    public SoundPathSpecifier? ReplaceSound;

    [DataField(required: true)]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklsit;

    [DataField]
    public float Delay = 5f;

    [DataField(required: true)]
    public EntProtoId? Prototype;
}

[Serializable, NetSerializable]
public sealed partial class ReplaceOnInteractWithDoAfterEvent : SimpleDoAfterEvent
{

}
