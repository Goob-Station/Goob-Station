using System.Numerics;
using Content.Goobstation.Shared.Mech.Components;
using Content.Shared._vg.TileMovement;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Camera;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mech.Components;
using Content.Shared.Mech.EntitySystems;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Mech.Systems;

public sealed class MechRamSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly SharedMechSystem _mech = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedCameraRecoilSystem _recoil = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private enum StepResult : byte
    {
        Moved,
        Stopped,
        WaitingForServer,
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MechComponent, MechRamActionEvent>(OnRam);
        SubscribeLocalEvent<MechRamComponent, UpdateCanMoveEvent>(OnCanMove);
        SubscribeLocalEvent<MechRamComponent, EntInsertedIntoContainerMessage>(OnPilotInserted);
        SubscribeLocalEvent<MechRamComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnPilotInserted(Entity<MechRamComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (_net.IsClient || !TryComp<MechComponent>(ent, out var mech) || args.Container.ID != mech.PilotSlotId)
            return;

        _actions.AddAction(args.Entity, ref ent.Comp.ActionEntity, ent.Comp.ActionProto, ent);
        _actions.SetToggled(ent.Comp.ActionEntity, ent.Comp.Active);
        Dirty(ent);
    }

    private void OnShutdown(Entity<MechRamComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.ActionEntity);
        PredictedQueueDel(ent.Comp.ActionEntity);
    }

    private void OnRam(Entity<MechComponent> ent, ref MechRamActionEvent args)
    {
        if (args.Handled || ent.Comp.Broken || !TryComp<MechRamComponent>(ent, out var ram) || ram.Active)
            return;

        var pilot = args.Performer;
        if (ent.Comp.Energy < ram.PowerCageEnergyCost)
        {
            _popup.PopupClient(Loc.GetString("mech-ability-no-power"), ent, pilot);
            return;
        }

        if (Transform(ent).GridUid == null)
        {
            _popup.PopupClient(Loc.GetString("mech-ram-no-grid"), ent, pilot);
            return;
        }

        args.Handled = true;
        _mech.TryChangeEnergy(ent, -ram.PowerCageEnergyCost, ent.Comp);

        ram.Trampled.Clear();
        ram.StepsLeft = ram.LungeDistance;
        ram.NextStep = _timing.CurTime + ram.TileStepInterval;
        SetActive((ent, ram), true);

        _audio.PlayPredicted(ram.StartSound, ent, pilot);
        _popup.PopupPredicted(Loc.GetString("mech-ram-start", ("mech", ent.Owner)), ent, pilot, PopupType.MediumCaution);
    }

    private void OnCanMove(Entity<MechRamComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (ent.Comp.Active)
            args.Cancel();
    }

    private void SetActive(Entity<MechRamComponent> ent, bool active)
    {
        if (ent.Comp.Active == active)
            return;

        ent.Comp.Active = active;
        _actions.SetToggled(ent.Comp.ActionEntity, active);
        _blocker.UpdateCanMove(ent);

        if (!active && ent.Comp.ActionEntity is { } action)
        {
            _actions.SetIcon(action, ent.Comp.CooldownIcon);
            ent.Comp.ShowingCooldownIcon = true;
        }

        Dirty(ent);
    }

    private void UpdateCooldownIcon(Entity<MechRamComponent> ent, TimeSpan curTime)
    {
        if (!ent.Comp.ShowingCooldownIcon || _actions.GetAction(ent.Comp.ActionEntity, false) is not { } action)
            return;

        if (action.Comp.Cooldown is { } cooldown && cooldown.End > curTime)
            return;

        _actions.SetIcon(action.AsNullable(), ent.Comp.ReadyIcon);
        ent.Comp.ShowingCooldownIcon = false;
        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<MechRamComponent, MechComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var ram, out var mech, out var xform))
        {
            if (_net.IsClient && (_player.LocalEntity is not { } local || local != mech.PilotSlot.ContainedEntity))
                continue;

            UpdateCooldownIcon((uid, ram), curTime);

            if (!ram.Active || curTime < ram.NextStep)
                continue;

            ram.NextStep = curTime + ram.TileStepInterval;
            Dirty(uid, ram);

            if (ram.StepsLeft <= 0 || mech.Broken || mech.PilotSlot.ContainedEntity is not { } pilot)
            {
                SetActive((uid, ram), false);
                continue;
            }

            if (Step((uid, ram), mech, pilot, xform) == StepResult.Stopped)
                SetActive((uid, ram), false);
        }
    }

    private StepResult Step(Entity<MechRamComponent> ent, MechComponent mech, EntityUid pilot, TransformComponent xform)
    {
        if (xform.GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid))
            return StepResult.Stopped;

        var worldFacing = _transform.GetWorldRotation(xform).ToWorldVec();
        var facing = (-_transform.GetWorldRotation(gridUid)).RotateVec(worldFacing);
        var dir = Math.Abs(facing.X) >= Math.Abs(facing.Y)
            ? new Vector2i(Math.Sign(facing.X), 0)
            : new Vector2i(0, Math.Sign(facing.Y));
        if (dir == Vector2i.Zero)
            return StepResult.Stopped;

        var next = _map.TileIndicesFor(gridUid, grid, xform.Coordinates) + dir;
        if (!_map.TryGetTileRef(gridUid, grid, next, out var tile) || tile.Tile.IsEmpty)
            return StepResult.Stopped;

        var mechMask = GetMask(ent);
        var box = Box2.CenteredAround(_map.GridTileToWorld(gridUid, grid, next).Position, new Vector2(0.7f, 0.7f));

        for (var attempt = 0; attempt < 3; attempt++)
        {
            EntityUid? wall = null;

            foreach (var other in _lookup.GetEntitiesIntersecting(xform.MapID, box, LookupFlags.Static | LookupFlags.Dynamic))
            {
                if (other == ent.Owner || other == pilot)
                    continue;

                if (HasComp<MobStateComponent>(other))
                {
                    Trample(ent, other, dir);
                    continue;
                }

                if (wall == null && IsBlocking(other, mechMask))
                    wall = other;
            }

            if (wall is not { } target)
                break;

            if (_net.IsClient)
                return StepResult.WaitingForServer;

            _damageable.TryChangeDamage(target, ent.Comp.StructureDamage, origin: ent);
            if (!TerminatingOrDeleted(target) && !EntityManager.IsQueuedForDeletion(target))
            {
                _damageable.TryChangeDamage(ent.Owner, ent.Comp.SelfDamage, true, false);
                _audio.PlayPvs(ent.Comp.ImpactSound, ent);
                Kick(ent, dir, ent.Comp.ImpactCameraKick);
                _popup.PopupEntity(Loc.GetString("mech-ram-impact", ("mech", ent.Owner), ("target", target)), ent, PopupType.LargeCaution);
                return StepResult.Stopped;
            }

            _audio.PlayPvs(ent.Comp.SmashSound, ent);
            Kick(ent, dir, ent.Comp.ImpactCameraKick * 0.5f);
        }

        _transform.SetCoordinates(ent, _map.GridTileToLocal(gridUid, grid, next));

        if (TryComp<TileMovementComponent>(ent, out var tileMove))
        {
            tileMove.SlideActive = false;
            tileMove.FailureSlideActive = false;
            tileMove.LastTickLocalCoordinates = null;
            Dirty(ent.Owner, tileMove);
        }

        _audio.PlayPredicted(ent.Comp.StepSound, ent, pilot);
        ent.Comp.StepsLeft--;
        Dirty(ent);
        return StepResult.Moved;
    }

    private bool IsBlocking(EntityUid other, int mechMask)
    {
        if (!Transform(other).Anchored
            || !HasComp<DamageableComponent>(other)
            || !TryComp<PhysicsComponent>(other, out var body)
            || !body.CanCollide
            || !TryComp<FixturesComponent>(other, out var fixtures))
            return false;

        foreach (var fixture in fixtures.Fixtures.Values)
            if (fixture.Hard && (fixture.CollisionLayer & mechMask) != 0)
                return true;

        return false;
    }

    private int GetMask(EntityUid mech)
    {
        if (!TryComp<FixturesComponent>(mech, out var fixtures))
            return (int) CollisionGroup.MobMask;

        var mask = 0;
        foreach (var fixture in fixtures.Fixtures.Values)
            mask |= fixture.CollisionMask;

        return mask == 0 ? (int) CollisionGroup.MobMask : mask;
    }



    private void Trample(Entity<MechRamComponent> ent, EntityUid target, Vector2i dir)
    {
        if (!ent.Comp.Trampled.Add(target))
            return;

        Dirty(ent);
        _stun.TryKnockdown(target, ent.Comp.MobKnockdownTime);
        _damageable.TryChangeDamage(target, ent.Comp.MobDamage, origin: ent);

        var side = new Vector2(-dir.Y, dir.X);
        var offset = _transform.GetWorldPosition(target) - _transform.GetWorldPosition(ent);
        if (Vector2.Dot(offset, side) < 0)
            side = -side;

        _throwing.TryThrow(target, side * 1.5f + new Vector2(dir.X, dir.Y), ent.Comp.ThrowSpeed, ent, playSound: false);
    }

    private void Kick(EntityUid mech, Vector2i dir, float strength)
    {
        foreach (var camera in _lookup.GetEntitiesInRange<CameraRecoilComponent>(Transform(mech).Coordinates, 6f))
            _recoil.KickCamera(camera, new Vector2(dir.X, dir.Y) * strength);
    }
}
