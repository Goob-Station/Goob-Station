// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Goobstation.Server.EntityEffects;

/// <summary>
///     Ignites mobs nearby.
/// </summary>
public sealed partial class IgniteNearby : EntityEffect
{

    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    [DataField]
    public float Range = 7;

    public override bool ShouldLog => true;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-ignite", ("chance", Probability));

    public override LogImpact LogImpact => LogImpact.Medium;

    public override void Effect(EntityEffectBaseArgs args)
    {
        var flamSys = args.EntityManager.System<FlammableSystem>();
        if (args is EntityEffectReagentArgs reagentArgs)
        {
            var lookup = _lookup.GetEntitiesInRange(args.TargetEntity, Range);
            var flammableEntities = lookup
                .Where(entity =>
                    entity != null && args.EntityManager.TryGetComponent(entity, out FlammableComponent? flammable))
                .Select(entity => (entity, args.EntityManager.GetComponent<FlammableComponent>(entity)))
                .ToHashSet();

            foreach (var (ent, flammable) in flammableEntities)
            {
                flamSys.Ignite(ent, args.TargetEntity, flammable: flammable);
            }
        }
    }
}
