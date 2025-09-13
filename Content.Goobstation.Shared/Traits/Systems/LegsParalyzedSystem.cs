using Content.Goobstation.Common.Traits;
using Content.Shared.Body.Systems;
using Content.Shared.Buckle.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Standing;
using Content.Shared.Throwing;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Traits.Assorted;

public sealed class LegsParalyzedSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;
    [Dependency] private readonly StandingStateSystem _standingSystem = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<LegsParalyzedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<LegsParalyzedComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<LegsParalyzedComponent, BuckledEvent>(OnBuckled);
        SubscribeLocalEvent<LegsParalyzedComponent, UnbuckledEvent>(OnUnbuckled);
        SubscribeLocalEvent<LegsParalyzedComponent, ThrowPushbackAttemptEvent>(OnThrowPushbackAttempt);
        SubscribeLocalEvent<LegsParalyzedComponent, StandAttemptEvent>(OnStandTry);
        SubscribeLocalEvent<LegsParalyzedComponent, DownedEvent>(OnDowned);
    }

    private void OnStartup(EntityUid uid, LegsParalyzedComponent component, ComponentStartup args)
    {
        // Completely paralyzed: cannot walk or run.
        _movementSpeedModifierSystem.ChangeBaseSpeed(uid, 0, 0, 20);
    }

    private void OnShutdown(EntityUid uid, LegsParalyzedComponent component, ComponentShutdown args)
    {
        _standingSystem.Stand(uid);
        _bodySystem.UpdateMovementSpeed(uid);
    }

    private void OnBuckled(EntityUid uid, LegsParalyzedComponent component, ref BuckledEvent args)
    {
        _standingSystem.Stand(uid);
        _movementSpeedModifierSystem.ChangeBaseSpeed(
            uid,
            component.CrawlMoveSpeed,
            component.CrawlMoveSpeed,
            component.CrawlMoveAcceleration);
    }

    private void OnUnbuckled(EntityUid uid, LegsParalyzedComponent component, ref UnbuckledEvent args)
    {
        _standingSystem.Down(uid);
    }

    private void OnDowned(EntityUid uid, LegsParalyzedComponent component, DownedEvent args)
    {
        _movementSpeedModifierSystem.ChangeBaseSpeed(
            uid,
            component.CrawlMoveSpeed,
            component.CrawlMoveSpeed,
            component.CrawlMoveAcceleration);
    }

    private void OnStandTry(EntityUid uid, LegsParalyzedComponent component, StandAttemptEvent args)
    {
        args.Cancel();
        _popupSystem.PopupClient(Loc.GetString("paralyzed-no-stand"), uid, uid, PopupType.Medium);
        _standingSystem.Down(uid);
    }

    private void OnThrowPushbackAttempt(EntityUid uid, LegsParalyzedComponent component, ThrowPushbackAttemptEvent args)
    {
        args.Cancel();
    }
}
