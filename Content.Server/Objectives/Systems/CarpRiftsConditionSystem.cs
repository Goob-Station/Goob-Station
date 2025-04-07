// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems;

public sealed class CarpRiftsConditionSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CarpRiftsConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(EntityUid uid, CarpRiftsConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = GetProgress(comp, _number.GetTarget(uid));
    }

    private float GetProgress(CarpRiftsConditionComponent comp, int target)
    {
        // prevent divide-by-zero
        if (target == 0)
            return 1f;

        if (comp.RiftsCharged >= target)
            return 1f;

        return (float) comp.RiftsCharged / (float) target;
    }

    /// <summary>
    /// Increments RiftsCharged, called after a rift fully charges.
    /// </summary>
    public void RiftCharged(EntityUid uid, CarpRiftsConditionComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.RiftsCharged++;
    }

    /// <summary>
    /// Resets RiftsCharged to 0, called after rifts get destroyed.
    /// </summary>
    public void ResetRifts(EntityUid uid, CarpRiftsConditionComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.RiftsCharged = 0;
    }
}