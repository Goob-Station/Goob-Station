using Content.Shared.ActionBlocker;
using Content.Shared.Buckle.Components;
using Content.Shared.Climbing.Events;
using Content.Shared.DoAfter;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Nyanotrasen.Item.PseudoItem;
using Content.Shared.Popups;
using Content.Shared.Resist;
using Content.Shared.Standing;
using Content.Shared.Storage;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Verbs;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using System.Numerics;

namespace Content.Shared._DV.Carrying;

public sealed class CarryingSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly CarryingSlowdownSystem _slowdown = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPseudoItemSystem _pseudoItem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StandingStateSystem _standingState = default!;
    [Dependency] private readonly SharedVirtualItemSystem  _virtualItem = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();

        _physicsQuery = GetEntityQuery<PhysicsComponent>();

        SubscribeLocalEvent<CarriableComponent, GetVerbsEvent<AlternativeVerb>>(AddCarryVerb);
        SubscribeLocalEvent<CarryingComponent, GetVerbsEvent<InnateVerb>>(AddInsertCarriedVerb);
        SubscribeLocalEvent<CarryingComponent, VirtualItemDeletedEvent>(OnVirtualItemDeleted);
        SubscribeLocalEvent<CarryingComponent, BeforeThrowEvent>(OnThrow);
        SubscribeLocalEvent<CarryingComponent, EntParentChangedMessage>(OnParentChanged);
        SubscribeLocalEvent<CarryingComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<BeingCarriedComponent, InteractionAttemptEvent>(OnInteractionAttempt);
        SubscribeLocalEvent<BeingCarriedComponent, UpdateCanMoveEvent>(OnMoveAttempt);
        SubscribeLocalEvent<BeingCarriedComponent, StandAttemptEvent>(OnStandAttempt);
        SubscribeLocalEvent<BeingCarriedComponent, GettingInteractedWithAttemptEvent>(OnInteractedWith);
        SubscribeLocalEvent<BeingCarriedComponent, PullAttemptEvent>(OnPullAttempt);
        SubscribeLocalEvent<BeingCarriedComponent, StartClimbEvent>(OnDrop);
        SubscribeLocalEvent<BeingCarriedComponent, BuckledEvent>(OnDrop);
        SubscribeLocalEvent<BeingCarriedComponent, UnbuckledEvent>(OnDrop);
        SubscribeLocalEvent<BeingCarriedComponent, StrappedEvent>(OnDrop);
        SubscribeLocalEvent<BeingCarriedComponent, UnstrappedEvent>(OnDrop);
        SubscribeLocalEvent<BeingCarriedComponent, EscapeInventoryEvent>(OnDrop);
        SubscribeLocalEvent<CarriableComponent, CarryDoAfterEvent>(OnDoAfter);
    }

    private void AddCarryVerb(Entity<CarriableComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        var user = args.User;
        var target = args.Target;
        if (!args.CanInteract || !args.CanAccess || user == target)
            return;

        if (!CanCarry(user, ent))
            return;

        args.Verbs.Add(new AlternativeVerb()
        {
            Act = () => StartCarryDoAfter(user, ent),
            Text = Loc.GetString("carry-verb"),
            Priority = 2
        });
    }

    private void AddInsertCarriedVerb(Entity<CarryingComponent> ent, ref GetVerbsEvent<InnateVerb> args)
    {
        // If the person is carrying someone, and the carried person is a pseudo-item, and the target entity is a storage,
        // then add an action to insert the carried entity into the target
        // AKA put carried felenid into a duffelbag
        if (args.Using is not {} carried || !args.CanAccess || !TryComp<PseudoItemComponent>(carried, out var pseudoItem))
            return;

        var target = args.Target;
        if (!TryComp<StorageComponent>(target, out var storageComp))
            return;

        if (!_pseudoItem.CheckItemFits((carried, pseudoItem), (target, storageComp)))
            return;

        args.Verbs.Add(new InnateVerb()
        {
            Act = () =>
            {
                DropCarried(ent, carried);
                _pseudoItem.TryInsert(target, carried, pseudoItem, storageComp);
            },
            Text = Loc.GetString("action-name-insert-other", ("target", carried)),
            Priority = 2
        });
    }

    /// <summary>
    /// Since the carried entity is stored as 2 virtual items, when deleted we want to drop them.
    /// </summary>
    private void OnVirtualItemDeleted(Entity<CarryingComponent> ent, ref VirtualItemDeletedEvent args)
    {
        if (HasComp<CarriableComponent>(args.BlockingEntity))
            DropCarried(ent, args.BlockingEntity);
    }

    /// <summary>
    /// Basically using virtual item passthrough to throw the carried person. A new age!
    /// Maybe other things besides throwing should use virt items like this...
    /// </summary>
    private void OnThrow(Entity<CarryingComponent> ent, ref BeforeThrowEvent args)
    {
        if (!TryComp<VirtualItemComponent>(args.ItemUid, out var virtItem) || !HasComp<CarriableComponent>(virtItem.BlockingEntity))
            return;

        var carried = virtItem.BlockingEntity;
        args.ItemUid = carried;

        args.ThrowSpeed = 5f * MassContest(ent, carried);
    }

    private void OnParentChanged(Entity<CarryingComponent> ent, ref EntParentChangedMessage args)
    {
        var xform = Transform(ent);
        if (xform.MapUid != args.OldMapId)
            return;

        // Do not drop the carried entity if the new parent is a grid
        if (xform.ParentUid == xform.GridUid)
            return;

        DropCarried(ent, ent.Comp.Carried);
    }

    private void OnMobStateChanged(Entity<CarryingComponent> ent, ref MobStateChangedEvent args)
    {
        DropCarried(ent, ent.Comp.Carried);
    }

    /// <summary>
    /// Only let the person being carried interact with their carrier and things on their person.
    /// </summary>
    private void OnInteractionAttempt(Entity<BeingCarriedComponent> ent, ref InteractionAttemptEvent args)
    {
        if (args.Target is not {} target)
            return;

        var targetParent = Transform(target).ParentUid;

        var carrier = ent.Comp.Carrier;
        if (target != carrier && targetParent != carrier && targetParent != ent.Owner)
            args.Cancelled = true;
    }

    private void OnMoveAttempt(Entity<BeingCarriedComponent> ent, ref UpdateCanMoveEvent args)
    {
        args.Cancel();
    }

    private void OnStandAttempt(Entity<BeingCarriedComponent> ent, ref StandAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnInteractedWith(Entity<BeingCarriedComponent> ent, ref GettingInteractedWithAttemptEvent args)
    {
        if (args.Uid != ent.Comp.Carrier)
            args.Cancelled = true;
    }

    private void OnPullAttempt(Entity<BeingCarriedComponent> ent, ref PullAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnDrop<TEvent>(Entity<BeingCarriedComponent> ent, ref TEvent args) // Augh
    {
        DropCarried(ent.Comp.Carrier, ent);
    }

    private void OnDoAfter(Entity<CarriableComponent> ent, ref CarryDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (!CanCarry(args.Args.User, ent))
            return;

        Carry(args.Args.User, ent);
        args.Handled = true;
    }

    private void StartCarryDoAfter(EntityUid carrier, Entity<CarriableComponent> carried)
    {
        TimeSpan length = GetPickupDuration(carrier, carried);

        if (length.TotalSeconds >= 9f)
        {
            _popup.PopupClient(Loc.GetString("carry-too-heavy"), carried, carrier, PopupType.SmallCaution);
            return;
        }

        if (!HasComp<KnockedDownComponent>(carried))
            length *= 2f;

        var ev = new CarryDoAfterEvent();
        var args = new DoAfterArgs(EntityManager, carrier, length, ev, carried, target: carried)
        {
            BreakOnMove = true,
            NeedHand = true,
            MultiplyDelay = false, // Goobstation
        };

        _doAfter.TryStartDoAfter(args);

        // Show a popup to the person getting picked up
        _popup.PopupEntity(Loc.GetString("carry-started", ("carrier", carrier)), carried, carried);
    }

    private void Carry(EntityUid carrier, EntityUid carried)
    {
        if (TryComp<PullableComponent>(carried, out var pullable))
            _pulling.TryStopPull(carried, pullable);

        var carrierXform = Transform(carrier);
        var xform = Transform(carried);
        _transform.AttachToGridOrMap(carrier, carrierXform);
        _transform.AttachToGridOrMap(carried, xform);
        _transform.SetParent(carried, xform, carrier, carrierXform);

        var carryingComp = EnsureComp<CarryingComponent>(carrier);
        carryingComp.Carried = carried;
        Dirty(carrier, carryingComp);
        var carriedComp = EnsureComp<BeingCarriedComponent>(carried);
        carriedComp.Carrier = carrier;
        Dirty(carried, carriedComp);
        EnsureComp<KnockedDownComponent>(carried);

        ApplyCarrySlowdown(carrier, carried);

        _actionBlocker.UpdateCanMove(carried);

        if (_net.IsClient) // no spawning prediction
            return;

        _virtualItem.TrySpawnVirtualItemInHand(carried, carrier);
        _virtualItem.TrySpawnVirtualItemInHand(carried, carrier);
    }

    public bool TryCarry(EntityUid carrier, Entity<CarriableComponent?> toCarry)
    {
        if (!Resolve(toCarry, ref toCarry.Comp, false))
            return false;

        if (!CanCarry(carrier, (toCarry, toCarry.Comp)))
            return false;

        // The second one means that carrier is a pseudo-item and is inside a bag.
        if (HasComp<BeingCarriedComponent>(carrier) || HasComp<ItemComponent>(carrier))
            return false;

        if (GetPickupDuration(carrier, toCarry).TotalSeconds > 9f)
            return false;

        Carry(carrier, toCarry);
        return true;
    }

    public void DropCarried(EntityUid carrier, EntityUid carried)
    {
        Drop(carried);
        RemComp<CarryingComponent>(carrier); // get rid of this first so we don't recursively fire that event
        RemComp<CarryingSlowdownComponent>(carrier);
        _virtualItem.DeleteInHandsMatching(carrier, carried);
        _movementSpeed.RefreshMovementSpeedModifiers(carrier);
    }

    private void Drop(EntityUid carried)
    {
        RemComp<BeingCarriedComponent>(carried);
        RemComp<KnockedDownComponent>(carried); // TODO SHITMED: make sure this doesnt let you make someone with no legs walk
        _actionBlocker.UpdateCanMove(carried);
        Transform(carried).AttachToGridOrMap();
        _standingState.Stand(carried);
    }

    private void ApplyCarrySlowdown(EntityUid carrier, EntityUid carried)
    {
        var massRatio = MassContest(carrier, carried);

        if (massRatio == 0)
            massRatio = 1;

        var massRatioSq = Math.Pow(massRatio, 2);
        var modifier = (1 - (0.15 / massRatioSq));
        modifier = Math.Max(0.1, modifier);
        _slowdown.SetModifier(carrier, (float) modifier);
    }

    public bool CanCarry(EntityUid carrier, Entity<CarriableComponent> carried)
    {
        return
            carrier != carried.Owner &&
            // can't carry multiple people, even if you have 4 hands it will break invariants when removing carryingcomponent for first carried person
            !HasComp<CarryingComponent>(carrier) &&
            // can't carry someone in a locker, buckled, etc
            HasComp<MapGridComponent>(Transform(carrier).ParentUid) &&
            // no tower of spacemen or stack overflow
            !HasComp<BeingCarriedComponent>(carrier) &&
            !HasComp<BeingCarriedComponent>(carried) &&
            // finally check that there are enough free hands
            TryComp<HandsComponent>(carrier, out var hands) &&
            hands.CountFreeHands() >= carried.Comp.FreeHandsRequired;
    }

    private float MassContest(EntityUid roller, EntityUid target)
    {
        if (!_physicsQuery.TryComp(roller, out var rollerPhysics) || !_physicsQuery.TryComp(target, out var targetPhysics))
            return 1f;

        if (targetPhysics.FixturesMass == 0)
            return 1f;

        return rollerPhysics.FixturesMass / targetPhysics.FixturesMass;
    }

    private TimeSpan GetPickupDuration(EntityUid carrier, EntityUid carried)
    {
        var length = TimeSpan.FromSeconds(3);

        var mod = MassContest(carrier, carried);
        if (mod != 0)
            length /= mod;

        return length;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BeingCarriedComponent, TransformComponent>();
        while (query.MoveNext(out var carried, out var comp, out var xform))
        {
            var carrier = comp.Carrier;
            if (TerminatingOrDeleted(carrier))
            {
                RemCompDeferred<BeingCarriedComponent>(carried);
                continue;
            }

            // SOMETIMES - when an entity is inserted into disposals, or a cryosleep chamber - it can get re-parented without a proper reparent event
            // when this happens, it needs to be dropped because it leads to weird behavior
            if (xform.ParentUid != carrier)
            {
                DropCarried(carrier, carried);
                continue;
            }

            // Make sure the carried entity is always centered relative to the carrier, as gravity pulls can offset it otherwise
            _transform.SetLocalPosition(carried, Vector2.Zero);
        }
    }
}
