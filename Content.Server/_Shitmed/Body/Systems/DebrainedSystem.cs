using Content.Server._Shitmed.DelayedDeath;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Body.Systems;
using Content.Server.Popups;
using Content.Shared.Speech;
using Content.Shared.Standing;
using Content.Shared.Stunnable;

namespace Content.Server._Shitmed.Body.Systems;

/// <summary>
///     This system handles behavior on entities when they lose their head or their brains are removed.
///     MindComponent fuckery should still be mainly handled on BrainSystem as usual.
/// </summary>
public sealed class DebrainedSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly StandingStateSystem _standingSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DebrainedComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<DebrainedComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<DebrainedComponent, SpeakAttemptEvent>(OnSpeakAttempt);
        SubscribeLocalEvent<DebrainedComponent, StandAttemptEvent>(OnStandAttempt);
    }

    private void OnComponentInit(EntityUid uid, DebrainedComponent _, ComponentInit args)
    {
        if (TerminatingOrDeleted(uid))
            return;

        EnsureComp<DelayedDeathComponent>(uid);
        EnsureComp<StunnedComponent>(uid);
        _standingSystem.Down(uid);
    }

    private void OnComponentRemove(EntityUid uid, DebrainedComponent _, ComponentRemove args)
    {
        if (TerminatingOrDeleted(uid))
            return;

        RemComp<DelayedDeathComponent>(uid);
        RemComp<StunnedComponent>(uid);
        if (_bodySystem.TryGetBodyOrganEntityComps<HeartComponent>(uid, out var _))
            RemComp<DelayedDeathComponent>(uid);
    }

    private void OnSpeakAttempt(EntityUid uid, DebrainedComponent _, SpeakAttemptEvent args)
    {
        _popupSystem.PopupEntity(Loc.GetString("speech-muted"), uid, uid);
        args.Cancel();
    }

    private void OnStandAttempt(EntityUid uid, DebrainedComponent _, StandAttemptEvent args)
    {
        args.Cancel();
    }
}
