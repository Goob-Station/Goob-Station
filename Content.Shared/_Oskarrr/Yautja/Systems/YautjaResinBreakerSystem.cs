using Content.Shared._Oskarrr.Yautja.Components;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Network;

namespace Content.Shared._Oskarrr.Yautja.Systems;

public sealed class YautjaResinBreakerSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<YautjaResinBreakerComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<YautjaResinBreakerComponent> ent, ref MeleeHitEvent args)
    {
        if (!_net.IsServer)
            return;

        foreach (var target in args.HitEntities)
        {
            if (TerminatingOrDeleted(target))
                continue;

            if (!TryComp<DamageableComponent>(target, out var damageable))
                continue;

            if (damageable.DamageModifierSetId != "Resin")
                continue;

            PredictedQueueDel(target);
        }
    }
}
