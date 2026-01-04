using Content.Server.Actions;
using Content.Server.Hands.Systems;
using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.EntityEffects;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.TouchSpell;

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
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TouchSpellActionComponent, ActionPerformedEvent>(OnEquipTouchSpell);

        SubscribeLocalEvent<TouchSpellComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<TouchSpellComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnEquipTouchSpell(Entity<TouchSpellActionComponent> ent, ref ActionPerformedEvent args)
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
            tsComp.AssociatedAction = ent.Owner;

        _actions.RemoveCooldown(ent.Owner);
        _charges.AddCharges(ent.Owner, 1); // jic
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
        if (action.HasValue && TryComp<ActionComponent>(action.Value, out var actionComp))
            ProcessAction(ent, (action.Value, actionComp));

        if (ent.Comp.QueueDelete)
            QueueDel(ent);
    }

    private void ProcessAction(Entity<TouchSpellComponent> ent, Entity<ActionComponent> action)
    {
        _actions.StartUseDelay(action.Owner);
        _charges.AddCharges(action.Owner, -1);
    }
}
