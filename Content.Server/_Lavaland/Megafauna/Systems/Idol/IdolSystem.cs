using Content.Shared._Lavaland.Megafauna.Components.Idol;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;

namespace Content.Server._Lavaland.Megafauna.Systems.Idol;

/// <summary>
/// Deletes idols on death. Damages Producer boss in the process.
/// </summary>
public sealed class IdolSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IdolComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(EntityUid uid, IdolComponent comp, MobStateChangedEvent args)
    {
        if (!_mobState.IsDead(uid))
            return;

        if (comp.Producer.HasValue
            && EntityManager.EntityExists(comp.Producer.Value)
            && TryComp<DamageableComponent>(comp.Producer.Value, out var producerDamageable))
        {
            var damage = new DamageSpecifier();
            damage.DamageDict.Add("Blunt", comp.DamageOnDeath);
            _damageable.TryChangeDamage(comp.Producer.Value, damage, ignoreResistances: true, damageable: producerDamageable);
        }

        QueueDel(uid);
    }
}
