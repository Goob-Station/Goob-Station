using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Specific.Components;
using Content.Shared.Speech;

namespace Content.Goobstation.Shared.MartialArts.Specific;

public sealed partial class KravMagaSystem : EntitySystem
{
    public override void Update(float frameTime)
    {
        var kravSilencedQuery = EntityQueryEnumerator<KravMagaSilencedComponent>();
        while (kravSilencedQuery.MoveNext(out var ent, out var comp))
        {
            if (_timing.CurTime < comp.SilencedTime)
                continue;
            RemCompDeferred(ent, comp);
        }

        var kravBlockedQuery = EntityQueryEnumerator<KravMagaBlockedBreathingComponent>();
        while (kravBlockedQuery.MoveNext(out var ent, out var comp))
        {
            if (_timing.CurTime < comp.BlockedTime)
                continue;
            RemCompDeferred(ent, comp);
        }
    }

    [SubscribeLocalEvent]
    private void OnSilencedSpeakAttempt(Entity<KravMagaSilencedComponent> ent, ref SpeakAttemptEvent args)
    {
        _popup.PopupEntity(Loc.GetString("popup-grabbed-cant-speak"),
            ent,
            ent); // You cant speak while someone is choking you
        args.Cancel();
    }
}
