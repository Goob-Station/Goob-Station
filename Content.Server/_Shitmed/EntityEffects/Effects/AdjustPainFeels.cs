using System.Text.Json.Serialization;
using Content.Server.Body.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Systems;
using Content.Shared.EntityEffects;
using Content.Goobstation.Maths.FixedPoint;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.EntityEffects.Effects;

[UsedImplicitly]
public sealed partial class AdjustPainFeels : EntityEffect
{
    [DataField(required: true)]
    [JsonPropertyName("amount")]
    public FixedPoint2 Amount = default!;

    [DataField]
    [JsonPropertyName("identifier")]
    public string ModifierIdentifier = "PainSuppressant";

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-suppress-pain", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        var scale = FixedPoint2.New(1);

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            scale = reagentArgs.Quantity * reagentArgs.Scale;
        }

        if (!args.EntityManager.System<ConsciousnessSystem>().TryGetNerveSystem(args.TargetEntity, out var nerveSys))
            return;

        foreach (var bodyPart in args.EntityManager.System<BodySystem>().GetBodyChildren(args.TargetEntity))
        {
            if (!args.EntityManager.System<PainSystem>()
                    .TryGetPainFeelsModifier(bodyPart.Id, nerveSys.Value, ModifierIdentifier, out var modifier))
            {
                args.EntityManager.System<PainSystem>()
                    .TryAddPainFeelsModifier(
                        nerveSys.Value,
                        ModifierIdentifier,
                        bodyPart.Id,
                        IoCManager.Resolve<IRobustRandom>().Prob(0.3f) ? Amount * scale : -Amount * scale);
            }
            else
            {
                var add = IoCManager.Resolve<IRobustRandom>().Prob(0.3f) ? Amount : -Amount;
                args.EntityManager.System<PainSystem>()
                    .TryChangePainFeelsModifier(
                        nerveSys.Value,
                        ModifierIdentifier,
                        bodyPart.Id,
                        add * scale);
            }
        }
    }
}
