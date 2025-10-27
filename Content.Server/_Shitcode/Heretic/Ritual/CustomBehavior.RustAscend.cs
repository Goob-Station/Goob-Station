// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Heretic.Prototypes;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualRustAscendBehavior : RitualSacrificeBehavior
{
    [DataField]
    public EntProtoId AscensionSpreader = "HereticRustAscensionSpreader";

    public override bool Execute(RitualData args, out string? outstr)
    {
        if (!base.Execute(args, out outstr))
            return false;

        var targets = new List<EntityUid>();
        for (var i = 0; i < Max; i++)
        {
            if (args.EntityManager.HasComponent<RottingComponent>(uids[i]) ||
                args.EntityManager.HasComponent<SiliconComponent>(uids[i]))
                targets.Add(uids[i]);
        }

        if (targets.Count < Min)
        {
            outstr = Loc.GetString("heretic-ritual-fail-sacrifice-rust");
            return false;
        }

        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        base.Finalize(args);

        args.EntityManager.Spawn(AscensionSpreader,
            args.EntityManager.System<TransformSystem>().GetMapCoordinates(args.Platform));
    }
}