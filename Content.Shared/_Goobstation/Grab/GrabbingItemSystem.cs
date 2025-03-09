using System.Diagnostics.CodeAnalysis;
using Content.Shared.Body.Systems;
using Content.Shared.Cuffs;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Pulling.Events;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Grab;

public sealed class GrabbingItemSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GrabbingItemComponent, AttemptMeleeEvent>(OnMeleeAttempt);
        SubscribeLocalEvent<GrabbingItemComponent, ComponentRemove>(OnRemove);

        SubscribeLocalEvent<BeingGrabbedComponent, BeingPulledAttemptEvent>(OnBeingPulledAttempt);
        SubscribeLocalEvent<BeingGrabbedComponent, AttemptStopPullingEvent>(OnAttemptStopPulling,
            after: new[] { typeof(SharedBodySystem), typeof(SharedCuffableSystem) });
        SubscribeLocalEvent<BeingGrabbedComponent, GrabBreakDoAfterEvent>(OnGrabBreak);
        SubscribeLocalEvent<BeingGrabbedComponent, PullStoppedMessage>(OnStopPull);
    }

    private void OnStopPull(Entity<BeingGrabbedComponent> ent, ref PullStoppedMessage args)
    {
        if (TryComp(ent.Comp.GrabberItemUid, out GrabbingItemComponent? item))
        {
            item.GrabbedEntity = null;
            Dirty(ent.Comp.GrabberItemUid.Value, item);
        }

        RemCompDeferred<BeingGrabbedComponent>(ent);
    }

    private void OnGrabBreak(Entity<BeingGrabbedComponent> ent, ref GrabBreakDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        if (TryComp<PullableComponent>(ent.Owner, out var pullable))
            _pulling.TryStopPull(ent.Owner, pullable);
    }

    private void OnAttemptStopPulling(Entity<BeingGrabbedComponent> ent, ref AttemptStopPullingEvent args)
    {
        if (!TryComp(ent.Comp.GrabberItemUid, out GrabbingItemComponent? grabbingItem))
        {
            RemCompDeferred<BeingGrabbedComponent>(ent.Owner);
            return;
        }

        if (args.Cancelled)
            return;

        if (args.User == null || !Exists(args.User.Value))
            return;

        if (args.User.Value != ent.Owner)
            return;

        args.Cancelled = true;

        var doAfterArgs = new DoAfterArgs(EntityManager,
            ent.Owner,
            grabbingItem.GrabBreakDelay,
            new GrabBreakDoAfterEvent(),
            ent.Owner)
        {
            CancelDuplicate = false,
        };
        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnRemove(Entity<GrabbingItemComponent> ent, ref ComponentRemove args)
    {
        if (ent.Comp.GrabbedEntity != null)
            RemComp<BeingGrabbedComponent>(ent.Comp.GrabbedEntity.Value);
    }

    public bool TryGetGrabbingItem(EntityUid uid, [NotNullWhen(true)] out Entity<GrabbingItemComponent>? item)
    {
        item = null;
        foreach (var held in _handsSystem.EnumerateHeld(uid))
        {
            if (!TryComp(held, out GrabbingItemComponent? grabbingItem) || grabbingItem.GrabbedEntity != null)
                continue;

            item = (held, grabbingItem);
            return true;
        }

        return false;
    }

    private void OnBeingPulledAttempt(Entity<BeingGrabbedComponent> ent, ref BeingPulledAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnMeleeAttempt(Entity<GrabbingItemComponent> ent, ref AttemptMeleeEvent args)
    {
        var grabbed = ent.Comp.GrabbedEntity;

        if (grabbed == null || args.Cancelled)
            return;

        args.Cancelled = true;
        args.Message = Loc.GetString("grabbing-item-attack-fail",
            ("item", Identity.Entity(ent.Owner, EntityManager)),
            ("grabbed", Identity.Entity(grabbed.Value, EntityManager)));
    }
}

[Serializable, NetSerializable]
public sealed partial class GrabBreakDoAfterEvent : SimpleDoAfterEvent
{
}
