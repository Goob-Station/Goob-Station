// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Factory.Slots;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Item;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Factory;

public sealed class RoboticArmSystem : EntitySystem
{
    [Dependency] private readonly AutomationSystem _automation = default!;
    [Dependency] private readonly CollisionWakeSystem _wake = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedDeviceLinkSystem _device = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    private EntityQuery<ItemComponent> _itemQuery;

    public override void Initialize()
    {
        base.Initialize();

        _itemQuery = GetEntityQuery<ItemComponent>();

        SubscribeLocalEvent<RoboticArmComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<RoboticArmComponent, AfterAutoHandleStateEvent>(OnHandleState);
        // input items
        SubscribeLocalEvent<RoboticArmComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<RoboticArmComponent, EndCollideEvent>(OnEndCollide);
        // HasItem visuals
        SubscribeLocalEvent<RoboticArmComponent, EntInsertedIntoContainerMessage>(OnItemModified);
        SubscribeLocalEvent<RoboticArmComponent, EntRemovedFromContainerMessage>(OnItemModified);
        // linking
        SubscribeLocalEvent<RoboticArmComponent, LinkAttemptEvent>(OnLinkAttempt);
        SubscribeLocalEvent<RoboticArmComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<RoboticArmComponent, PortDisconnectedEvent>(OnPortDisconnected);
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

            var ent = (uid, comp);
            StopMoving(ent);

            if (comp.HeldItem is {} item)
            {
                if (!TryDrop(ent, item))
                    continue;

                StartMoving(ent);
                _device.InvokePort(uid, comp.MovedPort);
            }
            else if (TryPickupAny(ent))
            {
                StartMoving(ent);
            }
        }
    }

    private void OnStartup(Entity<RoboticArmComponent> ent, ref ComponentStartup args)
    {
        _device.EnsureSinkPorts(ent, ent.Comp.InputPort);
        _device.EnsureSourcePorts(ent, ent.Comp.OutputPort, ent.Comp.MovedPort);

        UpdateSlots(ent);

        if (!_slots.TryGetSlot(ent, ent.Comp.ItemSlotId, out var slot))
        {
            Log.Warning($"Missing item slot {ent.Comp.ItemSlotId} on robotic arm {ToPrettyString(ent)}");
            RemCompDeferred<RoboticArmComponent>(ent);
            return;
        }

        ent.Comp.ItemSlot = slot;
    }

    private void OnHandleState(Entity<RoboticArmComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        // incase client didnt predict linked port changing, update them
        UpdateSlots(ent);
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
        DirtyField(ent, nameof(RoboticArmComponent.InputItems));
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
        DirtyField(ent, nameof(RoboticArmComponent.InputItems));
    }

    private void OnItemModified<T>(Entity<RoboticArmComponent> ent, ref T args) where T: ContainerModifiedMessage
    {
        _appearance.SetData(ent, RoboticArmVisuals.HasItem, ent.Comp.HasItem);
    }

    private void OnLinkAttempt(Entity<RoboticArmComponent> ent, ref LinkAttemptEvent args)
    {
        // only prevent linking machines, don't care about control ports
        var linkingOutput = args.SourcePort == ent.Comp.OutputPort;
        var linkingInput = args.SinkPort == ent.Comp.InputPort;
        if (!linkingOutput && !linkingInput)
            return;

        if (ent.Owner == args.Source && linkingOutput)
        {
            // only 1 machine
            if (GetOutputMachine(ent) != null)
            {
                args.Cancel();
                return;
            }

            // make sure the port is for an automation slot
            if (!_automation.HasSlot(args.Sink, args.SinkPort, input: true))
            {
                args.Cancel();
                return;
            }
        }
        else if (ent.Owner == args.Sink && linkingInput)
        {
            // only 1 machine
            if (GetInputMachine(ent) != null)
            {
                args.Cancel();
                return;
            }

            // make sure the port is for an automation slot
            if (!_automation.HasSlot(args.Source, args.SourcePort, input: false))
            {
                args.Cancel();
                return;
            }
        }
    }

    private void OnNewLink(Entity<RoboticArmComponent> ent, ref NewLinkEvent args)
    {
        if (args.SinkPort == ent.Comp.InputPort)
        {
            ent.Comp.InputMachine = GetNetEntity(args.Source);
            ent.Comp.InputMachinePort = args.SourcePort;
            ent.Comp.InputSlot = _automation.GetSlot(args.Source, args.SourcePort, input: false);
            DirtyField(ent, nameof(RoboticArmComponent.InputMachine));
            DirtyField(ent, nameof(RoboticArmComponent.InputMachinePort));
        }
        else if (args.SourcePort == ent.Comp.OutputPort)
        {
            ent.Comp.OutputMachine = GetNetEntity(args.Sink);
            ent.Comp.OutputMachinePort = args.SinkPort;
            ent.Comp.OutputSlot = _automation.GetSlot(args.Sink, args.SinkPort, input: true);
            DirtyField(ent, nameof(RoboticArmComponent.OutputMachine));
            DirtyField(ent, nameof(RoboticArmComponent.OutputMachinePort));
        }
    }

    private void OnPortDisconnected(Entity<RoboticArmComponent> ent, ref PortDisconnectedEvent args)
    {
        // this event is shit and doesnt have source/sink entity and port just 1 string
        // so if you made InputPort and OutputPort the same string it would silently break
        // absolute supercode
        if (args.Port == ent.Comp.InputPort)
        {
            ent.Comp.InputMachine = null;
            ent.Comp.InputMachinePort = null;
            ent.Comp.InputSlot = null;
            DirtyField(ent, nameof(RoboticArmComponent.InputMachine));
            DirtyField(ent, nameof(RoboticArmComponent.InputMachinePort));
        }
        else if (args.Port == ent.Comp.OutputPort)
        {
            ent.Comp.OutputMachine = null;
            ent.Comp.OutputMachinePort = null;
            ent.Comp.OutputSlot = null;
            DirtyField(ent, nameof(RoboticArmComponent.OutputMachine));
            DirtyField(ent, nameof(RoboticArmComponent.OutputMachinePort));
        }
    }

    /// <summary>
    /// If a machine is linked for the arm's output, tries to insert into it.
    /// If there is no machine linked it just gets dropped.
    /// </summary>
    public bool TryDrop(Entity<RoboticArmComponent> ent, EntityUid item)
    {
        if (GetOutputMachine(ent) is {} machine && ent.Comp.OutputSlot is {} slot)
            return TryInsert(ent, item, machine, slot);

        // no dropping items into walls
        if (IsOutputBlocked(ent))
            return false;

        // nothing linked, just drop it there
        _transform.SetCoordinates(item, OutputPosition(ent));
        return true;
    }

    public bool TryInsert(Entity<RoboticArmComponent> ent, EntityUid item, EntityUid machine, AutomationSlot slot)
    {
        // prevent linking a machine then moving it far away, it has to be at the output area
        var coords = OutputPosition(ent);
        if (!_transform.InRange(Transform(machine).Coordinates, coords, 0.25f))
            return false;

        return slot.Insert(machine, item);
    }

    public bool TryPickupAny(Entity<RoboticArmComponent> ent)
    {
        if (GetInputMachine(ent) is {} machine && ent.Comp.InputSlot is {} slot)
            return TryPickupFrom(ent, machine, slot);

        var count = ent.Comp.InputItems.Count;
        if (count == 0)
            return false;

        var output = ent.Comp.OutputSlot;
        var outputMachine = GetOutputMachine(ent) ?? EntityUid.Invalid;
        if (output == null && IsOutputBlocked(ent))
            return false;

        // check them in reverse since removing near the end is cheaper
        var found = EntityUid.Invalid;
        for (var i = count - 1; i >= 0; i--)
        {
            var netEnt = ent.Comp.InputItems[i].Item1;
            if (!TryGetEntity(netEnt, out var item))
                continue;

            // make sure the destination will accept it or it gets stuck
            if (output?.CanInsert(outputMachine, item.Value) ?? true)
            {
                ent.Comp.InputItems.RemoveAt(i);
                found = item.Value;
                break;
            }
        }

        // nothing :(
        if (!found.Valid)
            return false;

        // no longer need this
        _wake.SetEnabled(found, false);

        // insert it into the arm slot
        return _slots.TryInsert(ent, ent.Comp.ItemSlot, found, user: null);
    }

    public bool TryPickupFrom(Entity<RoboticArmComponent> ent, EntityUid machine, AutomationSlot slot)
    {
        // prevent linking a machine then moving it far away, it has to be at the input area
        var coords = InputPosition(ent);
        if (!_transform.InRange(Transform(machine).Coordinates, coords, 0.25f))
            return false;

        // TODO: pass filters
        if (slot.GetItem(machine, null) is not {} item)
            return false;

        return _slots.TryInsert(ent, ent.Comp.ItemSlot, item, user: null);
    }

    private void UpdateSlots(Entity<RoboticArmComponent> ent)
    {
        if (GetInputMachine(ent) is {} input && ent.Comp.InputMachinePort is {} inPort)
            ent.Comp.InputSlot = _automation.GetSlot(input, inPort, input: false);
        if (GetOutputMachine(ent) is {} output && ent.Comp.OutputMachinePort is {} outPort)
            ent.Comp.OutputSlot = _automation.GetSlot(output, outPort, input: true);
    }

    private bool IsOutputBlocked(EntityUid uid)
    {
        var coords = OutputPosition(uid);
        return coords.GetTileRef(EntityManager, _map) is {} turf &&
            _turf.IsTileBlocked(turf, CollisionGroup.MachineMask);
    }

    private void StartMoving(Entity<RoboticArmComponent> ent)
    {
        SetPowerDraw(ent, ent.Comp.MovingPowerDraw);
        ent.Comp.NextMove = _timing.CurTime + ent.Comp.MoveDelay;
        DirtyField(ent, nameof(RoboticArmComponent.NextMove));
    }

    private void StopMoving(Entity<RoboticArmComponent> ent)
    {
        SetPowerDraw(ent, ent.Comp.IdlePowerDraw);
        ent.Comp.NextMove = null;
        DirtyField(ent, nameof(RoboticArmComponent.NextMove));
    }

    private void SetPowerDraw(EntityUid uid, float draw)
    {
        SharedApcPowerReceiverComponent? receiver = null;
        if (_power.ResolveApc(uid, ref receiver))
            _power.SetLoad(receiver, draw);
    }

    public EntityCoordinates OutputPosition(EntityUid uid)
    {
        var xform = Transform(uid);
        var coords = xform.Coordinates;
        var offset = xform.LocalRotation.ToVec();
        // positive would be where the input fixture is...
        return xform.Coordinates.Offset(-offset);
    }

    public EntityCoordinates InputPosition(EntityUid uid)
    {
        var xform = Transform(uid);
        var offset = xform.LocalRotation.ToVec();
        return xform.Coordinates.Offset(offset);
    }

    private EntityUid? GetInputMachine(RoboticArmComponent comp)
    {
        TryGetEntity(comp.InputMachine, out var machine);
        return machine;
    }

    private EntityUid? GetOutputMachine(RoboticArmComponent comp)
    {
        TryGetEntity(comp.OutputMachine, out var machine);
        return machine;
    }
}
