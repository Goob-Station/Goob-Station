// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Heretic;
using Content.Shared.Heretic.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualPathBasedSpawnBehavior : RitualCustomBehavior
{
    [DataField(required: true)]
    public EntProtoId FallbackEntity;

    [DataField]
    public Dictionary<string, EntProtoId> SpawnedEntities = new();

    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        var heretic = args.Mind.Comp;

        var coords = args.EntityManager.GetComponent<TransformComponent>(args.Platform).Coordinates;

        if (heretic.CurrentPath != null && SpawnedEntities.TryGetValue(heretic.CurrentPath, out var toSpawn))
            args.EntityManager.SpawnEntity(toSpawn, coords);
        else
            args.EntityManager.SpawnEntity(FallbackEntity, coords);
    }
}
