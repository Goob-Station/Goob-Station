using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.EntityEffects;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Map;
using Robust.Shared.Network;
using System.Linq;

namespace Content.Goobstation.Shared.TouchSpell;

/// <summary>
///     Generic system for touch spells.
/// </summary>
/// <remarks>
///     Acts as a relay.
///     
///     Action gets called
///     -> TouchSpell appears
///     -> Spell performs the action referencing the user.
/// </remarks>
public sealed partial class TouchSpellSystem : EntitySystem
{
    [Dependency] private readonly EntityEffectSystem _effects = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TouchSpellActionComponent, ActionAttemptEvent>(OnActionAttempt);

        SubscribeLocalEvent<TouchSpellComponent, InteractEvent>(OnInteract);
        SubscribeLocalEvent<TouchSpellComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnActionAttempt(Entity<TouchSpellActionComponent> ent, ref ActionAttemptEvent args)
    {
        // thank you aviu! *does a squat
        if (_net.IsClient) return;

        if (!TryComp<HandsComponent>(args.User, out var handsComp))
            return;

        // is there any touch spells that we're missing
        foreach (var held in _hands.EnumerateHeld((args.User, handsComp)))
        {
            if (TryComp<TouchSpellComponent>(held, out var tsc) && tsc.AssociatedAction == ent.Owner)
            {
                QueueDel(held);
                return;
            }
        }

        if (!_hands.TryGetEmptyHand((args.User, handsComp), out var emptyHand))
            return;

        var ts = PredictedSpawnAtPosition(ent.Comp.TouchSpell, Transform(ent).Coordinates);
        if (!_hands.TryPickup(args.User, ts, emptyHand, animate: false, handsComp: handsComp))
        {
            QueueDel(ts);
            return;
        }

        // bind action to the spell
        if (TryComp<TouchSpellComponent>(ts, out var tsComp))
        {
            tsComp.AssociatedAction = ent.Owner;
            tsComp.AssociatedPerformer = args.User;
        }

        // we cancel this one because we process the action manually.
        args.Cancelled = true;
    }

    private void OnInteract(Entity<TouchSpellComponent> ent, ref InteractEvent args)
    {
        if (!args.Target.HasValue || !args.CanReach || args.Target == args.User)
            return;

        Touch(ent, args.Target.Value, args.ClickLocation);

        args.Handled = true;
    }

    private void OnMeleeHit(Entity<TouchSpellComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count <= 0)
            return;

        var target = args.HitEntities[0];
        if (target == args.User)
            return;

        Touch(ent, target);

        args.Handled = true;
    }

    public void Touch(Entity<TouchSpellComponent> ent, EntityUid target, EntityCoordinates? clickLocation = null)
    {
        foreach (var effect in ent.Comp.Effects)
        {
            var baseEffect = new EntityEffectBaseArgs(target, EntityManager);
            var args = new TouchSpellEffectArgs(baseEffect, ent, clickLocation);

            if (effect.Conditions != null && effect.Conditions.Any(q => !q.Condition(args)))
                continue;

            _effects.Effect(effect, new TouchSpellEffectArgs(baseEffect, ent, clickLocation));
        }

        var action = ent.Comp.AssociatedAction;
        var performer = ent.Comp.AssociatedPerformer;
        if (action.HasValue && TryComp<ActionsComponent>(performer, out var actionComp))
            ProcessAction(ent, (performer.Value, actionComp), target);

        if (ent.Comp.QueueDelete)
            QueueDel(ent);
    }

    private void ProcessAction(Entity<TouchSpellComponent> ent, Entity<ActionsComponent> performer, EntityUid target)
    {
        if (ent.Comp.AssociatedAction == null) // somehow
            return;

        var action = ent.Comp.AssociatedAction!.Value;
        if (!TryComp<ActionComponent>(action, out var ac)
        || !TryComp<TouchSpellActionComponent>(action, out var tac)
        || tac.Event == null)
            return;

        tac.Event.Target = target;
        _actions.PerformAction(performer.Owner, (action, ac), tac.Event);
    }
}
