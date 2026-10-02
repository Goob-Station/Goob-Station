using Content.Goobstation.Common.Weapons.Ranged;
using Content.Goobstation.Shared.Cyberware;
using Content.Goobstation.Shared.SmartLinkImplant.Components;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Goobstation.Shared.SmartLinkImplant;

public sealed class SmartLinkSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly CyberneticsSystem _cybernetics = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmartLinkHandComponent, ComponentInit>(OnLinkChanged);
        SubscribeLocalEvent<SmartLinkHandComponent, BodyPartAddedEvent>(OnLinkChanged);
        SubscribeLocalEvent<SmartLinkHandComponent, BodyPartRemovedEvent>(OnLinkChanged);
        SubscribeLocalEvent<SmartLinkHandComponent, CyberwareChangedEvent>(OnLinkChanged);

        SubscribeLocalEvent<SmartLinkComponent, AmmoShotUserEvent>(OnShot);
    }

    private void OnLinkChanged<T>(Entity<SmartLinkHandComponent> ent, ref T args)
        => UpdateComp(ent);

    private void UpdateComp(Entity<SmartLinkHandComponent> ent)
    {
        if (!TryComp<BodyPartComponent>(ent, out var part) || part.Body is not { } body)
            return;

        _cybernetics.TryGetImplants(body, out var implants);

        foreach (var (implant, ware) in implants)
            if (HasComp<SmartLinkHandComponent>(implant) && !ware.Enabled)
            {
                RemComp<SmartLinkComponent>(body);
                return;
            }

        foreach (var wired in _body.GetBodyChildrenOfType(body, part.PartType))
        {
            if (HasComp<SmartLinkHandComponent>(wired.Id) && _cybernetics.IsEnabled(wired.Id))
                continue;

            RemComp<SmartLinkComponent>(body);
            return;
        }

        EnsureComp<SmartLinkComponent>(body);
    }

    private void OnShot(Entity<SmartLinkComponent> ent, ref AmmoShotUserEvent args)
    {
        var (uid, comp) = ent;

        if (!TryComp<GunComponent>(args.Gun, out var gun))
            return;

        var overclocked = _cybernetics.LowestClock<SmartLinkHandComponent>(uid) >= 1;
        var target = gun.Target;

        if (overclocked && !IsLiving(target) && gun.ShootCoordinates is { } aim)
            target = AcquireTarget(uid, aim, comp.AcquisitionRange) ?? target;

        if (target is not { } mark)
            return;

        if (mark == Transform(uid).ParentUid || mark == uid)
            return;

        foreach (var projectile in args.FiredProjectiles)
        {
            if (HasComp<SmartLinkBlacklistComponent>(projectile))
                return;

            if (TryComp<PhysicsComponent>(projectile, out var physics))
                _physics.SetLinearVelocity(projectile, physics.LinearVelocity * comp.SpeedMultiplier, body: physics);

            var homing = EnsureComp<HomingProjectileComponent>(projectile);
            homing.Target = mark;
            Dirty(projectile, homing);
        }
    }

    private bool IsLiving(EntityUid? uid)
        => TryComp<MobStateComponent>(uid, out var state) && !_mobState.IsDead(uid.Value, state);

    /// <summary>
    /// Looks for the closest mob to wherever the user shot.
    /// </summary>
    private EntityUid? AcquireTarget(EntityUid shooter, EntityCoordinates aim, float range)
    {
        EntityUid? best = null;
        var bestDistance = float.MaxValue;
        var aimMap = _transform.ToMapCoordinates(aim);

        foreach (var mob in _lookup.GetEntitiesInRange<MobStateComponent>(aim, range))
        {
            if (mob.Owner == shooter || _mobState.IsDead(mob, mob.Comp))
                continue;

            var distance = (_transform.GetMapCoordinates(mob).Position - aimMap.Position).LengthSquared();
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = mob;
            }
        }

        return best;
    }
}
