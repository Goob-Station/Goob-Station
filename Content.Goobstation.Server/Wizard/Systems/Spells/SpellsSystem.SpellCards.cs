using System.Numerics;
using Content.Goobstation.Shaerd.Wizard.Components;
using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Wizard.Systems;

public sealed partial class SpellsSystem
{
    // TODO: shared, cant yet cuz of LinearSpread and stuff
    protected override void ShootSpellCardsRelay(SpellCardsEvent ev, EntProtoId proto)
    {
        var targetMap = _xform.ToMapCoordinates(ev.Target);

        var (_, mapCoords, spawnCoords, velocity) = GetProjectileData(ev.Performer);

        var mapDirection = targetMap.Position - mapCoords.Position;
        if (mapDirection == Vector2.Zero)
            return;
        var mapAngle = mapDirection.ToAngle();

        var angles = _gun.LinearSpread(mapAngle - ev.Spread / 2, mapAngle + ev.Spread / 2, ev.ProjectilesAmount);

        var linearDamping = _random.NextFloat(ev.MinMaxLinearDamping.X, ev.MinMaxLinearDamping.Y);

        var setHoming = Exists(ev.Entity) && ev.Entity != ev.Performer && HasComp<MobStateComponent>(ev.Entity);

        for (var i = 0; i < ev.ProjectilesAmount; i++)
        {
            var newUid = Spawn(proto, spawnCoords);
            _gun.ShootProjectile(newUid, angles[i].ToVec(), velocity, ev.Performer, ev.Performer, ev.ProjectileSpeed);

            if (!_physicsQuery.TryComp(newUid, out var physics))
                continue;

            _physics.SetAngularVelocity(newUid,
                _random.NextFloat(-ev.MaxAngularVelocity, ev.MaxAngularVelocity),
                false,
                body: physics);
            _physics.SetLinearDamping(newUid, physics, linearDamping, false);
            _tileFrictionController.SetModifier(newUid, linearDamping);

            var spellCard = EnsureComp<SpellCardComponent>(newUid);
            if (!setHoming)
            {
                Dirty(newUid, physics);
                continue;
            }

            spellCard.Target = ev.Entity;
            _gun.SetTarget(newUid, ev.Entity, out var targeted, false);
            Entity<SpellCardComponent, PhysicsComponent, TargetedProjectileComponent> ent = (newUid, spellCard, physics,
                targeted);
            Dirty(ent);
        }
    }
}