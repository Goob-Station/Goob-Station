// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;

/// <summary>
///     Ignites a mob.
/// </summary>
public sealed partial class Ignite : EntityEffect
{
    public override bool ShouldLog => true;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-ignite", ("chance", Probability));

    public override LogImpact LogImpact => LogImpact.Medium;

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent(args.TargetEntity, out FlammableComponent? flammable))
            return;

        var flamSys = args.EntityManager.System<FlammableSystem>();
        if (args is EntityEffectReagentArgs reagentArgs)
        {
            flamSys.Ignite(reagentArgs.TargetEntity, reagentArgs.OrganEntity ?? reagentArgs.TargetEntity, flammable: flammable);
        }
        else
        {
            flamSys.Ignite(args.TargetEntity, args.TargetEntity, flammable: flammable);
        }
    }
}