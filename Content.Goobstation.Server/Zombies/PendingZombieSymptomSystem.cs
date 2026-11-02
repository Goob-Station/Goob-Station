using Content.Shared.Chat;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Popups;
using Content.Shared.Speech.Components;
using Content.Shared.Zombies;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Zombies;

public sealed partial class PendingZombieSymptomSystem : EntitySystem
{
    [Dependency] private SharedChatSystem _chat = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    private static readonly LocId SymptomPopup = "zombie-infection-symptom";
    private static readonly ProtoId<EmotePrototype> CoughEmote = "Cough";

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<PendingZombieComponent>();
        while (query.MoveNext(out var uid, out var pending))
        {
            if (pending.GracePeriod > TimeSpan.Zero || now < pending.NextSymptom)
                continue;

            pending.NextSymptom = now + pending.SymptomInterval;

            var ev = new PendingZombieSymptomEvent(SymptomPopup);
            RaiseLocalEvent(uid, ref ev);
            _popup.PopupEntity(Loc.GetString(ev.Popup, ("target", uid)), uid, PopupType.SmallCaution);

            if (ev.Cough)
                PlayCough(uid);
        }
    }

    private void PlayCough(EntityUid uid)
    {
        if (!TryComp<VocalComponent>(uid, out var vocal)
            || vocal.EmoteSounds is not { } soundsId
            || !_proto.TryIndex(soundsId, out var sounds)
            || !sounds.Sounds.ContainsKey(CoughEmote))
            return;

        _chat.TryPlayEmoteSound(uid, sounds, CoughEmote);
    }
}

[ByRefEvent]
public record struct PendingZombieSymptomEvent(LocId Popup, bool Cough = true);
