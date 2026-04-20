using System.Numerics;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Map;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Projectiles;

public sealed class ProjectileImmunitySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ProjectileImmunityComponent, PreventCollideEvent>(OnPreventCollide);
        SubscribeLocalEvent<ProjectileImmunityComponent, ProjectileReflectAttemptEvent>(OnProjectileReflect);
        SubscribeLocalEvent<ProjectileImmunityComponent, HitScanReflectAttemptEvent>(OnHitscanReflect);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<ProjectileImmunityComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.ExpireTime != null && now >= comp.ExpireTime)
                RemCompDeferred<ProjectileImmunityComponent>(uid);
        }
    }

    private bool CanDodge(Entity<ProjectileImmunityComponent> ent)
    {
        if (ent.Comp.StaminaCostPerDodge > 0
            && TryComp<StaminaComponent>(ent, out var stamina)
            && stamina.Critical)
        {
            RemCompDeferred<ProjectileImmunityComponent>(ent);
            return false;
        }

        var ev = new ProjectileDodgeAttemptEvent();
        RaiseLocalEvent(ent, ref ev);
        if (ev.Cancelled)
        {
            RemCompDeferred<ProjectileImmunityComponent>(ent);
            return false;
        }

        return true;
    }

    private void OnPreventCollide(Entity<ProjectileImmunityComponent> ent, ref PreventCollideEvent args)
    {
        if (!HasComp<ProjectileComponent>(args.OtherEntity))
            return;

        if (!args.OtherFixture.Hard)
            return;

        if (!CanDodge(ent))
            return;

        args.Cancelled = true;

        if (_timing.IsFirstTimePredicted)
            TryHandleDodge(ent, args.OtherEntity);
    }

    private void OnProjectileReflect(Entity<ProjectileImmunityComponent> ent, ref ProjectileReflectAttemptEvent args)
    {
        if (!CanDodge(ent))
            return;

        args.Cancelled = true;

        if (_timing.IsFirstTimePredicted)
            TryHandleDodge(ent, args.ProjUid);
    }

    private void OnHitscanReflect(Entity<ProjectileImmunityComponent> ent, ref HitScanReflectAttemptEvent args)
    {
        if (args.Reflected)
            return;

        if (!CanDodge(ent))
            return;

        args.Reflected = true;

        if (ent.Comp.StaminaCostPerDodge > 0)
            _stamina.TakeStaminaDamage(ent, ent.Comp.StaminaCostPerDodge, logDamage: false);

        if (ent.Comp.DodgeEffect != null)
            SpawnAttachedTo(ent.Comp.DodgeEffect.Value, new EntityCoordinates(ent, Vector2.Zero));
    }

    private void TryHandleDodge(Entity<ProjectileImmunityComponent> ent, EntityUid projectile)
    {
        if (TerminatingOrDeleted(projectile))
            return;

        if (!ent.Comp.DodgedEntities.Add(projectile))
            return;

        if (ent.Comp.StaminaCostPerDodge > 0)
            _stamina.TakeStaminaDamage(ent, ent.Comp.StaminaCostPerDodge, logDamage: false);

        if (ent.Comp.DodgeEffect != null)
            SpawnAttachedTo(ent.Comp.DodgeEffect.Value, new EntityCoordinates(ent, Vector2.Zero));
    }
}
