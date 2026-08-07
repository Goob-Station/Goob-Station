using System.Numerics;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Projectiles;

/// <summary>
/// Entities with AutoDodgeComponent dodge projectiles and play a dodge animation.
/// </summary>
public sealed class AutoDodgeSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AutoDodgeComponent, PreventCollideEvent>(OnPreventCollide);
        SubscribeLocalEvent<AutoDodgeComponent, ProjectileReflectAttemptEvent>(OnProjectileReflect);
        SubscribeLocalEvent<AutoDodgeComponent, HitScanReflectAttemptEvent>(OnHitscanReflect);
    }

    /// <summary>
    /// checks if they can dodge.
    /// </summary>
    private bool CanDodge(Entity<AutoDodgeComponent> ent)
    {
        if (TryComp<MobStateComponent>(ent, out var mobState) && mobState.CurrentState != MobState.Alive
            || HasComp<KnockedDownComponent>(ent)
            || HasComp<StunnedComponent>(ent)
            || TryComp<StandingStateComponent>(ent, out var standing) && !standing.Standing)
            return false;

        return true;
    }

    private void OnPreventCollide(Entity<AutoDodgeComponent> ent, ref PreventCollideEvent args)
    {
        if (args.Cancelled
            || !args.OtherFixture.Hard
            || !HasComp<ProjectileComponent>(args.OtherEntity)
            || !CanDodge(ent))
            return;

        args.Cancelled = true;

        HandleProjectileDodge(ent, args.OtherEntity);
    }

    private void OnProjectileReflect(Entity<AutoDodgeComponent> ent, ref ProjectileReflectAttemptEvent args)
    {
        if (args.Cancelled || !CanDodge(ent))
            return;

        args.Cancelled = true;

        HandleProjectileDodge(ent, args.ProjUid);
    }

    private void OnHitscanReflect(Entity<AutoDodgeComponent> ent, ref HitScanReflectAttemptEvent args)
    {
        if (args.Reflected || !CanDodge(ent))
            return;

        args.Reflected = true;

        DoDodgeEffects(ent, args.Direction, _random.Prob(0.5f));
    }

    private void HandleProjectileDodge(Entity<AutoDodgeComponent> ent, EntityUid projectile)
    {
        if (TerminatingOrDeleted(projectile))
            return;

        if (TryComp<ProjectileComponent>(projectile, out var projComp))
        {
            var dirty = false;
            if (projComp.Shooter is { } shooter && TerminatingOrDeleted(shooter))
            {
                projComp.Shooter = null;
                dirty = true;
            }

            if (projComp.Weapon is { } weapon && TerminatingOrDeleted(weapon))
            {
                projComp.Weapon = null;
                dirty = true;
            }

            if (dirty)
                Dirty(projectile, projComp);
        }

        // Derived from the projectile id instead of RNG so client prediction picks the same side.
        var flipSide = (GetNetEntity(projectile).Id & 1) == 0;

        DoDodgeEffects(ent, _physics.GetMapLinearVelocity(projectile), flipSide);
    }

    private void DoDodgeEffects(Entity<AutoDodgeComponent> ent, Vector2 shotDirection, bool flipSide)
    {
        if (_timing.CurTime < ent.Comp.NextEffectTime)
            return;

        ent.Comp.NextEffectTime = _timing.CurTime + ent.Comp.EffectCooldown;
        Dirty(ent);

        var direction = ComputeDodgeDirection(shotDirection, flipSide);

        var ev = new AutoDodgeEffectEvent(GetNetEntity(ent), new Vector2(MathF.Sign(direction.X), 0f));
        RaiseNetworkEvent(ev, Filter.Pvs(ent, entityManager: EntityManager));

        _audio.PlayPvs(ent.Comp.DodgeSound, ent);
    }

    /// <summary>
    /// Picks the direction the sprite dodges towards.
    /// </summary>
    private static Vector2 ComputeDodgeDirection(Vector2 shotDirection, bool flipSide)
    {
        var sidestep = shotDirection.LengthSquared() > 0f
            ? Vector2.Normalize(new Vector2(-shotDirection.Y, shotDirection.X))
            : Vector2.UnitX;

        if (flipSide)
            sidestep = -sidestep;

        var signX = sidestep.X != 0f ? MathF.Sign(sidestep.X) : MathF.Sign(sidestep.Y);
        var x = signX * MathF.Max(MathF.Abs(sidestep.X), 0.8f);
        var y = Math.Clamp(sidestep.Y, -0.35f, 0.35f);
        return Vector2.Normalize(new Vector2(x, y));
    }
}
