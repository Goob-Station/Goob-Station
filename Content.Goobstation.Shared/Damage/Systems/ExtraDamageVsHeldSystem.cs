using Content.Shared.Damage.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Damage;

public sealed partial class ExtraDamageVsHeldSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExtraDamageVsHeldComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<ExtraDamageVsHeldComponent> ent, ref MeleeHitEvent args)
    {
        if (!_net.IsServer)
            return;

        foreach (var hit in args.HitEntities)
        {
            if (hit == args.User)
                continue;

            foreach (var held in _hands.EnumerateHeld(hit))
            {
                if (!_whitelist.IsWhitelistPass(ent.Comp.Whitelist, held))
                    continue;

                _damageable.TryChangeDamage(hit, ent.Comp.Damage, ent.Comp.IgnoreResistances, origin: args.User);
                break;
            }
        }
    }
}
