using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects.Effects;

public sealed partial class ModifySlimeComponent : EntityEffectBase<ModifySlimeComponent>
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
    /// Limit on extracts produced which cannot be exceeded.
    /// </summary>
    [DataField]
    public int ExtractLimit = 8;

    /// <summary>
    /// Limit on offspring which cannot be exceeded.
    /// </summary>
    [DataField]
    public int OffspringLimit = 6;

    /// <summary>
    /// How much will we increase/decrease the mutation chance?
    /// </summary>
    [DataField]
    public float ChanceModifier;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null; // TODO: add something here
}

public sealed partial class ModifySlimeComponentEffectSystem : EntityEffectSystem<SlimeComponent, ModifySlimeComponent>
{
    protected override void Effect(Entity<SlimeComponent> ent, ref EntityEffectEvent<ModifySlimeComponent> args)
    {
        var slime = ent.Comp;
        var effect = args.Effect;
        slime.ExtractsProduced += effect.ExtractBonus;
        slime.ExtractsProduced = Math.Min(slime.ExtractsProduced, effect.ExtractLimit);
        slime.MaxOffspring += effect.OffspringBonus;
        slime.MaxOffspring = Math.Min(slime.MaxOffspring, effect.OffspringLimit);
        slime.MutationChance = Math.Clamp(slime.MutationChance + effect.ChanceModifier, 0f, 1f);
        Dirty(ent);
    }
}
