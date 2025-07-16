using Content.Shared.Inventory;

namespace Content.Shared._CorvaxGoob.TTS;

public sealed class TransformSpeakerVoiceEvent : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.MASK;
    public EntityUid Sender;
    public string VoiceId;

    public TransformSpeakerVoiceEvent(EntityUid sender, string voiceId)
    {
        Sender = sender;
        VoiceId = voiceId;
    }

}
