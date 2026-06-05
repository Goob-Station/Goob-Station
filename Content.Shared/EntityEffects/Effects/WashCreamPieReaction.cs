// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Reagent;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects;

public sealed partial class WashCreamPieReaction : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-wash-cream-pie-reaction", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent(args.TargetEntity, out CreamPiedComponent? creamPied)) return;

        args.EntityManager.System<SharedCreamPieSystem>().SetCreamPied(args.TargetEntity, creamPied, false);
    }
}
