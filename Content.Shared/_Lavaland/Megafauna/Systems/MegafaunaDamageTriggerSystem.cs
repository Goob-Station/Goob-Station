using Content.Shared._Lavaland.Boss;
using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Events;
using Content.Shared.Damage;

namespace Content.Shared._Lavaland.Megafauna.Systems;

/// <summary>
/// Handles events for triggering other components whenever the Megafauna takes any form of damage, be it predicted or not.
/// </summary>
public sealed class MegafaunaDamageTriggerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MegafaunaDamageTriggerComponent, DamageChangedEvent>(OnDamage);
    }

    private void OnDamage(Entity<MegafaunaDamageTriggerComponent> ent, ref DamageChangedEvent args)
    {
        if (ent.Comp.Triggered)
            return;

        if (!TryComp<DamageableComponent>(ent, out var damage))
            return;

        if (damage.TotalDamage <= 0)
            return;

        ent.Comp.Triggered = true;

        var ev = new BossFirstDamageEvent(GetNetEntity(ent.Owner));
        RaiseNetworkEvent(ev);
    }

}

