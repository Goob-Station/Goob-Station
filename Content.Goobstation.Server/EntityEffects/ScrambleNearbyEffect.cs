// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Shitmed.StatusEffects;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Content.Shared.Humanoid;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.EntityEffects;

/// <summary>
///     Scrambles the dna of nearby humanoids.
/// </summary>
public sealed partial class ScrambleNearbyEffect : EntityEffect
{

    [DataField] public float Radius = 7;

    public override bool ShouldLog => true;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-scramble-nearby");

    public override LogImpact LogImpact => LogImpact.Medium;

    public override void Effect(EntityEffectBaseArgs args)
    {
        var entityManager = args.EntityManager;
        var lookupSys = entityManager.System<EntityLookupSystem>();
        var scramSys = entityManager.System<ScrambleDnaEffectSystem>();

        foreach (var entity in lookupSys.GetEntitiesInRange(args.TargetEntity, Radius))
            if (entityManager.HasComponent<HumanoidAppearanceComponent>(entity))
                scramSys.Scramble(entity);
    }
}
