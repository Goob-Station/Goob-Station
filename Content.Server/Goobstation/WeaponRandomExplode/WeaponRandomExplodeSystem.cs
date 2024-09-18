using Robust.Shared.Random;
using Content.Shared.Weapons.Ranged.Events;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Power.Components;

namespace Content.Server.Goobstation.WeaponRandomExplode
{
    public sealed class WeaponRandomExplodeSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly ExplosionSystem _explosionSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<WeaponRandomExplodeComponent, ShotAttemptedEvent>(OnShot);
        }

        private void OnShot(EntityUid uid, WeaponRandomExplodeComponent component, ShotAttemptedEvent args)
        {
            if (component.explosionChance == null)
                return;

            if (_random.Prob(component.explosionChance))
            {
                var intensity = 1; 
                if (component.multiplyByCharge)
                {
                    intensity = MultiplyByCharge(uid);                   
                }

                _explosionSystem.QueueExplosion(
                    (EntityUid) uid,
                    typeId: "Default",
                    totalIntensity: intensity,
                    slope: 5,
                    maxTileIntensity: 10);
                QueueDel(uid);
            }
        }

        private int MultiplyByCharge(EntityUid uid)
        {
            TryComp<BatteryComponent>(uid, out var battery);
            if (battery == null)
                return 1;
            return Convert.ToInt32(battery.CurrentCharge / 100);
        }
    }
}