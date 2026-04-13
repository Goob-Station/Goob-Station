// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Body;
using Content.Shared.EntityEffects;
using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Shitmed.EntityEffects.Effects;

/// <summary>
/// Evenly deals bone damage to each bone in the target mob.
/// The damage is split between them.
/// </summary>
public sealed partial class AdjustBoneDamage : EntityEffectBase<AdjustBoneDamage>
{
    [DataField(required: true)]
    public FixedPoint2 Amount = default!;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-adjust-bone-damage", ("amount", Amount));
}

public sealed class AdjustBoneDamageEffectSystem : EntityEffectSystem<BodyComponent, AdjustBoneDamage>
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;

    protected override void Effect(Entity<BodyComponent> ent, ref EntityEffectEvent<AdjustBoneDamage> args)
    {
        var parts = _body.GetBodyChildrenWithComponent<WoundableComponent>(ent.Owner, ent.Comp).ToList();
        if (parts.Count == 0)
            return;

        var amount = args.Effect.Amount / parts.Count;
        foreach (var (_, _, woundable) in parts)
        {
            var bone = woundable.Bone.ContainedEntities.FirstOrNull();
            if (bone == null || !TryComp<BoneComponent>(bone, out var boneComp))
                continue;

            // Yeah this is less efficient when theres not as many parts damaged but who tf cares,
            // its a bone medication so it should probs be strong enough to ignore this.
            _trauma.ApplyDamageToBone(bone.Value, amount, boneComp);
        }
    }
}
