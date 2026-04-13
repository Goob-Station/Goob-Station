
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Systems;
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Shitmed.EntityEffects.Effects;

public sealed partial class AdjustConsciousness : EntityEffectBase<AdjustConsciousness>
{
    [DataField(required: true)]
    public FixedPoint2 Amount = default!;

    [DataField(required: true)]
    public TimeSpan Time = default!;

    [DataField]
    public string Identifier = "ConsciousnessModifier";

    [DataField]
    public bool AllowNewModifiers = true;

    [DataField]
    public ConsciousnessModType ModifierType = ConsciousnessModType.Generic;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-adjust-consciousness");
}

public sealed class AdjustConsciousnessEffectSystem : EntityEffectSystem<BodyComponent, AdjustConsciousness>
{
    [Dependency] private readonly ConsciousnessSystem _consciousness = default!;

    protected override void Effect(Entity<BodyComponent> ent, ref EntityEffectEvent<AdjustConsciousness> args)
    {
        if (!_consciousness.TryGetNerveSystem(ent, out var nerveSys))
            return;

        var nerves = nerveSys.Value;
        var effect = args.Effect;
        var scale = FixedPoint2.New(args.Scale);

        // if it fails to edit and isn't allowed to make a new modifier, return
        if (_consciousness.EditConsciousnessModifier(ent,
                nerves,
                effect.Amount * scale,
                effect.Identifier,
                effect.Time) || !effect.AllowNewModifiers)
            return;

        _consciousness.AddConsciousnessModifier(ent,
            nerves,
            effect.Amount * scale,
            effect.Identifier,
            effect.ModifierType,
            effect.Time);
    }
}
