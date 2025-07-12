// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics;
using System.Linq;
using Content.Server._Shitmed.StatusEffects;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared._Shitmed.StatusEffects;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Content.Shared.Humanoid;
using Robust.Shared.Prototypes;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Goobstation.Server.EntityEffects;

/// <summary>
///     Scrambles the dna of nearby humanoids.
/// </summary>
public sealed partial class ScrambleNearby : EntityEffect
{

    [DataField]
    public float Range = 7;

    public override bool ShouldLog => true;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;

    public override LogImpact LogImpact => LogImpact.Medium;

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is not EntityEffectReagentArgs reagentArgs)
            return;

        var entityManager = args.EntityManager;
        var lookupSys = entityManager.System<EntityLookupSystem>();
        var scramSys = entityManager.System<ScrambleDnaEffectSystem>();

        foreach (var entity in lookupSys.GetEntitiesInRange(args.TargetEntity, Range))
            if (entityManager.HasComponent<HumanoidAppearanceComponent>(entity))
                scramSys.Scramble(entity);
    }
}
