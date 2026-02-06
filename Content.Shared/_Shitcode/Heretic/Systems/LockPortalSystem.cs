using Content.Goobstation.Common.BlockTeleport;
using Content.Goobstation.Common.MartialArts;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Heretic;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Teleportation.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;

namespace Content.Shared._Shitcode.Heretic.Systems;

public sealed class LockPortalSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedDoorSystem _door = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LockPortalComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<LockPortalComponent, EndCollideEvent>(OnEndCollide);
    }

    private void OnEndCollide(Entity<LockPortalComponent> ent, ref EndCollideEvent args)
    {
        var subject = args.OtherEntity;

        if (!ShouldCollide(subject))
            return;

        if (TryComp<PortalTimeoutComponent>(subject, out var timeout) && timeout.EnteredPortal != ent)
            RemCompDeferred<PortalTimeoutComponent>(subject);
    }

    private void OnCollide(Entity<LockPortalComponent> ent, ref StartCollideEvent args)
    {
        var subject = args.OtherEntity;

        if (!ShouldCollide(subject))
            return;

        // if they came from another portal, just return and wait for them to exit the portal
        if (HasComp<PortalTimeoutComponent>(subject))
            return;

        var ev = new TeleportAttemptEvent();
        RaiseLocalEvent(subject, ref ev);
        if (ev.Cancelled)
            return;

        var linkResolved = Exists(ent.Comp.LinkedPortal);
        var invertedBehavior = !linkResolved || HereticOrGhoul(subject) == ent.Comp.Inverted;

        if (invertedBehavior)
        {
            if (_net.IsClient)
                return;

            var parent = Transform(ent).ParentUid;

            if (HasComp<DoorComponent>(parent) && FindRandomDoor(parent) is { } destination)
                Teleport(subject, ent, destination.AsNullable(), false);
            return;
        }

        var link = ent.Comp.LinkedPortal!.Value;
        var linkParent = Transform(link).ParentUid;

        Teleport(subject, ent, linkParent, true);
    }

    private void Teleport(EntityUid uid,
        Entity<LockPortalComponent> portal,
        Entity<DoorComponent?, TransformComponent?, DoorBoltComponent?> destination,
        bool addTimeout)
    {
        var coords = Transform(uid).Coordinates;

        if (!Resolve(destination, ref destination.Comp1, ref destination.Comp2, false))
            return;

        var to = destination.Comp2.Coordinates;

        if (_net.IsServer)
        {
            _audio.PlayPvs(portal.Comp.DepartureSound, coords);
            _audio.PlayPvs(portal.Comp.ArrivalSound, to);
        }

        EntityUid? pulling = null;
        var grabStage = GrabStage.No;

        if (TryComp(uid, out PullerComponent? puller) && puller.Pulling != null)
        {
            pulling = puller.Pulling.Value;
            grabStage = puller.GrabStage;
        }

        if (Resolve(destination, ref destination.Comp3, false))
            _door.SetBoltsDown((destination, destination.Comp3), false);

        _door.StartOpening(destination, destination.Comp1);

        if (addTimeout)
        {
            var timeout = EnsureComp<PortalTimeoutComponent>(uid);
            timeout.EnteredPortal = portal.Owner;
            Dirty(uid, timeout);
        }

        _pulling.StopAllPulls(uid);
        _transform.SetCoordinates(uid, to);

        if (pulling == null)
            return;

        if (addTimeout)
        {
            var timeout2 = EnsureComp<PortalTimeoutComponent>(pulling.Value);
            timeout2.EnteredPortal = portal.Owner;
            Dirty(pulling.Value, timeout2);
        }

        _transform.SetCoordinates(pulling.Value, to);
        _pulling.TryStartPull(uid, pulling.Value, puller, null, grabStage, force: true);
    }

    private bool HereticOrGhoul(EntityUid uid)
    {
        return HasComp<HereticComponent>(uid) || HasComp<GhoulComponent>(uid);
    }

    private bool ShouldCollide(EntityUid uid)
    {
        return HasComp<MobStateComponent>(uid);
    }

    private Entity<DoorComponent, TransformComponent>? FindRandomDoor(EntityUid ourAirlock)
    {
        var ourXform = Transform(ourAirlock);

        if (ourXform.GridUid == null)
            return null;

        var query = EntityQueryEnumerator<DoorComponent, PhysicsComponent, TransformComponent>();
        List<Entity<DoorComponent, TransformComponent>> possibleDestinations = new();
        while (query.MoveNext(out var uid, out var door, out var body, out var xform))
        {
            if (!body.CanCollide || uid == ourAirlock || xform.MapID != ourXform.MapID ||
                xform.GridUid != ourXform.GridUid)
                continue;

            possibleDestinations.Add((uid, door, xform));
        }

        return possibleDestinations.Count == 0 ? null : _random.Pick(possibleDestinations);
    }
}
