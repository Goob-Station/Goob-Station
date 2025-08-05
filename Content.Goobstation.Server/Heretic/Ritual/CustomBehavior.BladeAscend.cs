// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Heretic.Prototypes;
using System.Linq;
using Content.Goobstation.Shared.Heretic.Prototypes;

namespace Content.Goobstation.Server.Heretic.Ritual;

public sealed partial class RitualBladeAscendBehavior : RitualSacrificeBehavior
{
    private SharedBodySystem _body = default!;

    public override bool Execute(RitualData args, out string? outstr)
    {
        if (!base.Execute(args, out outstr))
            return false;

        _body = args.EntityManager.System<SharedBodySystem>();

        var beheadedBodies = Entities
            .Where(uid => !_body.GetBodyChildrenOfType(uid, BodyPartType.Head)
                .Any())
            .ToList();

        if (beheadedBodies.Count < Min)
        {
            outstr = Loc.GetString("heretic-ritual-fail-sacrifice-blade");
            return false;
        }

        outstr = null;
        return true;
    }
}

