using Content.Server.Chat.Systems;
using Content.Server.Jittering;
using Content.Server.Popups;
using Content.Server.Speech.EntitySystems;
using Content.Server.Stunnable;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Heretic;
using Content.Shared.Popups;
using Content.Shared.Speech.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Store.Components;
using Robust.Shared.Audio.Systems;

namespace Content.Server.Heretic.EntitySystems;

public sealed class FeastOfOwlsSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly JitteringSystem _jitter = default!;
    [Dependency] private readonly StutteringSystem _stutter = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly HereticSystem _heretic = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var vocalQuery = GetEntityQuery<VocalComponent>();
        var query = EntityQueryEnumerator<FeastOfOwlsComponent, HereticComponent, StoreComponent, StatusEffectsComponent>();
        while (query.MoveNext(out var uid, out var comp, out var heretic, out var store, out var status))
        {
            if (comp.CurrentStep >= comp.Reward)
            {
                RemCompDeferred(uid, comp);
                continue;
            }

            comp.ElapsedTime += frameTime;

            if (comp.ElapsedTime < comp.Timer)
                continue;

            comp.ElapsedTime = 0f;

            if (comp.CurrentStep + 1 < comp.Reward && !_stun.TryParalyze(uid, comp.ParalyzeTime, true, status))
            {
                _heretic.UpdateKnowledge(uid, heretic, comp.Reward - comp.CurrentStep, store);
                RemCompDeferred(uid, comp);
                continue;
            }

            _jitter.DoJitter(uid, comp.JitterStutterTime, true, 10f, 10f,  true, status);
            _stutter.DoStutter(uid, comp.JitterStutterTime, true, status);

            if (vocalQuery.TryGetComponent(uid, out var vocal))
                _chat.TryEmoteWithChat(uid, vocal.ScreamId);

            _audio.PlayPvs(comp.KnowledgeGainSound, uid);

            _popup.PopupEntity(Loc.GetString("feast-of-owls-knowledge-gaim-message"), uid, uid, PopupType.LargeCaution);

            _heretic.UpdateKnowledge(uid, heretic, 1, store);

            comp.CurrentStep++;

            if (comp.CurrentStep < comp.Reward)
                continue;

            _status.TryRemoveStatusEffect(uid, "Stun", status);
            _status.TryRemoveStatusEffect(uid, "KnockedDown", status);
            RemCompDeferred(uid, comp);
        }
    }
}
