// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Containers.ItemSlots;
using Content.Shared.Item;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Factory;

public sealed class RoboticArmSystem : EntitySystem
{
    [Dependency] private readonly CollisionWakeSystem _wake = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    private EntityQuery<ItemComponent> _itemQuery;

    public override void Initialize()
    {
        base.Initialize();

        _itemQuery = GetEntityQuery<ItemComponent>();

        SubscribeLocalEvent<RoboticArmComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<RoboticArmComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<RoboticArmComponent, EndCollideEvent>(OnEndCollide);
        SubscribeLocalEvent<RoboticArmComponent, EntInsertedIntoContainerMessage>(OnItemModified);
        SubscribeLocalEvent<RoboticArmComponent, EntRemovedFromContainerMessage>(OnItemModified);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<RoboticArmComponent>();
        var now = _timing.CurTime;
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!_power.IsPowered(uid))
                continue;

            if (comp.NextMove is {} nextMove && now < nextMove)
                continue;

            comp.NextMove = null;

            // TODO: forced delay states to let animation play
            if (comp.HeldItem is {} item)
            {
                TryDrop((uid, comp), item);
            }
            else
            {
                TryPickupAny((uid, comp));
            }
        }
    }

    private void OnInit(Entity<RoboticArmComponent> ent, ref ComponentInit args)
    {
        if (!_slots.TryGetSlot(ent, ent.Comp.ItemSlotId, out var slot))
        {
            Log.Warning($"Missing item slot {ent.Comp.ItemSlotId} on robotic arm {ToPrettyString(ent)}");
            RemCompDeferred<RoboticArmComponent>(ent);
            return;
        }

        ent.Comp.ItemSlot = slot;
    }

    private void OnStartCollide(Entity<RoboticArmComponent> ent, ref StartCollideEvent args)
    {
        // only care about items in the input area
        if (args.OurFixtureId != ent.Comp.InputFixtureId)
            return;

        // never pick up non-items
        var item = args.OtherEntity;
        if (!_itemQuery.HasComp(item))
            return;

        // TODO: check filter against item

        var wake = CompOrNull<CollisionWakeComponent>(item);
        var wakeEnabled = wake?.Enabled ?? false;
        // need to only get EndCollide when it leaves the area, not when it sleeps
        _wake.SetEnabled(item, false, wake);
        ent.Comp.InputItems.Add((GetNetEntity(item), wakeEnabled));
        Dirty(ent);
    }

    private void OnEndCollide(Entity<RoboticArmComponent> ent, ref EndCollideEvent args)
    {
        // only care about items leaving the input area
        if (args.OurFixtureId != ent.Comp.InputFixtureId)
            return;

        var item = GetNetEntity(args.OtherEntity);
        var i = ent.Comp.InputItems.FindIndex(pair => pair.Item1 == item);
        if (i < 0)
            return;

        var wake = ent.Comp.InputItems[i].Item2;
        ent.Comp.InputItems.RemoveAt(i);
        _wake.SetEnabled(args.OtherEntity, wake); // don't break conveyors for skipped items
        Dirty(ent);
    }

    private void OnItemModified<T>(Entity<RoboticArmComponent> ent, ref T args) where T: ContainerModifiedMessage
    {
        _appearance.SetData(ent, RoboticArmVisuals.HasItem, ent.Comp.HasItem);
    }

    /// <summary>
    /// If a machine is linked for the arm's output, tries to insert into it.
    /// If there is no machine linked it just gets dropped.
    /// </summary>
    public bool TryDrop(Entity<RoboticArmComponent> ent, EntityUid item)
    {
        // TODO: linking

        // nothing linked, just drop it
        var coords = DropPosition(ent);

        // no dropping items into walls
        if (coords.GetTileRef(EntityManager, _map) is {} turf && _turf.IsTileBlocked(turf, CollisionGroup.Impassable))
            return false;

        _transform.SetCoordinates(item, coords);
        StartMoving(ent);
        return true;
    }

    public bool TryPickupAny(Entity<RoboticArmComponent> ent)
    {
        // TODO: linking
        var count = ent.Comp.InputItems.Count;
        if (count == 0)
            return false;

        // pop the last item from the list
        var i = count - 1;
        var (netEnt, _) = ent.Comp.InputItems[i];
        ent.Comp.InputItems.RemoveAt(i);
        var item = GetEntity(netEnt);

        // no longer need this
        _wake.SetEnabled(item, false);

        // insert it into the slot
        if (!_slots.TryInsert(ent, ent.Comp.ItemSlot, item, user: null))
            return false;

        StartMoving(ent);
        return true;
    }

    private void StartMoving(Entity<RoboticArmComponent> ent)
    {
        ent.Comp.NextMove = _timing.CurTime + ent.Comp.MoveDelay;
        Dirty(ent);
    }

    public EntityCoordinates DropPosition(EntityUid uid)
    {
        var xform = Transform(uid);
        var coords = xform.Coordinates;
        var offset = xform.LocalRotation.ToVec();
        // positive would be where the input fixture is...
        return coords.Offset(-offset);
    }
}
