using Content.Server.Heretic.Components;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Damage;
using Content.Shared.Follower;
using Content.Shared.Follower.Components;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;
using System.Linq;
using System.Numerics;

namespace Content.Server.Heretic.EntitySystems;

public sealed partial class ProtectiveBladeUsedEvent : EntityEventArgs
{
    public Entity<ProtectiveBladeComponent>? Used = null;
}

public sealed partial class ProtectiveBladeSystem : EntitySystem
{
    [Dependency] private readonly FollowerSystem _follow = default!;
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    [ValidatePrototypeId<EntityPrototype>] public const string BladePrototype = "HereticProtectiveBlade";
    [ValidatePrototypeId<EntityPrototype>] public const string BladeProjecilePrototype = "HereticProtectiveBladeProjectile";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ProtectiveBladeComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ProtectiveBladeComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<HereticComponent, BeforeDamageChangedEvent>(OnTakeDamage);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<ProtectiveBladeComponent>();
        while (eqe.MoveNext(out var uid, out var pbc))
        {
            pbc.Timer -= frameTime;

            if (pbc.Timer <= 0)
            {
                RemoveProtectiveBlade((uid, pbc));
            }
        }
    }

    private void OnInit(Entity<ProtectiveBladeComponent> ent, ref ComponentInit args)
    {
        ent.Comp.Timer = ent.Comp.Lifetime;
    }

    private void OnTakeDamage(Entity<HereticComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (!args.Damage.AnyPositive())
            return;

        var blades = GetBlades(ent);
        if (blades.Count == 0)
            return;

        var blade = blades[0];
        RemoveProtectiveBlade(blade);

        args.Cancelled = true;
    }

    private void OnInteract(Entity<ProtectiveBladeComponent> ent, ref InteractHandEvent args)
    {
        if (!TryComp<FollowerComponent>(ent, out var follower) || follower.Following != args.User)
            return;

        ThrowProtectiveBlade(args.User, ent);
    }

    public List<Entity<ProtectiveBladeComponent>> GetBlades(EntityUid ent)
    {
        var blades = new List<Entity<ProtectiveBladeComponent>>();

        if (!TryComp<FollowedComponent>(ent, out var followed))
            return blades;

        var linq = followed.Following
            .Where(f => HasComp<ProtectiveBladeComponent>(f))
            .ToList().ConvertAll(x => (x, Comp<ProtectiveBladeComponent>(x)));

        foreach (var blade in linq)
            blades.Add((blade.x, blade.Item2));

        return blades;
    }
    private EntityUid? GetNearestTarget(EntityUid origin, float range = 10f)
    {
        var oxform = Transform(origin);

        var lookup = _lookup.GetEntitiesInRange(origin, range)
            .Where(e => e != origin && TryComp<StatusEffectsComponent>(e, out _))
            .ToList();

        float? nearestPoint = null;
        EntityUid? ret = null;
        foreach (var look in lookup)
        {
            var distance = (oxform.LocalPosition - Transform(look).LocalPosition).LengthSquared();
            if (nearestPoint == null || distance < nearestPoint)
            {
                nearestPoint = distance;
                ret = look;
            }
        }

        return ret;
    }

    public void AddProtectiveBlade(EntityUid ent)
    {
        var pblade = Spawn(BladePrototype, Transform(ent).Coordinates);
        _follow.StartFollowingEntity(pblade, ent);

        if (TryComp<OrbitVisualsComponent>(pblade, out var vorbit))
        {
            // test scenario: 4 blades are currently following our heretic.
            // making each one somewhat distinct from each other
            vorbit.Orbit = GetBlades(ent).Count / 5;
        }
    }
    public void RemoveProtectiveBlade(Entity<ProtectiveBladeComponent> blade)
    {
        if (!TryComp<FollowerComponent>(blade, out var follower))
            return;

        var ev = new ProtectiveBladeUsedEvent() { Used = blade };
        RaiseLocalEvent(follower.Following, ev);

        QueueDel(blade);
    }
    public void ThrowProtectiveBlade(EntityUid origin, Entity<ProtectiveBladeComponent> pblade, EntityUid? target = null)
    {
        _follow.StopFollowingEntity(origin, pblade);

        var tgt = target ?? GetNearestTarget(origin);

        var direction = _xform.GetWorldRotation(origin).ToWorldVec();

        if (tgt != null)
            direction = _xform.GetWorldPosition((EntityUid) tgt) - _xform.GetWorldPosition(origin);

        var proj = Spawn(BladeProjecilePrototype, Transform(origin).Coordinates);
        _gun.ShootProjectile(proj, direction, Vector2.Zero, origin, origin);

        QueueDel(pblade);
    }
    public void ThrowProtectiveBlade(EntityUid origin, EntityUid? target = null)
    {
        var blades = GetBlades(origin);
        if (blades.Count > 0)
            ThrowProtectiveBlade(origin, blades[0], target);
    }
}
