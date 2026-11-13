// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.RandomChanceSpawner;

public sealed partial class RandomChanceSpawnerSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedHandsSystem _hands = default!;


    [SubscribeLocalEvent]
    public void OnMapInit(Entity<RandomChanceSpawnerComponent> ent, ref MapInitEvent args)
    {
        foreach (KeyValuePair<EntProtoId, float> kvp in ent.Comp.ToSpawn)
            if (kvp.Value >= _random.NextFloat(0.0f, 1.0f))
                ent.Comp.Spawned.Add(SpawnAtPosition(kvp.Key, Transform(ent).Coordinates));

        QueueDel(ent);
    }

    [SubscribeLocalEvent]
    private void OnRemoved(Entity<RandomChanceSpawnerComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (!ent.Comp.PutInHand)
            return;

        var holder = args.Container.Owner;
        var handId = args.Container.ID;
        if (!_hands.TryGetHand(holder, handId, out _))
            return;

        foreach (var spawned in ent.Comp.Spawned)
        {
            if (_hands.TryPickup(holder, spawned, handId, checkActionBlocker: false, animate: false))
                break;
        }
    }
}
