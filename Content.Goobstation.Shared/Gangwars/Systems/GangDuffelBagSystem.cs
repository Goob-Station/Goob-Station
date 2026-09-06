using Content.Goobstation.Shared.Gangwars.Components;
using Content.Goobstation.Shared.Gangwars.Events;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Pulling.Events;
using Content.Shared.StatusEffectNew;

namespace Content.Goobstation.Shared.Gangwars.Systems;

/// <summary>
/// Handles the gang duffel bag trap.
/// Requires a do-after to un-trap it which allows it to be pulled / moved and opened.
/// </summary>
public sealed class GangDuffelBagSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GangDuffelBagComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<GangDuffelBagComponent, BeingPulledAttemptEvent>(OnBeingPulledAttempt);
        SubscribeLocalEvent<GangDuffelBagComponent, GangDuffelBagUntrapDoAfterEvent>(OnUntrapDoAfter);
    }

    private void OnInteractHand(Entity<GangDuffelBagComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        var user = args.User;
        var bag = ent.Comp;

        if (bag.State == GangDuffelBagState.Trapped)
        {
            args.Handled = true;
            var doAfterArgs = new DoAfterArgs(EntityManager, user, bag.UntrapTime, new GangDuffelBagUntrapDoAfterEvent(), ent, ent)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
                NeedHand = true,
            };
            _doAfter.TryStartDoAfter(doAfterArgs);
            _popup.PopupClient(Loc.GetString("gang-duffel-bag-untrapping"), ent, user);
        }
    }

    private void OnBeingPulledAttempt(Entity<GangDuffelBagComponent> ent, ref BeingPulledAttemptEvent args)
    {
        if (ent.Comp.State != GangDuffelBagState.Trapped)
            return;

        args.Cancel();
    }

    private void OnUntrapDoAfter(Entity<GangDuffelBagComponent> ent, ref GangDuffelBagUntrapDoAfterEvent args)
    {
        if (TerminatingOrDeleted(ent)
            || args.Cancelled
            || ent.Comp.State != GangDuffelBagState.Trapped)
            return;

        ent.Comp.State = GangDuffelBagState.Closed;
        Dirty(ent);
        _popup.PopupClient(Loc.GetString("gang-duffel-bag-untrapped"), ent, args.User);
        _status.TryAddStatusEffectDuration(args.User, GangDuffelBagComponent.TrappedStatusEffect, ent.Comp.SlowDuration);
        _movementSpeed.RefreshMovementSpeedModifiers(args.User);
    }

}
