using Content.Shared.Hands.EntitySystems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Network;
using Robust.Shared.Random;

namespace Content.Shared._Goobstation.Security;

/// <summary>
/// Chance to drop held item upon hitting target.
/// </summary>

public sealed partial class DisarmOnHitSystem : EntitySystem
{
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DisarmOnHitComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<DisarmOnHitComponent> ent, ref MeleeHitEvent args)
    {
        if (!_net.IsServer) return;

        foreach (var hit in args.HitEntities)
        {
            if (hit == args.User || !_random.Prob(ent.Comp.Chance)) continue;

            var item = _hands.GetActiveItem(hit);

            if (item is null && !ent.Comp.ActiveHandOnly)
            {
                foreach (var held in _hands.EnumerateHeld(hit))
                {
                    item = held;
                    break;
                }
            }

            if (item is not { } toDrop) continue;

            _hands.TryDrop(hit, toDrop, checkActionBlocker: false);
        }
    }
}
