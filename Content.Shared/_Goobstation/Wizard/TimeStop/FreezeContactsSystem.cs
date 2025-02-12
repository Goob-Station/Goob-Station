using System.Linq;
using System.Numerics;
using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared._Goobstation.Wizard.Guardian;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Emoting;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Speech;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Spawners;

namespace Content.Shared._Goobstation.Wizard.TimeStop;

public sealed class FreezeContactsSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    private static readonly ProtoId<TagPrototype> FrozenIgnoreMindActionTag = "FrozenIgnoreMindAction";

    private const string ProjectileFixture = "projectile";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FreezeContactsComponent, StartCollideEvent>(OnEntityEnter);
        SubscribeLocalEvent<FreezeContactsComponent, EndCollideEvent>(OnEntityExit);

        SubscribeLocalEvent<FrozenComponent, UseAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<FrozenComponent, PickupAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<FrozenComponent, ThrowAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<FrozenComponent, InteractionAttemptEvent>(OnInteractAttempt);
        SubscribeLocalEvent<FrozenComponent, ComponentStartup>(MoveUpdate);
        SubscribeLocalEvent<FrozenComponent, ComponentShutdown>(MoveUpdate);
        SubscribeLocalEvent<FrozenComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<FrozenComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<FrozenComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
        SubscribeLocalEvent<FrozenComponent, PullAttemptEvent>(OnPullAttempt);
        SubscribeLocalEvent<FrozenComponent, AttackAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<FrozenComponent, ChangeDirectionAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<FrozenComponent, EmoteAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<FrozenComponent, SpeakAttemptEvent>(OnAttempt);
    }

    private void OnRemove(Entity<FrozenComponent> ent, ref ComponentRemove args)
    {
        var (uid, comp) = ent;

        if (TerminatingOrDeleted(uid))
            return;

        if (TryComp(uid, out PhysicsComponent? physics) && TryComp(uid, out FixturesComponent? fix))
        {
            _physics.SetAngularVelocity(uid, comp.OldAngularVelocity, false, fix, physics);
            _physics.SetLinearVelocity(uid, comp.OldLinearVelocity, true, true, fix, physics);
        }

        if (comp.HadCollisionWake)
            EnsureComp<CollisionWakeComponent>(uid);

        if (comp.FreezeTime <= 0f)
            return;

        if (_net.IsServer && TryComp(uid, out TimedDespawnComponent? despawn))
            despawn.Lifetime -= comp.FreezeTime;

        if (TryComp(uid, out FadingTimedDespawnComponent? fading) && !fading.FadeOutStarted)
            fading.Lifetime -= comp.FreezeTime;

        if (!TryComp(uid, out ThrownItemComponent? thrownItem) || thrownItem.LandTime == null)
            return;

        thrownItem.LandTime = thrownItem.LandTime.Value - TimeSpan.FromSeconds(comp.FreezeTime);
        Dirty(uid, thrownItem);
    }

    private void OnInit(Entity<FrozenComponent> ent, ref ComponentInit args)
    {
        var (uid, comp) = ent;

        if (!TryComp(uid, out PhysicsComponent? physics) || !TryComp(uid, out FixturesComponent? fix))
            return;

        comp.OldLinearVelocity = physics.LinearVelocity;
        comp.OldAngularVelocity = physics.AngularVelocity;
        _physics.SetAngularVelocity(uid, 0f, false, fix, physics);
        _physics.SetLinearVelocity(uid, Vector2.Zero, true, false, fix, physics);

        if (!HasComp<CollisionWakeComponent>(uid))
            return;

        comp.HadCollisionWake = true;
        RemComp<CollisionWakeComponent>(uid);
    }

    private void MoveUpdate(EntityUid uid, FrozenComponent component, EntityEventArgs args)
    {
        _blocker.UpdateCanMove(uid);
    }

    private void OnInteractAttempt(Entity<FrozenComponent> ent, ref InteractionAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnAttempt(EntityUid uid, FrozenComponent component, CancellableEntityEventArgs args)
    {
        args.Cancel();
    }

    private void OnPullAttempt(EntityUid uid, FrozenComponent component, PullAttemptEvent args)
    {
        if (args.PullerUid == uid)
            args.Cancelled = true;
    }

    private void OnUpdateCanMove(EntityUid uid, FrozenComponent component, UpdateCanMoveEvent args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        args.Cancel();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        var query = EntityQueryEnumerator<FrozenComponent, PhysicsComponent, FixturesComponent>();

        while (query.MoveNext(out var ent, out var frozen, out var physics, out var fix))
        {
            if (frozen.FreezeTime < 0f)
            {
                RemCompDeferred<FrozenComponent>(ent);
                continue;
            }

            _physics.SetAngularVelocity(ent, 0f, false, fix, physics);
            _physics.SetLinearVelocity(ent, Vector2.Zero, true, false, fix, physics);

            frozen.FreezeTime -= frameTime;
        }
    }

    private void OnEntityExit(EntityUid uid, FreezeContactsComponent component, ref EndCollideEvent args)
    {
        if (_net.IsClient)
            return;

        if (!ShouldCollideWith(args.OtherFixture, args.OtherFixtureId))
            return;

        var otherUid = args.OtherEntity;

        if (!TryComp<PhysicsComponent>(otherUid, out var body))
            return;

        var query = GetEntityQuery<FreezeContactsComponent>();
        if (_physics.GetContactingEntities(otherUid, body).Where(ent => ent != uid).Any(ent => query.HasComponent(ent)))
            return;

        RemCompDeferred<FrozenComponent>(otherUid);
    }

    private void OnEntityEnter(EntityUid uid, FreezeContactsComponent component, ref StartCollideEvent args)
    {
        if (!ShouldCollideWith(args.OtherFixture, args.OtherFixtureId))
            return;

        var otherUid = args.OtherEntity;

        if (!TryComp(uid, out TimedDespawnComponent? despawn) || despawn.Lifetime <= 0f)
            return;

        TimedDespawnComponent? otherDespawn;
        FadingTimedDespawnComponent? fading;
        ThrownItemComponent? thrownItem;
        if (TryComp(otherUid, out FrozenComponent? frozen))
        {
            if (despawn.Lifetime <= frozen.FreezeTime)
                return;

            var difference = despawn.Lifetime - frozen.FreezeTime;

            if (TryComp(otherUid, out thrownItem) && thrownItem.LandTime != null)
            {
                thrownItem.LandTime = thrownItem.LandTime.Value + TimeSpan.FromSeconds(difference);
                thrownItem.Animate = false;
                Dirty(otherUid, thrownItem);
            }

            if (TryComp(otherUid, out otherDespawn))
                otherDespawn.Lifetime += difference;

            if (TryComp(otherUid, out fading) && !fading.FadeOutStarted)
                fading.Lifetime += difference;

            frozen.FreezeTime = despawn.Lifetime;
            return;
        }

        if (IsImmune(otherUid) || TryComp(otherUid, out GuardianSharedComponent? guardian) && IsImmune(guardian.Host))
            return;

        EnsureComp<FrozenComponent>(otherUid).FreezeTime = despawn.Lifetime;

        if (TryComp(otherUid, out otherDespawn))
            otherDespawn.Lifetime += despawn.Lifetime;

        if (TryComp(otherUid, out fading) && !fading.FadeOutStarted)
            fading.Lifetime += despawn.Lifetime;

        if (!TryComp(otherUid, out thrownItem) || thrownItem.LandTime == null)
            return;

        thrownItem.LandTime = thrownItem.LandTime.Value + TimeSpan.FromSeconds(despawn.Lifetime);
        thrownItem.Animate = false;
        Dirty(otherUid, thrownItem);

        return;

        bool IsImmune(EntityUid entity)
        {
            return _actions.GetActions(entity).Any(e => _tag.HasTag(e.Id, FrozenIgnoreMindActionTag));
        }
    }

    private bool ShouldCollideWith(Fixture fix, string id)
    {
        return fix.Hard || id == ProjectileFixture;
    }
}
