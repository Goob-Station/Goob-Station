// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Heretic;
using Content.Shared.Mind;
using Content.Shared.Store;

namespace Content.Server.Store.Conditions;

public sealed partial class HereticPathCondition : ListingCondition
{
    [DataField] public HashSet<string>? Whitelist;
    [DataField] public HashSet<string>? Blacklist;
    [DataField] public int Stage = 0;

    public override bool Condition(ListingConditionArgs args)
    {
        var ent = args.EntityManager;
        var minds = ent.System<SharedMindSystem>();

        if (!minds.TryGetMind(args.Buyer, out var mindId, out var mind))
            return false;

        if (!ent.TryGetComponent<HereticComponent>(args.Buyer, out var hereticComp))
            return false;

        if (Stage > hereticComp.PathStage)
            return false;

        if (Whitelist != null)
        {
            foreach (var white in Whitelist)
                if (hereticComp.CurrentPath == white)
                    return true;
            return false;
        }

        if (Blacklist != null)
        {
            foreach (var black in Blacklist)
                if (hereticComp.CurrentPath == black)
                    return false;
            return true;
        }

        return true;
    }
}