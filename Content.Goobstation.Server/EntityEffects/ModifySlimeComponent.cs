// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects;

public sealed partial class ModifySlimeComponent : EntityEffect
{
    /// <summary>
    /// How many additional extracts will be produced?
    /// </summary>
    [DataField]
    public int ExtractBonus;

    /// <summary>
    /// How many additional offspring MAY be produced?
    /// </summary>
    [DataField]
    public int OffspringBonus;

    /// <summary>
    /// How much will we increase/decrease the mutation chance?
    /// </summary>
    [DataField]
    public float ChanceModifier;

    /// <summary>
    /// Should we increase, or decrease the mutation chance?
    /// </summary>
    [DataField]
    public bool ShouldSubtract;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent<SlimeComponent>(args.TargetEntity, out var slime))
            return;

        slime.ExtractsProduced += ExtractBonus != 0 ? ExtractBonus : 0;
        slime.MaxOffspring += OffspringBonus != 0 ? OffspringBonus : 0;

        if (ChanceModifier != 0)
        {
            slime.MutationChance = Math.Clamp(
                slime.MutationChance + (ShouldSubtract ? -ChanceModifier : ChanceModifier),
                0f,
                1f
            );
        }
    }
}
