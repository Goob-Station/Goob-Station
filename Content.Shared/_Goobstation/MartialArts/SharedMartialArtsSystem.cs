using Content.Shared._Goobstation.MartialArts.Components;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.MartialArts;

/// <summary>
/// Provides shared Martial Arts API.
/// </summary>
public abstract partial class SharedMartialArtsSystem : EntitySystem
{

    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeKravMaga();
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, ShotAttemptedEvent>(OnShotAttempt);
        SubscribeLocalEvent<KravMagaSilencedComponent, SpeakAttemptEvent>(OnSilencedSpeakAttempt);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var kravSilencedQuery = EntityQueryEnumerator<KravMagaSilencedComponent>();
        while(kravSilencedQuery.MoveNext(out var ent, out var comp))
        {
            if (_timing.CurTime < comp.SilencedTime)
                continue;
            RemComp<KravMagaSilencedComponent>(ent);
        }

        var kravBlockedQuery = EntityQueryEnumerator<KravMagaBlockedBreathingComponent>();
        while(kravBlockedQuery.MoveNext(out var ent, out var comp))
        {
            if (_timing.CurTime < comp.BlockedTime)
                continue;
            RemComp<KravMagaBlockedBreathingComponent>(ent);
        }
    }

    private void OnSilencedSpeakAttempt(Entity<KravMagaSilencedComponent> ent, ref SpeakAttemptEvent args)
    {
        _popupSystem.PopupEntity(Loc.GetString("popup-grabbed-cant-speak"), ent, ent);   // You cant speak while someone is choking you
        args.Cancel();
    }

    private void OnShotAttempt(Entity<MartialArtsKnowledgeComponent> ent, ref ShotAttemptedEvent args)
    {
        if(ent.Comp.MartialArtsForm != MartialArtsForms.SleepingCarp)
            return;
        _popupSystem.PopupClient(Loc.GetString("gun-disabled"), ent, ent);
        args.Cancel();
    }
}
