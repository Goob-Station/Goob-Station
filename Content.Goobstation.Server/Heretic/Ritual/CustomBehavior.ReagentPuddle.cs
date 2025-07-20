// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Reagent;
using Content.Shared.Fluids.Components;
using Content.Shared.Heretic.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Heretic.Ritual;

public sealed partial class RitualReagentPuddleBehavior : RitualCustomBehavior
{
    [DataField]
    public ProtoId<ReagentPrototype>? Reagent;

    private EntityLookupSystem _lookup = default!;

    private List<EntityUid> _entities = [];

    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;

        if (Reagent == null)
            return true;

        _lookup = args.EntityManager.System<EntityLookupSystem>();

        var lookup = _lookup.GetEntitiesInRange(args.Platform, 1.5f);

        foreach (var ent in lookup)
        {
            if (!args.EntityManager.TryGetComponent<PuddleComponent>(ent, out var puddle)
                || puddle.Solution == null)
                continue;

            var soln = puddle.Solution.Value;

            if (!soln.Comp.Solution.ContainsPrototype(Reagent))
                continue;

            _entities.Add(ent);
        }

        if (_entities.Count == 0)
        {
            outstr = Loc.GetString("heretic-ritual-fail-reagentpuddle", ("reagentname", Reagent!));
            return false;
        }

        return true;
    }

    public override void Finalize(RitualData args)
    {
        foreach (var uid in _entities)
            args.EntityManager.QueueDeleteEntity(uid);

        _entities.Clear();
    }
}
