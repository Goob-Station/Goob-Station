using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.EntityEffects;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.TouchSpell;

/// <summary>
///     Generic system for touch spells.
/// </summary>
/// <remarks>
///     Acts as a relay between actions (action gets called -> spell appears -> spell handles the action).
/// </remarks>
// ideally mansus grasp should also be using this one but ehhhhh @aviu @aviu @aviu
public sealed partial class TouchSpellSystem : EntitySystem
{
    [Dependency] private readonly EntityEffectSystem _effects = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TouchSpellActionComponent, EventTouchSpellInvoke>(OnInvokeTouchSpell);

        SubscribeLocalEvent<TouchSpellComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<TouchSpellComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnInvokeTouchSpell(Entity<TouchSpellActionComponent> ent, ref EventTouchSpellInvoke args)
    {
        if (!TryComp<HandsComponent>(ent, out var handsComp))
            return;

        // is there any touch spells that we're missing
        foreach (var held in _hands.EnumerateHeld((ent, handsComp)))
        {
            if (TryComp<TouchSpellComponent>(held, out var tsc) && tsc.AssociatedAction == ent.Owner)
            {
                QueueDel(held);
                return;
            }
        }

        if (!_hands.TryGetEmptyHand((ent, handsComp), out var emptyHand))
            return;

        var ts = Spawn(ent.Comp.TouchSpell, Transform(ent).Coordinates);
        if (!_hands.TryPickup(ent, ts, emptyHand, animate: false, handsComp: handsComp))
        {
            QueueDel(ts);
            return;
        }

        // bind action to the spell
        if (TryComp<TouchSpellComponent>(ts, out var tsComp))
        {
            tsComp.AssociatedAction = ent.Owner;
            tsComp.AssociatedPerformer = args.Performer;
        }

        // no args.Handled set because see ProcessAction()
    }

    private void OnAfterInteract(Entity<TouchSpellComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.Target.HasValue || !args.CanReach)
            return;

        Touch(ent, args.Target.Value, args.ClickLocation);

        args.Handled = true;
    }

    private void OnMeleeHit(Entity<TouchSpellComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count <= 0)
            return;

        Touch(ent, args.HitEntities[0]);

        args.Handled = true;
    }

    public void Touch(Entity<TouchSpellComponent> ent, EntityUid target, EntityCoordinates? clickLocation = null)
    {
        foreach (var effect in ent.Comp.Effects)
        {
            var baseEffect = new EntityEffectBaseArgs(target, EntityManager);
            _effects.Effect(effect, new TouchSpellEffectArgs(baseEffect, ent, clickLocation));
        }

        var action = ent.Comp.AssociatedAction;
        var performer = ent.Comp.AssociatedPerformer;
        if (action.HasValue && TryComp<ActionsComponent>(performer, out var actionComp))
            ProcessAction(ent, (performer.Value, actionComp));

        if (ent.Comp.QueueDelete)
            QueueDel(ent);
    }

    private void ProcessAction(Entity<TouchSpellComponent> ent, Entity<ActionsComponent> performer)
    {
        // manually invoke this shit
        var performed = new ActionPerformedEvent(performer);
        RaiseLocalEvent(ent.Comp.AssociatedAction!.Value, ref performed);
    }
}
