// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;
using Robust.Shared.Random;

namespace Content.Server.Objectives.Systems;

/// <summary>
/// Provides API for other components, handles picking the count and setting the title and description.
/// </summary>
public sealed class NumberObjectiveSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NumberObjectiveComponent, ObjectiveAssignedEvent>(OnAssigned);
        SubscribeLocalEvent<NumberObjectiveComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
    }

    private void OnAssigned(EntityUid uid, NumberObjectiveComponent comp, ref ObjectiveAssignedEvent args)
    {
        comp.Target = _random.Next(comp.Min, comp.Max);
    }

    private void OnAfterAssign(EntityUid uid, NumberObjectiveComponent comp, ref ObjectiveAfterAssignEvent args)
    {
        if (comp.Title != null)
            _metaData.SetEntityName(uid, Loc.GetString(comp.Title, ("count", comp.Target)), args.Meta);

        if (comp.Description != null)
            _metaData.SetEntityDescription(uid, Loc.GetString(comp.Description, ("count", comp.Target)), args.Meta);
    }

    /// <summary>
    /// Gets the objective's target count.
    /// </summary>
    public int GetTarget(EntityUid uid, NumberObjectiveComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return 0;

        return comp.Target;
    }
}