using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared._White.Grab;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wraith.Revenant;

public sealed class TouchOfEvilSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly GrabThrownSystem _throw = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TouchOfEvilComponent, TouchOfEvilEvent>(OnTouchOfEvil);

        SubscribeLocalEvent<ActiveTouchOfEvilComponent, MeleeHitEvent>(OnMeleeHit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<TouchOfEvilComponent>();
        while (eqe.MoveNext(out var uid, out var comp))
        {
            if (!comp.Active)
                continue;

            if (_timing.CurTime < comp.NextUpdate)
                continue;

            // revert to original damage
            if (!TryComp<MeleeWeaponComponent>(uid, out var melee)
                || comp.OriginalDamage == null)
                continue;

            melee.Damage = comp.OriginalDamage;

            comp.Active = false;
            comp.OriginalDamage = null;
            Dirty(uid, comp);

            RemCompDeferred<ActiveTouchOfEvilComponent>(uid);
        }
    }

    private void OnTouchOfEvil(Entity<TouchOfEvilComponent> ent, ref TouchOfEvilEvent args)
    {
        // dont activate if already active
        if (ent.Comp.Active
            || !TryComp<MeleeWeaponComponent>(ent.Owner, out var melee))
            return;

        var comp = EnsureComp<ActiveTouchOfEvilComponent>(ent.Owner);
        comp.ThrowSpeed = ent.Comp.ThrowSpeed;
        Dirty(ent.Owner, comp);

        ent.Comp.Active = true;
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.BuffDuration;
        ent.Comp.OriginalDamage = melee.Damage;
        Dirty(ent);

        melee.Damage *= ent.Comp.DamageBuff;

        args.Handled = true;
    }

    private void OnMeleeHit(Entity<ActiveTouchOfEvilComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.HitEntities.Any())
            return;

        foreach (var entity in args.HitEntities)
        {
            _throw.Throw(entity,
                ent.Owner,
                GetThrowDirection(ent.Owner, entity),
                ent.Comp.ThrowSpeed);
        }
    }

    private Vector2 GetThrowDirection(EntityUid user, EntityUid target)
    {
        var entPos = _transform.GetMapCoordinates(user).Position;
        var targetPos = _transform.GetMapCoordinates(target).Position;
        return targetPos - entPos;
    }
}
