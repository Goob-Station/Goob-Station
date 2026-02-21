using Content.Goobstation.Shared.LightDetection.Components;
using Content.Goobstation.Shared.PhaseShift;
using Content.Shared.Actions;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.DoAfter;
using Content.Shared.Speech.Muting;

namespace Content.Trauma.Shared.ShadowDemon;

public sealed class ShadowCrawlSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowCrawlComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ShadowCrawlComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<ShadowCrawlComponent, ShadowCrawlEvent>(OnCrawl);

        SubscribeLocalEvent<LightDetectionComponent, ShadowCrawlAttemptEvent>(OnCrawlAttempt);

        SubscribeLocalEvent<LightDetectionDamageComponent, ShadowCrawlActivatedEvent>(OnCrawlActivated);
        SubscribeLocalEvent<LightDetectionDamageComponent, ShadowCrawlDeActivatedEvent>(OnCrawlDeactivated);
    }

    private void OnMapInit(Entity<ShadowCrawlComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.ActionUid = _actionsSystem.AddAction(ent.Owner, ent.Comp.Action);
    }

    private void OnShutdown(Entity<ShadowCrawlComponent> ent, ref ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(ent.Owner, ent.Comp.ActionUid);
    }

    private void OnCrawl(Entity<ShadowCrawlComponent> ent, ref ShadowCrawlEvent args)
    {
        // We are already in jaunt, try get out of it
        if (ent.Comp.Active)
        {
            var attemptEvExit = new ShadowCrawlAttemptEvent();
            RaiseLocalEvent(ent.Owner, ref attemptEvExit);
            if (attemptEvExit.Cancelled)
            {
                // TODO: Popup here
                return;
            }

            RemCompDeferred<PhaseShiftedComponent>(ent.Owner);
            ent.Comp.Active = false;
            Dirty(ent);

            var deactivateEv = new ShadowCrawlDeActivatedEvent(ent.Comp.DamageModiferFromLights);
            RaiseLocalEvent(ent.Owner, ref deactivateEv);

            RemComp<PacifiedComponent>(ent.Owner);
            RemComp<MutedComponent>(ent.Owner);

            // Re-enable all actions
            var actionsForActivation = _actionsSystem.GetActions(ent.Owner);
            foreach (var action in actionsForActivation)
            {
                if (action == args.Action)
                    continue;

                _actionsSystem.SetEnabled((action.Owner, action.Comp), true);
            }

            return;
        }

        // Okay, we aren't in jaunt, try to confirm we are ready to activate the jaunt
        var attemptEv = new ShadowCrawlAttemptEvent();
        RaiseLocalEvent(ent.Owner, ref attemptEv);
        if (attemptEv.Cancelled)
        {
            // TODO: Popup here
            return;
        }

        // Cool, we can now jaunt
        var phase = new PhaseShiftedComponent();
        phase.PhaseInEffect = ent.Comp.PhaseIn;
        phase.PhaseOutEffect = ent.Comp.PhaseOut;
        phase.MovementSpeedBuff = ent.Comp.SpeedBuff;
        EntityManager.AddComponent(ent.Owner, phase, true);

        ent.Comp.Active = true;
        Dirty(ent);

        // Notify the activation and ensure we get halved damage from lights
        var activateEv = new ShadowCrawlActivatedEvent(ent.Comp.DamageModiferFromLights);
        RaiseLocalEvent(ent.Owner, ref activateEv);

        EnsureComp<PacifiedComponent>(ent.Owner); // Ensures we don't attack while invisible
        EnsureComp<MutedComponent>(ent.Owner); // Ensures armok doesn't make another wizard video

        // Disable all actions
        var actions = _actionsSystem.GetActions(ent.Owner);
        foreach (var action in actions)
        {
            if (action == args.Action)
                continue;

            _actionsSystem.SetEnabled((action.Owner, action.Comp), false);
        }
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
}
