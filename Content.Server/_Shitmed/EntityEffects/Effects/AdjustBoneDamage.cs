// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text.Json.Serialization;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared.EntityEffects;
using Content.Goobstation.Maths.FixedPoint;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Server.EntityEffects.Effects;

[UsedImplicitly]
public sealed partial class AdjustBoneDamage : EntityEffect
{
    [DataField(required: true)]
    [JsonPropertyName("amount")]
    public FixedPoint2 Amount = default!;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-adjust-bone-damage", ("amount", Amount));

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent<BodyComponent>(args.TargetEntity, out var body)
            || body.RootContainer.ContainedEntities.FirstOrNull() is not { } root)
            return;

        var traumaSystem = args.EntityManager.System<TraumaSystem>();
        var woundables = args.EntityManager.System<WoundSystem>().GetAllWoundableChildren(root).ToList();
        foreach (var woundable in woundables)
        {
            if (woundable.Comp.Bone.ContainedEntities.FirstOrNull() is not { } bone)
                continue;

            // Yeah this is less efficient when theres not as many parts damaged but who tf cares,
            // its a bone medication so it should probs be strong enough to ignore this.
            traumaSystem.ApplyDamageToBone(bone, Amount / woundables.Count);
        }
    }
}
