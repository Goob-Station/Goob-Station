using Content.Goobstation.CommonShared.Wizard.Components;
using Content.Goobstation.Shared.Wizard.Components;
using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared.Physics;
using Content.Shared.Random.Helpers;

namespace Content.Goobstation.Server.Wizard.Systems;

public sealed partial class SpellsSystem
{
    protected override void SpawnMonkeysRelay(SummonSimiansEvent ev)
    {
        if (!_prototypeManager.TryIndex(ev.Mobs, out var mobs) || !_prototypeManager.TryIndex(ev.Weapons, out var weapons))
            return;

        if (mobs.Weights.Count == 0)
            return;

        var positions = GetSpawnCoordinatesAroundPerformer(ev.Performer,
            ev.Range,
            ev.Amount,
            ev.SpawnAngle,
            (int) CollisionGroup.MobMask);
        foreach (var pos in positions)
        {
            var mob = Spawn(mobs.Pick(_random), pos);

            if (!_handsQuery.TryComp(mob, out var hands) || hands.Count == 0 || weapons.Weights.Count == 0)
                continue;

            var weapon = Spawn(weapons.Pick(_random), pos);

            if (!_hands.TryPickupAnyHand(mob, weapon, true, false, false, hands))
            {
                QueueDel(weapon);
                continue;
            }

            FadingTimedDespawnComponent? weaponDespawn;
            if (_timedDespawnQuery.TryComp(mob, out var despawn))
            {
                weaponDespawn = EnsureComp<FadingTimedDespawnComponent>(weapon);
                weaponDespawn.Lifetime = despawn.Lifetime + 30f;
                weaponDespawn.FadeOutTime = 4f;
                Dirty(weapon, weaponDespawn);
            }
            else if (_fadingTimedDespawnQuery.TryComp(mob, out var fading))
            {
                weaponDespawn = EnsureComp<FadingTimedDespawnComponent>(weapon);
                weaponDespawn.Lifetime = fading.Lifetime + 30f;
                weaponDespawn.FadeOutTime = 4f;
                Dirty(weapon, weaponDespawn);
            }
        }
    }
}