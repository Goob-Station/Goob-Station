using Content.Goobstation.Common.Weapons.DelayedDash;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Standing;
using Content.Shared.Throwing;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Weapons.DelayedDash;

public sealed partial class DelayedDashSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private UseDelaySystem _useDelay = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private StandingStateSystem _standing = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private ItemToggleSystem _toggle = default!;

    private const int DashCollisionLayer = (int) CollisionGroup.MidImpassable;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<DelayedDashRequestEvent>(OnRequest);
        SubscribeLocalEvent<DelayedDashComponent, DelayedDashDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<DelayedDashingComponent, LandEvent>(OnLand);
        SubscribeLocalEvent<DelayedDashingComponent, StopThrowEvent>(OnStopThrow);
        SubscribeLocalEvent<DelayedDashingComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<DelayedDashingComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnRequest(DelayedDashRequestEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user)
            return;

        var weapon = GetEntity(msg.Weapon);

        if (!TryComp(weapon, out DelayedDashComponent? dash))
            return;

        if (!_hands.IsHolding(user, weapon))
            return;

        if (!_toggle.IsActivated(weapon))
            return;

        var target = _transform.ToMapCoordinates(GetCoordinates(msg.Target));

        // If do-after already started new click just selects next destination.
        if (TryComp(user, out DelayedDashingComponent? existing))
        {
            if (existing.Weapon != weapon || existing.Dashing || existing.Destinations.Count >= dash.MaxDestinations)
                return;

            existing.Destinations.Add(target);
            Dirty(user, existing);
            return;
        }

        if (_standing.IsDown(user))
            return;

        if (_container.IsEntityInContainer(user))
            return;

        if (!TryComp(weapon, out UseDelayComponent? delay) || _useDelay.IsDelayed((weapon, delay)))
            return;

        var dashing = EnsureComp<DelayedDashingComponent>(user);
        dashing.Weapon = weapon;
        dashing.Destinations.Add(target);
        Dirty(user, dashing);

        var doAfterArgs = new DoAfterArgs(EntityManager, user, dash.WindupTime, new DelayedDashDoAfterEvent(), eventTarget: weapon, used: weapon)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnHandChange = true,
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
        {
            RemCompDeferred<DelayedDashingComponent>(user);
            return;
        }

        _audio.PlayPredicted(dash.WindupSound, user, user);
    }

    private void OnDoAfter(Entity<DelayedDashComponent> ent, ref DelayedDashDoAfterEvent args)
    {
        var user = args.User;

        if (args.Cancelled)
        {
            RemCompDeferred<DelayedDashingComponent>(user);
            return;
        }

        if (_net.IsClient)
            return;

        if (!TryComp(user, out DelayedDashingComponent? dashing))
            return;

        args.Handled = true;

        if (TryComp(ent, out UseDelayComponent? delay))
        {
            _useDelay.TryResetDelay((ent.Owner, delay));
        }

        _audio.PlayPvs(ent.Comp.DashSound, user);

        if (TryComp(user, out FixturesComponent? fixtureComponent))
        {
            foreach (var (key, fixture) in fixtureComponent.Fixtures)
            {
                if ((fixture.CollisionMask & DashCollisionLayer) == 0)
                    continue;

                dashing.ChangedFixtures.Add(key);
                _physics.SetCollisionMask(user,
                    key,
                    fixture,
                    fixture.CollisionMask & ~DashCollisionLayer,
                    manager: fixtureComponent);
            }
        }

        dashing.Dashing = true;
        Dirty(user, dashing);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<DelayedDashingComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Dashing)
                continue;

            if (!TryComp(comp.Weapon, out DelayedDashComponent? dash) || !_hands.IsHolding(uid, comp.Weapon))
            {
                RemCompDeferred(uid, comp);
                continue;
            }

            if (comp.Thrown)
            {
                // Fallback
                if (curTime >= comp.LegEndTime)
                {
                    EndLeg((uid, comp));
                    continue;
                }

                SpawnTrail(uid, comp, dash, curTime);
                continue;
            }

            if (curTime < comp.NextLegTime)
                continue;

            if (comp.Destinations.Count == 0)
            {
                RemCompDeferred(uid, comp);
                continue;
            }

            StartLeg(uid, comp, dash, curTime);
        }
    }

    private void StartLeg(EntityUid uid, DelayedDashingComponent comp, DelayedDashComponent dash, TimeSpan curTime)
    {
        var destination = comp.Destinations[0];
        comp.Destinations.RemoveAt(0);
        Dirty(uid, comp);

        var origin = _transform.GetMapCoordinates(uid);

        if (destination.MapId != origin.MapId)
            return;

        var offset = destination.Position - origin.Position;
        var length = MathF.Min(offset.Length(), dash.MaxDashLength);

        if (length <= 0f)
            return;

        var dir = offset.Normalized() * length;

        comp.Thrown = true;
        comp.LegEndTime = curTime + TimeSpan.FromSeconds(length / dash.DashSpeed + 0.5f);
        comp.NextTrailTime = curTime;

        _throwing.TryThrow(uid, dir, dash.DashSpeed, null, 0f, null, false, false, false, false, false);
    }

    private void SpawnTrail(EntityUid uid, DelayedDashingComponent comp, DelayedDashComponent dash, TimeSpan curTime)
    {
        if (dash.TrailPrototype is null)
            return;

        if (curTime < comp.NextTrailTime)
            return;

        comp.NextTrailTime = curTime + dash.TrailInterval;
        SpawnAtPosition(dash.TrailPrototype, Transform(uid).Coordinates);
    }

    private void EndLeg(Entity<DelayedDashingComponent> ent)
    {
        if (_net.IsClient)
            return;

        if (!ent.Comp.Thrown)
            return;

        ent.Comp.Thrown = false;

        // Wait before starting next leg for obvious reasons
        ent.Comp.NextLegTime = _timing.CurTime + _timing.TickPeriod * 2;
    }

    private void OnLand(Entity<DelayedDashingComponent> ent, ref LandEvent args)
    {
        EndLeg(ent);
    }

    private void OnStopThrow(Entity<DelayedDashingComponent> ent, ref StopThrowEvent args)
    {
        EndLeg(ent);
    }

    private void OnCollide(Entity<DelayedDashingComponent> ent, ref StartCollideEvent args)
    {
        var (uid, comp) = ent;

        if (_net.IsClient)
            return;

        if (!comp.Thrown)
            return;

        if (!TryComp(uid, out ActorComponent? actor))
            return;

        if (!TryComp(comp.Weapon, out MeleeWeaponComponent? melee))
            return;

        if (comp.HitEntities.Contains(args.OtherEntity))
            return;

        if (!HasComp<MobStateComponent>(args.OtherEntity))
            return;

        if (!_hands.IsHolding(uid, comp.Weapon))
            return;

        comp.HitEntities.Add(args.OtherEntity);

        var ev = new LightAttackEvent(GetNetEntity(args.OtherEntity),
            GetNetEntity(comp.Weapon.Value),
            GetNetCoordinates(Transform(args.OtherEntity).Coordinates));
        _melee.DoLightAttack(uid, ev, comp.Weapon.Value, melee, actor.PlayerSession);
    }

    private void OnShutdown(Entity<DelayedDashingComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        if (!TryComp(ent, out FixturesComponent? fixtureComponent))
            return;

        foreach (var key in ent.Comp.ChangedFixtures)
        {
            if (!fixtureComponent.Fixtures.TryGetValue(key, out var fixture))
                continue;

            _physics.SetCollisionMask(ent,
                key,
                fixture,
                fixture.CollisionMask | DashCollisionLayer,
                fixtureComponent);
        }
    }
}
