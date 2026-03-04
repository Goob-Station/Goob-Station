// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.LightDetection.Components;
using Content.Goobstation.Shared.PhaseShift;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.CombatMode;
using Content.Shared.Popups;

namespace Content.Trauma.Shared.ShadowDemon;

public sealed class ShadowCrawlSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedCombatModeSystem _combat = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowCrawlComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ShadowCrawlComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<ShadowCrawlComponent, ShadowCrawlEvent>(OnCrawl);

        SubscribeLocalEvent<ShadowCrawlComponent, ShootGrappleEvent>(OnGrappleShot);

        SubscribeLocalEvent<LightDetectionComponent, ShadowCrawlAttemptEvent>(OnCrawlAttempt);

        SubscribeLocalEvent<LightDetectionDamageComponent, ShadowCrawlActivatedEvent>(OnCrawlActivated);
        SubscribeLocalEvent<LightDetectionDamageComponent, ShadowCrawlDeActivatedEvent>(OnCrawlDeactivated);
    }

    private void OnMapInit(Entity<ShadowCrawlComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionUid, ent.Comp.ActionId);
        Dirty(ent);
    }

    private void OnShutdown(Entity<ShadowCrawlComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionUid);
        Dirty(ent);
    }

    private void OnCrawl(Entity<ShadowCrawlComponent> ent, ref ShadowCrawlEvent args)
    {
        // We are already in jaunt, try get out of it
        if (ent.Comp.Active)
        {
            if (!CanJaunt(ent.Owner))
                return;

            RemCompDeferred<PhaseShiftedComponent>(ent.Owner);
            ent.Comp.Active = false;
            Dirty(ent);

            var deactivateEv = new ShadowCrawlDeActivatedEvent(ent.Comp.DamageModiferFromLights);
            RaiseLocalEvent(ent.Owner, ref deactivateEv);

            // Re-enable all actions
            ToggleActions(args.Action, ent.Owner, true);

            // Activate cooldown only when exiting jaunt
            _actions.SetCooldown(args.Action.Owner, ent.Comp.ActionCooldown);

            return;
        }

        // Okay, we aren't in jaunt, try to confirm we are ready to activate the jaunt
        if (!CanJaunt(ent.Owner))
            return;

        var phase = new PhaseShiftedComponent();
        phase.PhaseInEffect = ent.Comp.PhaseIn;
        phase.PhaseOutEffect = ent.Comp.PhaseOut;
        phase.RevealOnDamage = false;
        AddComp(ent.Owner, phase, true);

        ent.Comp.Active = true;
        Dirty(ent);

        // Notify the activation and ensure we get halved damage from lights
        var activateEv = new ShadowCrawlActivatedEvent(ent.Comp.DamageModiferFromLights);
        RaiseLocalEvent(ent.Owner, ref activateEv);

        // Disable all actions while in jaunt except the jaunt itself
        ToggleActions(args.Action, ent.Owner, false);

        // Ensures we don't attack people while invisible
        _combat.SetInCombatMode(ent.Owner, false);

        _popup.PopupClient(Loc.GetString("shadow-crawl-success"), ent.Owner, ent.Owner, PopupType.Medium);

        args.Handled = true;
    }

    /// <summary>
    /// Add cooldown to shadow crawl when shooting a shadow grapple
    /// </summary>
    private void OnGrappleShot(Entity<ShadowCrawlComponent> ent, ref ShootGrappleEvent args) =>
        _actions.SetCooldown(ent.Comp.ActionUid, ent.Comp.ActionCooldownAfterGrapple);

    /// <summary>
    /// Ensures we are on darkness before attempting a crawl
    /// </summary>
    private void OnCrawlAttempt(Entity<LightDetectionComponent> ent, ref ShadowCrawlAttemptEvent args)
    {
        if (ent.Comp.OnLight)
            args.Cancelled = true;
    }

    /// <summary>
    /// Decreases the damage we take from lights by a number
    /// </summary>
    private void OnCrawlActivated(Entity<LightDetectionDamageComponent> ent, ref ShadowCrawlActivatedEvent args)
    {
        ent.Comp.DamageToDeal *= args.LightDamageModifier;
        Dirty(ent);
    }

    /// <summary>
    /// Increases the damage we take from lights by a number
    /// </summary>
    private void OnCrawlDeactivated(Entity<LightDetectionDamageComponent> ent, ref ShadowCrawlDeActivatedEvent args)
    {
        ent.Comp.DamageToDeal /= args.LightDamageModifier;
        Dirty(ent);
    }

    #region Helpers
    private bool CanJaunt(EntityUid uid)
    {
        var attemptEv = new ShadowCrawlAttemptEvent();
        RaiseLocalEvent(uid, ref attemptEv);
        if (attemptEv.Cancelled)
        {
            _popup.PopupClient(Loc.GetString("shadow-crawl-cancelled"), uid, uid, PopupType.MediumCaution);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Disables or enables all actions on the user for use.
    /// This is meant to prevent using actions while in jaunt
    /// </summary>
    private void ToggleActions(Entity<ActionComponent> ignoreAction, EntityUid uid, bool toggle)
    {
        var actions = _actions.GetActions(uid);
        foreach (var action in actions)
        {
            if (action == ignoreAction)
                continue;

            _actions.SetEnabled((action.Owner, action.Comp), toggle);
        }
    }

    #endregion
}
