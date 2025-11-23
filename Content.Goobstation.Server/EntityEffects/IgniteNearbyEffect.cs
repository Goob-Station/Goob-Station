// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects;

/// <summary>
///     Ignites mobs nearby.
/// </summary>
public sealed partial class IgniteNearbyEffect : EntityEffect
{

    [DataField] public float Radius = 7;

    [DataField] public float FireStacks = 2;

    public override bool ShouldLog => true;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-ignite", ("chance", Probability));

    public override LogImpact LogImpact => LogImpact.Medium;

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is not EntityEffectReagentArgs reagentArgs)
            return;

        var entityManager = args.EntityManager;
        var lookupSys = entityManager.System<EntityLookupSystem>();
        var flamSys = entityManager.System<FlammableSystem>();

        foreach (var entity in lookupSys.GetEntitiesInRange(args.TargetEntity, Radius))
            if (entityManager.TryGetComponent(entity, out FlammableComponent? flammable))
                flamSys.AdjustFireStacks(entity, FireStacks, flammable, true);
    }
}
