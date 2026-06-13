// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class ModifySlimeComponentSystem : EntityEffectSystem<SlimeComponent, ModifySlimeComponent>
{
    protected override void Effect(Entity<SlimeComponent> entity, ref EntityEffectEvent<ModifySlimeComponent> args)
    {
        var slime = entity.Comp;
        var effect = args.Effect;

        slime.ExtractsProduced += effect.ExtractBonus ?? 0;
        slime.MaxOffspring += effect.OffspringBonus ?? 0;

        if (effect.ChanceModifier is { } chanceMod)
            slime.MutationChance = Math.Clamp(slime.MutationChance + chanceMod, 0f, 1f);

        Dirty(entity);
    }
}

public sealed partial class ModifySlimeComponent : EntityEffectBase<ModifySlimeComponent>
{
    /// <summary>
    /// How many additional extracts will be produced?
    /// </summary>
    [DataField]
    public int? ExtractBonus;

    /// <summary>
    /// How many additional offspring MAY be produced?
    /// </summary>
    [DataField]
    public int? OffspringBonus;

    /// <summary>
    /// How much will we increase/decrease the mutation chance?
    /// </summary>
    [DataField]
    public float? ChanceModifier;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null; // todo add something here
}
