using Content.Shared.ActionBlocker;
using Content.Shared.DoAfter;
using Content.Shared.Emoting;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Popups;
using Content.Shared.Speech;
using Content.Shared.Standing;
using Content.Shared.Throwing;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Wizard.Traps;

public abstract class SharedIceCubeSystem : EntitySystem
{
    [Dependency] private readonly FixtureSystem _fixtures = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;

    private const string IceCubeFixture = "ice-cube-fixture";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IceCubeComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<IceCubeComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<IceCubeComponent, TileFrictionEvent>(OnTileFriction);
        SubscribeLocalEvent<IceCubeComponent, MoveInputEvent>(OnMoveInput);
        SubscribeLocalEvent<IceCubeComponent, BreakFreeDoAfterEvent>(OnBreakFree);
        SubscribeLocalEvent<IceCubeComponent, ComponentStartup>(IceCubeAdded);
        SubscribeLocalEvent<IceCubeComponent, ComponentShutdown>(IceCubeRemoved);
        SubscribeLocalEvent<IceCubeComponent, UseAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<IceCubeComponent, PickupAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<IceCubeComponent, ThrowAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<IceCubeComponent, InteractionAttemptEvent>(OnInteractAttempt);
        SubscribeLocalEvent<IceCubeComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
        SubscribeLocalEvent<IceCubeComponent, PullAttemptEvent>(OnPullAttempt);
        SubscribeLocalEvent<IceCubeComponent, AttackAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<IceCubeComponent, EmoteAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<IceCubeComponent, SpeakAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<IceCubeComponent, StandAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<IceCubeComponent, DownAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<IceCubeComponent, ChangeDirectionAttemptEvent>(OnAttempt);
    }

    private void OnStartCollide(Entity<IceCubeComponent> ent, ref StartCollideEvent args)
    {
        if (args.OtherBody.LinearVelocity.LengthSquared() < 0.01f)
            return;

        var xform = Transform(args.OtherEntity);

        var ray = new CollisionRay(_transform.GetWorldPosition(xform),
            args.OtherBody.LinearVelocity.Normalized(),
            args.OurBody.CollisionLayer);

        if (ent.Owner != _physics.IntersectRay(xform.MapID, ray, 1f, args.OtherEntity).FirstOrNull()?.HitEntity)
            return;

        _physics.ApplyLinearImpulse(ent,
            args.OtherBody.LinearVelocity * args.OtherBody.Mass * ent.Comp.VelocityMultiplier,
            body: args.OurBody);
    }

    private void OnMobStateChanged(Entity<IceCubeComponent> ent, ref MobStateChangedEvent args)
    {
        RemCompDeferred(ent.Owner, ent.Comp);
    }

    private void OnTileFriction(Entity<IceCubeComponent> ent, ref TileFrictionEvent args)
    {
        args.Modifier *= ent.Comp.TileFriction;
    }

    private void OnBreakFree(Entity<IceCubeComponent> ent, ref BreakFreeDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        RemCompDeferred(ent.Owner, ent.Comp);
    }

    private void OnMoveInput(Entity<IceCubeComponent> ent, ref MoveInputEvent args)
    {
        var (uid, comp) = ent;

        var doArgs = new DoAfterArgs(EntityManager, uid, comp.BreakFreeDelay, new BreakFreeDoAfterEvent(), uid)
        {
            Hidden = true,
            RequireCanInteract = false,
            MultiplyDelay = false,
            CancelDuplicate = false,
        };

        if (_doAfter.TryStartDoAfter(doArgs) && _net.IsServer)
            _popup.PopupEntity(Loc.GetString("ice-cube-break-free-start"), uid, uid);
    }

    private void OnInteractAttempt(Entity<IceCubeComponent> ent, ref InteractionAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnAttempt(EntityUid uid, IceCubeComponent component, CancellableEntityEventArgs args)
    {
        args.Cancel();
    }

    private void OnPullAttempt(EntityUid uid, IceCubeComponent component, PullAttemptEvent args)
    {
        if (args.PullerUid == uid)
            args.Cancelled = true;
    }

    private void OnUpdateCanMove(Entity<IceCubeComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (ent.Comp.LifeStage > ComponentLifeStage.Running)
            return;

        args.Cancel();
    }

    private void IceCubeRemoved(Entity<IceCubeComponent> ent, ref ComponentShutdown args)
    {
        var (uid, comp) = ent;

        if (TerminatingOrDeleted(uid))
            return;

        Shutdown(ent);

        _blocker.UpdateCanMove(uid);

        if (_net.IsClient)
            return;

        _popup.PopupEntity(Loc.GetString("ice-cube-melt"), uid);

        if (!TryComp(uid, out PhysicsComponent? physics) || !TryComp(uid, out FixturesComponent? fixtures))
            return;

        var xform = Transform(uid);

        var fixture = _fixtures.GetFixtureOrNull(uid, IceCubeFixture, fixtures);

        if (fixture != null)
            _fixtures.DestroyFixture(uid, IceCubeFixture, fixture, body: physics, manager: fixtures, xform: xform);
        else
            _fixtures.FixtureUpdate(uid, manager: fixtures, body: physics);

        if (comp.OldBodyType != null)
            _physics.SetBodyType(uid, comp.OldBodyType.Value, fixtures, physics, xform);
    }

    private void IceCubeAdded(Entity<IceCubeComponent> ent, ref ComponentStartup args)
    {
        Startup(ent);

        var (uid, comp) = ent;

        _blocker.UpdateCanMove(uid);

        if (_net.IsClient)
            return;

        if (!TryComp(uid, out PhysicsComponent? physics) || !TryComp(uid, out FixturesComponent? fixtures))
            return;

        var xform = Transform(uid);

        // For whatever reason I can't set bounds on PhysShapeAabb in code so I have to use polygon shape
        var shape = new PolygonShape();
        shape.SetAsBox(new Box2(-0.4f, -0.4f, 0.4f, 0.4f));
        _fixtures.TryCreateFixture(uid,
            shape,
            IceCubeFixture,
            collisionLayer: comp.CollisionLayer,
            collisionMask: comp.CollisionMask,
            restitution: comp.Restitution,
            manager: fixtures,
            body: physics,
            xform: xform);

        if (physics.BodyType != BodyType.KinematicController)
            return;

        comp.OldBodyType = physics.BodyType;
        _physics.SetBodyType(uid, comp.FrozenBodyType, fixtures, physics, xform);
    }

    protected virtual void Startup(Entity<IceCubeComponent> ent)
    {
    }

    protected virtual void Shutdown(Entity<IceCubeComponent> ent)
    {
    }
}

[Serializable, NetSerializable]
public sealed partial class BreakFreeDoAfterEvent : SimpleDoAfterEvent;
