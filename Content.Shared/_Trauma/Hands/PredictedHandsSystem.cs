using System.Numerics;
using Content.Shared._White.Grab;
using Content.Shared.ActionBlocker;
using Content.Shared.CombatMode;
using Content.Shared.Damage.Systems;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Input;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Random.Helpers;
using Content.Shared.Stacks;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Containers;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Trauma.Hands;

/// <summary>
/// Predicting hand-related stuff that used to be serverside for no reason.
/// </summary>
public sealed class PredictedHandsSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly GrabThrownSystem _grabThrown = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;
    private EntityQuery<VirtualItemComponent> _virtualQuery;

    public EntityUid? ThrowDropping { get; private set; }

    /// <summary>
    /// Items dropped when the holder falls down will be launched in
    /// a direction offset by up to this many degrees from the holder's
    /// movement direction.
    /// </summary>
    private const float DropHeldItemsSpread = 45;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HandsComponent, DisarmedEvent>(OnDisarmed, before: new[] { typeof(SharedStunSystem), typeof(SharedStaminaSystem) });
        SubscribeLocalEvent<HandsComponent, DropHandItemsEvent>(OnDropHandItems);

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.ThrowItemInHand, new PointerInputCmdHandler(HandleThrowItem))
            .Register<PredictedHandsSystem>();

        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _virtualQuery = GetEntityQuery<VirtualItemComponent>();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        CommandBinds.Unregister<PredictedHandsSystem>();
    }

    private void OnDisarmed(Entity<HandsComponent> ent, ref DisarmedEvent args)
    {
        if (args.Handled)
            return;

        var rand = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent));
        if (!rand.Prob(args.DisarmProbability)) // wdp shoving
            return;

        args.WasDisarmed = true;
        // Break any pulls
        if (TryComp(ent, out PullerComponent? puller) && TryComp(puller.Pulling, out PullableComponent? pullable))
            _pulling.TryStopPull(puller.Pulling.Value, pullable, ignoreGrab: true); // Goobstation edit added check for grab

        var offset = rand.NextAngle().RotateVec(new Vector2(rand.NextFloat(1f, 1.5f), 0));
        var coords = _transform.GetMoverCoordinates(args.Target).Offset(offset);
        if (!ThrowHeldItem(args.Target, coords))
            return;

        args.Handled = true; // Successful disarm
    }

    #region Interactions

    private bool HandleThrowItem(ICommonSession? session, EntityCoordinates coordinates, EntityUid entity)
    {
        if (session?.AttachedEntity is not { Valid: true } player || !Exists(player) || !coordinates.IsValid(EntityManager))
            return false;

        // <Goob>
        if (_hands.TryGetActiveItem(player, out var item) && _virtualQuery.TryComp(item, out var virtComp))
        {
            var userEv = new VirtualItemDropAttemptEvent(virtComp.BlockingEntity, player, item.Value, true);
            RaiseLocalEvent(player, userEv);

            var targEv = new VirtualItemDropAttemptEvent(virtComp.BlockingEntity, player, item.Value, true);
            RaiseLocalEvent(virtComp.BlockingEntity, targEv);

            if (userEv.Cancelled || targEv.Cancelled)
                return false;
        }
        // </Goob>

        var thrown = ThrowHeldItem(player, coordinates);
        // Goob - client always sends it to the server, but the server has to report it as handled
        // so later binds (e.g. heretic protective blades) don't also fire after a successful throw
        return _net.IsServer && thrown;
    }

    /// <summary>
    /// Throw the player's currently held item.
    /// </summary>
    public bool ThrowHeldItem(EntityUid player, EntityCoordinates coordinates, float minDistance = 0.1f)
    {
        if (_container.IsEntityInContainer(player) ||
            !TryComp(player, out HandsComponent? hands) ||
            !_hands.TryGetActiveItem((player, hands), out var throwEnt) ||
            !_actionBlocker.CanThrow(player, throwEnt.Value))
            return false;

        // <Goob> - added throwing for grabbed mobs, moved direction.
        var direction = _transform.ToMapCoordinates(coordinates).Position - _transform.GetWorldPosition(player);

        if (_virtualQuery.TryComp(throwEnt, out var virt))
        {
            var userEv = new VirtualItemThrownEvent(virt.BlockingEntity, player, throwEnt.Value, direction);
            RaiseLocalEvent(player, ref userEv);
            var targEv = new VirtualItemThrownEvent(virt.BlockingEntity, player, throwEnt.Value, direction);
            RaiseLocalEvent(virt.BlockingEntity, ref targEv);
        }
        // </Goob>

        if (_timing.CurTime < hands.NextThrowTime)
            return false;

        hands.NextThrowTime = _timing.CurTime + hands.ThrowCooldown;
        Dirty(player, hands);

        if (TryComp(throwEnt, out StackComponent? stack) && stack.Count > 1 && stack.ThrowIndividually)
        {
            if (_stack.Split(throwEnt.Value, 1, Transform(player).Coordinates, stack) is not { Valid: true } splitStack)
                return false;

            throwEnt = splitStack;
        }

        if (direction == Vector2.Zero)
            return true;

        var length = direction.Length();
        var distance = Math.Clamp(length, minDistance, hands.ThrowRange);
        direction *= distance / length;

        var throwSpeed = hands.BaseThrowspeed;

        // Let other systems change the thrown entity (useful for virtual items)
        // or the throw strength.
        // <Goob> - added thrower's velocity for inertia
        var holderVelocity = _physics.GetMapLinearVelocity(player);
        var modifier = MathF.Max(0f, Vector2.Dot(direction.Normalized(), holderVelocity));
        var ev = new BeforeThrowEvent(throwEnt.Value, direction * (1f + modifier * 0.1f), throwSpeed * (1f + modifier * 0.05f), player);
        // </Goob>
        RaiseLocalEvent(player, ref ev);
        RaiseLocalEvent(throwEnt.Value, ref ev); // Goobstation

        if (ev.Cancelled)
            return true;

        // This can grief the above event so we raise it afterwards
        if (_hands.IsHolding((player, hands), throwEnt, out _))
        {
            ThrowDropping = throwEnt;
            var dropped = _hands.TryDrop(player, throwEnt.Value);
            ThrowDropping = null;
            if (!dropped)
                return false;
        }

        // <Goob>
        if (!ev.GrabThrow)
            _throwing.TryThrow(ev.ItemUid, ev.Direction, ev.ThrowSpeed, ev.PlayerUid, compensateFriction: !HasComp<LandAtCursorComponent>(ev.ItemUid));
        else
            _grabThrown.Throw(ev.ItemUid, player, ev.Direction, ev.ThrowSpeed);
        // </Goob>

        return true;
    }

    private void OnDropHandItems(Entity<HandsComponent> entity, ref DropHandItemsEvent args)
    {
        if (args.Handled) // Goobstation
            return;

        // If the holder doesn't have a physics component, they ain't moving
        var holderVelocity = _physicsQuery.CompOrNull(entity)?.LinearVelocity ?? Vector2.Zero;
        var spreadMaxAngle = Angle.FromDegrees(DropHeldItemsSpread);

        var rand = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(entity));

        foreach (var hand in entity.Comp.Hands.Keys)
        {
            if (!_hands.TryGetHeldItem(entity.AsNullable(), hand, out var heldEntity))
                continue;

            var throwAttempt = new FellDownThrowAttemptEvent(entity);
            RaiseLocalEvent(heldEntity.Value, ref throwAttempt);

            if (throwAttempt.Cancelled)
                continue;

            if (!_hands.TryDrop(entity.AsNullable(), hand, checkActionBlocker: false))
                continue;

            // Rotate the item's throw vector a bit for each item
            var angleOffset = rand.NextAngle(-spreadMaxAngle, spreadMaxAngle);
            // Rotate the holder's velocity vector by the angle offset to get the item's velocity vector
            var itemVelocity = angleOffset.RotateVec(holderVelocity);
            // Decrease the distance of the throw by a random amount
            itemVelocity *= rand.NextFloat();
            // Heavier objects don't get thrown as far
            // If the item doesn't have a physics component, it isn't going to get thrown anyway, but we'll assume infinite mass
            itemVelocity *= _physicsQuery.CompOrNull(heldEntity.Value)?.InvMass ?? 0;
            // Throw at half the holder's intentional throw speed and
            // vary the speed a little to make it look more interesting
            var throwSpeed = entity.Comp.BaseThrowspeed * rand.NextFloat(0.45f, 0.55f);

            _throwing.TryThrow(heldEntity.Value,
                itemVelocity,
                throwSpeed,
                entity,
                pushbackRatio: 0,
                compensateFriction: false
            );
        }
    }

    #endregion
}
