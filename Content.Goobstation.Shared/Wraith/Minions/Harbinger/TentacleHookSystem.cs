using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Body.Components;
using Content.Shared.Physics;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using System.Numerics;
using Content.Shared.Stunnable;
using Robust.Shared.Spawners;

namespace Content.Goobstation.Shared.Wraith.Minions.Harbinger;

public sealed class TentacleHookSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedJointSystem _joints = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private const string TentacleJoint = "grappling";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TentacleHookComponent, TentacleHookEvent>(OnTentacleHook);

        SubscribeLocalEvent<TentacleHookProjectileComponent, ProjectileHitEvent>(OnTentacleHit);
        SubscribeLocalEvent<TentacleHookProjectileComponent, JointRemovedEvent>(OnJointRemoved);
        SubscribeLocalEvent<TentacleHookProjectileComponent, ProjectileEmbedEvent>(OnTentacleEmbed);
        SubscribeLocalEvent<TentacleHookProjectileComponent, TimedDespawnEvent>(OnDespawn);
    }

    private void OnTentacleHook(Entity<TentacleHookComponent> ent, ref TentacleHookEvent args)
    {
        if (_netManager.IsClient)
            return;

        var proj = SpawnAtPosition(ent.Comp.TentacleProto, Transform(ent.Owner).Coordinates);
        var projPos = _transform.GetWorldPosition(proj);
        var targetPos = _transform.GetWorldPosition(args.Target);

        var dir = (targetPos - projPos).Normalized();

        ent.Comp.Projectile = proj;

        var visuals = EnsureComp<JointVisualsComponent>(proj);
        visuals.Sprite = ent.Comp.RopeSprite;
        visuals.OffsetA = new Vector2(0f, 0.5f);
        visuals.Target = GetNetEntity(ent.Owner);
        Dirty(proj, visuals);

        _audio.PlayPredicted(ent.Comp.HookSound, ent.Owner, ent.Owner);
        _gun.ShootProjectile(proj,
            dir,
            Vector2.Zero,
            null,
            ent.Owner);

        args.Handled = true;
    }

    private void OnTentacleEmbed(Entity<TentacleHookProjectileComponent> ent, ref ProjectileEmbedEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (args.Shooter is not {} shooter
            || !HasComp<BodyComponent>(args.Embedded))
            return;

        EnsureComp<JointComponent>(ent.Owner);
        var joint = _joints.CreateDistanceJoint(ent.Owner, shooter, anchorA: new Vector2(0f, 0.5f), id: TentacleJoint);
        joint.MaxLength = joint.Length + 0.2f;
        joint.Stiffness = 1f;
        joint.MinLength = 0.35f;
        Dirty(ent);
    }

    private void OnTentacleHit(Entity<TentacleHookProjectileComponent> ent, ref ProjectileHitEvent args)
    {
        if (!HasComp<BodyComponent>(args.Target))
        {
            PredictedDel(ent.Owner);
            return;
        }

        ent.Comp.Target = args.Target;
        Dirty(ent);

        _stun.TrySlowdown(args.Target, ent.Comp.DurationSlow, false, 0.3f, 0.3f);

        var tentacle = EnsureComp<TentacleHookedComponent>(args.Target);
        tentacle.ThrowTowards = args.Shooter;
        tentacle.Projectile = ent.Owner;
        Dirty(args.Target, tentacle);
    }

    private void OnJointRemoved(Entity<TentacleHookProjectileComponent> ent, ref JointRemovedEvent args)
    {
        if (_netManager.IsServer)
            QueueDel(ent.Owner);
    }

    private void OnDespawn(Entity<TentacleHookProjectileComponent> ent, ref TimedDespawnEvent args)
    {
        if (ent.Comp.Target == null)
            return;

        RemCompDeferred<TentacleHookedComponent>(ent.Comp.Target.Value);
    }
}
