// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Server._Goobstation.Heretic.EntitySystems.PathSpecific;
using Content.Server.Body.Systems;
using Content.Server.Fluids.EntitySystems;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Heretic.Effects;

public sealed partial class SpillBlood : EntityEffect
{
    [DataField(required: true)]
    public FixedPoint2 Amount;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => "Spills target blood.";

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent(args.TargetEntity, out BloodstreamComponent? bloodStream))
            return;

        if (!args.EntityManager.System<SharedSolutionContainerSystem>()
                .ResolveSolution(args.TargetEntity,
                    bloodStream.BloodSolutionName,
                    ref bloodStream.BloodSolution,
                    out var bloodSolution))
            return;

        args.EntityManager.System<PuddleSystem>()
            .TrySpillAt(args.TargetEntity, bloodSolution.SplitSolution(Amount), out _);
    }
}
