// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 yglop <95057024+yglop@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.RandomChanceSpawner;

public sealed partial class RandomChanceSpawnerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RandomChanceSpawnerComponent, MapInitEvent>(OnMapInit);
    }

    public void OnMapInit(Entity<RandomChanceSpawnerComponent> ent, ref MapInitEvent args)
    {
        foreach(KeyValuePair<EntProtoId, float> kvp in ent.Comp.ToSpawn)
        {
            if (kvp.Value >= _random.NextFloat(0.0f, 1.0f))
                Spawn(kvp.Key, Transform(ent).Coordinates);
        }
        EntityManager.QueueDeleteEntity(ent);
    }
}