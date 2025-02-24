using System.Linq;
using System.Numerics;
using Content.Shared.Damage.Components;
using Content.Shared.Standing;
using Content.Shared.Wieldable;
using Content.Shared.Wieldable.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Weapons.SmartGun;

public abstract class SharedLaserPointerSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LaserPointerComponent, ItemWieldedEvent>(OnWield);
        SubscribeLocalEvent<LaserPointerComponent, ItemUnwieldedEvent>(OnUnwield);
        SubscribeLocalEvent<LaserPointerComponent, EntityTerminatingEvent>(OnTerminating);
        SubscribeAllEvent<LaserPointerEntityHoveredEvent>(OnHovered);
    }

    private void OnHovered(LaserPointerEntityHoveredEvent msg, EntitySessionEventArgs args)
    {
        var pointer = GetEntity(msg.LaserPointerEntity);

        if (!TryComp(pointer, out LaserPointerComponent? laser))
            return;

        laser.LastNetworkEventTime = Timing.CurTime;

        if (!TryComp(pointer, out WieldableComponent? wieldable))
            return;

        var hovered = GetEntity(msg.Hovered);

        if (hovered == args.SenderSession.AttachedEntity)
            hovered = null;

        AddOrRemoveLine(msg.LaserPointerEntity, laser, wieldable, Transform(pointer), msg.Dir, hovered);
    }

    public void AddLine(NetEntity laserPointer, Color color, Vector2 start, Vector2 end)
    {
        var query = EntityQueryEnumerator<LaserPointerManagerComponent>();
        Entity<LaserPointerManagerComponent>? manager = null;
        while (query.MoveNext(out var uid, out var managerComp))
        {
            manager = (uid, managerComp);
            break;
        }

        if (_net.IsClient && manager == null)
            return;

        if (manager == null)
        {
            var managerUid = Spawn();
            manager = (managerUid, AddComp<LaserPointerManagerComponent>(managerUid));
            PvsOverride(managerUid);
        }

        var comp = manager.Value.Comp;

        if (!comp.Data.TryGetValue(laserPointer, out var value))
        {
            value = new LaserPointerData(color, start, end);
            comp.Data.Add(laserPointer, value);
        }
        else
        {
            value.Color = color;
            value.Start = start;
            value.End = end;
        }

        Dirty(manager.Value);
    }

    public void RemoveLine(NetEntity laserPointer)
    {
        var query = EntityQueryEnumerator<LaserPointerManagerComponent>();
        while (query.MoveNext(out var uid, out var manager))
        {
            if (manager.Data.Remove(laserPointer))
                Dirty(uid, manager);
        }
    }

    public void AddOrRemoveLine(NetEntity laserPointer,
        LaserPointerComponent comp,
        WieldableComponent wieldable,
        TransformComponent xform,
        Vector2? direction,
        EntityUid? targetedEntity)
    {
        if (!wieldable.Wielded)
        {
            RemoveLine(laserPointer);
            return;
        }

        if (!TryComp(xform.ParentUid, out TransformComponent? parentXform))
        {
            if (_net.IsServer)
                RemoveLine(laserPointer);
            return;
        }

        var rayLength = 15f;

        // Lying people hit every object even if they are not aiming at it.
        var lying = TryComp(xform.ParentUid, out StandingStateComponent? standingState) &&
                    standingState.CurrentState != StandingState.Standing;

        var (pos, rot) = _transform.GetWorldPositionRotation(parentXform);
        var dir = direction ?? rot.ToWorldVec();

        if (dir.LengthSquared() < 0.0001f)
        {
            RemoveLine(laserPointer);
            return;
        }

        var normalized = dir.Normalized();

        var requiresTargetQuery = GetEntityQuery<RequireProjectileTargetComponent>();

        var ray = new CollisionRay(pos, normalized, comp.CollisionMask);
        var hit = _physics.IntersectRay(xform.MapID, ray, rayLength, xform.ParentUid, false)
            .OrderBy(x => x.Distance)
            .FirstOrNull(x =>
                x.HitEntity == targetedEntity || lying ||
                !requiresTargetQuery.TryComp(x.HitEntity, out var requiresTarget) || !requiresTarget.Active);
        if (hit != null)
            rayLength = hit.Value.Distance;

        var end = pos + normalized * rayLength;

        AddLine(laserPointer, targetedEntity == null ? comp.DefaultColor : comp.TargetedColor, pos, end);
    }

    private void OnTerminating(Entity<LaserPointerComponent> ent, ref EntityTerminatingEvent args)
    {
        RemoveLine(GetNetEntity(ent.Owner));
    }

    private void OnUnwield(Entity<LaserPointerComponent> ent, ref ItemUnwieldedEvent args)
    {
        RemoveLine(GetNetEntity(ent.Owner));
    }

    private void OnWield(Entity<LaserPointerComponent> ent, ref ItemWieldedEvent args)
    {
        _audio.PlayPredicted(ent.Comp.Sound, ent, args.User);
    }

    protected virtual void PvsOverride(EntityUid entity) { }
}

[Serializable, NetSerializable]
public sealed class LaserPointerEntityHoveredEvent(NetEntity? hovered, Vector2? dir, NetEntity pointer)
    : EntityEventArgs
{
    public NetEntity? Hovered = hovered;

    public Vector2? Dir = dir;

    public NetEntity LaserPointerEntity = pointer;
}
