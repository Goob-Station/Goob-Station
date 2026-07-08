// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Traits;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Standing;
using Content.Shared.Throwing;
using Content.Shared.Popups;
using Content.Shared.Stunnable;

// Goob note: This is a core system in goobmod. I dont know why. Probably undo this shit.
// This shit is heavy goob edit god help you, if you are upstreaming take upstream and fix it later.
namespace Content.Goobstation.Shared.Traits.Systems;

public sealed class LegsParalyzedSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;
    [Dependency] private readonly StandingStateSystem _standingSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<LegsParalyzedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<LegsParalyzedComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<LegsParalyzedComponent, ThrowPushbackAttemptEvent>(OnThrowPushbackAttempt);
        SubscribeLocalEvent<LegsParalyzedComponent, StandUpAttemptEvent>(OnStandTry);
    }

    private void OnStartup(EntityUid uid, LegsParalyzedComponent component, ComponentStartup args)
    {
        if (!TryComp<StandingStateComponent>(uid, out var standing) || !HasComp<CrawlerComponent>(uid))
            return;
        if (standing.Standing)
        {
            UnparalyzeLegsAndThenForceThemToCrawlAnyway(uid, component);
            return;
        }

        if (HasComp<KnockedDownComponent>(uid)) // This shouldnt happen. I dont trust it though.
            return;
        _standingSystem.Stand(uid); // run it back.
        _stunSystem.TryCrawling(uid);
    }

    private void OnShutdown(EntityUid uid, LegsParalyzedComponent component, ComponentShutdown args)
    {


        if (!TryComp<KnockedDownComponent>(uid, out var knockedDown))
            return; // if you don't have knockdown and you are in fact 'knocked down' god help you cause this code can't
                    // That shit should never happen.

        RemComp(uid, knockedDown);
        _standingSystem.Stand(uid, force: true); // force is a bad idea but fuck it. If you get unparalyzed you deserve it.
    }

    private void UnparalyzeLegsAndThenForceThemToCrawlAnyway(EntityUid uid, LegsParalyzedComponent component)
    {
        // This made more sense originally but im gonna say that generally any base speed effects are tied to your legs being bad or some shit
        // so it makes sense to reset that if you're now paralyzed instead
        // i.e. going from cane user -> total paralysis
        // total paralysis is still worse cause im disallowing any standing and the crawlspeed should be worse.
        _movementSpeedModifierSystem.ChangeBaseSpeed(uid, MovementSpeedModifierComponent.DefaultBaseWalkSpeed, MovementSpeedModifierComponent.DefaultBaseSprintSpeed, MovementSpeedModifierComponent.DefaultAcceleration);
        _stunSystem.TryCrawling(uid);
    }

    // goob: using standUPattempt here cause if you use standattempthere (letting them do the doafter) knockdowncomp tries to shutdown at the end and shit is wack
    private void OnStandTry(EntityUid uid, LegsParalyzedComponent component, ref StandUpAttemptEvent args)
    {
        args.Autostand = false;
        args.Cancelled = true;
        _popupSystem.PopupClient(Loc.GetString("paralyzed-no-stand"), uid, uid, PopupType.Medium);
    }

    // Goobstation this shit never runs cause of throw code refactor but even if it did i dont understand the logic, but this is core so im leaving it.
    private void OnThrowPushbackAttempt(EntityUid uid, LegsParalyzedComponent component, ThrowPushbackAttemptEvent args)
    {
        args.Cancel();
    }
}
