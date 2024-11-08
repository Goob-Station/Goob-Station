using Content.Server.TTS;
using Content.Shared.VoiceMask;
using Content.Shared.Inventory;

namespace Content.Server.VoiceMask;

public partial class VoiceMaskSystem
{
    /*
    [Dependency] private readonly InventorySystem _inventory = default!;

    private void InitializeTTS()
    {
        SubscribeLocalEvent<VoiceMaskComponent, TransformSpeakerVoiceEvent>(OnSpeakerVoiceTransform);
        SubscribeLocalEvent<VoiceMaskComponent, VoiceMaskChangeVoiceMessage>(OnChangeVoice);
    }

    private void OnSpeakerVoiceTransform(Entity<VoiceMaskComponent> entity, TransformSpeakerVoiceEvent args)
    {
        // if (component.Enabled)
        //     args.VoiceId = component.VoiceId;
    }

    private void OnChangeVoice(Entity<VoiceMaskComponent> entity, VoiceMaskChangeVoiceMessage message)
    {
        // component.VoiceId = message.Voice;

        _popupSystem.PopupEntity(Loc.GetString("voice-mask-voice-popup-success"), uid);

        TrySetLastKnownVoice(uid, message.Voice);

        UpdateUI(uid);
    }

    private void TrySetLastKnownVoice(EntityUid maskWearer, string? voiceId)
    {
        if (!HasComp<VoiceMaskComponent>(maskWearer))
            return;

        // maskComp.LastSetVoice = voiceId;
    }
    */
}
