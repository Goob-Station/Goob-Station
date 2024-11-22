using System.Linq;
using System.Numerics;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Components;
using Content.Server.Examine;
using Content.Server.Popups;
using Content.Server.Weapons.Ranged.Systems;
using Content.Server.WhiteDream.BloodCult.BloodBoilProjectile;
using Content.Shared.Projectiles;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Random;

namespace Content.Server.WhiteDream.BloodCult.Runes.BloodBoil;

public sealed class CultRuneBloodBoilSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly CultRuneBaseSystem _cultRune = default!;
    [Dependency] private readonly ExamineSystem _examine = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultRuneBloodBoilComponent, TryInvokeCultRuneEvent>(OnBloodBoilRuneInvoked);
    }

    private void OnBloodBoilRuneInvoked(Entity<CultRuneBloodBoilComponent> ent, ref TryInvokeCultRuneEvent args)
    {
        var targets = _cultRune.GetTargetsNearRune(ent,
                ent.Comp.TargetsLookupRange,
                entity =>
                    HasComp<BloodCultistComponent>(entity) ||
                    !HasComp<BloodstreamComponent>(entity) ||
                    !_examine.InRangeUnOccluded(ent, entity, ent.Comp.TargetsLookupRange))
            .ToList();

        if (targets.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("cult-blood-boil-rune-no-targets"), ent, args.User);
            args.Cancel();
            return;
        }

        for (var i = 0; i < ent.Comp.ProjectileCount; i++)
        {
            var target = _random.PickAndTake(targets);
            if (HasComp<FlammableComponent>(target))
            {
                _flammable.AdjustFireStacks(target, ent.Comp.FireStacksPerProjectile);
                _flammable.Ignite(target, ent);
            }

            Shoot(ent, target);
        }

        _audio.PlayPvs(ent.Comp.ActivationSound, ent, AudioParams.Default.WithMaxDistance(2f));
    }

    private void Shoot(Entity<CultRuneBloodBoilComponent> ent, EntityUid target)
    {
        var runeMapPos = _transform.GetMapCoordinates(ent);
        var targetMapPos = _transform.GetMapCoordinates(target);

        var projectileEntity = Spawn(ent.Comp.ProjectilePrototype, runeMapPos);
        var direction = targetMapPos.Position - runeMapPos.Position;

        if (!HasComp<ProjectileComponent>(projectileEntity))
            return;

        var bloodBoilProjectile = EnsureComp<BloodBoilProjectileComponent>(projectileEntity);
        bloodBoilProjectile.Target = target;

        _gun.ShootProjectile(projectileEntity, direction, Vector2.Zero, ent, ent, ent.Comp.ProjectileSpeed);
    }
}
