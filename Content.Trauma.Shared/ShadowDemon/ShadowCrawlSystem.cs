using Content.Goobstation.Shared.LightDetection.Components;
using Content.Goobstation.Shared.PhaseShift;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.CombatMode;
using Robust.Shared.Network;

namespace Content.Trauma.Shared.ShadowDemon;

public sealed class ShadowCrawlSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedCombatModeSystem _combat = default!;
    [Dependency] private readonly INetManager _net = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowCrawlComponent, ShadowCrawlEvent>(OnCrawl);

        SubscribeLocalEvent<LightDetectionComponent, ShadowCrawlAttemptEvent>(OnCrawlAttempt);

        SubscribeLocalEvent<LightDetectionDamageComponent, ShadowCrawlActivatedEvent>(OnCrawlActivated);
        SubscribeLocalEvent<LightDetectionDamageComponent, ShadowCrawlDeActivatedEvent>(OnCrawlDeactivated);
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
            _actionsSystem.SetCooldown(args.Action.Owner, ent.Comp.ActionCooldown);

            args.Handled = true;
            return;
        }

        // Okay, we aren't in jaunt, try to confirm we are ready to activate the jaunt
        if (!CanJaunt(ent.Owner))
            return;

        if (_net.IsClient)
            return;

        // Cool, we can now jaunt
        var phase = new PhaseShiftedComponent();
        phase.PhaseInEffect = ent.Comp.PhaseIn;
        phase.PhaseOutEffect = ent.Comp.PhaseOut;
        phase.MovementSpeedBuff = ent.Comp.SpeedBuff;
        AddComp(ent.Owner, phase);

        ent.Comp.Active = true;
        Dirty(ent);

        // Notify the activation and ensure we get halved damage from lights
        var activateEv = new ShadowCrawlActivatedEvent(ent.Comp.DamageModiferFromLights);
        RaiseLocalEvent(ent.Owner, ref activateEv);

        // Disable all actions while in jaunt except the jaunt itself
        ToggleActions(args.Action, ent.Owner, false);

        // Ensures we don't attack people while invisible
        _combat.SetInCombatMode(ent.Owner, false);

        args.Handled = true;
    }

    private void OnCrawlAttempt(Entity<LightDetectionComponent> ent, ref ShadowCrawlAttemptEvent args)
    {
        if (ent.Comp.OnLight)
            args.Cancelled = true;
    }

    private void OnCrawlActivated(Entity<LightDetectionDamageComponent> ent, ref ShadowCrawlActivatedEvent args)
    {
        ent.Comp.DamageToDeal *= args.LightDamageModifier;
        Dirty(ent);
    }

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
            // TODO: Popup here
            return false;
        }

        return true;
    }

    private void ToggleActions(Entity<ActionComponent> ignoreAction, EntityUid uid, bool toggle)
    {
        var actions = _actionsSystem.GetActions(uid);
        foreach (var action in actions)
        {
            if (action == ignoreAction)
                continue;

            _actionsSystem.SetEnabled((action.Owner, action.Comp), toggle);
        }
    }

    #endregion
}
