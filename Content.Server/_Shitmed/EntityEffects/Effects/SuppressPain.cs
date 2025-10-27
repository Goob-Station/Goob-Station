using System.Text.Json.Serialization;
using Content.Server.Database;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Systems;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.EntityEffects;
using Content.Goobstation.Maths.FixedPoint;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
namespace Content.Server.EntityEffects.Effects;

[UsedImplicitly]
public sealed partial class SuppressPain : EntityEffect
{
    [DataField(required: true)]
    [JsonPropertyName("amount")]
    public FixedPoint2 Amount = default!;

    [DataField(required: true)]
    [JsonPropertyName("time")]
    public TimeSpan Time = default!;

    [DataField]
    [JsonPropertyName("identifier")]
    public string ModifierIdentifier = "PainSuppressant";

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-suppress-pain");

    public override void Effect(EntityEffectBaseArgs args)
    {
        var scale = FixedPoint2.New(1);

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            scale = reagentArgs.Quantity * reagentArgs.Scale;
        }

        if (!args.EntityManager.System<ConsciousnessSystem>().TryGetNerveSystem(args.TargetEntity, out var nerveSys))
            return;

        var bodyPart = args.EntityManager.System<SharedBodySystem>()
            .GetBodyChildrenOfType(args.TargetEntity, BodyPartType.Head)
            .FirstOrNull();

        if (bodyPart == null)
            return;

        if (!args.EntityManager.System<PainSystem>()
                .TryGetPainModifier(nerveSys.Value, bodyPart.Value.Id, ModifierIdentifier, out var modifier))
        {
            args.EntityManager.System<PainSystem>()
                .TryAddPainModifier(nerveSys.Value, bodyPart.Value.Id, ModifierIdentifier, -Amount * scale, time: Time);
        }
        else
        {
            args.EntityManager.System<PainSystem>()
                .TryChangePainModifier(nerveSys.Value, bodyPart.Value.Id, ModifierIdentifier, modifier.Value.Change - Amount * scale, time: Time);
        }
    }
}
