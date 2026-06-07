// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Shitcode.Heretic.EntitySystems.PathSpecific;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Heretic.Prototypes;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualCosmosAscendBehavior : RitualSacrificeBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        if (!base.Execute(args, out outstr))
            return false;

        var targets = new List<EntityUid>();
        foreach (var uid in uids)
        {
            if (args.EntityManager.HasComponent<StarMarkComponent>(uid))
                targets.Add(uid);

            if (targets.Count >= Max)
                break;
        }

        if (targets.Count < Min)
        {
            outstr = Loc.GetString("heretic-ritual-fail-sacrifice-cosmos");
            return false;
        }

        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        base.Finalize(args);

        args.EntityManager.System<StarGazerSystem>()
            .ResolveStarGazer(args.Performer,
                out _,
                false,
                args.EntityManager.GetComponent<TransformComponent>(args.Platform).Coordinates);
    }
}
