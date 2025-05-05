// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.DoAfter;
using Content.Goobstation.Shared.Factory.Filters;
using Content.Shared.DeviceLinking;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Throwing;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Factory;

public abstract class SharedInteractorSystem : EntitySystem
{
    [Dependency] private readonly AutomationSystem _automation = default!;
    [Dependency] private readonly AutomationFilterSystem _filter = default!;
    [Dependency] private readonly CollisionWakeSystem _wake = default!;
    [Dependency] protected readonly SharedDeviceLinkSystem Device = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] protected readonly SharedPowerReceiverSystem Power = default!;

    private EntityQuery<ActiveDoAfterComponent> _doAfterQuery;
    private EntityQuery<HandsComponent> _handsQuery;
    private EntityQuery<ThrownItemComponent> _thrownQuery;

    public override void Initialize()
    {
        base.Initialize();

        _doAfterQuery = GetEntityQuery<ActiveDoAfterComponent>();
        _handsQuery = GetEntityQuery<HandsComponent>();
        _thrownQuery = GetEntityQuery<ThrownItemComponent>();

        SubscribeLocalEvent<InteractorComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<InteractorComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<InteractorComponent, DoAfterEndedEvent>(OnDoAfterEnded);
        // target entities
        SubscribeLocalEvent<InteractorComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<InteractorComponent, EndCollideEvent>(OnEndCollide);
    }

    private void OnInit(Entity<InteractorComponent> ent, ref ComponentInit args)
    {
        Device.EnsureSinkPorts(ent, ent.Comp.StartPort);
        Device.EnsureSourcePorts(ent, ent.Comp.StartedPort, ent.Comp.CompletedPort, ent.Comp.FailedPort);
    }

    private void OnExamined(Entity<InteractorComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(_filter.GetSlot(ent) is {} filter
            ? Loc.GetString("robotic-arm-examine-filter", ("filter", filter))
            : Loc.GetString("robotic-arm-examine-no-filter"));
    }

    private void OnStartCollide(Entity<InteractorComponent> ent, ref StartCollideEvent args)
    {
        // only care about entities in the target area
        if (args.OurFixtureId != ent.Comp.TargetFixtureId)
            return;

        AddTarget(ent, args.OtherEntity);
    }

    private void AddTarget(Entity<InteractorComponent> ent, EntityUid target)
    {
        // thrown items move too fast to be "clicked" on...
        if (_thrownQuery.HasComp(target))
            return;

        // ignore non-filtered entities
        if (_filter.IsBlocked(_filter.GetSlot(ent), target))
            return;

        var wake = CompOrNull<CollisionWakeComponent>(target);
        var wakeEnabled = wake?.Enabled ?? false;
        // need to only get EndCollide when it leaves the area, not when it sleeps
        _wake.SetEnabled(target, false, wake);
        ent.Comp.TargetEntities.Add((GetNetEntity(target), wakeEnabled));
        DirtyField(ent, ent.Comp, nameof(InteractorComponent.TargetEntities));
    }

    private void OnEndCollide(Entity<InteractorComponent> ent, ref EndCollideEvent args)
    {
        // only care about entities leaving the input area
        if (args.OurFixtureId != ent.Comp.TargetFixtureId)
            return;

        var target = GetNetEntity(args.OtherEntity);
        var i = ent.Comp.TargetEntities.FindIndex(pair => pair.Item1 == target);
        if (i < 0)
            return;

        var wake = ent.Comp.TargetEntities[i].Item2;
        ent.Comp.TargetEntities.RemoveAt(i);
        DirtyField(ent, ent.Comp, nameof(InteractorComponent.TargetEntities));
        _wake.SetEnabled(args.OtherEntity, wake); // don't break conveyors for skipped entities
    }

    private void OnDoAfterEnded(Entity<InteractorComponent> ent, ref DoAfterEndedEvent args)
    {
        if (!Power.IsPowered(ent.Owner))
            return;

        if (args.Target is not {} target)
            return;

        Device.InvokePort(ent, args.Cancelled ? ent.Comp.FailedPort : ent.Comp.CompletedPort);
        TryRemoveTarget(ent, target);
    }

    protected bool HasDoAfter(EntityUid uid) => _doAfterQuery.HasComp(uid);

    protected bool InteractWith(Entity<InteractorComponent> ent, EntityUid target)
    {
        if (_handsQuery.CompOrNull(ent)?.ActiveHandEntity is not {} tool)
            return _interaction.InteractHand(ent, target);

        var coords = Transform(target).Coordinates;
        return _interaction.InteractUsing(ent, tool, target, coords);
    }

    protected void TryRemoveTarget(Entity<InteractorComponent> ent, EntityUid target)
    {
        // if it still exists and is still allowed by the filter keep it
        if (!TerminatingOrDeleted(target) && _filter.IsAllowed(_filter.GetSlot(ent), target))
            return;

        RemoveTarget(ent, target);
    }

    protected void RemoveTarget(Entity<InteractorComponent> ent, EntityUid target)
    {
        var netEnt = GetNetEntity(target);
        ent.Comp.TargetEntities.RemoveAll(pair => pair.Item1 == netEnt);
        DirtyField(ent, ent.Comp, nameof(InteractorComponent.TargetEntities));
    }
}
