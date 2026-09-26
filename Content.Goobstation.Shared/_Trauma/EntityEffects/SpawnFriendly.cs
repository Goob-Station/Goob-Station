// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects.EntitySpawning;
using Content.Shared.NPC.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared._Trauma.EntityEffects;

/// <summary>
/// Like SpawnEntity but makes every spawned NPC friendly to the target entity.
/// </summary>
public sealed partial class SpawnFriendly : BaseSpawnEntityEntityEffect<SpawnFriendly>;

/// <summary>
/// Spawns a number of entities of a given prototype at the coordinates of this entity.
/// Amount is modified by scale.
/// </summary>
public sealed partial class SpawnFriendlyEffectSystem : EntityEffectSystem<TransformComponent, SpawnFriendly>
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private NpcFactionSystem _faction = default!;

    protected override void Effect(Entity<TransformComponent> entity, ref EntityEffectEvent<SpawnFriendly> args)
    {
        if (_net.IsClient)
            return;

        var quantity = args.Effect.Number * (int)Math.Floor(args.Scale);
        var proto = args.Effect.Entity;

        for (var i = 0; i < quantity; i++)
        {
            var spawned = PredictedSpawnNextToOrDrop(proto, entity, entity.Comp);
            _faction.IgnoreEntity(spawned, entity.Owner);
        }
    }
}
